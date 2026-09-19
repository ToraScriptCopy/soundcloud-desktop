using System;
using System.IO;
using Microsoft.Web.WebView2.Core;
namespace WpfApp1
{
    public static class AdBlock
    {
        public static bool Enabled = false;

        private static readonly string[] Hosts = new string[]
        {
            "doubleclick.net", "googlesyndication.com", "googleadservices.com",
            "googletagservices.com", "2mdn.net", "adsrvr.org", "moatads.com",
            "criteo.com", "criteo.net", "outbrain.com", "taboola.com",
            "amazon-adsystem.com", "pubmatic.com", "rubiconproject.com",
            "openx.net", "openxadexchange.com", "ads.yahoo.com", "advertising.com",
            "smartadserver.com", "agkn.com", "mathtag.com", "addthis.com",
            "ads-twitter.com", "ads.linkedin.com", "ads.pinterest.com",
            "ads.tiktok.com", "ads.snapchat.com",
            "freewheel.tv", "fwmrm.net", "adswizz.com", "spotxchange.com",
            "spotx.tv", "springserve.com", "tremorhub.com", "tremorvideo.com",
            "lkqd.com", "aniview.com", "stickyadstv.com", "jwpltx.com",
            "vidazoo.com", "primis.tech", "connatix.com", "exelbid.com",
            "bidswitch.net", "contextweb.com", "lijit.com", "sovrn.com",
            "simpli.fi", "tapad.com", "bluekai.com", "rlcdn.com", "eyeota.net",
            "rfihub.com", "yandexadexchange.net", "adfox.ru", "ad.mail.ru",
            "hilltopads.net", "popads.net", "popcash.net", "propellerads.com",
            "adcash.com", "exoclick.com", "juicyads.com", "adsterra.com",
            "unrulymedia.com", "teads.tv", "teads.com", "sharethrough.com",
            "triplelift.com", "triplelift.net", "indexww.com", "casalemedia.com",
            "adform.com", "adform.net", "sizmek.com", "sizmek.net",
            "flashtalking.com", "celtra.com", "mediamath.com", "turn.com",
            "rocketfuel.com", "quantserve.com", "quantcast.com", "crwdcntrl.net",
            "lotame.com", "krxd.net", "everesttech.net", "m6r.eu",
            "adnxs.com", "adnxs-simple.com", "liverail.com", "mookie1.com",
            "zedo.com", "undertone.com", "tritondigital.com", "podtrac.com",
            "chartable.com", "podsights.com"
        };
        private static readonly string[] Patterns = new string[]
        {
            "/ads/", "/ads.", "/adserver", "/adservice", "adsystem", "pagead",
            "doubleclick", "googlesyndication", "adsense", "prebid",
            "criteo", "taboola", "outbrain", "spotx", "springserve",
            "freewheel", "fwmrm", "adswizz", "tremor", "aniview",
            "stickyads", "moatads", "smartad", "rubicon", "pubmatic",
            "openx", "sovrn", "lijit", "sponsored", "/nativeads",
            "hilltopads", "popads", "popcash", "propeller", "adcash",
            "exoclick", "juicyads", "adsterra", "fbevents", "facebook.com/tr",
            "vast", "vpaid", "ima3", "imasdk", "googleads", "preroll",
            "midroll", "postroll", "adbreak", "dai.google", "unruly",
            "teads", "sharethrough", "triplelift", "indexww", "casale",
            "adform", "sizmek", "flashtalking", "celtra", "mediamath",
            "quantserve", "triton", "podtrac", "chartable", "podsights"
        };
        private static readonly string[] AuthHosts = new string[]
        {
            "soundcloud.com", "api.soundcloud.com", "sndcdn.com",
            "accounts.google.com", "apis.google.com", "ssl.gstatic.com",
            "www.gstatic.com", "accounts.youtube.com",
            "appleid.apple.com", "id.apple.com",
            "auth0.com"
        };
        public const string CosmeticCss =
            "[id*='adSlot']:not([class*='auth']):not([class*='login']):not([class*='signup']):not([class*='modal'])," +
            "[class*='adSlot']:not([class*='auth']):not([class*='login']):not([class*='signup']):not([class*='modal'])," +
            "[class*='AdSlot']:not([class*='auth']):not([class*='login']):not([class*='signup']):not([class*='modal'])" +
            "{display:none!important;}";
        public static bool IsAuthUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            string low = url.ToLowerInvariant();
            if (low.Contains("accounts.google.com") || low.Contains("apis.google.com")
                || low.Contains("appleid.apple.com") || low.Contains("id.apple.com")
                || low.Contains("/login") || low.Contains("/signin") || low.Contains("/signup")
                || low.Contains("/register") || low.Contains("/oauth") || low.Contains("/auth")
                || low.Contains("facebook.com/login") || low.Contains("facebook.com/dialog")
                || low.Contains("soundcloud.com/signin") || low.Contains("soundcloud.com/signup"))
                return true;
            try
            {
                var u = new Uri(url);
                string host = u.Host.ToLowerInvariant();
                for (int i = 0; i < AuthHosts.Length; i++)
                    if (host == AuthHosts[i] || host.EndsWith("." + AuthHosts[i]))
                    {
                        if (AuthHosts[i] == "soundcloud.com" || AuthHosts[i] == "api.soundcloud.com" || AuthHosts[i] == "sndcdn.com")
                            return true;
                        return true;
                    }
            }
            catch { }
            return false;
        }
        public static bool ShouldBlock(string url)
        {
            if (!Enabled || string.IsNullOrEmpty(url)) return false;
            if (IsAuthUrl(url)) return false;
            Uri u;
            try { u = new Uri(url); }
            catch { return false; }
            if (u.Scheme != Uri.UriSchemeHttp && u.Scheme != Uri.UriSchemeHttps) return false;
            string host = u.Host.ToLowerInvariant();
            for (int i = 0; i < Hosts.Length; i++)
                if (host == Hosts[i] || host.EndsWith("." + Hosts[i])) return true;
            string hay = (u.Host + u.PathAndQuery).ToLowerInvariant();
            for (int i = 0; i < Patterns.Length; i++)
                if (hay.Contains(Patterns[i])) return true;
            return false;
        }
        public static void Attach(CoreWebView2 core)
        {
            try
            {
                core.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
                core.WebResourceRequested += delegate(object sender, CoreWebView2WebResourceRequestedEventArgs e)
                {
                    try
                    {
                        if (ShouldBlock(e.Request.Uri))
                        {
                            e.Response = core.Environment.CreateWebResourceResponse(
                                new MemoryStream(new byte[0]), 200, "OK", "Content-Type: text/plain");
                        }
                    }
                    catch { }
                };
            }
            catch { }
        }
    }
}
