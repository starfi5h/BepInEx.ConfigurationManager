// Made by MarC0 / ManlyMarco
// Copyright 2018 GNU General Public License v3.0

using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using ConfigurationManager.Utilities;
using UnityEngine;

namespace ConfigurationManager.UI
{
    /// <summary>
    /// Manages the configuration window rendering and layout
    /// </summary>
    internal partial class ConfigWindow
    {
        private int _tipsHeight;
        private Vector2 _settingWindowScrollPos;

        private void DrawScrollItems()
        {
            _settingWindowScrollPos = GUILayout.BeginScrollView(_settingWindowScrollPos, false, true);

            var scrollPosition = _settingWindowScrollPos.y;
            var scrollHeight = SettingWindowRect.height;

            GUILayout.BeginVertical();
            {
                if (string.IsNullOrEmpty(_dataManager.SearchString))
                {
                    DrawTips();

                    if (_tipsHeight == 0 && Event.current.type == EventType.Repaint)
                        _tipsHeight = (int)GUILayoutUtility.GetLastRect().height;
                }

                var currentHeight = _tipsHeight;

                foreach (var plugin in _dataManager.FilteredSettings)
                {
                    var visible = plugin.Height == 0 || currentHeight + plugin.Height >= scrollPosition && currentHeight <= scrollPosition + scrollHeight;

                    if (visible)
                    {
                        try
                        {
                            DrawSinglePlugin(plugin, _dataManager.SearchString);
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
                    GUILayout.Label("Plugins with no options available: ".Translate() + _dataManager.ModsWithoutSettings);
                }
                else
                {
                    // Always leave some space in case there's a dropdown box at the very bottom of the list
                    GUILayout.Space(70);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawTips()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Tip: Click plugin names to expand. Click setting names to see their descriptions.".Translate());
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
                GUILayout.BeginHorizontal();
                var origColor = GUI.color;
                GUI.color = Color.gray;
                if (GUILayout.Button(new GUIContent("Reload".Translate() + "      ", "Trigger Config.Reload() event to apply settings from the config file".Translate()), GUI.skin.label, GUILayout.ExpandWidth(false)))
                    Utils.TryReloadConfig(plugin.Info.GUID);
                GUI.color = origColor;

                if (SettingFieldDrawer.DrawPluginHeader(categoryHeader, plugin.Collapsed && !isSearching) && !isSearching)
                    plugin.Collapsed = !plugin.Collapsed;


                GUI.color = Color.gray;
                if (GUILayout.Button(new GUIContent("Open File".Translate(), "Open the plugin config file".Translate()), GUI.skin.label, GUILayout.ExpandWidth(false)))
                    Utils.TryOpen(Path.Combine(Paths.ConfigPath, plugin.Info.GUID + ".cfg"));
                GUI.color = origColor;
                GUILayout.EndHorizontal();
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
                    GUILayout.Label("Failed to draw this field, check log for details.".Translate());
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSettingName(SettingEntryBase setting)
        {
            if (setting.HideSettingName) return;

            var origColor = GUI.color;
            if (setting.IsAdvanced)
                GUI.color = _settings.AdvancedSettingColor.Value;

            GUILayout.Label(new GUIContent(setting.DispName.TrimStart('!'), setting.Description),
                GUILayout.Width(LeftColumnWidth - 20), GUILayout.MaxWidth(LeftColumnWidth));

            GUI.color = origColor;
        }

        private static void DrawDefaultButton(SettingEntryBase setting)
        {
            if (setting.HideDefaultButton) return;

            bool DefaultButton()
            {
                GUILayout.Space(5);
                return GUILayout.Button("Reset".Translate(), GUILayout.ExpandWidth(false));
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
    }
}
