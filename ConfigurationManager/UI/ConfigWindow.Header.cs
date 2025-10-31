// Made by MarC0 / ManlyMarco
// Copyright 2018 GNU General Public License v3.0

using System;
using BepInEx.Logging;
using ConfigurationManager.Utilities;
using UnityEngine;

namespace ConfigurationManager.UI
{
    internal partial class ConfigWindow
    {
        private const string SearchBoxName = "searchBox";

        private void DrawWindowHeader()
        {
            // First line: Filter Options
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.Label("Show: ".Translate(), GUILayout.ExpandWidth(false));

                var newVal = GUILayout.Toggle(_settings.ShowSettings.Value, "Normal settings".Translate());
                if (_settings.ShowSettings.Value != newVal)
                {
                    _settings.ShowSettings.Value = newVal;
                    _ = _dataManager.BuildFilteredSettingListAsync();
                }

                newVal = GUILayout.Toggle(_settings.ShowKeybinds.Value, "Keyboard shortcuts".Translate());
                if (_settings.ShowKeybinds.Value != newVal)
                {
                    _settings.ShowKeybinds.Value = newVal;
                    _ = _dataManager.BuildFilteredSettingListAsync();
                }

                var origColor = GUI.color;
                GUI.color = _settings.AdvancedSettingColor.Value;
                newVal = GUILayout.Toggle(_settings.ShowAdvanced.Value, "Advanced settings".Translate());
                if (_settings.ShowAdvanced.Value != newVal)
                {
                    _settings.ShowAdvanced.Value = newVal;
                    _ = _dataManager.BuildFilteredSettingListAsync();
                }
                GUI.color = origColor;

                newVal = GUILayout.Toggle(_settings.ShowDebug, "Debug mode".Translate());
                if (_settings.ShowDebug != newVal)
                {
                    _settings.ShowDebug = newVal;
                    _dataManager.BuildSettingList();
                }
            }
            GUILayout.EndHorizontal();

            // Second line: Search
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.Label("Search settings: ".Translate(), GUILayout.ExpandWidth(false));

                GUI.SetNextControlName(SearchBoxName);
                _dataManager.SearchString = GUILayout.TextField(_dataManager.SearchString, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Clear".Translate(), GUILayout.ExpandWidth(false)))
                    _dataManager.SearchString = string.Empty;
            }
            GUILayout.EndHorizontal();

            // Third line: Others
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.Label("Language: ".Translate(), GUILayout.ExpandWidth(false));

                if (GUILayout.Button("EN / 中文", GUILayout.ExpandWidth(false)))
                {
                    LocalizationManager.ToggleLanguage();
                    _settings.Language.Value = LocalizationManager.CurrentLanguage;
                }

                if (GUILayout.Button("Config Folder".Translate(), GUILayout.ExpandWidth(false)))
                {
                    try { Utils.TryOpen(BepInEx.Paths.ConfigPath); }
                    catch (SystemException ex)
                    {
                        ConfigurationManager.Logger.Log(LogLevel.Message | LogLevel.Error, ex.Message);
                    }
                }

                if (_settings.ShowDebug && GUILayout.Button("Open Unity Log".Translate(), GUILayout.ExpandWidth(false)))
                {
                    try { Utils.OpenLog(); }
                    catch (SystemException ex)
                    {
                        ConfigurationManager.Logger.Log(LogLevel.Message | LogLevel.Error, ex.Message);
                    }
                }

                if (_settings.ShowDebug && GUILayout.Button("Open BepInEx Log".Translate(), GUILayout.ExpandWidth(false)))
                {
                    try { Utils.OpenBepInExLog(); }
                    catch (SystemException ex)
                    {
                        ConfigurationManager.Logger.Log(LogLevel.Message | LogLevel.Error, ex.Message);
                    }
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(_settings.PluginConfigCollapsedDefault.Value ? "Expand All".Translate() : "Collapse All".Translate(), GUILayout.ExpandWidth(false)))
                {
                    var newValue = !_settings.PluginConfigCollapsedDefault.Value;
                    _settings.PluginConfigCollapsedDefault.Value = newValue;
                    foreach (var plugin in _dataManager.FilteredSettings)
                        plugin.Collapsed = newValue;
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
