using System;
using EFT;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Custom.Models;
using SPT.Custom.Patches;

namespace RaidSettingsSkipper
{
    // A skipped MatchmakerOfflineRaidScreen never runs its own Show and Close, nor the patches SPT and Fika
    // hang off them, so the parts that touch raid settings or profiles are replayed here.
    internal static class RaidSettingsScreenEffects
    {
        // Mirrors SPT's SetPreRaidSettingsScreenDefaultsPatch and CopyPmcQuestsAndWishlistToPlayerScavPatch (Show).
        internal static void OnScreenShown(RaidSettings offlineRaidSettings)
        {
            // SPT turns the offline toggle off, and Show's CheckCoopGameAvailable then sets SetRaidMode(false).
            offlineRaidSettings.RaidMode = ERaidMode.Online;

            try
            {
                DefaultRaidSettings defaults = Json.Deserialize<DefaultRaidSettings>(RequestHandler.GetJson("/singleplayer/settings/raid/menu"));
                if (defaults != null)
                {
                    offlineRaidSettings.BotSettings.BotAmount = defaults.AiAmount;
                    offlineRaidSettings.WavesSettings.BotAmount = defaults.AiAmount;
                    offlineRaidSettings.WavesSettings.BotDifficulty = defaults.AiDifficulty;
                    offlineRaidSettings.WavesSettings.IsBosses = defaults.BossEnabled;
                    offlineRaidSettings.BotSettings.IsScavWars = false;
                    offlineRaidSettings.WavesSettings.IsTaggedAndCursed = defaults.TaggedAndCursed;
                    offlineRaidSettings.TimeAndWeatherSettings.IsRandomWeather = defaults.RandomWeather;
                    offlineRaidSettings.TimeAndWeatherSettings.IsRandomTime = defaults.RandomTime;
                }
            }
            catch (Exception ex)
            {
                Plugin.LOG.LogWarning($"Could not load the server's raid settings defaults: {ex.Message}");
            }

            try
            {
                CopyPmcQuestsAndWishlistToPlayerScavPatch.PatchPostfix();
            }
            catch (Exception ex)
            {
                Plugin.LOG.LogWarning($"Could not copy PMC quests and wishlist to the scav profile: {ex.Message}");
            }
        }

        // Mirrors MatchmakerOfflineRaidScreen.Close and Fika's MatchmakerOfflineRaidScreen_Close_Patch, the only way
        // the offline settings reach a Fika raid.
        internal static void OnScreenClosed(RaidSettings raidSettings, RaidSettings offlineRaidSettings)
        {
            raidSettings.RaidMode = offlineRaidSettings.RaidMode;

            if (!FikaDetection.IsLoaded)
            {
                return;
            }

            raidSettings.TimeAndWeatherSettings = offlineRaidSettings.TimeAndWeatherSettings;
            raidSettings.WavesSettings = offlineRaidSettings.WavesSettings;
            raidSettings.MetabolismDisabled = offlineRaidSettings.MetabolismDisabled;
            raidSettings.PlayersSpawnPlace = offlineRaidSettings.PlayersSpawnPlace;
        }
    }
}
