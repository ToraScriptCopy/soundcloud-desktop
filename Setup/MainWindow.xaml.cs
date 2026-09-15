using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WpfApp1;
using UiControls = Wpf.Ui.Controls;
namespace Setup
{
    public partial class MainWindow : UiControls.FluentWindow
    {
        private const string AppName = "SoundCloud Desktop Beta";
        private const string AppId = "SoundCloudDesktopBeta";
        private const string AppExe = "SoundCloudDesk.exe";
        public MainWindow()
        {
            InitializeComponent();
            string[] args = Environment.GetCommandLineArgs();
            foreach (string a in args)
            {
                if (a == "/uninstall")
                {
                    Loaded += delegate { RunUninstall(); };
                    return;
                }
            }
            Loc.Current = Loc.Resolve("auto");
            Loc.FillLangCombo(LangBox, "auto");
            PathBox.Text = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "SoundCloud Desktop Beta");
            OptDesktop.IsChecked = true;
            OptStartMenu.IsChecked = true;
            OptLaunch.IsChecked = true;
            ApplyLoc();
            CheckWebView();
        }
        private void ApplyLoc()
        {
            Title = AppName + " — " + Loc.Get("SetupTitle");
            SetupBar.Title = AppName + " — " + Loc.Get("SetupTitle");
            LangLbl.Text = Loc.Get("SetupLangLbl");
            LangBox.Items[0] = Loc.Get("LangAuto");
            BtnNext1.Content = Loc.Get("SetupNext");
            PathLbl.Text = Loc.Get("SetupPathLbl");
            BtnBrowse.Content = Loc.Get("SetupBrowse");
            OptDesktop.Content = Loc.Get("SetupDesktopOpt");
            OptStartMenu.Content = Loc.Get("SetupStartOpt");
            OptLaunch.Content = Loc.Get("SetupLaunchOpt");
            BtnBack.Content = Loc.Get("SetupBack");
            BtnInstall.Content = Loc.Get("SetupInstall");
            WebViewWarn.Text = Loc.Get("SetupWebViewMsg");
            BtnWebView.Content = Loc.Get("SetupGetWebView");
            DoneText.Text = Loc.Get("SetupDone");
            BtnClose.Content = Loc.Get("SetupClose");
        }
        private void LangBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LangBox.SelectedIndex < 0) return;
            Loc.Current = Loc.Resolve(Loc.PrefCodes[LangBox.SelectedIndex]);
            ApplyLoc();
        }
        private void CheckWebView()
        {
            bool ok = Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\ClientState\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}",
                "pv", null) != null;
            WebViewWarn.Visibility = ok ? Visibility.Collapsed : Visibility.Visible;
            BtnWebView.Visibility = WebViewWarn.Visibility;
        }
        private void BtnWebView_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start("https://go.microsoft.com/fwlink/p/?LinkId=2124703"); }
            catch { }
        }
        private void BtnNext1_Click(object sender, RoutedEventArgs e) { ShowPage(PageMain); }
        private void BtnBack_Click(object sender, RoutedEventArgs e) { ShowPage(PageLang); }
        private void BtnClose_Click(object sender, RoutedEventArgs e) { Close(); }
        private void ShowPage(UIElement page)
        {
            PageLang.Visibility = Visibility.Collapsed;
            PageMain.Visibility = Visibility.Collapsed;
            PageProgress.Visibility = Visibility.Collapsed;
            PageDone.Visibility = Visibility.Collapsed;
            page.Visibility = Visibility.Visible;
        }
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = Loc.Get("SetupPathLbl");
            dlg.SelectedPath = PathBox.Text;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                PathBox.Text = dlg.SelectedPath;
        }
        private bool HasPayload()
        {
            return Array.Exists(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames(),
                delegate(string n) { return n == "Setup.payload.app.zip"; });
        }
        private string SourceDir()
        {
            string near = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app");
            if (Directory.Exists(near)) return near;
            return Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, @"..\WpfApp1\bin\Release\net48"));
        }
        private async void BtnInstall_Click(object sender, RoutedEventArgs e)
        {
            string target = PathBox.Text.Trim();
            if (target.Length == 0) return;
            BtnInstall.IsEnabled = false;
            ShowPage(PageProgress);
            try
            {
                if (HasPayload())
                    await Task.Run(delegate { ExtractPayload(target); });
                else
                    await Task.Run(delegate { CopyAll(SourceDir(), target); });
                File.Copy(Process.GetCurrentProcess().MainModule.FileName,
                    Path.Combine(target, "Uninstaller.exe"), true);
                WriteUninstallKey(target);
                MakeShortcuts(target, OptDesktop.IsChecked == true, OptStartMenu.IsChecked == true);
                ShowPage(PageDone);
                if (OptLaunch.IsChecked == true)
                {
                    try { Process.Start(Path.Combine(target, AppExe)); }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                ProgStatus.Text = ex.Message;
                BtnInstall.IsEnabled = true;
                ShowPage(PageMain);
            }
        }
        private void ExtractPayload(string dst)
        {
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Setup.payload.app.zip"))
            {
                using (var zip = new System.IO.Compression.ZipArchive(stream))
                {
                    int done = 0;
                    int total = zip.Entries.Count;
                    foreach (var entry in zip.Entries)
                    {
                        if (entry.FullName.EndsWith("/"))
                        {
                            done++;
                            continue;
                        }
                        string dest = Path.Combine(dst, entry.FullName);
                        Directory.CreateDirectory(Path.GetDirectoryName(dest));
                        using (var es = entry.Open())
                        {
                            using (var fs = File.Create(dest))
                                es.CopyTo(fs);
                        }
                        done++;
                        int pct = done * 100 / total;
                        Dispatcher.Invoke(new Action(delegate
                        {
                            ProgBar.Value = pct;
                            ProgStatus.Text = Loc.Get("SetupInstalling") + " " + pct + "%";
                        }));
                    }
                }
            }
        }
        private void CopyAll(string src, string dst)
        {
            string[] files = Directory.GetFiles(src, "*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                string rel = files[i].Substring(src.Length).TrimStart('\\', '/');
                string dest = Path.Combine(dst, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                File.Copy(files[i], dest, true);
                int pct = (i + 1) * 100 / files.Length;
                Dispatcher.Invoke(new Action(delegate
                {
                    ProgBar.Value = pct;
                    ProgStatus.Text = Loc.Get("SetupInstalling") + " " + pct + "%";
                }));
            }
        }
        private void WriteUninstallKey(string target)
        {
            using (var key = Registry.LocalMachine.CreateSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + AppId))
            {
                if (key == null) return;
                key.SetValue("DisplayName", AppName);
                key.SetValue("DisplayVersion", "1.0");
                key.SetValue("Publisher", "SoundCloud Desktop Beta");
                key.SetValue("InstallLocation", target);
                key.SetValue("DisplayIcon", Path.Combine(target, AppExe));
                key.SetValue("UninstallString", "\"" + Path.Combine(target, "Uninstaller.exe") + "\" /uninstall");
                key.SetValue("NoModify", 1);
                key.SetValue("NoRepair", 1);
            }
        }
        private void MakeShortcuts(string target, bool desktop, bool startmenu)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                dynamic shell = Activator.CreateInstance(shellType);
                string exe = Path.Combine(target, AppExe);
                if (desktop)
                {
                    string lnk = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                        AppName + ".lnk");
                    dynamic s = shell.CreateShortcut(lnk);
                    s.TargetPath = exe;
                    s.WorkingDirectory = target;
                    s.IconLocation = exe + ",0";
                    s.Save();
                }
                if (startmenu)
                {
                    string dir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                        "Programs", AppName);
                    Directory.CreateDirectory(dir);
                    dynamic s = shell.CreateShortcut(Path.Combine(dir, AppName + ".lnk"));
                    s.TargetPath = exe;
                    s.WorkingDirectory = target;
                    s.IconLocation = exe + ",0";
                    s.Save();
                }
            }
            catch { }
        }
        private void RunUninstall()
        {
            try
            {
                string dir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
                string desktopLnk = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    AppName + ".lnk");
                try { if (File.Exists(desktopLnk)) File.Delete(desktopLnk); }
                catch { }
                try
                {
                    string sm = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                        "Programs", AppName);
                    if (Directory.Exists(sm)) Directory.Delete(sm, true);
                }
                catch { }
                try { Registry.LocalMachine.DeleteSubKeyTree(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + AppId, false); }
                catch { }
                try { Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true)
                    .DeleteValue(AppId, false); }
                catch { }
                string self = Process.GetCurrentProcess().MainModule.FileName;
                foreach (string f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                {
                    if (string.Equals(f, self, StringComparison.OrdinalIgnoreCase)) continue;
                    try { File.Delete(f); }
                    catch { }
                }
                foreach (string d in Directory.GetDirectories(dir))
                {
                    try { Directory.Delete(d, true); }
                    catch { }
                }
                MessageBox.Show(Loc.Get("SetupUninstDone"), AppName);
                try
                {
                    var psi = new ProcessStartInfo("cmd.exe",
                        "/c timeout /t 2 /nobreak >nul & del \"" + self + "\" & rmdir \"" + dir + "\"");
                    psi.CreateNoWindow = true;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    Process.Start(psi);
                }
                catch { }
            }
            catch { }
            Environment.Exit(0);
        }
    }
}
