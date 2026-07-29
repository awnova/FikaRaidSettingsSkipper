using System;
using System.Linq;
using System.Reflection;

namespace FikaRaidSettingsSkipper
{
    // CanEditRaidSettings moved from a field on FikaPlugin (<=2.2.3) to a property on FikaPlugin.Settings (2.2.4+).
    internal static class FikaRaidSettings
    {
        private static bool _resolved;
        private static PropertyInfo _instance;
        private static PropertyInfo _settings;
        private static MemberInfo _canEdit;

        internal static bool CanEdit
        {
            get
            {
                if (!_resolved)
                {
                    Resolve();
                    _resolved = true;
                }

                if (_canEdit == null)
                {
                    return true;
                }

                try
                {
                    object target = _instance.GetValue(null, null);
                    if (_settings != null)
                    {
                        target = _settings.GetValue(target, null);
                    }

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

        private static void Resolve()
        {
            Type plugin = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType("Fika.Core.FikaPlugin", false))
                .FirstOrDefault(type => type != null);

            _instance = plugin?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (_instance == null)
            {
                Plugin.LOG.LogWarning("Fika not found; the raid settings screen is left as-is.");
                return;
            }

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
