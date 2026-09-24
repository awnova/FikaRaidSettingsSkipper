using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using RaidSettingsSkipper.Patches;
using SPT.Reflection.Patching;

namespace RaidSettingsSkipper
{
    [BepInPlugin("com.awnova.raidsettingsskipper", "RaidSettingsSkipper", "1.3.0")]
    [BepInProcess("EscapeFromTarkov.exe")]
    // Fika.Headless drives this menu itself and already prefixes method_50.
    [BepInIncompatibility("com.fika.headless")]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource LOG;
        internal static ConfigEntry<bool> SkipRaidSettings;

        // Fika is only consulted when the user opted out: a server that disables raid settings
        // leaves the screen empty, so it is skipped either way.
        internal static bool ShouldSkip => SkipRaidSettings.Value || !FikaDetection.CanEditRaidSettings;

        private void Awake()
        {
            LOG = Logger;
            SkipRaidSettings = Config.Bind(
                "General",
                "Skip raid settings screen",
                true,
                "Skip the pre-raid settings screen. When off, the screen is still skipped if a Fika server has raid settings disabled."
            );

            EnablePatchSafely(new SkipRaidSettingsScreenPatch(), nameof(SkipRaidSettingsScreenPatch));
            EnablePatchSafely(new SkipScavRaidSettingsScreenPatch(), nameof(SkipScavRaidSettingsScreenPatch));
        }

        private static void EnablePatchSafely(ModulePatch patch, string name)
        {
            try
            {
                patch.Enable();
            }
            catch (Exception ex)
            {
                LOG.LogWarning($"Failed to enable {name}; the game version is likely newer or older than this build. {ex}");
            }
        }
    }
}
