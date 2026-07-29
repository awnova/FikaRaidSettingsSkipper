using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using RaidSettingsSkipper.Patches;

namespace RaidSettingsSkipper
{
    [BepInPlugin("com.awnova.raidsettingsskipper", "RaidSettingsSkipper", "1.1.0")]
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

            new SkipRaidSettingsScreenPatch().Enable();
        }
    }
}
