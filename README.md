# SoundCloud Desktop

A decent SoundCloud client for Windows. Open it, play some music, minimize to tray. Nothing extra.

> Fan project, not affiliated with SoundCloud. All music, the name and the logo belong to SoundCloud and its artists.

## Download

Go to [Releases](../../releases) and grab the latest zip.

No install needed at all. Unpack it wherever you want and run `SoundCloudDesk.exe`. I removed the installer, it kept breaking with those payload logs, so it is gone for good.

You only need [WebView2 Runtime](https://go.microsoft.com/fwlink/p/?LinkId=2124703). Win10 and Win11 usually have it already.

## What is inside

- Login sticks. Sign in with Google, Apple or whatever you like, restart, and you are still signed in
- Separate "Now playing" window. It pops up when a track starts, with cover art, artist, buttons and minimize to tray
- PiP player for a track or playlist. The button moved down next to the volume, it no longer sticks out at the top
- Site animations. Turn on light animations for cards and buttons in settings
- SoundCloud ReDesign (beta). A Material Design 3 restyle in settings, rounded cards and buttons. Rough around the edges but already pretty
- Extensions. Pick a folder with an unpacked Chrome extension in settings and it gets loaded
- Ad blocking. Off by default, single toggle. Login and signup windows are never touched
- Numpad hotkeys (1/2/3 tracks, 4/5 volume), everything is rebindable
- 10 themes and 10 languages with auto detect
- Tray, autostart, sidebar, volume that actually saves
- Settings are stored locally encrypted, only for your Windows user

## Build it yourself

You need VS2022 with ".NET desktop development" or just the .NET SDK:

```powershell
dotnet build WpfApp1/WpfApp1.csproj -c Release
```

The exe lands in `WpfApp1/bin/Release/net48`.

## A couple of notes

- Playback and login are the SoundCloud site itself inside WebView2. If they change the layout, some buttons may break, just open an issue
- The blocker is simple, hosts and patterns based, it is not uBlock. If something does not load, turn it off
- The data folder lives in `%LocalAppData%/SoundCloudDesktopBeta`, the browser profile is there too, that is why login survives restarts
