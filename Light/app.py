"""SoundCloud Light - just SoundCloud in a window, nothing else.

Login and site data live in system files, never next to the exe.
Single-file builds (x64 and x86) via PyInstaller.
"""
import os
import sys

APP_TITLE = 'SoundCloud Light'
HOME_URL = 'https://soundcloud.com/'
WEBVIEW2_LINK = 'https://go.microsoft.com/fwlink/p/?LinkId=2124703'


def profile_dir():
    # System files, never next to the exe. Login survives restarts either way.
    if os.name == 'nt':
        base = os.environ.get('LOCALAPPDATA') or os.path.expanduser('~')
        d = os.path.join(base, 'SoundCloudLight', 'EBWebView')
    else:
        base = os.environ.get('XDG_DATA_HOME') or os.path.join(
            os.path.expanduser('~'), '.local', 'share')
        d = os.path.join(base, 'soundcloud-light', 'webview')
    os.makedirs(d, exist_ok=True)
    return d


def smooth_flags():
    # Keep the page alive and smooth in the background: no throttling
    # of timers, renderers or media when the window is occluded.
    os.environ['WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS'] = (
        '--disable-background-timer-throttling '
        '--disable-backgrounding-occluded-windows '
        '--disable-renderer-backgrounding'
    )


def ensure_webview2():
    # Windows only: offer a one-click WebView2 install when it is missing.
    if os.name != 'nt':
        return True
    found = False
    try:
        import winreg
        paths = [
            (winreg.HKEY_LOCAL_MACHINE,
             r'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\ClientState\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}'),
            (winreg.HKEY_LOCAL_MACHINE,
             r'SOFTWARE\Microsoft\EdgeUpdate\ClientState\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}'),
            (winreg.HKEY_CURRENT_USER,
             r'Software\Microsoft\EdgeUpdate\ClientState\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}'),
        ]
        for root, p in paths:
            try:
                with winreg.OpenKey(root, p) as k:
                    v, _ = winreg.QueryValueEx(k, 'pv')
                    if v:
                        found = True
                        break
            except OSError:
                pass
    except Exception:
        pass
    if found:
        return True
    try:
        import ctypes
        r = ctypes.windll.user32.MessageBoxW(
            None,
            'WebView2 Runtime was not found. It is needed to show SoundCloud.\n\n'
            'Download and install it now?',
            APP_TITLE, 0x4 | 0x20)
        if r != 6:  # IDYES
            return False
        import tempfile
        import urllib.request
        tmp = os.path.join(tempfile.gettempdir(), 'WebView2Setup.exe')
        urllib.request.urlretrieve(WEBVIEW2_LINK, tmp)
        os.startfile(tmp)
    except Exception:
        pass
    return False


def main():
    if not ensure_webview2():
        return 0
    smooth_flags()
    try:
        import webview
    except Exception as ex:
        print('Python dependencies are missing: ' + str(ex))
        print('Run: pip install pywebview')
        return 1
    profile = profile_dir()
    webview.create_window(
        APP_TITLE, HOME_URL,
        width=1180, height=760, min_size=(860, 560),
    )
    try:
        webview.start(private_mode=False, storage_path=profile, gui='edgechromium')
    except Exception as ex:
        msg = str(ex)
        if os.name != 'nt' and 'webkit' in msg.lower():
            print('WebKitGTK is missing. Install it first, e.g.:')
            print('  sudo apt install python3-gi gir1.2-webkit2-4.1 libwebkit2gtk-4.1-0')
        else:
            print('Could not start the browser view: ' + msg)
        return 1
    return 0


if __name__ == '__main__':
    sys.exit(main())
