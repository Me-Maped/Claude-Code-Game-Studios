using System;
using Godot;
using LynxFramework.Core;
using LynxFramework.Log;

namespace LynxFramework.Network;

/// <summary>
/// 网络模块。
/// TCP 连接管理、心跳、自动重连、协议编解码。
/// </summary>
public partial class NetworkModule : IModule
{
    private EventBus _eventBus;
    private LogService _logService;
    private UpdateDriver _updateDriver;
    private StreamPeerTcp _tcpClient;
    private PacketPeerStream _packetStream;
    private IProtocolHandler _protocol;
    private ConnectionState _state = ConnectionState.Disconnected;
    private float _heartbeatInterval = 30f;
    private float _reconnectInterval = 5f;
    private double _lastHeartbeatTime;
    private double _lastReconnectTime;
    private string _host;
    private int _port;
    private UpdateEntry _updateEntry;

    public int Priority => 110;

    public void OnInit()
    {
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        _logService = FrameworkEntry.Instance.GetModule<LogService>();
        _updateDriver = FrameworkEntry.Instance.GetModule<UpdateDriver>();
        _updateEntry = _updateDriver.Register(UpdatePhase.Process, 200, OnUpdate);
    }

    public void OnShutdown()
    {
        Disconnect();
        if (_updateEntry != null)
            _updateDriver.Unregister(_updateEntry);
    }

    /// <summary>连接到服务器。</summary>
    public void ConnectToServer(string host, int port)
    {
        _host = host;
        _port = port;
        _tcpClient = new StreamPeerTcp();
        _packetStream = new PacketPeerStream { StreamPeer = _tcpClient };

        var err = _tcpClient.ConnectToHost(host, port);
        if (err != Error.Ok)
        {
            _logService?.Error($"TCP connect failed: {err}");
            return;
        }

        SetState(ConnectionState.Connecting);
        _logService?.Info($"Connecting to {host}:{port}...");
    }

    /// <summary>断开连接。</summary>
    public void Disconnect()
    {
        if (_tcpClient != null && _tcpClient.GetStatus() != StreamPeerTcp.Status.None)
            _tcpClient.DisconnectFromHost();
        SetState(ConnectionState.Disconnected);
    }

    /// <summary>发送消息。</summary>
    public void SendMessage(int msgId, byte[] data)
    {
        if (_state != ConnectionState.Connected) return;

        // 消息头: msgId(4 bytes) + length(4 bytes) + body
        var header = new byte[8];
        BitConverter.GetBytes(msgId).CopyTo(header, 0);
        BitConverter.GetBytes(data.Length).CopyTo(header, 4);

        var packet = new byte[header.Length + data.Length];
        Buffer.BlockCopy(header, 0, packet, 0, header.Length);
        Buffer.BlockCopy(data, 0, packet, header.Length, data.Length);
        _packetStream.PutPacket(packet);
    }

    /// <summary>设置协议编解码器。</summary>
    public void SetProtocolHandler(IProtocolHandler handler) => _protocol = handler;

    /// <summary>获取当前连接状态。</summary>
    public ConnectionState GetConnectionState() => _state;

    /// <summary>发送 HTTP 请求。</summary>
    public async void Request(string url, string method, string[] headers, string body)
    {
        var httpClient = new HttpRequest();
        FrameworkEntry.Instance.AddChild(httpClient);

        httpClient.RequestCompleted += (result, responseCode, responseHeaders, responseBody) =>
        {
            _logService?.Info($"HTTP {method} {url} → {responseCode}");
            httpClient.QueueFree();
        };

        var err = httpClient.Request(url, headers, (HttpClient.Method)Enum.Parse(typeof(HttpClient.Method), method, true), body);
        if (err != Error.Ok)
        {
            _logService?.Error($"HTTP request failed: {err}");
            httpClient.QueueFree();
        }
    }

    /// <summary>设置心跳间隔（秒）。</summary>
    public void SetHeartbeatInterval(float interval) => _heartbeatInterval = interval;

    /// <summary>设置重连间隔（秒）。</summary>
    public void SetReconnectInterval(float interval) => _reconnectInterval = interval;

    private void OnUpdate(float delta)
    {
        if (_state == ConnectionState.Disconnected) return;

        var status = _tcpClient.GetStatus();

        if (status == StreamPeerTcp.Status.Connected)
        {
            if (_state == ConnectionState.Connecting || _state == ConnectionState.Reconnecting)
            {
                SetState(ConnectionState.Connected);
                _logService?.Info($"Connected to {_host}:{_port}");
            }

            // 接收数据
            while (_packetStream.GetAvailablePacketCount() > 0)
            {
                var packet = _packetStream.GetPacket();
                var decoded = _protocol?.Decode(packet);
                _eventBus?.Emit("network_data_received", new NetworkDataEventArg { Data = packet });
            }

            // 心跳
            _lastHeartbeatTime += delta;
            if (_lastHeartbeatTime >= _heartbeatInterval)
            {
                SendMessage(0, System.Array.Empty<byte>());
                _lastHeartbeatTime = 0;
            }
        }
        else if (status == StreamPeerTcp.Status.Error || status == StreamPeerTcp.Status.None)
        {
            // 自动重连
            if (_state == ConnectionState.Connected)
            {
                SetState(ConnectionState.Reconnecting);
                _logService?.Warning("Connection lost, attempting reconnect...");
            }

            _lastReconnectTime += delta;
            if (_lastReconnectTime >= _reconnectInterval)
            {
                _tcpClient.ConnectToHost(_host, _port);
                _lastReconnectTime = 0;
                _logService?.Info("Reconnect attempt...");
            }
        }
    }

    private void SetState(ConnectionState state)
    {
        if (_state == state) return;
        _state = state;
        _eventBus?.Emit("network_state_changed", new NetworkStateEventArg { State = state });
    }
}
