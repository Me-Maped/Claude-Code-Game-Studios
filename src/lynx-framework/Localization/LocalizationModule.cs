using System.Collections.Generic;
using Godot;
using LynxFramework.Core;
using LynxFramework.Log;
using LynxFramework.Resource;

namespace LynxFramework.Localization;

/// <summary>
/// 本地化模块。
/// 封装 Godot TranslationServer，支持动态语言切换和事件通知。
/// </summary>
public class LocalizationModule : IModule
{
    private ResourceService _resourceService;
    private EventBus _eventBus;
    private LogService _logService;
    private string _currentLanguage = "en";
    private readonly Dictionary<string, Translation> _translations = new();

    public int Priority => 70;

    public void OnInit()
    {
        _resourceService = FrameworkEntry.Instance.GetModule<ResourceService>();
        _eventBus = FrameworkEntry.Instance.GetModule<EventBus>();
        _logService = FrameworkEntry.Instance.GetModule<LogService>();
    }

    public void OnShutdown() => _translations.Clear();

    /// <summary>切换语言。触发 language_changed 事件。</summary>
    public void SetLanguage(string lang)
    {
        if (_currentLanguage == lang) return;
        _currentLanguage = lang;
        TranslationServer.SetLocale(lang);
        _eventBus?.Emit("language_changed", new LanguageChangedEventArg { Language = lang });
        _logService?.Info($"Language changed: {lang}");
    }

    /// <summary>获取当前语言。</summary>
    public string GetLanguage() => _currentLanguage;

    /// <summary>翻译指定 key。</summary>
    public string GetText(string key) => TranslationServer.Translate(key);

    /// <summary>添加翻译资源。</summary>
    public void AddTranslation(Translation translation)
    {
        _translations[translation.Locale] = translation;
        TranslationServer.AddTranslation(translation);
    }

    /// <summary>从文件加载翻译资源。</summary>
    public void AddTranslationFromResource(string path)
    {
        var translation = _resourceService.LoadCached<Translation>(path);
        if (translation != null)
            AddTranslation(translation);
        else
            _logService?.Warning($"Translation resource not found: {path}");
    }
}
