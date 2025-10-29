using BepInEx.Configuration;
using UnityEngine;

namespace ConfigurationManager
{
    /// <summary>
    /// Manages all configuration settings for the Configuration Manager
    /// </summary>
    internal class ConfigManagerSettings
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
        public ConfigEntry<int> FontSize { get; }

        // Window settings
        public ConfigEntry<int> WindowX { get; }
        public ConfigEntry<int> WindowY { get; }
        public ConfigEntry<int> WindowWidth { get; }
        public ConfigEntry<int> WindowHeight { get; }

        public bool ShowDebug { get; set; }

        public ConfigManagerSettings(ConfigFile config)
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
            FontSize = config.Bind("UI", "Font size", 14,
                new ConfigDescription("", new AcceptableValueRange<int>(9, 40)));

            WindowX = config.Bind("Window", "X Position", -1);
            WindowY = config.Bind("Window", "Y Position", -1);
            WindowWidth = config.Bind("Window", "Width", 600);
            WindowHeight = config.Bind("Window", "Height", -1);
        }
    }
}
