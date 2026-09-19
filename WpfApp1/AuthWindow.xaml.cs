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
            Loaded += delegate { Fx.Enter(this); };
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
