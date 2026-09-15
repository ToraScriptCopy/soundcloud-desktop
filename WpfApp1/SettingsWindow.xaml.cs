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
                var fade = new System.Windows.Media.Animation.DoubleAnimation(
                    0, 1, new Duration(TimeSpan.FromMilliseconds(320)));
                fade.EasingFunction = new System.Windows.Media.Animation.CubicEase
                {
                    EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut
                };
                BeginAnimation(OpacityProperty, fade);
            };
        }
        private void RefreshFromState()
        {
            Loc.FillLangCombo(LangBox2, _state.Lang);
            ThemeBox.Items.Clear();
            for (int i = 0; i < Themes.Count; i++)
                ThemeBox.Items.Add(Loc.Get("Theme" + i));
            ThemeBox.SelectedIndex = _state.Theme >= 0 && _state.Theme < Themes.Count ? _state.Theme : 0;
            HideHeaderSwitch2.IsChecked = _state.HideHeader;
            TopmostSwitch.IsChecked = _state.Topmost;
            TrayHideSwitch.IsChecked = _state.TrayHide;
            AutostartSwitch.IsChecked = _state.Autostart;
            AdBlockSwitch.IsChecked = _state.AdBlockOn;
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
            HideHeaderSwitch2.Content = Loc.Get("HideHeader");
            TopmostSwitch.Content = Loc.Get("TopmostMain");
            TrayHideSwitch.Content = Loc.Get("TrayHide");
            AutostartSwitch.Content = Loc.Get("Autostart");
            AdBlockSwitch.Content = Loc.Get("AdBlockLbl");
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
