using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace FikaRaidSettingsSkipper.Patches
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
            if (FikaRaidSettings.CanEdit)
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
