<div align="center">

<img src="https://github.com/ToraScriptCopy/soundcloud-desktop/raw/main/WpfApp1/assets/sc-icon.png" width="96" alt="SoundCloud Desktop logo" />

# SoundCloud Desktop

**The most lightweight unofficial SoundCloud client. About 3 MB, no installer, no services, no telemetry.**

[![Latest release](https://img.shields.io/github/v/release/ToraScriptCopy/soundcloud-desktop)](https://github.com/ToraScriptCopy/soundcloud-desktop/releases)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux-blue)](https://github.com/ToraScriptCopy/soundcloud-desktop/releases)
[![License](https://img.shields.io/github/license/ToraScriptCopy/soundcloud-desktop)](https://github.com/ToraScriptCopy/soundcloud-desktop/blob/main/LICENSE)
[![Downloads](https://img.shields.io/github/downloads/ToraScriptCopy/soundcloud-desktop/total)](https://github.com/ToraScriptCopy/soundcloud-desktop/releases)
[![Website](https://img.shields.io/badge/site-GitHub%20Pages-orange)](https://torascriptcopy.github.io/soundcloud-desktop/)

> Fan project, not affiliated with SoundCloud. All music, the name and the logo belong to SoundCloud and its artists.

[Download](#-download) - [Builds](#-builds) - [Features](#-features) - [Platforms](#-platforms) - [FAQ](#-faq)

</div>

## 📦 Download

Grab the latest release on the [Releases page](https://github.com/ToraScriptCopy/soundcloud-desktop/releases). Everything is portable: unpack and run, no install, no admin rights.

| Build | Windows x64 | Windows x86 | Linux x64 | Size |
|---|---|---|---|---|
| **Classic** (WPF) | `...-Portable-vX.zip` | - | - | ~3 MB |
| **Alternative UI** (Electron + Radix, experimental) | `...-AlternativeUI-vX.zip` | - | `...-Linux-AlternativeUI-vX.tar.gz` | ~160 MB |
| **Ultra Light** (single file) | `SoundCloudUltraLight.exe` | `SoundCloudUltraLight-x86.exe` | `...-Linux-UltraLight-vX.tar.gz` | ~100 MB |

The Classic build needs [WebView2 Runtime](https://go.microsoft.com/fwlink/p/?LinkId=2124703). Win10 and Win11 usually have it already. Alt and Ultra Light on Windows need nothing extra. On Linux, Alt needs basic desktop libs (`libnss3`, `libatk`, `libcups`), Ultra Light needs WebKitGTK (`libwebkit2gtk-4.1-0`).

##  Builds

flowchart TB
    %% Nodes & Grouping Definitions
    
    subgraph Origin [" Base Data Provider "]
        SC[("soundcloud.com\nSoundCloud Web & API Ecosystem")]
    end

    subgraph Matrix [" Architectural Variants & Technical Specifications "]
        
        subgraph C_Group [" 1. Classic Shell (WPF / .NET Native) "]
            direction TB
            C_Arch["Engine: C# / WPF Core"]
            C_UI["UI Framework: Native Windows Controls"]
            C_Binary["Binary Executable: ~3 MB Overhead"]
            C_Footprint["RAM Consumption: ~30-50 MB"]
            C_Features["Scope: Native Audio Output, Global Hotkeys, Light Memory Footprint"]
        end

        subgraph A_Group [" 2. Alternative Client (Electron + Radix UI) "]
            direction TB
            A_Arch["Engine: Node.js Runtime + Chromium Core"]
            A_UI["UI Framework: React / Radix UI Design System"]
            A_Binary["Binary Executable: ~80-120 MB Package"]
            A_Footprint["RAM Consumption: ~200-400 MB"]
            A_Features["Scope: Modern Custom UI, 20+ Built-in Extras, Discord Rich Presence, EQ, Themes"]
        end

        subgraph U_Group [" 3. Ultra-Light Wrapper (Standalone Executable) "]
            direction TB
            U_Arch["Engine: Native OS WebView Engine (WebView2 / MSHTML)"]
            U_UI["UI Framework: Direct Remote Web Rendering"]
            U_Binary["Binary Executable: Single Executable File (~1 MB)"]
            U_Footprint["RAM Consumption: ~15-30 MB"]
            U_Features["Scope: Raw Web Application, Zero Dependencies, Minimal Overhead"]
        end

    end

    subgraph Integration [" System & OS Integration Layer "]
        SMTC["OS System Media Transport Controls"]
        AUDIO["Direct System Audio Stream Output"]
        EXTRAS["Custom Modules & Extension Engine"]
    end

    %% Flow Pipelines
    SC ==>|"REST / WebSocket API Data Pipeline"| C_Arch
    SC ==>|"Full Web Stack & API Integration"| A_Arch
    SC ==>|"Direct HTTPS Web Interface Rendering"| U_Arch

    %% Internal Data Mapping
    C_Arch --> C_UI --> C_Binary --> C_Footprint --> C_Features
    A_Arch --> A_UI --> A_Binary --> A_Footprint --> A_Features
    U_Arch --> U_UI --> U_Binary --> U_Footprint --> U_Features

    %% Integration Pipelines
    C_Features --> SMTC & AUDIO
    A_Features --> EXTRAS & SMTC & AUDIO
    U_Features --> AUDIO

    %% Styling Classes for GitHub Dark/Light Themes
    classDef main fill:#ff5500,stroke:#222,stroke-width:2px,color:#fff,font-weight:bold;
    classDef classic fill:#0f172a,stroke:#38bdf8,stroke-width:2px,color:#f8fafc;
    classDef alt fill:#0f172a,stroke:#a855f7,stroke-width:2px,color:#f8fafc;
    classDef ultra fill:#0f172a,stroke:#22c55e,stroke-width:2px,color:#f8fafc;
    classDef system fill:#1e293b,stroke:#64748b,stroke-width:1px,color:#cbd5e1;

    class SC main;
    class C_Arch,C_UI,C_Binary,C_Footprint,C_Features classic;
    class A_Arch,A_UI,A_Binary,A_Footprint,A_Features alt;
    class U_Arch,U_UI,U_Binary,U_Footprint,U_Features ultra;
    class SMTC,AUDIO,EXTRAS system;

- **Classic (WPF)** - the main build. Fluent shell, Now playing popup, PiP player, hotkeys, extensions, 14 themes, encrypted local settings.
- **Alternative UI** - the experimental playground. Real Radix Themes interface with a full shell (navigation, sidebar, bottom bar) plus 20 extra desktop features. Bigger download, needs no WebView2.
- **Ultra Light** - one exe and nothing else, just SoundCloud in a window titled SoundCloud Light. Login lives in system files, the folder stays clean.

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

## ✨ Features

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

## 🚀 Why it runs lighter than the rest

- **Tiny.** Classic is about 3 MB. Electron based clients bundle a full browser and idle at 100+ MB of RAM. Here the browser (WebView2) is shared with the system, so the app itself adds almost nothing.
- **No installer, no services.** Nothing runs when the app is closed, unless you turn on autostart yourself.
- **One browser profile.** Login stored once and reused, minimal flags, fast start.
- **No tracking.** Settings live locally encrypted, only for your Windows user.

## 🧭 Platforms

| Build | Windows x64 | Windows x86 | Linux x64 |
|---|---|---|---|
| Classic (WPF) | Yes | No | No, WPF is Windows-only |
| Alternative UI | Yes | No | Yes, portable tar |
| Ultra Light | Yes, single exe | Yes, single exe | Binary via CI, or run from source: `pip install pywebview` then `python app.py` |

Linux builds are produced automatically by CI on every release. Classic cannot come to Linux: WPF only exists on Windows.

## ❓ FAQ

<details>
<summary>Is this official?</summary>
No. Fan project, not affiliated with SoundCloud.
</details>

<details>
<summary>Where is my login stored?</summary>
In system files, never next to the exe. Classic uses LocalAppData, Alt uses its Electron profile, Ultra Light uses its own LocalAppData folder (XDG data dir on Linux).
</details>

<details>
<summary>Something broke after a SoundCloud update?</summary>
Playback and login are the real site inside the app. If SoundCloud changes its layout, open an issue.
</details>

## 📄 License

MIT. See [LICENSE](https://github.com/ToraScriptCopy/soundcloud-desktop/blob/main/LICENSE).
