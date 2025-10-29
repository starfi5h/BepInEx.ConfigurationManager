using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using ConfigurationManager.Utilities;

namespace ConfigurationManager
{
    /// <summary>
    /// Manages the collection and filtering of plugin settings
    /// </summary>
    internal class PluginSettingsDataManager
    {
        private readonly ConfigManagerSettings _settings;

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

        public PluginSettingsDataManager(ConfigManagerSettings settings)
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

            BuildFilteredSettingList();
        }

        /// <summary>
        /// Apply current filters to the setting list
        /// </summary>
        public void BuildFilteredSettingList()
        {
            IEnumerable<SettingEntryBase> results = _allSettings;

            var searchStrings = SearchString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (searchStrings.Length > 0)
            {
                results = results.Where(x => ContainsSearchString(x, searchStrings));
            }
            else
            {
                if (!_settings.ShowAdvanced.Value)
                    results = results.Where(x => x.IsAdvanced != true);
                if (!_settings.ShowKeybinds.Value)
                    results = results.Where(x => !IsKeyboardShortcut(x));
                if (!_settings.ShowSettings.Value)
                    results = results.Where(x => x.IsAdvanced == true || IsKeyboardShortcut(x));
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
                    var originalCategoryOrder = pluginSettings.Select(x => x.Category).Distinct().ToList();

                    var categories = pluginSettings
                        .GroupBy(x => x.Category)
                        .OrderBy(x => originalCategoryOrder.IndexOf(x.Key))
                        .ThenBy(x => x.Key)
                        .Select(x => new PluginSettingsData.PluginSettingsGroupData
                        {
                            Name = x.Key,
                            Settings = x.OrderByDescending(set => set.Order).ThenBy(set => set.DispName).ToList()
                        });

                    var website = Utils.GetWebsite(pluginSettings.First().PluginInstance);

                    return new PluginSettingsData
                    {
                        Info = pluginSettings.Key,
                        Categories = categories.ToList(),
                        Collapsed = nonDefaultCollapsingStateByPluginName.Contains(pluginSettings.Key.Name)
                            ? !settingsAreCollapsed
                            : settingsAreCollapsed,
                        Website = website
                    };
                })
                .OrderBy(x => x.Info.Name)
                .ToList();
        }

        private static bool IsKeyboardShortcut(SettingEntryBase x)
        {
            return x.SettingType == typeof(KeyboardShortcut);
        }

        private static bool ContainsSearchString(SettingEntryBase setting, string[] searchStrings)
        {
            var combinedSearchTarget = setting.PluginInfo.Name + "\n" +
                                       setting.PluginInfo.GUID + "\n" +
                                       setting.DispName + "\n" +
                                       setting.Category + "\n" +
                                       setting.Description + "\n" +
                                       setting.DefaultValue + "\n" +
                                       setting.Get();

            return searchStrings.All(s => combinedSearchTarget.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }
    }
}