# RaidSettingsSkipper

A client-side BepInEx mod for SPT that skips the pre-raid settings screen — the step between
location select and the raid. Fika is supported but not required.

## What it does

The screen is skipped whenever either of these is true:

- **The config checkbox is ticked** (the default). You go straight from location select to insurance
  (online raids) or match accept.
- **Fika is installed and the server sets `canEditRaidSettings = false`.** Fika blocks the settings
  window and only shows a "raid settings are disabled" notification, so the screen has nothing on
  it. That is skipped even with the checkbox off — there is nothing to come back for.

Untick the checkbox on a plain SPT install, or on a Fika server that allows raid settings, and the
screen behaves exactly as vanilla.

Because the screen is never built and never enters the screen queue, it is also gone in the back
direction: pressing Back on the following screen returns you to location/map select.

## Configuration

`BepInEx\config\com.awnova.raidsettingsskipper.cfg`, or the F12 menu in game:

| Section | Setting | Default |
| --- | --- | --- |
| General | Skip raid settings screen | `true` |

The value is read each time the screen would be queued, so changes apply immediately — no restart.

## How it works

`MainMenuControllerClass.method_50` is the only place `MatchmakerOfflineRaidScreen` is ever
constructed and queued — location select, map points and the pocket map all funnel through it. A
single Harmony prefix takes the screen's own "Next" branch and skips the body: `ERaidMode.Online`
continues to the insurance screen, anything else goes straight to match accept.

Skipping it forward removes it backward too, for free. Queued screens form a linked list — each one
walks back to the current screen controller and stores it as its previous screen. A screen that
never calls `ShowScreen` never joins that list, so Back on the following screen returns to location
select rather than to a settings screen you never saw.

Fika is never referenced at compile time. `CanEditRaidSettings` is looked up reflectively both
because Fika is optional and because the flag moved between versions — a field on `FikaPlugin` up to
2.2.3, a property on `FikaPlugin.Settings` from 2.2.4 on. If Fika is absent or neither is found, the
mod reports "can edit" and the config entry alone decides. With the checkbox ticked the lookup never
runs at all.

There is no simulated button click and no coroutine waiting on UI layout.

## Requirements

- SPT client install. Fika optional.
- .NET Framework 4.8 developer tools (for building).

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
these menu steps are best removed at the `MainMenuControllerClass` transition rather than papered
over in the UI like my first attempt.
