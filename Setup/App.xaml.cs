using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Setup
{
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                File.AppendAllText(Path.Combine(Path.GetTempPath(), "scd-setup-crash.log"),
                    DateTime.Now + "\r\n" + e.Exception + "\r\n\r\n");
            }
            catch { }
            try
            {
                MessageBox.Show(e.Exception.Message, "Setup",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch { }
            e.Handled = true;
        }
    }
}
