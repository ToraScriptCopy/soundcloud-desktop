<div align="center">

<img src="https://github.com/ToraScriptCopy/soundcloud-desktop/raw/main/WpfApp1/assets/sc-icon.png" width="96" alt="SoundCloud Desktop logo" />

# SoundCloud Desktop

**The most lightweight unofficial SoundCloud client. About 3 MB, no installer, no services, no telemetry.**

[![Latest release](https://img.shields.io/github/v/release/ToraScriptCopy/soundcloud-desktop)](https://github.com/ToraScriptCopy/soundcloud-desktop/releases)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux-blue)](https://github.com/ToraScriptCopy/soundcloud-desktop/releases)
[![License](https://img.shields.io/github/license/ToraScriptCopy/soundcloud-desktop)](https://github.com/ToraScriptCopy/soundcloud-desktop/blob/main/LICENSE)
[![Website](https://img.shields.io/badge/site-GitHub%20Pages-orange)](https://torascriptcopy.github.io/soundcloud-desktop/)

> Fan project, not affiliated with SoundCloud. All music, the name and the logo belong to SoundCloud and its artists.

[Download](#download) - [Builds](#builds) - [Screenshots](#screenshots) - [Features](#features) - [Platforms](#platforms) - [Build](#build) - [Source map](#source-map) - [FAQ](#faq)

</div>

## Download

Grab the latest release on the [Releases page](https://github.com/ToraScriptCopy/soundcloud-desktop/releases). Everything is portable: unpack and run, no install, no admin rights.

| Build | Windows x64 | Windows x86 | Linux x64 | macOS | Size |
|---|---|---|---|---|---|
| **Classic** (WPF) | `...-Portable-vX.zip` | - | - | - | ~3 MB |
| **Alternative UI** (Electron + Radix, experimental) | `...-AlternativeUI-vX.zip` | - | `...-Linux-AlternativeUI-vX.tar.gz` | `...-Mac-AlternativeUI-{arch}-vX.tar.gz` | ~160 MB |
| **Light** (single file) | `SoundCloudLight.exe` | `SoundCloudLight-x86.exe` | `SoundCloudLight-Linux-vX.tar.gz` | `SoundCloudLight-Mac-{arch}-vX.tar.gz` | ~100 MB |

The Classic build needs [WebView2 Runtime](https://go.microsoft.com/fwlink/p/?LinkId=2124703). Win10 and Win11 usually have it already. Light installs it for you when it is missing. Alt needs nothing extra on Windows. On Linux, Alt needs basic desktop libs (`libnss3`, `libatk`, `libcups`), Light needs WebKitGTK (`libwebkit2gtk-4.1-0`). Mac builds are unsigned, right-click the app and choose Open on first launch.

- **Classic (WPF)** - the main build. Fluent shell, Now playing popup, PiP player, hotkeys, extensions, 14 themes, encrypted local settings.
- **Alternative UI** - the experimental playground. Real Radix Themes interface with a full shell (navigation, sidebar, bottom bar) plus 20 extra desktop features. Bigger download, needs no WebView2.
- **Light** - one exe and nothing else, just SoundCloud in a window titled SoundCloud Light. Login lives in system files, the folder stays clean. Installs WebView2 itself when it is missing.

## Screenshots

Real windows captured from running builds.

| Classic | Alternative UI |
|---|---|
| ![Classic](https://github.com/ToraScriptCopy/soundcloud-desktop/raw/main/docs/screenshots/classic.png) | ![Alternative UI](https://github.com/ToraScriptCopy/soundcloud-desktop/raw/main/docs/screenshots/alt.png) |

<details>
<summary><strong>All 20 Alt extras (click to expand)</strong></summary>

1. Jump List with recently played tracks
2. Mini player mode (compact always-on-top)
3. Sleep timer (15/30/60/90 min)
4. Playback speed (0.75x to 2x)
5. Repeat one
6. Like button in shell and player
7. Track-change desktop notifications
8. Tray tooltip with the current track
9. Page zoom (Ctrl + / - / 0)
10. Recently played list in the sidebar
11. Quick links (saved pages in the sidebar)
12. Start minimized
13. Boss key (F9 hides everything)
14. Always on top
15. Shell themes (dark / light / system, 6 accents)
16. Adblock blocked-counter
17. Listening stats (tracks + time)
18. Built-in update checker
19. Settings export and import
20. Playback progress bar in the bottom bar

</details>

## Features

- **Login sticks.** Google, Apple or password. Restart, still signed in.
- **Now playing popup.** Cover art, artist, controls, minimize to tray.
- **PiP widget.** Any track or playlist in an always-on-top window.
- **ReDesign with flags.** Every part of the site restyled separately: cards, buttons, header, bottom player, comments, sidebar, inputs, popups. Text colors stay as they are.
- **Promo killer.** Become-an-author banners removed by their root container, always, without touching login or signup.
- **Site animations.** Simple fades and pops for cards, modals and menus.
- **Extensions.** Unpacked Chrome extensions from a folder.
- **Quiet adblock.** Off by default, never touches login windows.
- **Hotkeys.** Numpad controls that work even unfocused.
- **Tray, autostart, sidebar, volume that actually saves.**

## Why it runs lighter than the rest

- **Tiny.** Classic is about 3 MB. Electron based clients bundle a full browser and idle at 100+ MB of RAM. Here the browser (WebView2) is shared with the system, so the app itself adds almost nothing.
- **No installer, no services.** Nothing runs when the app is closed, unless you turn on autostart yourself.
- **One browser profile.** Login stored once and reused, minimal flags, fast start.
- **No tracking.** Settings live locally encrypted, only for your Windows user.

## Platforms

| Build | Windows x64 | Windows x86 | Linux x64 | macOS x64 and arm64 |
|---|---|---|---|---|
| Classic (WPF) | Yes | No | No, WPF is Windows-only | No, WPF is Windows-only |
| Alternative UI | Yes | No | Yes, portable tar | Yes, app tar via CI |
| Light | Yes, single exe | Yes, single exe | Yes, portable tar | Yes, binary tar via CI |

Linux and Mac builds are produced automatically by CI on every release. Classic cannot leave Windows: WPF only exists there. Mac builds are unsigned, right-click the app and choose Open on first launch.

## Source map

How the program works, file by file.

```mermaid
flowchart TB
    U(["User"]) --> CW["Classic window"]
    U --> AW["Alt shell"]
    U --> LW["Light window"]
    SC["soundcloud.com"]

    subgraph Classic ["Classic — WPF on .NET Framework"]
        direction TB
        MW["MainWindow\nshell plus browser plus poll loop\nvolume, hotkeys, tray, player"]
        AD["AdBlock\nhost and pattern filter\nplus auth allowlist"]
        SE["SiteExtras\n8 CSS flags plus promo killer\nplus animations"]
        SS["SecureStore\nencrypted vault\nplus test profile override"]
        SW["SettingsWindow\ntoggles, hotkeys, extensions\nplus pin to Start"]
        RW["RedesignWindow\n8 live flags"]
        PW["PlayerWindow\nNow playing popup"]
        PI["PipWindow\nembed widget"]
        AU["AuthWindow\nOAuth popup"]
        ST["Strings\n10 languages"]
        TH["Themes\n14 themes"]
        FX["Fx\nshell animations"]
        MW --> AB
        MW --> SE
        MW --> SS
        MW --> SW
        MW --> PW
        MW --> PI
        MW --> AU
        SW --> RW
        SW --> ST
        SW --> TH
    end

    subgraph Alt ["Alternative UI — Electron plus Radix, experimental"]
        direction TB
        MJ["electron/main.js\nwindows, tray, IPC\nshortcuts, updates, stats"]
        PR["preload-shell.js\nsecure window.api bridge"]
        SH["shell.jsx\nnav plus sidebar plus bottom bar\nplus recent and links"]
        PL["player.jsx\ntransport plus speed\nplus repeat and mini"]
        ST2["settings.jsx\nshell themes plus flags\nplus tools"]
        MJ --> SH
        MJ --> PL
        MJ --> ST2
        SH --> PR
        PL --> PR
        ST2 --> PR
    end

    subgraph Light ["Light — Python plus pywebview"]
        direction TB
        AP["app.py\nwindow plus system profile\nplus WebView2 auto install"]
    end

    subgraph Web ["Website and CI"]
        direction TB
        DOCS["docs/\nlanding plus releases\nplus source browser"]
        CI["linux.yml\nLinux and Mac builds"]
    end

    CW --> MW
    AW --> MJ
    LW --> AP
    MW -.->|"injects CSS and JS"| SC
    MJ -.->|"injects CSS and JS"| SC
    LW -.->|"plain view"| SC
    SS --> VAULT[("vault.dat<br/>encrypted settings")]
    MJ --> STORE[("settings.json<br/>plain prefs")]
    CI -.->|"attaches tars"| REL[("GitHub Release")]
```

**WpfApp1/ - the Classic build (C#, WPF, .NET Framework 4.8)**

| File | Responsibility |
|---|---|
| `MainWindow.xaml` / `.xaml.cs` | Main window: top nav bar, sidebar, site view, bottom volume bar. Owns the WebView2 browser, the poll loop (track info), volume, hotkeys, tray icon, PiP and player windows, extension loading. Entry point of the whole app. |
| `AdBlock.cs` | Host + pattern request blocker with an auth allowlist. Single toggle, off by default, login windows are never blocked. |
| `SiteExtras.cs` | Everything injected into soundcloud.com: ReDesign CSS split into 8 flags (cards, buttons, header, player, comments, sidebar, inputs, popups), site animations, thin scrollbars, and the promo killer script. |
| `SecureStore.cs` | Settings vault: JSON + SHA256 hash, DPAPI-encrypted per Windows user. Holds `AppState` with all toggles, volume, hotkeys, flags, extension folders. `SCD_PROFILE` env override for isolated test runs. |
| `SettingsWindow.xaml` / `.xaml.cs` | Settings: language, theme, toggles, hotkey binds, extensions list, cache clear, reset, button into the ReDesign window. |
| `RedesignWindow.xaml` / `.xaml.cs` | The 8 ReDesign flag toggles, applied live to the site. |
| `PlayerWindow.xaml` / `.xaml.cs` | The Now playing popup: artwork, title, artist, time, transport buttons, PiP, minimize to tray. |
| `PipWindow.xaml` / `.xaml.cs` | Always-on-top widget with the SoundCloud embed player for any link. |
| `AuthWindow.xaml` / `.xaml.cs` | Popup window for login flows (Google, Apple). Shares the main browser profile, so login lands in the app. |
| `Strings.cs` | All UI text in 10 languages with auto-detect. |
| `Themes.cs` | 14 Fluent themes (system, dark, light + accent colors). |
| `Fx.cs` | Shell animation helpers: fades, slides, window entrances. Only opacity and transforms, WebView2 is never animated. |
| `App.xaml` / `.xaml.cs` | App bootstrap, global resources, subtle slider and scrollbar styles, crash log. |
| `Icons.xaml` | Hand-drawn icon geometries for shell buttons. |
| `WpfApp1.csproj` | Project file: net48, x64, WPF-UI + WebView2 packages. |

**AltElectron/ - the experimental build (Electron + React + Radix Themes)**

| File | Responsibility |
|---|---|
| `electron/main.js` | Everything behind the windows: windows, tray, Jump List, shortcuts, adblock, extensions, update checker, stats, sleep timer, IPC, settings store. Mirrors the WPF site CSS/JS. |
| `electron/preload-shell.js` | Secure bridge between React windows and the main process (`window.api`). |
| `src/shell.jsx` | Main window UI: top nav bar, sidebar (nav, PiP, recent, quick links), bottom bar with progress, volume, like. |
| `src/player.jsx` | Now playing window: artwork, transport, speed, repeat, like, mini mode, volume. |
| `src/settings.jsx` | Settings window: shell themes, playback, blocking, ReDesign flags, stats, extensions, updates, data tools. |
| `src/shell.css` | Shell animations, thin scrollbars, subtle sliders, artwork rounding. |
| `scripts/pack-portable.js` | Assembles the portable folder from the Electron runtime + app files. Works for Windows and Linux targets. |
| `vite.config.js`, `player.html`, `settings.html`, `shell.html` | Vite multi-page build for the three React windows. |

**Light/ - the single-file build (Python + pywebview)**

| File | Responsibility |
|---|---|
| `app.py` | The whole app: window with soundcloud.com, profile in system files, anti-throttle flags for smooth background playback, one-click WebView2 install when missing. Builds to one exe with PyInstaller (x64 and x86). |
| `ul-icon.ico` | App icon embedded into the exe. |

**docs/ - the website (static, GitHub Pages)**

| File | Responsibility |
|---|---|
| `index.html` | Landing: builds, screenshots, features, platforms, live releases, source browser, verification, FAQ. |
| `styles.css` | Material 3 dark baseline (light theme included), reveal animations, responsive layout. |
| `i18n.js` | All 10 interface languages with flag picker, choice remembered. |
| `site.js` | Live GitHub data (releases, file tree, hashes), theme toggle, copy buttons, fullscreen editor. |
| `sitemap.xml` + `sitemap-*.xml`, `robots.txt` | Search engine index with hidden sitemaps. |
| `screenshots/` | Real captures from running builds. |

**.github/workflows/linux.yml** - CI: builds the Linux Alt tar and Light binary on every release and attaches them automatically.

## FAQ

<details>
<summary>Is this official?</summary>
No. Fan project, not affiliated with SoundCloud.
</details>

<details>
<summary>Where is my login stored?</summary>
In system files, never next to the exe. Classic uses LocalAppData, Alt uses its Electron profile, Light uses its own LocalAppData folder (XDG data dir on Linux).
</details>

<details>
<summary>Something broke after a SoundCloud update?</summary>
Playback and login are the real site inside the app. If SoundCloud changes its layout, open an issue.
</details>

## License

MIT. See [LICENSE](https://github.com/ToraScriptCopy/soundcloud-desktop/blob/main/LICENSE).
