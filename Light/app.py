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
    win = webview.create_window(
        APP_TITLE, HOME_URL,
        width=1180, height=760, min_size=(860, 560),
    )

    def slim_chrome():
        # Barely visible scrollbars plus a quiet volume slider. Nothing else.
        try:
            win.evaluate_js(
                "(function(){var s=document.getElementById('__scLite');"
                "if(!s){s=document.createElement('style');s.id='__scLite';"
                "(document.head||document.documentElement).appendChild(s);}"
                "s.textContent='html{scrollbar-width:thin;"
                "scrollbar-color:rgba(255,255,255,.15) transparent!important}"
                "::-webkit-scrollbar{width:6px!important;height:6px!important}"
                "::-webkit-scrollbar-thumb{background:rgba(255,255,255,.15)"
                "!important;border-radius:99px!important;border:none!important}"
                "::-webkit-scrollbar-thumb:hover{background:rgba(255,255,255,.35)"
                "!important}::-webkit-scrollbar-track{background:transparent"
                "!important}.volume__sliderBackground,.volume__sliderWrapper{"
                "background:rgba(255,255,255,.12)!important;border-radius:99px"
                "!important}.volume__sliderWrapper{opacity:.5!important}"
                ".volume__sliderWrapper:hover{opacity:1!important}';})()")
        except Exception:
            pass

    def popup_bridge():
        # pywebview cannot open popup windows, and OAuth starts life as
        # window.open('about:blank'). Route auth popups into this same tab
        # instead, so the login finishes here with the full session.
        # Anything else stays untouched.
        try:
            win.evaluate_js(
                "(function(){if(window.__scPopBridge)return;"
                "window.__scPopBridge=true;"
                "function go(u){try{if(u)window.location.href=u;}catch(e){}}"
                "function auth(u){u=String(u||'').toLowerCase();"
                "if(u===''||u==='about:blank')return true;"
                "if(u.indexOf('http')!==0)return false;"
                "return u.indexOf('soundcloud.com')>=0"
                "||u.indexOf('sndcdn.com')>=0"
                "||u.indexOf('accounts.google.com')>=0"
                "||u.indexOf('apis.google.com')>=0"
                "||u.indexOf('googleusercontent.com')>=0"
                "||u.indexOf('gstatic.com')>=0"
                "||u.indexOf('appleid.apple.com')>=0"
                "||u.indexOf('id.apple.com')>=0"
                "||u.indexOf('facebook.com')>=0"
                "||u.indexOf('facebook.net')>=0"
                "||u.indexOf('fbcdn.net')>=0"
                "||u.indexOf('connect.facebook')>=0"
                "||u.indexOf('login')>=0||u.indexOf('signin')>=0"
                "||u.indexOf('signup')>=0||u.indexOf('register')>=0"
                "||u.indexOf('oauth')>=0||u.indexOf('auth')>=0;}"
                "function shim(){return{closed:false,"
                "location:{set href(v){if(auth(v))go(v);},"
                "get href(){try{return window.location.href;}catch(e){return '';}}},"
                "document:{write:function(){},writeln:function(){},close:function(){}},"
                "postMessage:function(){},addEventListener:function(){},"
                "removeEventListener:function(){},"
                "close:function(){},focus:function(){},blur:function(){}};}"
                "try{window.open=function(u){try{"
                "if(auth(u)){if(u&&u!=='about:blank')go(u);return shim();}"
                "}catch(e){}return shim();};}catch(e){}"
                "try{document.addEventListener('click',function(e){"
                "try{var t=e.target;"
                "var a=t&&t.closest?t.closest('a[target=\"_blank\"]'):null;"
                "if(!a||!a.href)return;"
                "if(auth(a.href)){e.preventDefault();go(a.href);}"
                "}catch(err){}},true);}catch(e){}"
                "})()")
        except Exception:
            pass

    try:
        win.events.loaded += slim_chrome
    except Exception:
        pass
    try:
        win.events.loaded += popup_bridge
    except Exception:
        pass
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
