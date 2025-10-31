// Made by MarC0 / ManlyMarco
// Copyright 2018 GNU General Public License v3.0

#pragma warning disable IDE0051 // Remove unused private members - Unity lifecycle method

using BepInEx;
using BepInEx.Logging;
using ConfigurationManager.UI;
using ConfigurationManager.Utilities;
using System;
using System.IO;

namespace ConfigurationManager
{
    /// <summary>
    /// An easy way to let user configure how a plugin behaves without the need to make your own GUI. 
    /// The user can change any of the settings you expose, even keyboard shortcuts.
    /// https://github.com/ManlyMarco/BepInEx.ConfigurationManager
    /// </summary>
    [BepInPlugin(GUID, "Configuration Manager", Version)]
    public class ConfigurationManager : BaseUnityPlugin
    {
        /// <summary>
        /// GUID of this plugin
        /// </summary>
        public const string GUID = "com.bepis.bepinex.configurationmanager";

        /// <summary>
        /// Version constant
        /// </summary>
        public const string Version = "17.0";

        internal static new ManualLogSource Logger;
        internal static ConfigurationManager Instance;

        private ManagerSettings _settings;
        private ConfigWindow _windowManager;
        private PluginSettingsDataManager _settingsDataManager;
        private CursorManager _cursorManager;

        /// <summary>
        /// Event fired every time the manager window is shown or hidden.
        /// </summary>
        public event EventHandler<ValueChangedEventArgs<bool>> DisplayingWindowChanged;

        /// <summary>
        /// Disable the hotkey check used by config manager. 
        /// If enabled you have to set <see cref="DisplayingWindow"/> to show the manager.
        /// </summary>
        public bool OverrideHotkey;

        private bool _displayingWindow;

        /// <summary>
        /// Is the config manager main window displayed on screen
        /// </summary>
        public bool DisplayingWindow
        {
            get => _displayingWindow;
            set
            {
                if (_displayingWindow == value) return;
                _displayingWindow = value;

                SettingFieldDrawer.ClearCache();

                if (_displayingWindow)
                {
                    _windowManager.OnWindowShown();
                    _settingsDataManager.BuildSettingList();
                    _cursorManager.SaveCursorState();
                }
                else
                {
                    _cursorManager.RestoreCursorState();
                    _windowManager.SaveWindowPosition();
                }

                DisplayingWindowChanged?.Invoke(this, new ValueChangedEventArgs<bool>(value));
            }
        }

        /// <summary>
        /// Register a custom setting drawer for a given type. 
        /// The action is ran in OnGui in a single setting slot.
        /// Do not use any Begin / End layout methods, and avoid raising height from standard.
        /// </summary>
        public static void RegisterCustomSettingDrawer(Type settingType, Action<SettingEntryBase> onGuiDrawer)
        {
            if (settingType == null) throw new ArgumentNullException(nameof(settingType));
            if (onGuiDrawer == null) throw new ArgumentNullException(nameof(onGuiDrawer));

            if (SettingFieldDrawer.SettingDrawHandlers.ContainsKey(settingType))
                Logger.LogWarning("Tried to add a setting drawer for type " + settingType.FullName + " while one already exists.");
            else
                SettingFieldDrawer.SettingDrawHandlers[settingType] = onGuiDrawer;
        }

        /// <summary>
        /// Rebuild the setting list. Use to update the config manager window 
        /// if config settings were removed or added while it was open.
        /// </summary>
        public void BuildSettingList()
        {
            _settingsDataManager.BuildSettingList();
        }

        private void Start()
        {
            Logger = base.Logger;
            Instance = this;
            _settings = new ManagerSettings(Config);
            _settingsDataManager = new PluginSettingsDataManager(_settings);
            _windowManager = new ConfigWindow(_settings, _settingsDataManager);
            _cursorManager = new CursorManager();
            _cursorManager.Initialize();
            LocalizationManager.CurrentLanguage = _settings.Language.Value;

            // Check if user has permissions to write config files to disk
            try
            {
                Config.Save();
            }
            catch (IOException ex)
            {
                Logger.Log(LogLevel.Message | LogLevel.Warning,
                    "WARNING: Failed to write to config directory, expect issues!\nError message:" + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Log(LogLevel.Message | LogLevel.Warning,
                    "WARNING: Permission denied to write to config directory, expect issues!\nError message:" + ex.Message);
            }
        }

        private void Update()
        {
            if (DisplayingWindow)
                _cursorManager.SetUnlockCursor(0, true);

            if (OverrideHotkey) return;

            if (_settings.Keybind.Value.IsDown())
                DisplayingWindow = !DisplayingWindow;
        }

        private void LateUpdate()
        {
            if (DisplayingWindow)
                _cursorManager.SetUnlockCursor(0, true);
        }

        private void OnGUI()
        {
            if (DisplayingWindow)
            {
                _cursorManager.SetUnlockCursor(0, true);
                _windowManager.DrawWindow();
            }
        }

#if DEBUG
        private void OnDestroy()
        {
            StyleManager.CleanCache();
        }
#endif

    }
}