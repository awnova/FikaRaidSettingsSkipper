using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPT.SinglePlayer.Patches.ScavMode;

namespace RaidSettingsSkipper.Patches
{
    // Scav raids never reach method_50: SPT's LoadOfflineRaidScreenPatch rewires CG_method_77 to build the screen
    // itself in LoadOfflineRaidScreenForScav. Skipping that goes straight to SPT's own Next handler.
    internal sealed class SkipScavRaidSettingsScreenPatch : ModulePatch
    {
        private static Func<MainMenuShowOperation> _getMenuController;
        private static Action _showNextScreen;

        protected override MethodBase GetTargetMethod()
        {
            Type patch = typeof(LoadOfflineRaidScreenPatch);
            MethodInfo getMenuController = AccessTools.Method(patch, "GetMenuController");
            MethodInfo showNextScreen = AccessTools.Method(patch, "LoadOfflineRaidNextScreen");
            if (getMenuController == null || showNextScreen == null)
            {
                return null;
            }

            _getMenuController = AccessTools.MethodDelegate<Func<MainMenuShowOperation>>(getMenuController);
            _showNextScreen = AccessTools.MethodDelegate<Action>(showNextScreen);

            return AccessTools.Method(patch, "LoadOfflineRaidScreenForScav");
        }

        [PatchPrefix]
        private static bool Prefix()
        {
            if (!Plugin.ShouldSkip)
            {
                return true;
            }

            MainMenuShowOperation menuController = _getMenuController();
            if (menuController == null)
            {
                return true;
            }

            RaidSettingsScreenEffects.OnScreenShown(menuController.raidSettings_1);
            _showNextScreen();
            RaidSettingsScreenEffects.OnScreenClosed(menuController.raidSettings_0, menuController.raidSettings_1);

            return false;
        }
    }
}
