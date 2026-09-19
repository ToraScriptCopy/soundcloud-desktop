using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace WpfApp1
{
    public partial class App : Application
    {
        private static Mutex _singleMutex;
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        private const int SW_RESTORE = 9;
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            bool fresh;
            bool testProfile = false;
            try
            {
                testProfile = !string.IsNullOrWhiteSpace(
                    Environment.GetEnvironmentVariable("SCD_PROFILE"));
            }
            catch { }
            try
            {
                _singleMutex = new Mutex(true, "SoundCloudDesktopBeta_SingleInstance", out fresh);
            }
            catch { fresh = true; }
            if (!fresh && !testProfile)
            {
                // Another copy is already running (maybe hidden in tray).
                // Bring its window back instead of starting a second one.
                try { FocusRunningCopy(); }
                catch { }
                try { if (_singleMutex != null) _singleMutex.Dispose(); }
                catch { }
                Shutdown();
                return;
            }
            base.OnStartup(e);
        }
        private static void FocusRunningCopy()
        {
            int me = Process.GetCurrentProcess().Id;
            string exe;
            try { exe = Process.GetCurrentProcess().MainModule.FileName; }
            catch { return; }
            foreach (Process p in Process.GetProcessesByName(
                Path.GetFileNameWithoutExtension(exe)))
            {
                try
                {
                    if (p.Id == me) continue;
                    string other;
                    try { other = p.MainModule.FileName; }
                    catch { continue; }
                    if (!string.Equals(other, exe, StringComparison.OrdinalIgnoreCase)) continue;
                    IntPtr h = p.MainWindowHandle;
                    if (h == IntPtr.Zero) continue;
                    ShowWindow(h, SW_RESTORE);
                    SetForegroundWindow(h);
                    return;
                }
                catch { }
            }
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
                MessageBox.Show(e.Exception.Message, "SoundCloud Desktop",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch { }
            e.Handled = true;
        }
    }
}
