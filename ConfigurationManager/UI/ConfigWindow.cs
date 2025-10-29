// Made by MarC0 / ManlyMarco
// Copyright 2018 GNU General Public License v3.0

using System;
using BepInEx.Logging;
using ConfigurationManager.Utilities;
using UnityEngine;

namespace ConfigurationManager.UI
{
    /// <summary>
    /// Manages the configuration window rendering and layout
    /// </summary>
    internal class ConfigWindow
    {
        private const int WindowId = -68;
        private const string SearchBoxName = "searchBox";
        private static readonly Color AdvancedSettingColor = new Color(1f, 0.95f, 0.67f, 1f);

        private readonly ConfigManagerSettings _settings;
        private readonly WindowResizeHandler _resizeHandler;
        private readonly SettingFieldDrawer _fieldDrawer;

        internal Rect SettingWindowRect { get; private set; }
        internal int LeftColumnWidth { get; private set; }
        internal int RightColumnWidth { get; private set; }
        internal static Texture2D TooltipBg { get; private set; }
        internal static Texture2D WindowBackground { get; private set; }

        private Vector2 _settingWindowScrollPos;
        private int _tipsHeight;

        public ConfigWindow(ConfigManagerSettings settings)
        {
            _settings = settings;
            _resizeHandler = new WindowResizeHandler();
            _fieldDrawer = new SettingFieldDrawer(this);
        }

        /// <summary>
        /// Initialize textures and resources
        /// </summary>
        public void Initialize()
        {
            var background = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            background.SetPixel(0, 0, Color.black);
            background.Apply();
            TooltipBg = background;

            var windowBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            windowBackground.SetPixel(0, 0, _settings.BackgroundColor.Value);
            windowBackground.Apply();
            WindowBackground = windowBackground;
        }

        /// <summary>
        /// Called when window is shown
        /// </summary>
        public void OnWindowShown()
        {
            CalculateWindowRect();
        }

        /// <summary>
        /// Save current window position to config
        /// </summary>
        public void SaveWindowPosition()
        {
            _settings.WindowX.Value = (int)SettingWindowRect.x;
            _settings.WindowY.Value = (int)SettingWindowRect.y;
            _settings.WindowWidth.Value = (int)SettingWindowRect.width;
            _settings.WindowHeight.Value = (int)SettingWindowRect.height;
        }

        /// <summary>
        /// Draw the main configuration window
        /// </summary>
        public void DrawWindow(PluginSettingsDataManager settingsManager)
        {
            GUI.Box(SettingWindowRect, GUIContent.none,
                new GUIStyle { normal = new GUIStyleState { background = WindowBackground } });

            SettingWindowRect = _resizeHandler.HandleWindowResize(SettingWindowRect, out var columnWidthsChanged);

            if (columnWidthsChanged)
            {
                LeftColumnWidth = Mathf.RoundToInt(SettingWindowRect.width / 2.5f);
                RightColumnWidth = (int)SettingWindowRect.width - LeftColumnWidth - 115;
            }

            SettingWindowRect = GUILayout.Window(WindowId, SettingWindowRect,
                id => SettingsWindow(id, settingsManager), "Plugin / mod settings");

            // Eat only left mouse click in the window
            bool inWindow = SettingWindowRect.Contains(Event.current.mousePosition);
            bool isClick = Input.GetMouseButton(0) && Input.GetMouseButtonDown(0);
            if (!SettingFieldDrawer.SettingKeyboardShortcut && inWindow && isClick)
                Input.ResetInputAxes();
        }

        private void CalculateWindowRect()
        {
            var width = _settings.WindowWidth.Value > 0 ? _settings.WindowWidth.Value : Mathf.Min(Screen.width, 650);
            var height = _settings.WindowHeight.Value > 0 ? _settings.WindowHeight.Value : (Screen.height < 560 ? Screen.height : Screen.height - 100);
            var offsetX = _settings.WindowX.Value >= 0 ? _settings.WindowX.Value : Mathf.RoundToInt((Screen.width - width) / 2f);
            var offsetY = _settings.WindowY.Value >= 0 ? _settings.WindowY.Value : Mathf.RoundToInt((Screen.height - height) / 2f);
            SettingWindowRect = new Rect(offsetX, offsetY, width, height);

            LeftColumnWidth = Mathf.RoundToInt(SettingWindowRect.width / 2.5f);
            RightColumnWidth = (int)SettingWindowRect.width - LeftColumnWidth - 115;
        }

        private void SettingsWindow(int id, PluginSettingsDataManager settingsManager)
        {
            // Close button
            GUILayout.BeginArea(new Rect(SettingWindowRect.width - 27f, 1f, 25f, 21f));
            if (GUILayout.Button("X"))
            {
                // This will be handled by the main ConfigurationManager class
                var configManager = BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<ConfigurationManager>();
                if (configManager != null)
                    configManager.DisplayingWindow = false;
            }
            GUILayout.EndArea();

            // Title bar drag
            var titleBarRect = new Rect(0, 0, SettingWindowRect.width, 20);
            GUI.DragWindow(titleBarRect);

            GUILayout.Space(3);
            DrawWindowHeader(settingsManager);

            _settingWindowScrollPos = GUILayout.BeginScrollView(_settingWindowScrollPos, false, true);

            var scrollPosition = _settingWindowScrollPos.y;
            var scrollHeight = SettingWindowRect.height;

            GUILayout.BeginVertical();
            {
                if (string.IsNullOrEmpty(settingsManager.SearchString))
                {
                    DrawTips();

                    if (_tipsHeight == 0 && Event.current.type == EventType.Repaint)
                        _tipsHeight = (int)GUILayoutUtility.GetLastRect().height;
                }

                var currentHeight = _tipsHeight;

                foreach (var plugin in settingsManager.FilteredSettings)
                {
                    var visible = plugin.Height == 0 || currentHeight + plugin.Height >= scrollPosition && currentHeight <= scrollPosition + scrollHeight;

                    if (visible)
                    {
                        try
                        {
                            DrawSinglePlugin(plugin, settingsManager.SearchString);
                        }
                        catch (ArgumentException)
                        {
                            // Needed to avoid GUILayout: Mismatched LayoutGroup.Repaint crashes on large lists
                        }

                        if (plugin.Height == 0 && Event.current.type == EventType.Repaint)
                            plugin.Height = (int)GUILayoutUtility.GetLastRect().height;
                    }
                    else
                    {
                        try
                        {
                            GUILayout.Space(plugin.Height);
                        }
                        catch (ArgumentException)
                        {
                            // Needed to avoid GUILayout: Mismatched LayoutGroup.Repaint crashes on large lists
                        }
                    }

                    currentHeight += plugin.Height;
                }

                if (_settings.ShowDebug)
                {
                    GUILayout.Space(10);
                    GUILayout.Label("Plugins with no options available: " + settingsManager.ModsWithoutSettings);
                }
                else
                {
                    // Always leave some space in case there's a dropdown box at the very bottom of the list
                    GUILayout.Space(70);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            if (!SettingFieldDrawer.DrawCurrentDropdown())
                DrawTooltip(SettingWindowRect);
        }

        private void DrawTips()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Tip: Click plugin names to expand. Click setting and group names to see their descriptions.");
            }
            GUILayout.EndHorizontal();
        }

        private void DrawWindowHeader(PluginSettingsDataManager settingsManager)
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.Label("Show: ", GUILayout.ExpandWidth(false));

                GUI.enabled = settingsManager.SearchString == string.Empty;

                var newVal = GUILayout.Toggle(_settings.ShowSettings.Value, "Normal settings");
                if (_settings.ShowSettings.Value != newVal)
                {
                    _settings.ShowSettings.Value = newVal;
                    settingsManager.BuildFilteredSettingList();
                }

                newVal = GUILayout.Toggle(_settings.ShowKeybinds.Value, "Keyboard shortcuts");
                if (_settings.ShowKeybinds.Value != newVal)
                {
                    _settings.ShowKeybinds.Value = newVal;
                    settingsManager.BuildFilteredSettingList();
                }

                var origColor = GUI.color;
                GUI.color = AdvancedSettingColor;
                newVal = GUILayout.Toggle(_settings.ShowAdvanced.Value, "Advanced settings");
                if (_settings.ShowAdvanced.Value != newVal)
                {
                    _settings.ShowAdvanced.Value = newVal;
                    settingsManager.BuildFilteredSettingList();
                }
                GUI.color = origColor;

                GUI.enabled = true;

                newVal = GUILayout.Toggle(_settings.ShowDebug, "Debug mode");
                if (_settings.ShowDebug != newVal)
                {
                    _settings.ShowDebug = newVal;
                    settingsManager.BuildSettingList();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.Label("Search settings: ", GUILayout.ExpandWidth(false));

                GUI.SetNextControlName(SearchBoxName);
                settingsManager.SearchString = GUILayout.TextField(settingsManager.SearchString, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                    settingsManager.SearchString = string.Empty;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                if (_settings.ShowDebug && GUILayout.Button("Open Unity Log", GUILayout.ExpandWidth(false)))
                {
                    try { Utils.OpenLog(); }
                    catch (SystemException ex)
                    {
                        ConfigurationManager.Logger.Log(LogLevel.Message | LogLevel.Error, ex.Message);
                    }
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(_settings.PluginConfigCollapsedDefault.Value ? "Expand All" : "Collapse All", GUILayout.ExpandWidth(false)))
                {
                    var newValue = !_settings.PluginConfigCollapsedDefault.Value;
                    _settings.PluginConfigCollapsedDefault.Value = newValue;
                    foreach (var plugin in settingsManager.FilteredSettings)
                        plugin.Collapsed = newValue;
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSinglePlugin(PluginSettingsData plugin, string searchString)
        {
            GUILayout.BeginVertical(GUI.skin.box);

            var categoryHeader = _settings.ShowDebug ?
                new GUIContent($"{plugin.Info.Name.TrimStart('!')} {plugin.Info.Version}", "GUID: " + plugin.Info.GUID) :
                new GUIContent($"{plugin.Info.Name.TrimStart('!')} {plugin.Info.Version}");

            var isSearching = !string.IsNullOrEmpty(searchString);

            {
                var hasWebsite = plugin.Website != null;
                if (hasWebsite)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(29); // Same as the URL button to keep the plugin name centered
                }

                if (SettingFieldDrawer.DrawPluginHeader(categoryHeader, plugin.Collapsed && !isSearching) && !isSearching)
                    plugin.Collapsed = !plugin.Collapsed;

                if (hasWebsite)
                {
                    var origColor = GUI.color;
                    GUI.color = Color.gray;
                    if (GUILayout.Button(new GUIContent("URL", plugin.Website), GUI.skin.label, GUILayout.ExpandWidth(false)))
                        Utils.OpenWebsite(plugin.Website);
                    GUI.color = origColor;
                    GUILayout.EndHorizontal();
                }
            }

            if (isSearching || !plugin.Collapsed)
            {
                foreach (var category in plugin.Categories)
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    if (!string.IsNullOrEmpty(category.Name))
                    {
                        if (plugin.Categories.Count > 1 || !_settings.HideSingleSection.Value)
                            SettingFieldDrawer.DrawCategoryHeader(category.Name);
                    }

                    foreach (var setting in category.Settings)
                    {
                        DrawSingleSetting(setting);
                        GUILayout.Space(2);
                    }
                    GUILayout.EndVertical();
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawSingleSetting(SettingEntryBase setting)
        {
            GUILayout.BeginHorizontal();
            {
                try
                {
                    DrawSettingName(setting);
                    _fieldDrawer.DrawSettingValue(setting);
                    DrawDefaultButton(setting);
                }
                catch (Exception ex)
                {
                    ConfigurationManager.Logger.Log(LogLevel.Error, $"Failed to draw setting {setting.DispName} - {ex}");
                    GUILayout.Label("Failed to draw this field, check log for details.");
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSettingName(SettingEntryBase setting)
        {
            if (setting.HideSettingName) return;

            var origColor = GUI.color;
            if (setting.IsAdvanced == true)
                GUI.color = AdvancedSettingColor;

            GUILayout.Label(new GUIContent(setting.DispName.TrimStart('!'), setting.Description),
                GUILayout.Width(LeftColumnWidth), GUILayout.MaxWidth(LeftColumnWidth));

            GUI.color = origColor;
        }

        private static void DrawDefaultButton(SettingEntryBase setting)
        {
            if (setting.HideDefaultButton) return;

            bool DefaultButton()
            {
                GUILayout.Space(5);
                return GUILayout.Button("Reset", GUILayout.ExpandWidth(false));
            }

            if (setting.DefaultValue != null)
            {
                if (DefaultButton())
                    setting.Set(setting.DefaultValue);
            }
            else if (setting.SettingType.IsClass)
            {
                if (DefaultButton())
                    setting.Set(null);
            }
        }

        private static void DrawTooltip(Rect area)
        {
            if (!string.IsNullOrEmpty(GUI.tooltip))
            {
                var currentEvent = Event.current;

                var style = new GUIStyle
                {
                    normal = new GUIStyleState { textColor = Color.white, background = TooltipBg },
                    wordWrap = true,
                    alignment = TextAnchor.MiddleCenter
                };

                const int width = 400;
                var height = style.CalcHeight(new GUIContent(GUI.tooltip), 400) + 10;

                var x = currentEvent.mousePosition.x + width > area.width
                    ? area.width - width
                    : currentEvent.mousePosition.x;

                var y = currentEvent.mousePosition.y + 25 + height > area.height
                    ? currentEvent.mousePosition.y - height
                    : currentEvent.mousePosition.y + 25;

                GUI.Box(new Rect(x, y, width, height), GUI.tooltip, style);
            }
        }
    }
}
