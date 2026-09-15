using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Extensions;
using UiControls = Wpf.Ui.Controls;

namespace WpfApp1
{
    // Main window: Fluent shell around soundcloud.com.
    public partial class MainWindow : UiControls.FluentWindow
    {
        private const string HomeUrl = "https://soundcloud.com/";
        private const string ChartsUrl = "https://soundcloud.com/charts/top";
        private const string LikesUrl = "https://soundcloud.com/you/likes";
        private const int WM_HOTKEY = 0x0312;

        private const string JsToggle =
            "(function(){var b=document.querySelector('button[aria-label=\"Pause\"]')"
            + "||document.querySelector('button[aria-label=\"Play\"]')"
            + "||document.querySelector('button[title=\"Pause\"]')"
            + "||document.querySelector('button[title=\"Play\"]')"
            + "||document.querySelector('.playControls__play');"
            + "if(!b)return 'no-btn';b.click();"
            + "var a=document.querySelector('audio');"
            + "return (a&&!a.paused)?'playing':'paused';})()";

        private const string JsNext =
            "(function(){var b=document.querySelector('button[aria-label=\"Next track\"]')"
            + "||document.querySelector('button[title=\"Next\"]')"
            + "||document.querySelector('button[title=\"Play next\"]')"
            + "||document.querySelector('.skipControl__next');"
            + "if(!b)return 'no-btn';b.click();return 'ok';})()";

        private const string JsPrev =
            "(function(){var b=document.querySelector('button[aria-label=\"Previous track\"]')"
            + "||document.querySelector('button[title=\"Previous\"]')"
            + "||document.querySelector('button[title=\"Play previous\"]')"
            + "||document.querySelector('.skipControl__previous');"
            + "if(!b)return 'no-btn';b.click();return 'ok';})()";

        private const string JsPoll =
            "(function(){var t=0,d=0;"
            + "try{var a=document.querySelector('audio');"
            + "if(a){t=Math.floor(a.currentTime||0);d=Math.floor(a.duration||0);}}catch(e){}"
            + "try{if(window.__scPipUpdate)window.__scPipUpdate(location.href);}catch(e){}"
            + "return t+'|'+d+'|'+encodeURIComponent(document.title);})()";

        // Встроенная PiP-кнопка сайта (Lucide picture-in-picture-2), правый верхний угол.
        private const string JsPipButton =
            @"(function(){if(window.__scPip)return;window.__scPip=true;
var css=document.createElement('style');css.id='__scPipCss';
css.textContent='#__scPipBtn{position:fixed;right:20px;top:14px;z-index:999999;width:54px;height:54px;border-radius:50%;border:none;background:#ff5500;cursor:pointer;box-shadow:0 4px 16px rgba(0,0,0,.45);display:none;align-items:center;justify-content:center;}#__scPipBtn:hover{background:#e04a00;}#__scPipBtn svg{display:block;margin:auto;}';
(document.head||document.documentElement).appendChild(css);
var b=document.createElement('button');b.id='__scPipBtn';b.title='PiP';
b.innerHTML=""<svg xmlns='http://www.w3.org/2000/svg' width='24' height='24' viewBox='0 0 24 24' fill='none' stroke='white' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'><path d='M21 9V6a2 2 0 0 0-2-2H5a2 2 0 0 0-2 2v10c0 1.1.9 2 2 2h4'/><rect width='10' height='7' x='12' y='13' rx='2'/></svg>"";
b.onclick=function(){try{window.chrome.webview.postMessage('pip:'+location.href);}catch(e){}};
document.body.appendChild(b);
window.__scPipUpdate=function(url){try{var show=false;
try{show=/(^|\.)soundcloud\.com$/.test(location.hostname)&&/^\/[^\/]+\/[^\/]+/.test(location.pathname)&&!/^\/(charts|search|you|discover|feed|library|settings|notifications|messages|upload|popular|terms|pages)(\/|$)/.test(location.pathname);}catch(e){}
b.style.display=show?'flex':'none';}catch(e){}};
window.__scPipUpdate(location.href);})()";

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private bool _initialized;
        private bool _polling;
        private bool _hasTitle;
        private AppState _state;
        private PipWindow _pip;
        private string _pipUrl = "";
        private SettingsWindow _settingsWin;
        private double _lastVolume = 0.8;
        private IntPtr _hwnd = IntPtr.Zero;
        private HwndSource _hwndSource;
        private readonly ISnackbarService _snackbar;
        private readonly IContentDialogService _dialogs;
        private readonly DispatcherTimer _poll = new DispatcherTimer();

        public AppState State { get { return _state; } }

        public MainWindow()
        {
            InitializeComponent();

            bool tampered;
            _state = SecureStore.Load(out tampered);
            Loc.Current = Loc.Resolve(_state.Lang);

            _snackbar = new SnackbarService();
            _snackbar.SetSnackbarPresenter(SnackbarPresenter);
            _dialogs = new ContentDialogService();
            _dialogs.SetDialogHost(DialogHost);

            ApplyStateToUi();
            ApplyLoc();

            _initialized = true;

            AdBlock.Enabled = _state.AdBlockOn;
            AdBlock.Strict = _state.AdStrict;
            Themes.Apply(_state.Theme);
            Topmost = _state.Topmost;
            SetupTray();
            ApplyAutostart();
            Loaded += delegate { FadeIn(this, 350); };

            if (tampered)
                Snack(Loc.Get("ErrTitle"), Loc.Get("VaultBroken"),
                    UiControls.ControlAppearance.Caution);

            _poll.Interval = TimeSpan.FromSeconds(1);
            _poll.Tick += Poll_Tick;
            _poll.Start();

            InitBrowser();
        }

        // ---------- Состояние -> UI ----------

        private void ApplyStateToUi()
        {
            VolSlider.Value = _state.Volume * 100;
            VolLabel.Text = ((int)Math.Round(_state.Volume * 100)) + "%";
            _lastVolume = _state.Volume > 0 ? _state.Volume : 0.8;
            MuteSwitch.IsChecked = _state.Muted;
            UpdateVolIcon();
            PlaylistBox.Text = _state.PlaylistUrl;
            AddrBox.Text = _state.LastUrl;
            ApplySidebar();
        }

        private void ApplySidebar()
        {
            SidebarCol.Width = GridLength.Auto;
            if (!_state.SidebarOpen)
            {
                SidebarView.Width = 0;
                SidebarView.Visibility = Visibility.Collapsed;
            }
            else
            {
                SidebarView.Width = 240;
                SidebarView.Visibility = Visibility.Visible;
            }
        }

        private void AnimateSidebar()
        {
            SidebarCol.Width = GridLength.Auto;
            double from = SidebarView.Width;
            if (_state.SidebarOpen)
            {
                SidebarView.Visibility = Visibility.Visible;
                if (double.IsNaN(from)) from = 0;
                var anim = new System.Windows.Media.Animation.DoubleAnimation(
                    from, 240, new Duration(TimeSpan.FromMilliseconds(220)));
                anim.EasingFunction = new System.Windows.Media.Animation.QuadraticEase();
                SidebarView.BeginAnimation(FrameworkElement.WidthProperty, anim);
            }
            else
            {
                if (double.IsNaN(from)) from = 240;
                var anim = new System.Windows.Media.Animation.DoubleAnimation(
                    from, 0, new Duration(TimeSpan.FromMilliseconds(220)));
                anim.EasingFunction = new System.Windows.Media.Animation.QuadraticEase();
                anim.Completed += delegate { SidebarView.Visibility = Visibility.Collapsed; };
                SidebarView.BeginAnimation(FrameworkElement.WidthProperty, anim);
            }
        }

        private void FadeIn(UIElement el, int ms)
        {
            el.Opacity = 0;
            var anim = new System.Windows.Media.Animation.DoubleAnimation(
                0, 1, new Duration(TimeSpan.FromMilliseconds(ms)));
            el.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        // ---------- Локализация ----------

        public void ApplyLocPublic() { ApplyLoc(); }

        private void ApplyLoc()
        {
            BtnMenu.ToolTip = Loc.Get("MenuTip");
            BtnBack.ToolTip = Loc.Get("TipBack");
            BtnFwd.ToolTip = Loc.Get("TipFwd");
            BtnReload.ToolTip = Loc.Get("TipReload");
            BtnHome.ToolTip = HomeUrl;
            AddrBox.PlaceholderText = Loc.Get("AddrPh");
            BtnGo.Content = Loc.Get("Go");
            BtnPipHere.ToolTip = Loc.Get("TipPipHere");
            BtnSettings.ToolTip = Loc.Get("SettingsTitle");
            NavHomeTxt.Text = Loc.Get("NavHome");
            NavChartsTxt.Text = Loc.Get("NavCharts");
            NavLikesTxt.Text = Loc.Get("NavLikes");
            PipHeader.Text = Loc.Get("PipGroup");
            PlaylistBox.PlaceholderText = Loc.Get("PlaylistPh");
            BtnPipPlay.Content = Loc.Get("PipPlay");
            BtnPipOpen.Content = Loc.Get("PipOpen");
            PipHint.Text = Loc.Get("PipHint");
            BtnCopy.ToolTip = Loc.Get("CopyLink");
            BtnExt.ToolTip = Loc.Get("OpenExt");
            MuteSwitch.Content = Loc.Get("Mute");
            BuildTrayMenu();
            if (!_hasTitle) NowPlaying.Text = Loc.Get("IdleTrack");
        }

        // ---------- Хранилище (молча) ----------

        public void SaveAllState()
        {
            _state.Volume = VolSlider.Value / 100.0;
            _state.Muted = MuteSwitch.IsChecked == true;
            _state.PlaylistUrl = PlaylistBox.Text != null ? PlaylistBox.Text.Trim() : "";
            SecureStore.Save(_state);
        }

        public void ResetAllSettings()
        {
            string langKept = _state.Lang;
            _state = new AppState();
            _state.Lang = langKept;
            Loc.Current = Loc.Resolve(_state.Lang);
            AdBlock.Enabled = _state.AdBlockOn;
            AdBlock.Strict = _state.AdStrict;
            AdBlock.Enabled = _state.AdBlockOn;
            AdBlock.Strict = _state.AdStrict;
            ApplyStateToUi();
            ApplyLoc();
            BuildTrayMenu();
            ApplyThemeNow();
            Topmost = _state.Topmost;
            ApplyAutostart();
            RefreshHotkeys();
            FireJs(SiteCssJs(_state.HideHeader));
            NavigateSmart(HomeUrl);
            SaveAllState();
        }

        private bool _allowExit;

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (_state.TrayHide && !_allowExit)
                {
                    e.Cancel = true;
                    Hide();
                    return;
                }
                for (int id = 1; id <= 5; id++)
                    if (_hwnd != IntPtr.Zero) UnregisterHotKey(_hwnd, id);
                if (_tray != null) _tray.Visible = false;
                if (Browser != null && Browser.Source != null)
                    _state.LastUrl = Browser.Source.ToString();
                SaveAllState();
            }
            catch { }
        }

        // ---------- Трей и автозапуск ----------

        private System.Windows.Forms.NotifyIcon _tray;

        private void SetupTray()
        {
            _tray = new System.Windows.Forms.NotifyIcon();
            try
            {
                _tray.Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                    Process.GetCurrentProcess().MainModule.FileName);
            }
            catch { }
            _tray.Text = "SoundCloud Desktop Beta";
            _tray.DoubleClick += delegate { ShowMain(); };
            BuildTrayMenu();
            _tray.Visible = true;
        }

        private void BuildTrayMenu()
        {
            if (_tray == null) return;
            var menu = new System.Windows.Forms.ContextMenu();
            var open = new System.Windows.Forms.MenuItem(Loc.Get("TrayOpen"));
            open.Click += delegate { ShowMain(); };
            var exit = new System.Windows.Forms.MenuItem(Loc.Get("TrayExit"));
            exit.Click += delegate { _allowExit = true; Close(); };
            menu.MenuItems.Add(open);
            menu.MenuItems.Add(exit);
            _tray.ContextMenu = menu;
        }

        private void ShowMain()
        {
            try
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            }
            catch { }
        }

        public void ApplyAutostart()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key == null) return;
                    if (_state.Autostart)
                        key.SetValue("SoundCloudDesktopBeta", "\""
                            + Process.GetCurrentProcess().MainModule.FileName + "\"");
                    else
                        key.DeleteValue("SoundCloudDesktopBeta", false);
                }
            }
            catch { }
        }

        // ---------- Глобальные хоткеи ----------

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _hwnd = new WindowInteropHelper(this).Handle;
            _hwndSource = HwndSource.FromHwnd(_hwnd);
            if (_hwndSource != null)
            {
                _hwndSource.AddHook(WndProc);
                RefreshHotkeys();
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                handled = true;
                OnHotkey(wParam.ToInt32());
            }
            return IntPtr.Zero;
        }

        public void RefreshHotkeys()
        {
            if (_hwnd == IntPtr.Zero) return;
            int[] vks = new int[] { _state.HotPrev, _state.HotPlay, _state.HotNext, _state.HotVolDn, _state.HotVolUp };
            for (int id = 1; id <= 5; id++)
            {
                UnregisterHotKey(_hwnd, id);
                if (vks[id - 1] != 0)
                    RegisterHotKey(_hwnd, id, 0, (uint)vks[id - 1]);
            }
        }

        private async void OnHotkey(int id)
        {
            if (!_initialized) return;
            if (id == 1)
            {
                if (await ClickPlayerAsync(JsPrev) != "no-btn") await UpdateTitleAsync();
            }
            else if (id == 2)
            {
                string r = await ClickPlayerAsync(JsToggle);
                if (r == "playing" || r == "paused") await UpdateTitleAsync();
            }
            else if (id == 3)
            {
                if (await ClickPlayerAsync(JsNext) != "no-btn") await UpdateTitleAsync();
            }
            else if (id == 4)
            {
                VolSlider.Value = Math.Max(0, VolSlider.Value - 5);
            }
            else if (id == 5)
            {
                VolSlider.Value = Math.Min(100, VolSlider.Value + 5);
            }
        }

        // ---------- Браузер ----------

        private async void InitBrowser()
        {
            try
            {
                string dir = Path.Combine(SecureStore.DataDir, "EBWebView");
                var env = await CoreWebView2Environment.CreateAsync(null, dir);
                await Browser.EnsureCoreWebView2Async(env);

                var settings = Browser.CoreWebView2.Settings;
                settings.AreDevToolsEnabled = false;
                settings.IsStatusBarEnabled = false;
                settings.IsGeneralAutofillEnabled = false;
                settings.IsPasswordAutosaveEnabled = false;

                try
                {
                    Browser.CoreWebView2.Profile.PreferredColorScheme = Themes.IsDark(_state.Theme)
                        ? CoreWebView2PreferredColorScheme.Dark
                        : CoreWebView2PreferredColorScheme.Light;
                }
                catch { }

                AdBlock.Attach(Browser.CoreWebView2);
                Browser.CoreWebView2.NavigationStarting += Browser_NavigationStarting;
                Browser.SourceChanged += Browser_SourceChanged;
                Browser.NavigationCompleted += Browser_NavigationCompleted;
                Browser.WebMessageReceived += Browser_WebMessageReceived;

                ApplyVolumeNow();
                NavigateSmart(_state.LastUrl);
            }
            catch (Exception ex)
            {
                try
                {
                    if (!Directory.Exists(SecureStore.DataDir)) Directory.CreateDirectory(SecureStore.DataDir);
                    File.WriteAllText(Path.Combine(SecureStore.DataDir, "webview2-error.log"),
                        DateTime.Now + "\r\n" + ex + "\r\n");
                }
                catch { }
                await _dialogs.ShowAlertAsync("WebView2", Loc.Get("WebMsg") + "\n" + ex.Message,
                    "OK", CancellationToken.None);
            }
        }

        private void Browser_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            string uri = e.Uri ?? "";
            if (uri.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || uri.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || uri.StartsWith("about:", StringComparison.OrdinalIgnoreCase)
                || uri.StartsWith("edge-chromium-", StringComparison.OrdinalIgnoreCase))
                return;

            e.Cancel = true;
            try { Process.Start(uri); }
            catch { }
        }

        private void Browser_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            if (!_initialized || Browser.Source == null) return;
            AddrBox.Text = Browser.Source.ToString();
            UpdateNavButtons();
        }

        private async void Browser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!_initialized) return;
            UpdateNavButtons();
            await RunJs(JsPipButton);
            await RunJs(SiteCssJs(_state.HideHeader));
            await UpdateTitleAsync();
            await ApplyVolumeAsync();
        }

        private void Browser_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string msg = "";
            try { msg = e.TryGetWebMessageAsString(); }
            catch { return; }
            if (msg != null && msg.StartsWith("pip:", StringComparison.Ordinal))
            {
                string url = msg.Substring(4);
                Dispatcher.BeginInvoke(new Action(delegate { OpenPipForUrl(url, true); }));
            }
        }

        private static string SiteCssJs(bool hide)
        {
            return "(function(){var s=document.getElementById('__scNative');"
                + "if(!s){s=document.createElement('style');s.id='__scNative';"
                + "(document.head||document.documentElement).appendChild(s);}"
                + "s.textContent=" + (hide ? "'header.header{display:none!important;}'" : "''") + ";})()";
        }

        private void UpdateNavButtons()
        {
            if (Browser == null || Browser.CoreWebView2 == null) return;
            BtnBack.IsEnabled = Browser.CanGoBack;
            BtnFwd.IsEnabled = Browser.CanGoForward;
        }

        // ---------- Навигация ----------

        private void NavigateSmart(string text)
        {
            if (Browser == null || Browser.CoreWebView2 == null) return;
            text = (text ?? "").Trim();
            string url;
            if (text.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || text.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                url = text;
            else if (text.Length == 0)
                url = HomeUrl;
            else if (!text.Contains(" ") && text.Contains("."))
                url = "https://" + text;
            else
                url = "https://soundcloud.com/search/sounds?q=" + Uri.EscapeDataString(text);

            try { Browser.Source = new Uri(url); }
            catch { Snack(Loc.Get("ErrTitle"), url, UiControls.ControlAppearance.Caution); }
        }

        private void AddrBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) NavigateSmart(AddrBox.Text);
        }

        private void BtnGo_Click(object sender, RoutedEventArgs e) { NavigateSmart(AddrBox.Text); }
        private void BtnBack_Click(object sender, RoutedEventArgs e) { if (Browser.CanGoBack) Browser.GoBack(); }
        private void BtnFwd_Click(object sender, RoutedEventArgs e) { if (Browser.CanGoForward) Browser.GoForward(); }
        private void BtnReload_Click(object sender, RoutedEventArgs e) { Browser.Reload(); }
        private void BtnHome_Click(object sender, RoutedEventArgs e) { NavigateSmart(HomeUrl); }
        private void BtnCharts_Click(object sender, RoutedEventArgs e) { NavigateSmart(ChartsUrl); }
        private void BtnLikes_Click(object sender, RoutedEventArgs e) { NavigateSmart(LikesUrl); }

        private void BtnMenu_Click(object sender, RoutedEventArgs e)
        {
            _state.SidebarOpen = !_state.SidebarOpen;
            AnimateSidebar();
            SaveAllState();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            try { if (_settingsWin != null) _settingsWin.Close(); }
            catch { }
            _settingsWin = new SettingsWindow(this);
            _settingsWin.Owner = this;
            _settingsWin.Show();
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            string url = Browser.Source != null ? Browser.Source.ToString() : "";
            if (url.Length == 0) return;
            try
            {
                Clipboard.SetText(url);
                Snack(Loc.Get("CopyLink"), Loc.Get("LinkCopied"),
                    UiControls.ControlAppearance.Success);
            }
            catch { }
        }

        private void BtnExt_Click(object sender, RoutedEventArgs e)
        {
            string url = Browser.Source != null ? Browser.Source.ToString() : "";
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return;
            try { Process.Start(url); }
            catch { }
        }

        // ---------- Звук: IsMuted + layered JS ----------

        private async Task<string> RunJs(string script)
        {
            try
            {
                if (Browser == null || Browser.CoreWebView2 == null) return "no-webview";
                return await Browser.ExecuteScriptAsync(script);
            }
            catch (Exception ex) { return "err:" + ex.Message; }
        }

        private async void FireJs(string script)
        {
            try
            {
                if (Browser == null || Browser.CoreWebView2 == null) return;
                await Browser.ExecuteScriptAsync(script);
            }
            catch { }
        }

        private static string Unquote(string json)
        {
            if (string.IsNullOrEmpty(json)) return "";
            json = json.Trim();
            if (json.Length >= 2 && json[0] == '"' && json[json.Length - 1] == '"')
            {
                json = json.Substring(1, json.Length - 2);
                json = json.Replace("\\\"", "\"").Replace("\\\\", "\\")
                           .Replace("\\n", " ").Replace("\\u0026", "&");
            }
            return json;
        }

        private static string SafeUnescape(string s)
        {
            try { return Uri.UnescapeDataString(s ?? ""); }
            catch { return s ?? ""; }
        }

        private static string FmtTime(string s)
        {
            int sec;
            if (!int.TryParse(s, out sec) || sec < 0) sec = 0;
            int h = sec / 3600, m = (sec % 3600) / 60, ss = sec % 60;
            if (h > 0) return h + ":" + m.ToString("D2") + ":" + ss.ToString("D2");
            return m + ":" + ss.ToString("D2");
        }

        // SoundCloud volume is a custom vertical div slider, not an <input>.
        // Set media elements + synthesize pointer events on .volume__sliderWrapper,
        // then verify via aria-valuenow and retry. Mute is enforced by the engine too.
        private static string VolumeJs(double v)
        {
            return @"(function(v){
window.__scVol=v;var n=0;
try{document.querySelectorAll('audio,video').forEach(function(m){try{m.volume=v;n++;}catch(e){}});}catch(e){}
function setSlider(val){
var w=document.querySelector('.volume__sliderWrapper');if(!w)return 'no-slider';
var t=w.querySelector('.volume__sliderBackground')||w;
var r=t.getBoundingClientRect();if(!r||r.height<2)return 'no-rect';
var x=r.left+r.width/2,y=r.top+r.height*(1-val);
function ev(type,el){try{el.dispatchEvent(new PointerEvent(type,{bubbles:true,cancelable:true,clientX:x,clientY:y,pointerId:1,isPrimary:true,buttons:1}));}catch(e){try{var m=document.createEvent('MouseEvents');m.initMouseEvent(type,true,true,window,1,x,y,x,y,false,false,false,false,0,null);el.dispatchEvent(m);}catch(e2){}}}
ev('pointerover',w);ev('pointerenter',w);ev('pointerdown',w);ev('pointermove',document);ev('pointerup',document);
ev('mousedown',w);ev('mouseup',document);ev('click',w);
return 'ok';}
var r1=setSlider(v);
try{if(!window.__scVolObs){window.__scVolObs=new MutationObserver(function(muts){muts.forEach(function(mu){if(!mu.addedNodes)return;for(var i=0;i<mu.addedNodes.length;i++){var nd=mu.addedNodes[i];if(!nd||!nd.querySelectorAll)continue;try{nd.querySelectorAll('audio,video').forEach(function(m){try{m.volume=window.__scVol;}catch(e){}});}catch(e){}}});});window.__scVolObs.observe(document.documentElement,{childList:true,subtree:true});}}catch(e){}
var cur='?';try{var w2=document.querySelector('.volume__sliderWrapper');if(w2)cur=w2.getAttribute('aria-valuenow');}catch(e){}
return n+'|'+r1+'|'+cur;})(" + v.ToString(CultureInfo.InvariantCulture) + ")";
        }

        private void ApplyVolumeNow()
        {
            try
            {
                if (Browser != null && Browser.CoreWebView2 != null)
                    Browser.CoreWebView2.IsMuted = MuteSwitch.IsChecked == true;
            }
            catch { }
            double v = MuteSwitch.IsChecked == true ? 0 : VolSlider.Value / 100.0;
            FireJs(VolumeJs(v));
        }

        private async Task ApplyVolumeAsync()
        {
            bool muted = MuteSwitch.IsChecked == true;
            try
            {
                if (Browser != null && Browser.CoreWebView2 != null)
                    Browser.CoreWebView2.IsMuted = muted;
            }
            catch { }
            double v = muted ? 0 : VolSlider.Value / 100.0;
            for (int i = 0; i < 3; i++)
            {
                string[] p = Unquote(await RunJs(VolumeJs(v))).Split('|');
                if (p.Length >= 3)
                {
                    double cur;
                    if (double.TryParse(p[2], NumberStyles.Float, CultureInfo.InvariantCulture, out cur)
                        && Math.Abs(cur - v) < 0.09) return;
                    if (p[1] == "no-slider" && p[0] != "0") return;
                }
                await Task.Delay(250);
            }
        }

        private async Task UpdateTitleAsync()
        {
            string title = Unquote(await RunJs("document.title"));
            if (!string.IsNullOrWhiteSpace(title) && title != "null")
            {
                _hasTitle = true;
                NowPlaying.Text = title;
            }
        }

        private async void Poll_Tick(object sender, EventArgs e)
        {
            if (_polling || !_initialized || Browser == null || Browser.CoreWebView2 == null) return;
            _polling = true;
            try
            {
                string[] p = Unquote(await RunJs(JsPoll)).Split('|');
                if (p.Length >= 3)
                {
                    string title = SafeUnescape(p[2]);
                    if (!string.IsNullOrWhiteSpace(title) && title != "null")
                    {
                        _hasTitle = true;
                        string time = FmtTime(p[0]) + " / " + FmtTime(p[1]);
                        NowPlaying.Text = (p[1] != "0" && p[1].Length > 0 ? time + "  •  " : "") + title;
                    }
                    double v = MuteSwitch.IsChecked == true ? 0 : VolSlider.Value / 100.0;
                    FireJs(VolumeJs(v));
                }
            }
            catch { }
            _polling = false;
        }

        private async Task<string> ClickPlayerAsync(string js)
        {
            for (int i = 0; i < 3; i++)
            {
                string r = Unquote(await RunJs(js));
                if (r == "playing" || r == "paused" || r == "ok") return r;
                await Task.Delay(300);
            }
            return "no-btn";
        }

        private async void VolSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_initialized || VolSlider == null || VolLabel == null) return;
            int pct = (int)Math.Round(VolSlider.Value);
            VolLabel.Text = pct + "%";
            if (VolSlider.Value > 0 && MuteSwitch != null && MuteSwitch.IsChecked == true)
                MuteSwitch.IsChecked = false;
            if (VolSlider.Value > 0) _lastVolume = VolSlider.Value / 100.0;
            await ApplyVolumeAsync();
        }

        private async void MuteSwitch_Changed(object sender, RoutedEventArgs e)
        {
            if (!_initialized || MuteSwitch == null) return;
            if (MuteSwitch.IsChecked == true)
            {
                if (VolSlider.Value > 0) _lastVolume = VolSlider.Value / 100.0;
                if (_lastVolume <= 0) _lastVolume = 0.8;
            }
            else
            {
                VolSlider.Value = _lastVolume * 100;
            }
            UpdateVolIcon();
            await ApplyVolumeAsync();
        }

        private void UpdateVolIcon()
        {
            if (VolIcon == null) return;
            bool muted = MuteSwitch != null && MuteSwitch.IsChecked == true;
            try { VolIcon.Data = (System.Windows.Media.Geometry)FindResource(muted ? "LucideVolX" : "LucideVol"); }
            catch { }
        }

        public void ReapplySiteCss()
        {
            FireJs(SiteCssJs(_state.HideHeader));
        }

        public void ApplyThemeNow()
        {
            Themes.Apply(_state.Theme);
            try
            {
                if (Browser != null && Browser.CoreWebView2 != null)
                    Browser.CoreWebView2.Profile.PreferredColorScheme = Themes.IsDark(_state.Theme)
                        ? CoreWebView2PreferredColorScheme.Dark
                        : CoreWebView2PreferredColorScheme.Light;
            }
            catch { }
        }

        public async Task<bool> ClearCacheAsync()
        {
            try
            {
                if (Browser == null || Browser.CoreWebView2 == null) return false;
                await Browser.CoreWebView2.Profile.ClearBrowsingDataAsync();
                return true;
            }
            catch { return false; }
        }

        // ---------- CPU: сворачивание ----------

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (Browser == null) return;
            if (WindowState == WindowState.Minimized)
            {
                Browser.Visibility = Visibility.Collapsed; // страница не рисуется, звук идёт
                _poll.Interval = TimeSpan.FromSeconds(3);
            }
            else
            {
                Browser.Visibility = Visibility.Visible;
                _poll.Interval = TimeSpan.FromSeconds(1);
            }
        }

        // ---------- PiP ----------

        private void BtnPipPlay_Click(object sender, RoutedEventArgs e) { OpenPipForUrl(PlaylistOrCurrent(), true); }
        private void BtnPipOpen_Click(object sender, RoutedEventArgs e) { OpenPipForUrl(PlaylistOrCurrent(), false); }
        private void BtnPipHere_Click(object sender, RoutedEventArgs e) { OpenPipForUrl(PlaylistOrCurrent(), true); }

        private void PlaylistBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) OpenPipForUrl(PlaylistOrCurrent(), true);
        }

        private string PlaylistOrCurrent()
        {
            string url = PlaylistBox.Text != null ? PlaylistBox.Text.Trim() : "";
            if (url.Length == 0 && Browser.Source != null)
                url = Browser.Source.ToString();
            return url;
        }

        private void OpenPipForUrl(string url, bool autoplay)
        {
            if (string.IsNullOrEmpty(url) || !url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                Snack(Loc.Get("ErrTitle"), Loc.Get("NeedLink"),
                    UiControls.ControlAppearance.Caution);
                return;
            }

            try
            {
                // Second click on the same link toggles PiP closed.
                if (_pip != null && _pipUrl == url)
                {
                    _pip.Close();
                    _pip = null;
                    _pipUrl = "";
                    return;
                }
                if (_pip != null) _pip.Close();
            }
            catch { }

            _state.PlaylistUrl = url;
            SaveAllState();

            _pip = new PipWindow(url, autoplay);
            _pipUrl = url;
            _pip.Owner = this;
            _pip.Closed += delegate
            {
                if (_pipUrl == url) { _pip = null; _pipUrl = ""; }
            };
            var area = SystemParameters.WorkArea;
            _pip.Left = area.Right - _pip.Width - 16;
            _pip.Top = area.Bottom - _pip.Height - 16;
            _pip.Show();
        }

        // ---------- Мелочь ----------

        private void Snack(string title, string message, UiControls.ControlAppearance appearance)
        {
            try { _snackbar.Show(title, message, appearance, TimeSpan.FromSeconds(3)); }
            catch { }
        }
    }
}
