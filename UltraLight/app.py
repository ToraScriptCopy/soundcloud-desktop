"""SoundCloud Desktop Ultra Light - just SoundCloud in a window, nothing else.

Login and site data live in system files (%LOCALAPPDATA%/SoundCloudDesktopUltraLight),
never next to the exe. Single-file build via PyInstaller.
"""
import os
import sys

import webview

HOME_URL = 'https://soundcloud.com/'


def profile_dir():
    base = os.environ.get('LOCALAPPDATA') or os.path.expanduser('~')
    d = os.path.join(base, 'SoundCloudDesktopUltraLight', 'EBWebView')
    os.makedirs(d, exist_ok=True)
    return d


def main():
    profile = profile_dir()
    win = webview.create_window(
        'SoundCloud',
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
