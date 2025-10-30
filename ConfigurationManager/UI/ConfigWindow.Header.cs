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
                GUILayout.Label("Show: ", GUILayout.ExpandWidth(false));

                GUI.enabled = _dataManager.SearchString == string.Empty;

                var newVal = GUILayout.Toggle(_settings.ShowSettings.Value, "Normal settings");
                if (_settings.ShowSettings.Value != newVal)
                {
                    _settings.ShowSettings.Value = newVal;
                    _dataManager.BuildFilteredSettingList();
                }

                newVal = GUILayout.Toggle(_settings.ShowKeybinds.Value, "Keyboard shortcuts");
                if (_settings.ShowKeybinds.Value != newVal)
                {
                    _settings.ShowKeybinds.Value = newVal;
                    _dataManager.BuildFilteredSettingList();
                }

                var origColor = GUI.color;
                GUI.color = _settings.AdvancedSettingColor.Value;
                newVal = GUILayout.Toggle(_settings.ShowAdvanced.Value, "Advanced settings");
                if (_settings.ShowAdvanced.Value != newVal)
                {
                    _settings.ShowAdvanced.Value = newVal;
                    _dataManager.BuildFilteredSettingList();
                }
                GUI.color = origColor;

                GUI.enabled = true;

                newVal = GUILayout.Toggle(_settings.ShowDebug, "Debug mode");
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
                GUILayout.Label("Search settings: ", GUILayout.ExpandWidth(false));

                GUI.SetNextControlName(SearchBoxName);
                _dataManager.SearchString = GUILayout.TextField(_dataManager.SearchString, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                    _dataManager.SearchString = string.Empty;
            }
            GUILayout.EndHorizontal();

            // Third line: Others
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

                if (_settings.ShowDebug && GUILayout.Button("Open BepInEx Log", GUILayout.ExpandWidth(false)))
                {
                    try { Utils.OpenBepInExLog(); }
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
                    foreach (var plugin in _dataManager.FilteredSettings)
                        plugin.Collapsed = newValue;
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
