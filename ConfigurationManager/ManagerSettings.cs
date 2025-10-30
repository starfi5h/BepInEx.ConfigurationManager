using BepInEx.Configuration;
using ConfigurationManager.UI;
using UnityEngine;

namespace ConfigurationManager
{
    /// <summary>
    /// Manages all configuration settings for the Configuration Manager
    /// </summary>
    internal class ManagerSettings
    {
        // Filtering settings
        public ConfigEntry<bool> ShowAdvanced { get; }
        public ConfigEntry<bool> ShowKeybinds { get; }
        public ConfigEntry<bool> ShowSettings { get; }
        public ConfigEntry<KeyboardShortcut> Keybind { get; }
        public ConfigEntry<bool> HideSingleSection { get; }
        public ConfigEntry<bool> PluginConfigCollapsedDefault { get; }

        // UI settings
        public ConfigEntry<Color> BackgroundColor { get; }
        public ConfigEntry<Color> FontColor { get; }
        public ConfigEntry<int> FontSize { get; }
        public ConfigEntry<Color> AdvancedSettingColor { get;  }

        // Window settings
        public ConfigEntry<int> WindowX { get; }
        public ConfigEntry<int> WindowY { get; }
        public ConfigEntry<int> WindowWidth { get; }
        public ConfigEntry<int> WindowHeight { get; }
        public ConfigEntry<float> ColumnLeftRatio { get; }

        public bool ShowDebug { get; set; }
        public bool IsWindowChanged { get; set; }

        public ManagerSettings(ConfigFile config)
        {
            ShowAdvanced = config.Bind("Filtering", "Show advanced", false);
            ShowKeybinds = config.Bind("Filtering", "Show keybinds", true);
            ShowSettings = config.Bind("Filtering", "Show settings", true);

            Keybind = config.Bind("General", "Show config manager", new KeyboardShortcut(KeyCode.F1),
                new ConfigDescription("The shortcut used to toggle the config manager window on and off.\n" +
                                      "The key can be overridden by a game-specific plugin if necessary, in that case this setting is ignored."));

            HideSingleSection = config.Bind("General", "Hide single sections", false,
                new ConfigDescription("Show section title for plugins with only one section"));

            PluginConfigCollapsedDefault = config.Bind("General", "Plugin collapsed default", true,
                new ConfigDescription("If set to true plugins will be collapsed when opening the configuration manager window"));

            BackgroundColor = config.Bind("UI", "Background color", new Color(0f, 0f, 0f, 0.75f));
            FontColor = config.Bind("UI", "Font color", new Color(1f, 1f, 1f, 1f));
            FontSize = config.Bind("UI", "Font size", 14, new ConfigDescription("", new AcceptableValueRange<int>(8, 40)));
            AdvancedSettingColor = config.Bind("UI", "Advanced setting color", new Color(1f, 0.95f, 0.67f, 1f));
            BackgroundColor.SettingChanged += OnUISettingsChanged;
            FontSize.SettingChanged += OnUISettingsChanged;

            WindowX = config.Bind("Window", "X Position", -1);
            WindowY = config.Bind("Window", "Y Position", -1);
            WindowWidth = config.Bind("Window", "Width", 650);
            WindowHeight = config.Bind("Window", "Height", -1);
            ColumnLeftRatio = config.Bind("Window", "ColumnLeftRatio", 0.40f,
                new ConfigDescription("The width ratio of the setting name", new AcceptableValueRange<float>(0f, 1f)));
            WindowX.SettingChanged += (obj, args) => OnWindowChanged(this);
            WindowY.SettingChanged += (obj, args) => OnWindowChanged(this);
            WindowWidth.SettingChanged += (obj, args) => OnWindowChanged(this);
            WindowHeight.SettingChanged += (obj, args) => OnWindowChanged(this);
            ColumnLeftRatio.SettingChanged += (obj, args) => OnWindowChanged(this);
        }

        private static void OnUISettingsChanged(object obj, System.EventArgs args)
        {
            StyleManager.CleanCache();
        }

        private static void OnWindowChanged(ManagerSettings settings)
        {
            settings.IsWindowChanged = true;
        }
    }
}
