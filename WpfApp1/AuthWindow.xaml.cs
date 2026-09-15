using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using UiControls = Wpf.Ui.Controls;

namespace WpfApp1
{
    public partial class AuthWindow : UiControls.FluentWindow
    {
        public AuthWindow()
        {
            InitializeComponent();
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

        public async void OpenPopupAsync(CoreWebView2Environment env, CoreWebView2NewWindowRequestedEventArgs e, CoreWebView2Deferral deferral)
        {
            try
            {
                await PopupWeb.EnsureCoreWebView2Async(env);
                PopupWeb.CoreWebView2.Settings.AreDevToolsEnabled = false;
                PopupWeb.CoreWebView2.WindowCloseRequested += delegate
                {
                    try { Close(); }
                    catch { }
                };
                PopupWeb.NavigationCompleted += async delegate
                {
                    try
                    {
                        string t = await PopupWeb.ExecuteScriptAsync("document.title");
                        t = t.Trim().Trim('"');
                        if (t.Length > 0 && t != "null") Title = t;
                    }
                    catch { }
                };
                e.NewWindow = PopupWeb.CoreWebView2;
            }
            catch
            {
                try { Close(); }
                catch { }
            }
            finally
            {
                try { deferral.Complete(); }
                catch { }
            }
        }
    }
}
