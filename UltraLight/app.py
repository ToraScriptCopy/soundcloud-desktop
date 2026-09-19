"""SoundCloud Desktop Ultra Light - just SoundCloud in a window, nothing else.

Login and site data live in system files (%LOCALAPPDATA%/SoundCloudDesktopUltraLight),
never next to the exe. Single-file build via PyInstaller.
"""
import os
import sys

import webview

HOME_URL = 'https://soundcloud.com/'


def profile_dir():
    # System files, never next to the exe. Windows: LocalAppData,
    # Linux: XDG data dir. Login survives restarts either way.
    if os.name == 'nt':
        base = os.environ.get('LOCALAPPDATA') or os.path.expanduser('~')
        d = os.path.join(base, 'SoundCloudDesktopUltraLight', 'EBWebView')
    else:
        base = os.environ.get('XDG_DATA_HOME') or os.path.join(
            os.path.expanduser('~'), '.local', 'share')
        d = os.path.join(base, 'soundcloud-ultra-light', 'webview')
    os.makedirs(d, exist_ok=True)
    return d


def main():
    profile = profile_dir()
    win = webview.create_window(
        'SoundCloud Light',
        HOME_URL,
        width=1180,
        height=760,
        min_size=(860, 560),
    )
    # storage_path pins the WebView2 profile to system files,
    # so login survives restarts and the exe folder stays clean.
    webview.start(private_mode=False, storage_path=profile, gui='edgechromium')


if __name__ == '__main__':
    sys.exit(main())
