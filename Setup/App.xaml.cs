using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace Setup
{
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAsm;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private static Assembly ResolveAsm(object sender, ResolveEventArgs e)
        {
            string name = new AssemblyName(e.Name).Name + ".dll";
            if (name != "Wpf.Ui.dll" && name != "Wpf.Ui.Abstractions.dll") return null;
            try
            {
                using (var st = Assembly.GetExecutingAssembly().GetManifestResourceStream("Setup.lib." + name))
                {
                    if (st == null) return null;
                    byte[] buf = new byte[st.Length];
                    int off = 0;
                    while (off < buf.Length)
                    {
                        int n = st.Read(buf, off, buf.Length - off);
                        if (n <= 0) break;
                        off += n;
                    }
                    return Assembly.Load(buf);
                }
            }
            catch { return null; }
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
