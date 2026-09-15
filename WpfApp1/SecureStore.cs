using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;

namespace WpfApp1
{
    [DataContract]
    public sealed class AppState
    {
        [DataMember] public int Theme { get; set; }
        [DataMember] public double Volume { get; set; }
        [DataMember] public bool Muted { get; set; }
        [DataMember] public string LastUrl { get; set; }
        [DataMember] public string PlaylistUrl { get; set; }
        [DataMember] public string Lang { get; set; }
        [DataMember] public bool HideHeader { get; set; }
        [DataMember] public bool SidebarOpen { get; set; }
        [DataMember] public bool Topmost { get; set; }
        [DataMember] public bool TrayHide { get; set; }
        [DataMember] public bool Autostart { get; set; }
        [DataMember] public bool AdBlockOn { get; set; }
        [DataMember] public bool AdStrict { get; set; }
        [DataMember] public int HotPrev { get; set; }
        [DataMember] public int HotPlay { get; set; }
        [DataMember] public int HotNext { get; set; }
        [DataMember] public int HotVolDn { get; set; }
        [DataMember] public int HotVolUp { get; set; }

        public AppState()
        {
            Theme = 0;
            Volume = 0.8;
            Muted = false;
            LastUrl = "https://soundcloud.com/";
            PlaylistUrl = "";
            Lang = "auto";
            HideHeader = true;
            SidebarOpen = true;
            Topmost = false;
            TrayHide = true;
            Autostart = false;
            AdBlockOn = true;
            AdStrict = false;
            HotPrev = 0x61;
            HotPlay = 0x62;
            HotNext = 0x63;
            HotVolDn = 0x64;
            HotVolUp = 0x65;
        }

        public void Normalize()
        {
            if (Theme < 0 || Theme >= Themes.Count) Theme = 0;
            if (Volume < 0) Volume = 0;
            if (Volume > 1) Volume = 1;
            if (string.IsNullOrWhiteSpace(LastUrl)) LastUrl = "https://soundcloud.com/";
            if (PlaylistUrl == null) PlaylistUrl = "";
            if (Lang == null) Lang = "auto";
        }
    }

    [DataContract]
    internal sealed class VaultEnvelope
    {
        [DataMember] public string Hash { get; set; }
        [DataMember] public string Json { get; set; }
    }

    // Local vault: JSON -> SHA256 integrity hash -> DPAPI (current Windows user only).
    public static class SecureStore
    {
        public static readonly string DataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SoundCloudDesktopBeta");

        public static readonly string VaultPath = Path.Combine(DataDir, "vault.dat");

        public static string LastError { get; private set; }

        public static AppState Load(out bool tampered)
        {
            tampered = false;
            try
            {
                if (!File.Exists(VaultPath))
                    return new AppState();

                byte[] blob = File.ReadAllBytes(VaultPath);
                byte[] plain = ProtectedData.Unprotect(blob, null, DataProtectionScope.CurrentUser);
                var env = (VaultEnvelope)FromJson(Encoding.UTF8.GetString(plain), typeof(VaultEnvelope));

                if (env == null || env.Json == null || env.Hash == null)
                {
                    tampered = true;
                    return new AppState();
                }

                if (!string.Equals(Sha256Hex(env.Json), env.Hash, StringComparison.OrdinalIgnoreCase))
                {
                    tampered = true; // файл подменили или побился
                    return new AppState();
                }

                var state = (AppState)FromJson(env.Json, typeof(AppState));
                if (state == null)
                {
                    tampered = true;
                    return new AppState();
                }

                state.Normalize();
                return state;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                tampered = true;
                return new AppState();
            }
        }

        public static bool Save(AppState state)
        {
            try
            {
                if (state == null) state = new AppState();
                state.Normalize();

                string json = ToJson(state, typeof(AppState));
                var env = new VaultEnvelope { Json = json, Hash = Sha256Hex(json) };
                byte[] blob = ProtectedData.Protect(
                    Encoding.UTF8.GetBytes(ToJson(env, typeof(VaultEnvelope))),
                    null, DataProtectionScope.CurrentUser);

                if (!Directory.Exists(DataDir)) Directory.CreateDirectory(DataDir);
                File.WriteAllBytes(VaultPath, blob);
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        public static string Sha256Hex(string text)
        {
            using (var sha = SHA256Managed.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? ""));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private static string ToJson(object graph, Type type)
        {
            using (var ms = new MemoryStream())
            {
                new DataContractJsonSerializer(type).WriteObject(ms, graph);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private static object FromJson(string json, Type type)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return new DataContractJsonSerializer(type).ReadObject(ms);
            }
        }
    }
}
