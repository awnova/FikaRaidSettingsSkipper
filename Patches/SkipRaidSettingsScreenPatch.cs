using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace FikaRaidSettingsSkipper.Patches
{
    /// <summary>
    /// Skips the pre-raid settings screen when the Fika server has raid settings disabled
    /// (<c>canEditRaidSettings = false</c>), and leaves it alone when they are enabled.
    ///
    /// <para>
    /// <c>MainMenuControllerClass.method_50</c> is the only place the screen is ever constructed and
    /// queued; location select, map points and the pocket map all funnel through it. Advancing past
    /// the screen is just its own "Next" branch: insurance for online raids, match accept otherwise.
    /// </para>
    ///
    /// <para>
    /// Skipping it forward removes it backward too. Queued screens form a linked list - each one
    /// walks back to the current controller and stores it as its previous screen. A screen that
    /// never calls ShowScreen never joins that list, so Back on the following screen returns to
    /// location select instead of to a settings screen that was never shown.
    /// </para>
    /// </summary>
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

            if (__instance.RaidSettings_0.RaidMode == ERaidMode.Online)
            {
                __instance.method_51();
            }
            else
            {
                __instance.method_52();
            }

            return false;
        }
    }
}
