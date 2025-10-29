using System;
using BepInEx;
using BepInEx.Configuration;
using ConfigurationManager.Utilities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using UnityEngine;

namespace ConfigurationManager
{
    internal static class SettingSearcher
    {
        private static readonly ICollection<string> _updateMethodNames = new[]
        {
            "Update",
            "FixedUpdate",
            "LateUpdate",
            "OnGUI"
        };

        public static void CollectSettings(out IEnumerable<SettingEntryBase> results, out List<string> modsWithoutSettings, bool showDebug)
        {
            modsWithoutSettings = new List<string>();

            try
            {
                results = GetBepInExCoreConfig();
            }
            catch (Exception ex)
            {
                results = Enumerable.Empty<SettingEntryBase>();
                ConfigurationManager.Logger.LogError(ex);
            }

            foreach (var plugin in Utils.FindPlugins())
            {
                if (plugin == null) continue;
                try
                {
                    var type = plugin.GetType();

                    var pluginInfo = plugin.Info.Metadata;
                    var pluginName = pluginInfo?.Name ?? plugin.GetType().FullName;

                    if (type.GetCustomAttributes(typeof(BrowsableAttribute), false).Cast<BrowsableAttribute>()
                            .Any(x => !x.Browsable))
                    {
                        modsWithoutSettings.Add(pluginName);
                        continue;
                    }

                    var detected = new List<SettingEntryBase>();

                    detected.AddRange(GetPluginConfig(plugin).Cast<SettingEntryBase>());

                    detected.RemoveAll(x => x.Browsable == false);

                    if (detected.Count == 0)
                        modsWithoutSettings.Add(pluginName);

                    // Allow to enable/disable plugin if it uses any update methods ------
                    if (showDebug && type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Any(x => _updateMethodNames.Contains(x.Name)))
                    {
                        var enabledSetting = new PropertySettingEntry(plugin, type.GetProperty("enabled"), plugin);
                        enabledSetting.DispName = "!Allow plugin to run on every frame";
                        enabledSetting.Description = "Disabling this will disable some or all of the plugin's functionality.\nHooks and event-based functionality will not be disabled.\nThis setting will be lost after game restart.";
                        enabledSetting.IsAdvanced = true;
                        detected.Add(enabledSetting);
                    }

                    if (detected.Count > 0)
                        results = results.Concat(detected);
                }
                catch (Exception ex)
                {
                    string pluginName = plugin?.Info?.Metadata?.Name ?? plugin?.GetType().FullName;
                    ConfigurationManager.Logger.LogError($"Failed to collect settings of the following plugin: {pluginName}");
                    ConfigurationManager.Logger.LogError(ex);
                }
            }
        }

        /// <summary>
        /// Get entries for all core BepInEx settings
        /// </summary>
        private static IEnumerable<SettingEntryBase> GetBepInExCoreConfig()
        {
            var coreConfigProp = typeof(ConfigFile).GetProperty("CoreConfig", BindingFlags.Static | BindingFlags.NonPublic);
            if (coreConfigProp == null) throw new ArgumentNullException(nameof(coreConfigProp));

            var coreConfig = (ConfigFile)coreConfigProp.GetValue(null, null);
            var bepinMeta = new BepInPlugin("BepInEx", "BepInEx", typeof(BepInEx.Bootstrap.Chainloader).Assembly.GetName().Version.ToString());

            return coreConfig.Select(kvp => (SettingEntryBase)new ConfigSettingEntry(kvp.Value, null) { IsAdvanced = true, PluginInfo = bepinMeta });
        }

        /// <summary>
        /// Get entries for all settings of a plugin
        /// </summary>
        private static IEnumerable<ConfigSettingEntry> GetPluginConfig(BaseUnityPlugin plugin)
        {
            return plugin.Config.Select(kvp => new ConfigSettingEntry(kvp.Value, plugin));
        }
    }
}