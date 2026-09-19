using System;
using System.Windows;
using System.Windows.Media.Imaging;
using UiControls = Wpf.Ui.Controls;
namespace WpfApp1
{
    public partial class PlayerWindow : UiControls.FluentWindow
    {
        private readonly MainWindow _main;
        private string _art = "";
        public PlayerWindow(MainWindow main)
        {
            InitializeComponent();
            _main = main;
            ApplyLocPublic();
            Loaded += delegate { Fx.Enter(this); };
        }
        public void ApplyLocPublic()
        {
            try
            {
                Title = Loc.Get("PlayerTitle");
                PlayerBar.Title = Loc.Get("PlayerTitle");
                BtnPrev.ToolTip = Loc.Get("TipPrev");
                BtnPlay.ToolTip = Loc.Get("TipPlay");
                BtnNext.ToolTip = Loc.Get("TipNext");
                BtnTray.ToolTip = Loc.Get("PlayerToTray");
                BtnPip.ToolTip = Loc.Get("TipPipHere");
                BtnClose.ToolTip = Loc.Get("TipClose");
            }
            catch { }
        }
        public void UpdateInfo(string title, string artist, string artworkUrl, string time, bool playing)
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    TrackTitle.Text = string.IsNullOrEmpty(title) ? Loc.Get("IdleTrack") : title;
                    TrackArtist.Text = artist ?? "";
                    TrackTime.Text = time ?? "";
                    try { PlayIcon.Symbol = playing ? Wpf.Ui.Controls.SymbolRegular.Pause24 : Wpf.Ui.Controls.SymbolRegular.Play24; }
                    catch { }
                    if (!string.IsNullOrEmpty(artworkUrl) && artworkUrl != _art)
                    {
                        _art = artworkUrl;
                        try
                        {
                            var bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.UriSource = new Uri(artworkUrl, UriKind.Absolute);
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.EndInit();
                            bmp.Freeze();
                            ArtImg.Source = bmp;
                        }
                        catch { ArtImg.Source = null; }
                    }
                    else if (string.IsNullOrEmpty(artworkUrl))
                    {
                        _art = "";
                        ArtImg.Source = null;
                    }
                }));
            }
            catch { }
        }
        private async void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            try { await _main.TogglePlayAsync(); }
            catch { }
        }
        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            try { await _main.NextAsync(); }
            catch { }
        }
        private async void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            try { await _main.PrevAsync(); }
            catch { }
        }
        private void BtnPip_Click(object sender, RoutedEventArgs e)
        {
            try { _main.OpenPipForCurrent(); }
            catch { }
        }
        private void BtnTray_Click(object sender, RoutedEventArgs e)
        {
            try { _main.MinimizeToTray(); }
            catch { }
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            try { Hide(); }
            catch { }
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Close button hides the window, app keeps playing in background.
            e.Cancel = true;
            try { Hide(); }
            catch { }
        }
    }
}
