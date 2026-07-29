using BepInEx;
using BepInEx.Logging;
using FikaRaidSettingsSkipper.Patches;

namespace FikaRaidSettingsSkipper
{
    [BepInPlugin("com.awnova.fikaraidsettingsskipper", "FikaRaidSettingsSkipper", "1.0.0")]
    [BepInProcess("EscapeFromTarkov.exe")]
    [BepInDependency("com.fika.core")]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource LOG;

        private void Awake()
        {
            LOG = Logger;
            new SkipRaidSettingsScreenPatch().Enable();
        }
    }
}
