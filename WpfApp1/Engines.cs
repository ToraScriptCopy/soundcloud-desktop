using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace WpfApp1
{
    public static class Engines
    {
        public const int Custom = 0;
        public const int Edge = 1;
        public const int Chrome = 2;
        public const int Firefox = 3;

        private static string Pf64()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        }

        private static string Pf86()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        }

        public static string NameKey(int i)
        {
            if (i == 1) return "EngineEdge";
            if (i == 2) return "EngineChrome";
            if (i == 3) return "EngineFirefox";
            return "EngineCustom";
        }

        public static bool IsAvailable(int i)
        {
            if (i == 0) return true;
            if (i == 3) return FirefoxExe() != null;
            return ExeFolder(i) != null;
        }

        public static string ExeFolder(int i)
        {
            string exe = i == 1 ? "msedge.exe" : "chrome.exe";
            string[] roots = i == 1
                ? new string[] { Path.Combine(Pf86(), "Microsoft", "Edge", "Application"), Path.Combine(Pf64(), "Microsoft", "Edge", "Application") }
                : new string[] { Path.Combine(Pf64(), "Google", "Chrome", "Application"), Path.Combine(Pf86(), "Google", "Chrome", "Application") };
            foreach (string root in roots)
            {
                string best = null;
                try
                {
                    if (File.Exists(Path.Combine(root, exe))) best = root;
                    foreach (string d in Directory.GetDirectories(root))
                    {
                        if (File.Exists(Path.Combine(d, exe)) && (best == null || string.Compare(d, best, StringComparison.OrdinalIgnoreCase) > 0))
                            best = d;
                    }
                }
                catch { }
                if (best != null) return best;
            }
            return null;
        }

        public static string SharedProfileDir(int i)
        {
            string lad = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return i == 1
                ? Path.Combine(lad, "Microsoft", "Edge", "User Data")
                : Path.Combine(lad, "Google", "Chrome", "User Data");
        }

        public static string FirefoxExe()
        {
            string[] cands = new string[]
            {
                Path.Combine(Pf64(), "Mozilla Firefox", "firefox.exe"),
                Path.Combine(Pf86(), "Mozilla Firefox", "firefox.exe")
            };
            foreach (string c in cands)
                if (File.Exists(c)) return c;
            return null;
        }

        public sealed class EnvResult
        {
            public CoreWebView2Environment Env;
            public bool Shared;
        }

        public static async Task<EnvResult> CreateAsync(int engine, string isolatedDir)
        {
            var res = new EnvResult();
            if (engine == 1 || engine == 2)
            {
                string exeDir = ExeFolder(engine);
                if (exeDir == null) throw new InvalidOperationException("engine missing");
                try
                {
                    res.Env = await CoreWebView2Environment.CreateAsync(exeDir, SharedProfileDir(engine), Opts(engine));
                    res.Shared = true;
                    return res;
                }
                catch
                {
                    res.Env = await CoreWebView2Environment.CreateAsync(exeDir, isolatedDir, Opts(engine));
                    return res;
                }
            }
            res.Env = await CoreWebView2Environment.CreateAsync(null, isolatedDir, Opts(engine));
            return res;
        }

        private static CoreWebView2EnvironmentOptions Opts(int engine)
        {
            if (engine != 0) return null;
            return new CoreWebView2EnvironmentOptions(
                "--disable-features=Translate,OptimizationHints --autoplay-policy=no-user-gesture-required");
        }
    }
}
