using System;
using System.Windows;
using System.Windows.Controls;
using UiControls = Wpf.Ui.Controls;
namespace WpfApp1
{
    public partial class SettingsWindow : UiControls.FluentWindow
    {
        private readonly MainWindow _owner;
        private readonly AppState _state;
        private bool _initialized;
        public SettingsWindow(MainWindow owner)
        {
            InitializeComponent();
            _owner = owner;
            _state = owner.State;
            RefreshFromState();
            ApplyLoc();
            _initialized = true;
            Loaded += delegate
            {
                Opacity = 0;
                BeginAnimation(OpacityProperty,
                    new System.Windows.Media.Animation.DoubleAnimation(
                        0, 1, new Duration(TimeSpan.FromMilliseconds(250))));
            };
        }
        private void RefreshFromState()
        {
            Loc.FillLangCombo(LangBox2, _state.Lang);
            ThemeBox.Items.Clear();
            for (int i = 0; i < Themes.Count; i++)
                ThemeBox.Items.Add(Loc.Get("Theme" + i));
            ThemeBox.SelectedIndex = _state.Theme >= 0 && _state.Theme < Themes.Count ? _state.Theme : 0;
            EngineBox.Items.Clear();
            for (int i = 0; i <= 3; i++)
            {
                string name = Loc.Get(Engines.NameKey(i));
                if (i != 0 && !Engines.IsAvailable(i)) name += Loc.Get("EngineMissing");
                EngineBox.Items.Add(name);
            }
            EngineBox.SelectedIndex = _state.Engine >= 0 && _state.Engine <= 3 ? _state.Engine : 0;
            RefreshEngineStatus();
            HideHeaderSwitch2.IsChecked = _state.HideHeader;
            TopmostSwitch.IsChecked = _state.Topmost;
            TrayHideSwitch.IsChecked = _state.TrayHide;
            AutostartSwitch.IsChecked = _state.Autostart;
            AdBlockSwitch.IsChecked = _state.AdBlockOn;
            StrictSwitch.IsChecked = _state.AdStrict;
            Loc.FillKeyCombo(HotPrevBox, _state.HotPrev);
            Loc.FillKeyCombo(HotPlayBox, _state.HotPlay);
            Loc.FillKeyCombo(HotNextBox, _state.HotNext);
            Loc.FillKeyCombo(HotVolDnBox, _state.HotVolDn);
            Loc.FillKeyCombo(HotVolUpBox, _state.HotVolUp);
        }
        private bool _applyingLoc;
        private void ApplyLoc()
        {
            if (_applyingLoc) return;
            _applyingLoc = true;
            try
            {
                RefillLang();
                RefillThemes();
                RefillEngines();
                RefillKeys(HotPrevBox, _state.HotPrev);
                RefillKeys(HotPlayBox, _state.HotPlay);
                RefillKeys(HotNextBox, _state.HotNext);
                RefillKeys(HotVolDnBox, _state.HotVolDn);
                RefillKeys(HotVolUpBox, _state.HotVolUp);
                ApplyLocTexts();
            }
            finally { _applyingLoc = false; }
        }
        private void RefillLang()
        {
            int s = LangBox2.SelectedIndex;
            Loc.FillLangCombo(LangBox2, _state.Lang);
            if (s >= 0 && s < LangBox2.Items.Count) LangBox2.SelectedIndex = s;
        }
        private void RefillThemes()
        {
            int s = ThemeBox.SelectedIndex;
            ThemeBox.Items.Clear();
            for (int i = 0; i < Themes.Count; i++)
                ThemeBox.Items.Add(Loc.Get("Theme" + i));
            if (s >= 0 && s < ThemeBox.Items.Count) ThemeBox.SelectedIndex = s;
        }
        private void RefillEngines()
        {
            int s = EngineBox.SelectedIndex;
            EngineBox.Items.Clear();
            for (int i = 0; i <= 3; i++)
            {
                string name = Loc.Get(Engines.NameKey(i));
                if (i != 0 && !Engines.IsAvailable(i)) name += Loc.Get("EngineMissing");
                EngineBox.Items.Add(name);
            }
            if (s >= 0 && s < EngineBox.Items.Count) EngineBox.SelectedIndex = s;
        }
        private void RefillKeys(ComboBox box, int cur)
        {
            int s = box.SelectedIndex;
            Loc.FillKeyCombo(box, cur);
            if (s >= 0 && s < box.Items.Count) box.SelectedIndex = s;
        }
        private void ApplyLocTexts()
        {
            Title = Loc.Get("SettingsTitle");
            SetBar.Title = Loc.Get("SettingsTitle");
            LangLabel2.Text = Loc.Get("LangLabel");
            LangBox2.Items[0] = Loc.Get("LangAuto");
            ThemeLabel2.Text = Loc.Get("ThemeLabel");
            EngineLabel2.Text = Loc.Get("EngineLabel");
            RefreshEngineStatus();
            HideHeaderSwitch2.Content = Loc.Get("HideHeader");
            TopmostSwitch.Content = Loc.Get("TopmostMain");
            TrayHideSwitch.Content = Loc.Get("TrayHide");
            AutostartSwitch.Content = Loc.Get("Autostart");
            AdBlockSwitch.Content = Loc.Get("AdBlockLbl");
            StrictSwitch.Content = Loc.Get("StrictBlock");
            StrictHint.Text = Loc.Get("StrictHint");
            BindsHeader.Text = Loc.Get("BindsGroup");
            HotPrevLbl.Text = Loc.Get("HotPrev");
            HotPlayLbl.Text = Loc.Get("HotPlay");
            HotNextLbl.Text = Loc.Get("HotNext");
            HotVolDnLbl.Text = Loc.Get("HotVolDn");
            HotVolUpLbl.Text = Loc.Get("HotVolUp");
            HotHint.Text = Loc.Get("HotHint");
            DefaultsBtn.Content = Loc.Get("DefaultsBtn");
            CacheBtn.Content = Loc.Get("CacheBtn");
            VaultResetBtn.Content = Loc.Get("VaultResetBtn");
        }
        private void LangBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized || LangBox2.SelectedIndex < 0) return;
            _state.Lang = Loc.PrefCodes[LangBox2.SelectedIndex];
            Loc.Current = Loc.Resolve(_state.Lang);
            _owner.SaveAllState();
            _owner.ApplyLocPublic();
            ApplyLoc();
        }
        private void ThemeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized || ThemeBox.SelectedIndex < 0) return;
            _state.Theme = ThemeBox.SelectedIndex;
            _owner.ApplyThemeNow();
            _owner.SaveAllState();
        }
        private void EngineBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized || EngineBox.SelectedIndex < 0) return;
            int want = EngineBox.SelectedIndex;
            if (want == 3)
            {
                EngineBox.SelectedIndex = _state.Engine;
                System.Windows.MessageBox.Show(Loc.Get("FirefoxSoon"),
                    Loc.Get("SettingsTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (want != _state.Engine)
            {
                _state.Engine = want;
                _owner.SaveAllState();
                _owner.RestartApp();
            }
        }
        private void RefreshEngineStatus()
        {
            int cur = EngineBox.SelectedIndex < 0 ? _state.Engine : EngineBox.SelectedIndex;
            EngineDlBtn.Visibility = Visibility.Collapsed;
            if (cur == 0)
            {
                bool ok = Microsoft.Win32.Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\ClientState\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}",
                    "pv", null) != null;
                EngineStatus.Text = ok ? "WebView2 Runtime OK" : Loc.Get("SetupWebViewMsg");
                if (!ok)
                {
                    EngineDlBtn.Visibility = Visibility.Visible;
                    EngineDlBtn.Content = Loc.Get("EngineDl") + " WebView2";
                }
            }
            else if (cur == 1 || cur == 2)
            {
                string dir = Engines.ExeFolder(cur);
                if (dir == null)
                {
                    EngineStatus.Text = Loc.Get(Engines.NameKey(cur)) + Loc.Get("EngineMissing");
                    EngineDlBtn.Visibility = Visibility.Visible;
                    EngineDlBtn.Content = Loc.Get("EngineDl") + " " + (cur == 1 ? "Edge" : "Chrome");
                }
                else
                {
                    string ver = "";
                    try
                    {
                        string exe = System.IO.Path.Combine(dir, cur == 1 ? "msedge.exe" : "chrome.exe");
                        ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(exe).ProductVersion;
                    }
                    catch { }
                    EngineStatus.Text = Loc.Get(Engines.NameKey(cur)) + " " + ver;
                }
            }
            else
            {
                EngineStatus.Text = Loc.Get("EngineFirefox");
            }
        }
        private void EngineDlBtn_Click(object sender, RoutedEventArgs e)
        {
            int cur = EngineBox.SelectedIndex < 0 ? 0 : EngineBox.SelectedIndex;
            string url = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";
            if (cur == 1) url = "https://go.microsoft.com/fwlink/?linkid=2108834";
            else if (cur == 2) url = "https://dl.google.com/chrome/install/latest/chrome_installer.exe";
            try { System.Diagnostics.Process.Start(url); }
            catch { }
        }
        private void HideHeaderSwitch2_Changed(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _state.HideHeader = HideHeaderSwitch2.IsChecked == true;
            _owner.SaveAllState();
            _owner.ReapplySiteCss();
        }
        private void TopmostSwitch_Changed(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _state.Topmost = TopmostSwitch.IsChecked == true;
            _owner.Topmost = _state.Topmost;
            _owner.SaveAllState();
        }
        private void TrayHideSwitch_Changed(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _state.TrayHide = TrayHideSwitch.IsChecked == true;
            _owner.SaveAllState();
        }
        private void AutostartSwitch_Changed(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _state.Autostart = AutostartSwitch.IsChecked == true;
            _owner.ApplyAutostart();
            _owner.SaveAllState();
        }
        private void AdBlockSwitch_Changed(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _state.AdBlockOn = AdBlockSwitch.IsChecked == true;
            AdBlock.Enabled = _state.AdBlockOn;
            _owner.SaveAllState();
        }
        private void StrictSwitch_Changed(object sender, RoutedEventArgs e)
        {
            if (!_initialized) return;
            _state.AdStrict = StrictSwitch.IsChecked == true;
            AdBlock.Strict = _state.AdStrict;
            _owner.SaveAllState();
        }
        private void HotBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized) return;
            ComboBox box = sender as ComboBox;
            if (box == null || box.SelectedIndex < 0) return;
            int vk = Loc.KeyChoices[box.SelectedIndex];
            if (box == HotPrevBox) _state.HotPrev = vk;
            else if (box == HotPlayBox) _state.HotPlay = vk;
            else if (box == HotNextBox) _state.HotNext = vk;
            else if (box == HotVolDnBox) _state.HotVolDn = vk;
            else if (box == HotVolUpBox) _state.HotVolUp = vk;
            _owner.RefreshHotkeys();
            _owner.SaveAllState();
        }
        private void DefaultsBtn_Click(object sender, RoutedEventArgs e)
        {
            _state.HotPrev = 0x61;
            _state.HotPlay = 0x62;
            _state.HotNext = 0x63;
            _state.HotVolDn = 0x64;
            _state.HotVolUp = 0x65;
            Loc.FillKeyCombo(HotPrevBox, _state.HotPrev);
            Loc.FillKeyCombo(HotPlayBox, _state.HotPlay);
            Loc.FillKeyCombo(HotNextBox, _state.HotNext);
            Loc.FillKeyCombo(HotVolDnBox, _state.HotVolDn);
            Loc.FillKeyCombo(HotVolUpBox, _state.HotVolUp);
            _owner.RefreshHotkeys();
            _owner.SaveAllState();
        }
        private async void CacheBtn_Click(object sender, RoutedEventArgs e)
        {
            CacheStatus.Text = "...";
            bool ok = await _owner.ClearCacheAsync();
            CacheStatus.Text = ok ? Loc.Get("CacheDone") : Loc.Get("ErrTitle");
        }
        private void VaultResetBtn_Click(object sender, RoutedEventArgs e)
        {
            _owner.ResetAllSettings();
            RefreshFromState();
            ApplyLoc();
            CacheStatus.Text = Loc.Get("VaultResetDone");
        }
    }
}
