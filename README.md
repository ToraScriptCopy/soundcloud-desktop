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

[Download](#-download) - [Builds](#-builds) - [Screenshots](#screenshots) - [Features](#-features) - [Platforms](#-platforms) - [FAQ](#-faq)

</div>


## 📦 Download

Grab the latest release on the [Releases page](https://github.com/ToraScriptCopy/soundcloud-desktop/releases). Everything is portable: unpack and run, no install, no admin rights.

| Build | Windows x64 | Windows x86 | Linux x64 | Size |
|---|---|---|---|---|
| **Classic** (WPF) | `...-Portable-vX.zip` | - | - | ~3 MB |
| **Alternative UI** (Electron + Radix, experimental) | `...-AlternativeUI-vX.zip` | - | `...-Linux-AlternativeUI-vX.tar.gz` | ~160 MB |
| **Light** (single file) | `SoundCloudLight.exe` | `SoundCloudLight-x86.exe` | `SoundCloudLight-Linux-vX.tar.gz` | ~100 MB |

The Classic build needs [WebView2 Runtime](https://go.microsoft.com/fwlink/p/?LinkId=2124703). Win10 and Win11 usually have it already. Light installs it for you when it is missing. Alt needs nothing extra on Windows. On Linux, Alt needs basic desktop libs (`libnss3`, `libatk`, `libcups`), Light needs WebKitGTK (`libwebkit2gtk-4.1-0`).


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
| Light | Yes, single exe | Yes, single exe | Yes, portable tar |

Linux builds are produced automatically by CI on every release. Classic cannot come to Linux: WPF only exists on Windows.

## ❓ FAQ

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

## 📄 License

MIT. See [LICENSE](https://github.com/ToraScriptCopy/soundcloud-desktop/blob/main/LICENSE).
