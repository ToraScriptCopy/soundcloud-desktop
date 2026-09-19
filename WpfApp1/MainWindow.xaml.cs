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
            "(function(){var t=0,d=0,playing=false,title='',art='',artist='';"
            + "try{var list=document.querySelectorAll('audio');"
            + "for(var i=0;i<list.length;i++){var m=list[i];"
            + "var dd=Math.floor(m.duration||0);"
            + "if(dd>d){d=dd;t=Math.floor(m.currentTime||0);}"
            + "if(!m.paused&&!m.ended&&m.currentTime>0)playing=true;}}catch(e){}"
            + "try{title=document.title||'';}catch(e){}"
            + "try{var ti=document.querySelector('.playbackSoundBadge__titleLink');"
            + "if(ti&&(ti.title||ti.textContent))title=ti.title||ti.textContent;}catch(e){}"
            + "try{var ar=document.querySelector('.playbackSoundBadge__lightLink');"
            + "if(ar&&(ar.title||ar.textContent))artist=ar.title||ar.textContent;}catch(e){}"
            + "try{var im=document.querySelector('.playbackSoundBadge__avatar img')||document.querySelector('.playControls__soundBadge img');"
            + "if(im&&im.src)art=im.src;}catch(e){}"
            + "return t+'|'+d+'|'+encodeURIComponent(title)+'|'+(playing?'1':'0')+'|'+encodeURIComponent(art)+'|'+encodeURIComponent(artist);})()";
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private bool _initialized;
        private bool _polling;
        private bool _hasTitle;
        private bool _wasPlaying;
        private string _lastTrackKey = "";
        private string _lpTitle = "";
        private string _lpArtist = "";
        private string _lpArt = "";
        private string _lpTime = "";
        private double _lastSentVol = -1;
        private bool _lastSentMute;
        private AppState _state;
        private PipWindow _pip;
        private string _pipUrl = "";
        private PlayerWindow _player;
        private CoreWebView2Environment _env;
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
            // --open-url= lets an external call (or a test) open a page directly.
            try
            {
                foreach (string a in Environment.GetCommandLineArgs())
                {
                    if (a.StartsWith("--open-url=", StringComparison.OrdinalIgnoreCase))
                    {
                        string u = a.Substring(11).Trim();
                        if (u.Length > 0) _state.LastUrl = u;
                    }
                }
            }
            catch { }
            // Migrate old vaults: the redesign used to be one toggle,
            // now it is flags. If it was on, turn every part on.
            if (_state.ReDesign && !(_state.RdCards || _state.RdButtons || _state.RdHeader
                || _state.RdPlayer || _state.RdComments || _state.RdSidebar
                || _state.RdInputs || _state.RdPopups))
            {
                _state.RdCards = _state.RdButtons = _state.RdHeader = _state.RdPlayer =
                    _state.RdComments = _state.RdSidebar = _state.RdInputs = _state.RdPopups = true;
            }
            Loc.Current = Loc.Resolve(_state.Lang);
            _snackbar = new SnackbarService();
            _snackbar.SetSnackbarPresenter(SnackbarPresenter);
            _dialogs = new ContentDialogService();
            _dialogs.SetDialogHost(DialogHost);
            ApplyStateToUi();
            ApplyLoc();
            _initialized = true;
            AdBlock.Enabled = _state.AdBlockOn;
            Themes.Apply(_state.Theme);
            Topmost = _state.Topmost;
            SetupTray();
            ApplyAutostart();
            Loaded += delegate { Fx.Fade(this, 260); };
            if (tampered)
                Snack(Loc.Get("ErrTitle"), Loc.Get("VaultBroken"),
                    UiControls.ControlAppearance.Caution);
            _poll.Interval = TimeSpan.FromMilliseconds(1500);
            _poll.Tick += Poll_Tick;
            _poll.Start();
            InitBrowser();
        }
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
                SidebarView.Width = 210;
                SidebarView.Visibility = Visibility.Visible;
            }
        }
        private void AnimateSidebar()
        {
            // Instant toggle plus slide and fade on the sidebar itself.
            // WebView2 is not inside, so it stays smooth.
            ApplySidebar();
            if (_state.SidebarOpen)
            {
                Fx.Fade(SidebarView, 180);
                Fx.SlideX(SidebarView, 200, -18);
            }
        }
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
            BtnPipMini.ToolTip = Loc.Get("TipPipHere");
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
            try { if (_pip != null) _pip.ApplyLocPublic(); }
            catch { }
            try { if (_player != null) _player.ApplyLocPublic(); }
            catch { }
            if (!_hasTitle) NowPlaying.Text = Loc.Get("IdleTrack");
        }
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
            ApplyStateToUi();
            ApplyLoc();
            BuildTrayMenu();
            ApplyThemeNow();
            Topmost = _state.Topmost;
            ApplyAutostart();
            RefreshHotkeys();
            FireJs(SiteCssJs());
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
            _tray.Text = "SoundCloud Desktop";
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
            var player = new System.Windows.Forms.MenuItem(Loc.Get("PlayerTitle"));
            player.Click += delegate { ShowPlayerWindow("", "", "", "", _wasPlaying); ShowMain(); };
            var exit = new System.Windows.Forms.MenuItem(Loc.Get("TrayExit"));
            exit.Click += delegate { _allowExit = true; Close(); };
            menu.MenuItems.Add(open);
            menu.MenuItems.Add(player);
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
        public void MinimizeToTray()
        {
            try
            {
                if (_player != null) _player.Hide();
                Hide();
                if (_tray != null)
                {
                    _tray.BalloonTipTitle = "SoundCloud Desktop";
                    _tray.BalloonTipText = Loc.Get("TrayHidden");
                    try { _tray.ShowBalloonTip(1500); }
                    catch { }
                }
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
            if (id == 1) await PrevAsync();
            else if (id == 2) await TogglePlayAsync();
            else if (id == 3) await NextAsync();
            else if (id == 4) VolSlider.Value = Math.Max(0, VolSlider.Value - 5);
            else if (id == 5) VolSlider.Value = Math.Min(100, VolSlider.Value + 5);
        }
        public async Task<string> TogglePlayAsync()
        {
            string r = await ClickPlayerAsync(JsToggle);
            if (r == "playing" || r == "paused") await UpdateTitleAsync();
            return r;
        }
        public async Task NextAsync()
        {
            if (await ClickPlayerAsync(JsNext) != "no-btn") await UpdateTitleAsync();
        }
        public async Task PrevAsync()
        {
            if (await ClickPlayerAsync(JsPrev) != "no-btn") await UpdateTitleAsync();
        }
        // Minimal flags so cookies, login and extensions keep working.
        private static readonly string EngineArgs =
            "--autoplay-policy=no-user-gesture-required"
            + " --disable-features=Translate,MediaRouter,OptimizationHints";
        private async void InitBrowser()
        {
            try
            {
                string dir = Path.Combine(SecureStore.DataDir, "EBWebView");
                var env = await CoreWebView2Environment.CreateAsync(
                    null, dir, new CoreWebView2EnvironmentOptions(EngineArgs));
                _env = env;
                await Browser.EnsureCoreWebView2Async(env);
                var settings = Browser.CoreWebView2.Settings;
                settings.AreDevToolsEnabled = false;
                settings.IsStatusBarEnabled = false;
                settings.IsGeneralAutofillEnabled = true;
                settings.IsPasswordAutosaveEnabled = true;
                try
                {
                    Browser.CoreWebView2.Profile.PreferredColorScheme = Themes.IsDark(_state.Theme)
                        ? CoreWebView2PreferredColorScheme.Dark
                        : CoreWebView2PreferredColorScheme.Light;
                }
                catch { }
                AdBlock.Attach(Browser.CoreWebView2);
                Browser.CoreWebView2.NavigationStarting += Browser_NavigationStarting;
                Browser.CoreWebView2.NewWindowRequested += Browser_NewWindowRequested;
                Browser.SourceChanged += Browser_SourceChanged;
                Browser.NavigationCompleted += Browser_NavigationCompleted;
                await LoadExtensionsAsync();
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
        private async Task LoadExtensionsAsync()
        {
            try
            {
                if (Browser == null || Browser.CoreWebView2 == null) return;
                if (_state.Extensions == null || _state.Extensions.Count == 0) return;
                foreach (string dir in _state.Extensions)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                            await Browser.CoreWebView2.Profile.AddBrowserExtensionAsync(dir);
                    }
                    catch { }
                }
            }
            catch { }
        }
        public async Task<bool> InstallExtensionAsync(string dir)
        {
            try
            {
                if (Browser == null || Browser.CoreWebView2 == null) return false;
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return false;
                await Browser.CoreWebView2.Profile.AddBrowserExtensionAsync(dir);
                if (_state.Extensions == null) _state.Extensions = new System.Collections.Generic.List<string>();
                bool exists = false;
                foreach (string d in _state.Extensions)
                    if (string.Equals(d, dir, StringComparison.OrdinalIgnoreCase)) exists = true;
                if (!exists) _state.Extensions.Add(dir);
                SaveAllState();
                return true;
            }
            catch { return false; }
        }
        private void Browser_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            // Every popup is a potential login window. Never block it.
            e.Handled = true;
            var deferral = e.GetDeferral();
            try
            {
                var auth = new AuthWindow();
                auth.Owner = this;
                auth.Show();
                auth.OpenPopupAsync(_env, e, deferral);
            }
            catch
            {
                try { deferral.Complete(); }
                catch { }
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
            await RunJs(SiteCssJs());
            FireJs(SiteExtras.PromoJs);
            await UpdateTitleAsync();
            _lastSentVol = -1;
            await ApplyVolumeAsync();
        }
        private string SiteCssJs()
        {
            string css = SiteExtras.BuildCss(_state, AdBlock.Enabled);
            return SiteExtras.ToJs(css);
        }
        private void UpdateNavButtons()
        {
            if (Browser == null || Browser.CoreWebView2 == null) return;
            BtnBack.IsEnabled = Browser.CanGoBack;
            BtnFwd.IsEnabled = Browser.CanGoForward;
        }
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
            _lastSentVol = v;
            _lastSentMute = MuteSwitch.IsChecked == true;
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
            if (Math.Abs(v - _lastSentVol) < 0.001 && muted == _lastSentMute) return;
            _lastSentVol = v;
            _lastSentMute = muted;
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
                if (p.Length >= 6)
                {
                    string title = SafeUnescape(p[2]);
                    bool playing = p[3] == "1";
                    string art = SafeUnescape(p[4]);
                    string artist = SafeUnescape(p[5]);
                    if (!string.IsNullOrWhiteSpace(title) && title != "null")
                    {
                        _hasTitle = true;
                        string time = FmtTime(p[0]) + " / " + FmtTime(p[1]);
                        NowPlaying.Text = (p[1] != "0" && p[1].Length > 0 ? time + "  -  " : "") + title;
                        _lpTitle = title;
                        _lpArtist = artist;
                        _lpArt = art;
                        _lpTime = time;
                    }
                    string key = title + "||" + artist;
                    if (playing && !string.IsNullOrWhiteSpace(title) && title != "null"
                        && _state.PlayerPopup && (!_wasPlaying || key != _lastTrackKey))
                        ShowPlayerWindow(title, artist, art, FmtTime(p[0]) + " / " + FmtTime(p[1]), true);
                    else if (_player != null && _player.IsVisible)
                        _player.UpdateInfo(title, artist, art, FmtTime(p[0]) + " / " + FmtTime(p[1]), playing);
                    _wasPlaying = playing;
                    _lastTrackKey = key;
                }
            }
            catch { }
            _polling = false;
        }
        private void ShowPlayerWindow(string title, string artist, string art, string time, bool playing)
        {
            try
            {
                if (_player == null)
                {
                    _player = new PlayerWindow(this);
                    _player.Closed += delegate { _player = null; };
                    var area = SystemParameters.WorkArea;
                    _player.Left = area.Right - _player.Width - 20;
                    _player.Top = area.Bottom - _player.Height - 20;
                }
                _player.UpdateInfo(title, artist, art, time, playing);
                if (!_player.IsVisible) _player.Show();
                else if (_player.WindowState == WindowState.Minimized) _player.WindowState = WindowState.Normal;
                try { _player.Activate(); }
                catch { }
            }
            catch { }
        }
        private void NowPlaying_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_lpTitle)) return;
                ShowPlayerWindow(_lpTitle, _lpArtist, _lpArt, _lpTime, _wasPlaying);
            }
            catch { }
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
            UpdateVolIcon();
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
        public void RestartApp()
        {
            try
            {
                SaveAllState();
                Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            }
            catch { }
            _allowExit = true;
            Close();
        }
        public void ReapplySiteCss()
        {
            FireJs(SiteCssJs());
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
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (Browser == null) return;
            if (WindowState == WindowState.Minimized)
            {
                Browser.Visibility = Visibility.Collapsed;
                _poll.Interval = TimeSpan.FromSeconds(3);
            }
            else
            {
                Browser.Visibility = Visibility.Visible;
                _poll.Interval = TimeSpan.FromMilliseconds(1500);
            }
        }
        private void BtnPipPlay_Click(object sender, RoutedEventArgs e) { OpenPipForUrl(PlaylistOrCurrent(), true); }
        private void BtnPipOpen_Click(object sender, RoutedEventArgs e) { OpenPipForUrl(PlaylistOrCurrent(), false); }
        private void BtnPipMini_Click(object sender, RoutedEventArgs e) { OpenPipForUrl(PlaylistOrCurrent(), true); }
        public void OpenPipForCurrent() { OpenPipForUrl(PlaylistOrCurrent(), true); }
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
        private void Snack(string title, string message, UiControls.ControlAppearance appearance)
        {
            try { _snackbar.Show(title, message, appearance, TimeSpan.FromSeconds(3)); }
            catch { }
        }
    }
}
