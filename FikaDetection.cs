using System;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;

namespace RaidSettingsSkipper
{
    // Fika is optional, so it is probed reflectively: CanEditRaidSettings is a property on FikaPlugin.Settings.
    internal static class FikaDetection
    {
        private const string FikaGuid = "com.fika.core";

        private static bool _resolved;
        private static object _plugin;
        private static PropertyInfo _settings;
        private static PropertyInfo _canEdit;

        internal static bool IsLoaded
        {
            get
            {
                EnsureResolved();
                return _plugin != null;
            }
        }

        internal static bool CanEditRaidSettings
        {
            get
            {
                EnsureResolved();

                if (_canEdit == null)
                {
                    return true;
                }

                try
                {
                    return (bool)_canEdit.GetValue(_settings.GetValue(_plugin, null), null);
                }
                catch (Exception ex)
                {
                    Plugin.LOG.LogWarning($"Could not read Fika's raid settings config: {ex.Message}");
                    return true;
                }
            }
        }

        private static void EnsureResolved()
        {
            if (_resolved)
            {
                return;
            }

            _resolved = true;
            try
            {
                Resolve();
            }
            catch (Exception ex)
            {
                _canEdit = null;
                Plugin.LOG.LogWarning($"Could not inspect Fika; the config entry alone decides. {ex.Message}");
            }
        }

        private static void Resolve()
        {
            if (!Chainloader.PluginInfos.TryGetValue(FikaGuid, out PluginInfo info) || info.Instance == null)
            {
                Plugin.LOG.LogInfo("Fika not found; only the config entry decides whether the raid settings screen is skipped.");
                return;
            }

            _plugin = info.Instance;

            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            _settings = _plugin.GetType().GetProperty("Settings", flags);
            _canEdit = _settings?.PropertyType.GetProperty("CanEditRaidSettings", flags);

            if (_canEdit == null || _canEdit.PropertyType != typeof(bool))
            {
                _canEdit = null;
                Plugin.LOG.LogWarning("Fika's CanEditRaidSettings moved; the config entry alone decides.");
            }
        }
    }
}
