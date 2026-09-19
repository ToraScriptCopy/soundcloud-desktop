# SoundCloud Desktop

The most lightweight unofficial SoundCloud client for Windows. Other wrappers ship an entire Electron browser per app and happily sit at 100+ MB on disk and in RAM. This one is a thin native shell around the real site: about 3 MB to download, no installer, no background services, no telemetry. Open it, play music, minimize to tray.

> Fan project, not affiliated with SoundCloud. All music, the name and the logo belong to SoundCloud and its artists.

## Download

Go to [Releases](../../releases) and grab the latest.

There are three builds:

- **Classic (WPF)** - `SoundCloudDesktopBeta-Portable-vX.zip`. The main build with a Fluent design shell. About 3 MB, the lightest one.
- **Alternative UI** - `SoundCloudDesktopBeta-Portable-AlternativeUI-vX.zip`. A separate build on Electron with a real Radix Themes interface: full shell with navigation, sidebar and bottom bar, same features as Classic. Bigger download since it bundles its own Chromium, needs no WebView2.
- **Ultra Light** - `SoundCloudDesktopBeta-UltraLight-vX.zip`. One exe and nothing else, just SoundCloud in a window. Login lives in system files, so the folder stays clean.

All are portable. Unpack wherever you want and run the exe. No install, no admin rights.

The Classic build needs [WebView2 Runtime](https://go.microsoft.com/fwlink/p/?LinkId=2124703). Win10 and Win11 usually have it already. The Alternative UI and Ultra Light builds need nothing extra.

## Why it runs lighter than the rest

- Tiny. The whole app is about 3 MB. Electron based clients bundle a full browser and idle at 100+ MB of RAM. Here the browser (WebView2) is shared with the system, so the app itself adds almost nothing on top.
- No installer, no services, no background junk. Nothing runs when the app is closed, unless you turn on autostart yourself.
- One browser profile with minimal flags. Login is stored once and reused, the engine starts fast and does not sync anything extra.
- Thin UI. The shell is native, the top and bottom bars are compact, so the site gets almost the whole window.
- Ad blocking is off by default, so pages load at full speed unless you turn it on.
- No tracking in the app itself. Settings are stored locally encrypted, only for your Windows user.

## What is inside

- Login sticks. Sign in with Google, Apple or whatever you like, restart, and you are still signed in
- Separate "Now playing" window. It pops up when a track starts, with cover art, artist, buttons and minimize to tray
- PiP player for a track or playlist. The button sits at the bottom next to the volume
- Site animations. Turn on light animations for cards, popups and buttons in settings
- SoundCloud ReDesign. A full restyle of every part of the site: cards, buttons, header, bottom player, comments, sidebar, inputs, popups. Each part has its own toggle in the ReDesign options window, text colors stay as they are
- Become-an-author promo banners are removed automatically, always, without touching login or signup
- Extensions. Pick a folder with an unpacked Chrome extension in settings and it gets loaded
- Ad blocking. Off by default, single toggle. Login and signup windows are never touched
- Numpad hotkeys (1/2/3 tracks, 4/5 volume), everything is rebindable
- 10 themes and 10 languages with auto detect
- Tray, autostart, sidebar, volume that actually saves

## A couple of notes

- Playback and login are the SoundCloud site itself inside WebView2. If they change the layout, some buttons may break, just open an issue
- The blocker is simple, hosts and patterns based, it is not uBlock. If something does not load, turn it off
- The data folder lives in `%LocalAppData%/SoundCloudDesktopBeta`, the browser profile is there too, that is why login survives restarts. The Alternative UI build keeps its own folder, so you sign in there once
