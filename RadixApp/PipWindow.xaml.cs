using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
namespace WpfApp1
{
    public partial class PipWindow : Wpf.Ui.Controls.FluentWindow
    {
        private const string PipAdCss =
            "(function(){var s=document.getElementById('__scPipAd');"
            + "if(!s){s=document.createElement('style');s.id='__scPipAd';"
            + "(document.head||document.documentElement).appendChild(s);}"
            + "s.textContent=\"[id*='adSlot']:not([class*='auth']):not([class*='login']){display:none!important;}\";})()";
        private readonly string _pageUrl;
        private readonly bool _autoplay;
        public PipWindow(string pageUrl, bool autoplay)
        {
            InitializeComponent();
            _pageUrl = pageUrl;
            _autoplay = autoplay;
            PipTitle.Text = Loc.Get("PipTitle");
            PipCloseItem.Header = Loc.Get("TipClose");
            Loaded += delegate { Fx.Fade(this, 200); };
            Loaded += PipWindow_Loaded;
        }
        public static string BuildWidgetUrl(string pageUrl, bool autoplay)
        {
            return "https://w.soundcloud.com/player/?url=" + Uri.EscapeDataString(pageUrl)
                + "&color=%23ff5500&auto_play=" + (autoplay ? "true" : "false")
                + "&hide_related=false&show_comments=true&show_user=true"
                + "&show_reposts=false&show_teaser=true&visual=true";
        }
        public void ApplyLocPublic()
        {
            PipTitle.Text = Loc.Get("PipTitle");
            PipCloseItem.Header = Loc.Get("TipClose");
            Title = Loc.Get("PipTitle");
        }
        private async void PipWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string dir = Path.Combine(SecureStore.DataDir, "EBWebView-PiP");
                var env = await CoreWebView2Environment.CreateAsync(null, dir);
                await PipWeb.EnsureCoreWebView2Async(env);
                PipWeb.CoreWebView2.Settings.AreDevToolsEnabled = false;
                PipWeb.CoreWebView2.Settings.IsStatusBarEnabled = false;
                AdBlock.Attach(PipWeb.CoreWebView2);
                PipWeb.NavigationCompleted += async delegate
                {
                    try { await PipWeb.ExecuteScriptAsync(PipAdCss); }
                    catch { }
                };
                PipWeb.Source = new Uri(BuildWidgetUrl(_pageUrl, _autoplay));
            }
            catch (Exception ex)
            {
                MessageBox.Show(Loc.Get("WebMsg") + "\n" + ex.Message,
                    Loc.Get("PipTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void PipDrag_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try { DragMove(); }
                catch (InvalidOperationException) { }
            }
        }
        private void PipCloseItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
