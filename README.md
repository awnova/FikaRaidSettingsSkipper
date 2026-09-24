# RaidSettingsSkipper

A client-side BepInEx mod for SPT that skips the pre-raid settings screen — the step between
location select and the raid — for both PMC and Scav raids. Fika is supported but not required.

## What it does

The screen is skipped whenever either of these is true:

- **The config checkbox is ticked** (the default). PMCs go straight from location select to insurance
  (online raids) or match accept; Scavs go straight to match accept.
- **Fika is installed and the server sets `canEditRaidSettings = false`.** Fika blocks the settings
  window and only shows a "raid settings are disabled" notification, so the screen isn't needed.

Untick the checkbox on a plain SPT install, or on a Fika server that allows raid settings, and the
screen behaves exactly as vanilla.

Skipped raids still use the server's raid settings. The defaults the screen would have loaded from
the SPT server (AI amount and difficulty, bosses, tagged & cursed, random weather/time) are loaded
anyway, and on Fika they are handed to the raid the same way closing the screen would.

Because the screen is never built and never enters the screen queue, it is also gone in the back
direction: pressing Back on the following screen returns you to location/map select.

## Configuration

`BepInEx\config\com.awnova.raidsettingsskipper.cfg`, or the F12 menu in game:

| Section | Setting | Default |
| --- | --- | --- |
| General | Skip raid settings screen | `true` |

The value is read each time the screen would be queued, so changes apply immediately — no restart.

## How it works

`MatchmakerOfflineRaidScreen` is built in two places:

- **PMC:** `MainMenuShowOperation.method_50` — location select, map points and the pocket map all
  funnel through it. A Harmony prefix takes the screen's own "Next" branch (`CG_method_80`) and skips
  the body: `ERaidMode.Online` continues to the insurance screen, anything else goes straight to match
  accept.
- **Scav:** SPT's `LoadOfflineRaidScreenPatch` rewires the location screen to build the screen itself
  in `LoadOfflineRaidScreenForScav`. A second prefix skips that and calls SPT's own Next handler.

A skipped screen never runs the patches SPT and Fika attach to it, so the mod replays the ones that
matter: SPT's `SetPreRaidSettingsScreenDefaultsPatch` (server raid-menu defaults) and
`CopyPmcQuestsAndWishlistToPlayerScavPatch` when the screen would show, and Fika's
`MatchmakerOfflineRaidScreen_Close_Patch` (time/weather, waves, metabolism, spawn place) when it would
close.

Skipping it forward removes it backward too, for free. Queued screens form a linked list — each one
walks back to the current screen controller and stores it as its previous screen. A screen that
never calls `ShowScreen` never joins that list, so Back on the following screen returns to location
select rather than to a settings screen you never saw.

Fika is never referenced at compile time. `CanEditRaidSettings` is looked up reflectively both
because Fika is optional and because the flag moved between versions — a field on `FikaPlugin` up to
2.2.3, a property on `FikaPlugin.Settings` from 2.2.4 on. If Fika is absent or neither is found, the
mod reports "can edit" and the config entry alone decides. Fika is found through BepInEx's plugin
list (`com.fika.core`), not by scanning assemblies.

The mod does not load alongside Fika.Headless, which drives this menu itself and patches the same
method.

There is no simulated button click and no coroutine waiting on UI layout.

## Requirements

- **SPT 4.1** (checked against 4.1.2 and 4.1.6). Fika optional (checked against 2.4.2).
- .NET Framework 4.8 developer tools (for building).

SPT 4.1 deobfuscated the client, renaming the class this mod patches. Versions 1.2.0 and later target 4.1 only;
use 1.1.0 for SPT 4.0.x.

## Upgrading to 1.3.0

Drop-in replacement for 1.2.0; your config is kept. New: Scav raids are skipped too, and skipped raids
now pick up the server's raid-menu defaults (and hand them to Fika raids).

## Upgrading to 1.2.0

1.2.0 is SPT 4.1 only. It will not load on 4.0.x — the type it patches does not exist there, and
BepInEx logs a patch error at startup. The DLL name and plugin GUID are unchanged since 1.1.0, so it
overwrites the old file cleanly and your config is kept.

## Upgrading from 1.0.x

Delete `BepInEx\plugins\FikaRaidSettingsSkipper.dll` before installing. The DLL name and the plugin
GUID both changed in 1.1.0, so the old file is not overwritten and both copies would load and patch.

## Build

1. Set `SPTBaseDir` in `RaidSettingsSkipper.csproj` to your SPT root folder (default `C:\SPT`),
   or pass it on the command line: `dotnet build -c Release -p:SPTBaseDir=C:\SPT`.
2. Build `Release`.

The post-build step copies the DLL to `<ProjectRoot>\Build\BepInEx\plugins\RaidSettingsSkipper.dll`.
Drop that into your SPT `BepInEx\plugins` folder.

## Credits

Approach inspired by [no-insurance](https://gitlab.com/vibrantrida/no-insurance), which showed me that
these menu steps are best removed at the `MainMenuShowOperation` transition rather than papered
over in the UI like my first attempt.
