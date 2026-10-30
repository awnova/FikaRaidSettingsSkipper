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
            // The prefix binds to these directly; checking them here turns a mismatched game build into an
            // Enable failure instead of a MissingMethodException when the player clicks Next.
            if (AccessTools.Method(typeof(MainMenuShowOperation), nameof(MainMenuShowOperation.CG_method_80)) == null
                || AccessTools.Field(typeof(MainMenuShowOperation), nameof(MainMenuShowOperation.raidSettings_0)) == null
                || AccessTools.Field(typeof(MainMenuShowOperation), nameof(MainMenuShowOperation.raidSettings_1)) == null)
            {
                return null;
            }

            return AccessTools.Method(
                typeof(MainMenuShowOperation),
                nameof(MainMenuShowOperation.method_50)
            );
        }

        [PatchPrefix]
        private static bool Prefix(MainMenuShowOperation __instance)
        {
            if (!Plugin.ShouldSkip)
            {
                return true;
            }

            RaidSettingsScreenEffects.OnScreenShown(__instance.raidSettings_1);

            __instance.CG_method_80();

            // Vanilla closes the screen after the Next handler ran, so Labs' forced bosses are carried over too.
            RaidSettingsScreenEffects.OnScreenClosed(__instance.raidSettings_0, __instance.raidSettings_1);

            return false;
        }
    }
}
