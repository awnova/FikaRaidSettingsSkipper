using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using RaidSettingsSkipper.Patches;
using SPT.Reflection.Patching;

namespace RaidSettingsSkipper
{
    [BepInPlugin("com.awnova.raidsettingsskipper", "RaidSettingsSkipper", "1.2.0")]
    [BepInProcess("EscapeFromTarkov.exe")]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource LOG;
        internal static ConfigEntry<bool> SkipRaidSettings;

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
