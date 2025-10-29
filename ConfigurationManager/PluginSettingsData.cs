// Made by MarC0 / ManlyMarco
// Copyright 2018 GNU General Public License v3.0

using System.Collections.Generic;
using BepInEx;

namespace ConfigurationManager
{
    /// <summary>
    /// Represents a plugin's settings data with categories and state
    /// </summary>
    internal sealed class PluginSettingsData
    {
        /// <summary>
        /// Plugin information
        /// </summary>
        public BepInPlugin Info { get; set; }

        /// <summary>
        /// List of setting categories for this plugin
        /// </summary>
        public List<PluginSettingsGroupData> Categories { get; set; }

        /// <summary>
        /// Cached height for rendering optimization
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Plugin website URL if available
        /// </summary>
        public string Website { get; set; }

        private bool _collapsed;

        /// <summary>
        /// Whether the plugin section is collapsed
        /// </summary>
        public bool Collapsed
        {
            get => _collapsed;
            set
            {
                _collapsed = value;
                Height = 0; // Reset height when collapse state changes
            }
        }

        /// <summary>
        /// Represents a group of settings within a category
        /// </summary>
        public sealed class PluginSettingsGroupData
        {
            /// <summary>
            /// Category name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// List of settings in this category
            /// </summary>
            public List<SettingEntryBase> Settings { get; set; }
        }
    }
}