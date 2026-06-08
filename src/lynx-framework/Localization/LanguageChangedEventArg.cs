namespace LynxFramework.Localization;

/// <summary>语言切换事件载荷。</summary>
public partial class LanguageChangedEventArg : EventArg
{
    public string Language { get; set; }
}
