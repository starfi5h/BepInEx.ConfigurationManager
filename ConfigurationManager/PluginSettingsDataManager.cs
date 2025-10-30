using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BepInEx.Configuration;
using UnityEngine;

namespace ConfigurationManager
{
    /// <summary>
    /// Manages the collection and filtering of plugin settings
    /// </summary>
    internal class PluginSettingsDataManager
    {
        private readonly ManagerSettings _settings;

        private string _modsWithoutSettings;
        private List<SettingEntryBase> _allSettings;
        private List<PluginSettingsData> _filteredSettings = new List<PluginSettingsData>();
        private string _searchString = string.Empty;

        public IReadOnlyList<PluginSettingsData> FilteredSettings => _filteredSettings;
        public string ModsWithoutSettings => _modsWithoutSettings;

        /// <summary>
        /// String currently entered into the search box
        /// </summary>
        public string SearchString
        {
            get => _searchString;
            set
            {
                if (value == null)
                    value = string.Empty;

                if (_searchString == value)
                    return;

                _searchString = value;
                BuildFilteredSettingList();
            }
        }

        public PluginSettingsDataManager(ManagerSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Rebuild the complete setting list from all plugins
        /// </summary>
        public void BuildSettingList()
        {
            SettingSearcher.CollectSettings(out var results, out var modsWithoutSettings, _settings.ShowDebug);

            _modsWithoutSettings = string.Join(", ", modsWithoutSettings.Select(x => x.TrimStart('!')).OrderBy(x => x).ToArray());
            _allSettings = results.ToList();

            _ = BuildFilteredSettingListAsync();
        }

        public async Task BuildFilteredSettingListAsync()
        {
            await Task.Run(() =>
            {
                BuildFilteredSettingList();
            });
        }

        /// <summary>
        /// Apply current filters to the setting list
        /// </summary>
        public void BuildFilteredSettingList()
        {
            IEnumerable<SettingEntryBase> results = _allSettings;

            var searchStrings = SearchString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (!_settings.ShowAdvanced.Value)
                results = results.Where(x => x.IsAdvanced != true);
            if (!_settings.ShowKeybinds.Value)
                results = results.Where(x => !IsKeyboardShortcut(x));
            if (!_settings.ShowSettings.Value)
                results = results.Where(x => x.IsAdvanced == true || IsKeyboardShortcut(x));

            if (searchStrings.Length > 0)
            {
                results = results.Where(x => ContainsSearchString(x, searchStrings));
            }

            var settingsAreCollapsed = _settings.PluginConfigCollapsedDefault.Value;

            var nonDefaultCollapsingStateByPluginName = new HashSet<string>();
            foreach (var pluginSetting in _filteredSettings)
            {
                if (pluginSetting.Collapsed != settingsAreCollapsed)
                {
                    nonDefaultCollapsingStateByPluginName.Add(pluginSetting.Info.Name);
                }
            }

            _filteredSettings = results
                .GroupBy(x => x.PluginInfo)
                .Select(pluginSettings =>
                {
                    var categoryOrder = pluginSettings
                        .Select(x => x.Category)
                        .Distinct()
                        .Select((category, index) => new { category, index })
                        .ToDictionary(pair => pair.category, pair => pair.index);

                    var categories = pluginSettings
                        .GroupBy(x => x.Category)
                        .OrderBy(x => categoryOrder.TryGetValue(x.Key, out var index) ? index : int.MaxValue)
                        .ThenBy(x => x.Key)
                        .Select(x => new PluginSettingsData.PluginSettingsGroupData
                        {
                            Name = x.Key,
                            Settings = x.OrderByDescending(set => set.Order).ThenBy(set => set.DispName).ToList()
                        });

                    return new PluginSettingsData
                    {
                        Info = pluginSettings.Key,
                        Categories = categories.ToList(),
                        Collapsed = nonDefaultCollapsingStateByPluginName.Contains(pluginSettings.Key.Name)
                            ? !settingsAreCollapsed
                            : settingsAreCollapsed
                    };
                })
                .OrderBy(x => x.Info.Name)
                .ToList();
        }

        private static bool IsKeyboardShortcut(SettingEntryBase x)
        {
            return x.SettingType == typeof(KeyboardShortcut) || x.SettingType == typeof(KeyCode);
        }

        private static bool ContainsSearchString(SettingEntryBase setting, string[] searchStrings)
        {
            //  helper function to avoid repeating the check
            bool Check(string target, string s) =>
                target != null && target.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0;

            foreach (var s in searchStrings)
            {
                bool found = Check(setting.PluginInfo.Name, s) ||
                             Check(setting.PluginInfo.GUID, s) ||
                             Check(setting.DispName, s) ||
                             Check(setting.Category, s) ||
                             Check(setting.Description, s) ||
                             Check(setting.DefaultValue?.ToString(), s) ||
                             Check(setting.Get()?.ToString(), s);

                if (!found)
                    return false;
            }
            return true;
        }
    }
}