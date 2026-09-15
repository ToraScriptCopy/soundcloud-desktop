# SoundCloud Desktop Beta

Fan-made Windows client for SoundCloud. The site wrapped in a native Fluent shell, plus a PiP player, global hotkeys, volume that actually sticks, tracker blocking and encrypted local settings.

> Fan project, not affiliated with SoundCloud. Built for fun — not to make money off anyone or claim anything. All music, the name and the logo belong to SoundCloud and its artists. SoundCloud itself was founded by Alexander Ljung and Eric Wahlforss.

## Get it

Check [Releases](../../releases):

- **Setup.exe** — normal install into Program Files, shortcuts, uninstall entry.
- **Portable zip** — unpack anywhere, run `WpfApp1.exe`.

Both need [WebView2 Runtime](https://go.microsoft.com/fwlink/p/?LinkId=2124703) — it's preinstalled on most Win10/11 machines, the installer will tell you if it's missing.

## What's inside

- PiP widget player, always on top, drag it by the slim header
- Global numpad hotkeys (1/2/3 prev/play/next, 4/5 volume), rebindable in settings
- 10 Fluent themes, 10 interface languages with auto-detect
- Engine picker: built-in Chromium, or Edge/Chrome with your own profile
- Host + pattern adblock (strict mode included), works in PiP too
- Settings vault: JSON + SHA256, DPAPI-encrypted for your Windows user
- Tray icon, autostart, collapsible sidebar, installer + uninstaller

## Build

VS2022 with ".NET desktop development", or the .NET SDK:

```powershell
dotnet build WpfApp1/WpfApp1.csproj -c Release
```

For the single-file installer, zip the app output into the payload first, then build setup:

```powershell
Compress-Archive WpfApp1/bin/Release/net48/* Setup/payload/app.zip -Force
dotnet build Setup/Setup.csproj -c Release
```

## Notes

- The blocker is host/pattern based, not the full uBlock engine. Strict mode can break Google/Facebook login, so it's off by default.
- Playback and login are SoundCloud's own site inside WebView2. If they change the player DOM, the injected buttons may need new selectors.
