using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace RaidSettingsSkipper.Patches
{
    // Skipping method_50 means Back never lands here either, since a screen only joins the back chain via ShowScreen.
    internal sealed class SkipRaidSettingsScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(MainMenuControllerClass),
                nameof(MainMenuControllerClass.method_50)
            );
        }

        [PatchPrefix]
        private static bool Prefix(MainMenuControllerClass __instance)
        {
            // Fika is only consulted when the user opted out: a server that disables raid settings
            // leaves the screen empty, so it is skipped either way.
            if (!Plugin.SkipRaidSettings.Value && FikaDetection.CanEditRaidSettings)
            {
                return true;
            }

            // Other patches on method_80 may rewrite RaidMode to pick a branch; the raid must still start in the mode it was queued in.
            ERaidMode raidMode = __instance.RaidSettings_0.RaidMode;
            __instance.method_80();
            __instance.RaidSettings_0.RaidMode = raidMode;

            return false;
        }
    }
}
