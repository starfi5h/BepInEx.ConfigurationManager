// Localization System for ConfigurationManager
// Simplified version: English and Simplified Chinese only

using System.Collections.Generic;

namespace ConfigurationManager.Utilities
{
    /// <summary>
    /// Manages localization and translations
    /// </summary>
    public static class LocalizationManager
    {
        public enum Language
        {
            English,
            Chinese
        }

        private static Language _currentLanguage = Language.English;

        public static Language CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (value == _currentLanguage) return;
                _currentLanguage = value;
                OnLanguageChanged?.Invoke(value);
            }
        }

        public static event System.Action<Language> OnLanguageChanged;

        private static readonly Dictionary<string, string> _chineseTranslations = new Dictionary<string, string>
        {
            // Window and UI
            ["Plugin / mod settings"] = "插件/模组设置",

            // Header - Filter Options
            ["Show: "] = "显示: ",
            ["Normal settings"] = "常规设置",
            ["Keyboard shortcuts"] = "键位设置",
            ["Advanced settings"] = "进阶设置",
            ["Debug mode"] = "调试模式",

            // Search
            ["Search settings: "] = "搜索设置: ",
            ["Clear"] = "清除",

            // Buttons
            ["Open Unity Log"] = "打开 Unity 日志",
            ["Open BepInEx Log"] = "打开 BepInEx 日志",
            ["Expand All"] = "全部展开",
            ["Collapse All"] = "全部收起",

            // Tips
            ["Tip: Click plugin names to expand. Click setting names to see their descriptions."] =
                "提示：点击插件名称可展开，点击设置名称可查看说明。",

            // Plugin actions
            ["Reload"] = "重新加载",
            ["Trigger Config.Reload() event to apply settings from the config file"] =
                "触发 Config.Reload() 事件以应用配置文件中的设置",
            ["Open File"] = "打开文件",
            ["Open the plugin config file"] = "打开插件配置文件",
            ["Reset"] = "重置",

            // Setting values
            ["Enabled"] = "已启用",
            ["Disabled"] = "已禁用",

            // Keyboard shortcut
            ["Press any key combination"] = "按下任意按键组合",
            ["Cancel"] = "取消",

            // Debug info
            ["Plugins with no options available: "] = "无配置选项的mod: ",
            ["Failed to draw this field, check log for details."] =
                "无法绘制此字段，详情请查看日志。",

            // Language selector
            ["Language: "] = "语言: "
        };

        /// <summary>
        /// Add a new translation entry at runtime
        /// </summary>
        public static void AddTranslation(string englishText, string chineseText)
        {
            _chineseTranslations[englishText] = chineseText;
        }

        /// <summary>
        /// Get translation for a key in current language
        /// </summary>
        public static string GetTranslation(string text)
        {
            if (_currentLanguage == Language.English)
                return text;

            return _chineseTranslations.TryGetValue(text, out var translation)
                ? translation
                : text;
        }

        /// <summary>
        /// Toggle between English and Chinese
        /// </summary>
        public static void ToggleLanguage()
        {
            CurrentLanguage = _currentLanguage == Language.English
                ? Language.Chinese
                : Language.English;
        }
    }

    /// <summary>
    /// String extension methods for easy translation
    /// </summary>
    public static class LocalizationExtensions
    {
        /// <summary>
        /// Translate this string to the current language
        /// Usage: "Hello World".Translate()
        /// </summary>
        public static string Translate(this string text)
        {
            return LocalizationManager.GetTranslation(text);
        }
    }
}
