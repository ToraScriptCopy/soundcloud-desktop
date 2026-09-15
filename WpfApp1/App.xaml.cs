using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace WpfApp1
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
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SoundCloudDesktopBeta");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "crash.log"),
                    DateTime.Now + "\r\n" + e.Exception + "\r\n\r\n");
            }
            catch { }
            try
            {
                MessageBox.Show(e.Exception.Message, "SoundCloud Desktop Beta",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch { }
            e.Handled = true;
        }
    }
}
