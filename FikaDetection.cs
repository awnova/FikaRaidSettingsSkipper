using System;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;

namespace RaidSettingsSkipper
{
    // Fika is optional, so it is probed reflectively. CanEditRaidSettings moved from a field on
    // FikaPlugin (<=2.2.3) to a property on FikaPlugin.Settings (2.2.4+).
    internal static class FikaDetection
    {
        private const string FikaGuid = "com.fika.core";

        private static bool _resolved;
        private static object _plugin;
        private static PropertyInfo _settings;
        private static MemberInfo _canEdit;

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
                    object target = _settings != null ? _settings.GetValue(_plugin, null) : _plugin;

                    return _canEdit is FieldInfo fieldInfo
                        ? (bool)fieldInfo.GetValue(target)
                        : (bool)((PropertyInfo)_canEdit).GetValue(target, null);
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
            Type plugin = _plugin.GetType();

            _canEdit = Find(plugin, "CanEditRaidSettings");
            if (_canEdit != null)
            {
                return;
            }

            _settings = plugin.GetProperty("Settings", BindingFlags.Public | BindingFlags.Instance);
            _canEdit = _settings == null ? null : Find(_settings.PropertyType, "CanEditRaidSettings");

            if (_canEdit == null)
            {
                Plugin.LOG.LogWarning("Fika's CanEditRaidSettings moved; the raid settings screen is left as-is.");
            }
        }

        private static MemberInfo Find(Type type, string name)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            return (MemberInfo)type.GetField(name, flags) ?? type.GetProperty(name, flags);
        }
    }
}
