# SoundCloud Desktop Beta

A Windows desktop client for SoundCloud. It's basically soundcloud.com wrapped in a native Fluent shell, with a few things the website doesn't give you: a real PiP player, global hotkeys, per-site volume that actually sticks, tracker blocking, and an encrypted local settings vault.

![stack](https://img.shields.io/badge/.NET_Framework-4.8-512BD4) ![ui](https://img.shields.io/badge/UI-WPF--UI_v4-blue) ![webview](https://img.shields.io/badge/engine-WebView2-green)

> Unofficial fan-made client. Not affiliated with SoundCloud. The SoundCloud logo and name belong to SoundCloud Ltd.

## What it does

- **Full client in one exe** — browse, search, charts, likes, login all work, session persists between runs.
- **PiP player** — pops open bottom-right, always on top, drag it by the slim header, resize from the corner. Plays the original playlist widget, promos hidden. There's also a PiP button injected right into the site (top-right corner on track/playlist pages).
- **Sound that works** — volume drives the site's real slider control (it's a custom div slider, hence the pointer-event injection), verified and retried; mute is enforced by the engine so it never misses.
- **Global hotkeys** — numpad by default (1 prev, 2 play/pause, 3 next, 4/5 volume), rebindable in settings, work even when the window isn't focused.
- **10 themes** — system, dark, light, orange, green, red, blue, purple, pink, teal (Fluent base + accent).
- **10 languages** — RU, EN, UA, ZH, DE, FR, ES, PL, TR, IT. Auto-detects the system language, falls back to English.
- **Ad/tracker blocking** — host + pattern lists in the EasyList/uBlock spirit (audio-ad hosts like adswizz/freewheel included), strict mode for analytics. Toggleable.
- **Encrypted vault** — settings live in `%LocalAppData%\SoundCloudDesktopBeta\vault.dat`, JSON + SHA256 integrity hash, DPAPI-encrypted for your Windows user only.
- **Light on resources** — when minimized the page stops rendering (audio keeps playing) and background polling slows down.
- **Tray + autostart** — minimize to tray, start with Windows, all optional.

## Install / portable

**Option A — installer:** grab `Setup.exe` from [Releases](../../releases), pick a language, point it at Program Files (or anywhere). It creates shortcuts, an uninstall entry, and checks for WebView2 Runtime.

**Option B — portable:** grab the `SoundCloudDesktopBeta-Portable.zip` from [Releases](../../releases), unpack anywhere, run `WpfApp1.exe`. Settings stay in LocalAppData either way.

You need [WebView2 Runtime](https://go.microsoft.com/fwlink/p/?LinkId=2124703) (it's already on most Windows 10/11 machines).

## Hotkeys (defaults)

| Keys | Action |
|---|---|
| Num 1 / Num 3 | Previous / next track |
| Num 2 | Play / pause |
| Num 4 / Num 5 | Volume −5% / +5% |

Change them in Settings. Any of Num 0–9, F1–F12, or off.

## Build from source

- Visual Studio 2022 with ".NET desktop development", or just the .NET SDK.
- .NET Framework 4.8 targeting pack (comes with VS; `dotnet build` restores the rest from NuGet).

```powershell
dotnet build WpfApp1/WpfApp1.csproj -c Release
dotnet build Setup/Setup.csproj -c Release
```

Run `WpfApp1.exe` from `WpfApp1/bin/Release/net48`. For the installer layout, put the app files into an `app/` folder next to `Setup.exe` (that's what the zip in Releases looks like).

## Project layout

```
WpfApp1/          the client (WPF-UI v4 + WebView2, net48/x64)
  MainWindow.*    browser shell, media/volume, hotkeys, tray
  PipWindow.*     always-on-top widget player
  SettingsWindow.*  language, themes, binds, adblock, cache
  SecureStore.cs  DPAPI + SHA256 settings vault
  AdBlock.cs      host/pattern block lists
  Themes.cs       10 Fluent themes
  Strings.cs      all 10 localizations, no resx
  Icons.xaml      Lucide icons as geometries
  assets/         app icon (.ico built from the SC cloud)
Setup/            WPF installer (language, Program Files, shortcuts, uninstall)
```

## Notes

- Ad blocking here is host/pattern based — it kills most audio/video ads and trackers, but it's not the full uBlock Origin engine. Strict mode can break Google/Facebook login, so it's off by default.
- Playback, likes and login are SoundCloud's own site running inside WebView2 — if the site changes its DOM (player buttons, volume slider), the injected controls may need updated selectors.
