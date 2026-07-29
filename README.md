# FikaRaidSettingsSkipper

A client-side BepInEx mod for SPT + Fika that skips the pre-raid settings screen when the Fika
server has raid settings turned off.

## What it does

The screen shown between location select and the raid follows Fika's server config:

- `canEditRaidSettings = false` — Fika blocks the settings window and only shows a "raid settings
  are disabled" notification, so the screen has nothing on it. This mod takes it out of the flow:
  you go straight to insurance (online raids) or match accept.
- `canEditRaidSettings = true` — the screen is shown exactly as normal. The mod does nothing.

Because the screen is never built and never enters the screen queue, it is also gone in the back
direction: pressing Back on the following screen returns you to location/map select.

## How it works

`MainMenuControllerClass.method_50` is the only place `MatchmakerOfflineRaidScreen` is ever
constructed and queued — location select, map points and the pocket map all funnel through it. A
single Harmony prefix takes the screen's own "Next" branch and skips the body: `ERaidMode.Online`
continues to the insurance screen, anything else goes straight to match accept.

Skipping it forward removes it backward too, for free. Queued screens form a linked list — each one
walks back to the current screen controller and stores it as its previous screen. A screen that
never calls `ShowScreen` never joins that list, so Back on the following screen returns to location
select rather than to a settings screen you never saw.

Fika's `CanEditRaidSettings` flag is read reflectively because it moved between versions — a field
on `FikaPlugin` up to 2.2.3, a property on `FikaPlugin.Settings` from 2.2.4 on. If neither is found,
the mod reports "can edit" and leaves the screen alone.

There is no config entry, no simulated button click, and no coroutine waiting on UI layout.

## Requirements

- SPT client install with Fika.
- .NET Framework 4.8 developer tools (for building).

## Build

1. Set `SPTBaseDir` in `FikaRaidSettingsSkipper.csproj` to your SPT root folder (default `C:\SPT`),
   or pass it on the command line: `dotnet build -c Release -p:SPTBaseDir=C:\SPT`.
2. Build `Release`.

The post-build step copies the DLL to `<ProjectRoot>\Build\BepInEx\plugins\FikaRaidSettingsSkipper.dll`.
Drop that into your SPT `BepInEx\plugins` folder.

## Credits

Approach inspired by [no-insurance](https://gitlab.com/vibrantrida/no-insurance), which showed me that
these menu steps are best removed at the `MainMenuControllerClass` transition rather than papered
over in the UI like my first attempt.
