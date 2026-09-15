using System;
using System.IO;
using Microsoft.Web.WebView2.Core;

namespace WpfApp1
{
    // Host + pattern blocking in the spirit of EasyList/uBlock,
    // cut down to what a WebView2 filter can check fast.
    // Strict mode also cuts analytics (may break social login).
    public static class AdBlock
    {
        public static bool Enabled = true;
        public static bool Strict = false;

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
            "adcash.com", "exoclick.com", "juicyads.com", "adsterra.com"
        };

        private static readonly string[] StrictHosts = new string[]
        {
            "google-analytics.com", "googletagmanager.com", "facebook.net",
            "connect.facebook.net", "bat.bing.com", "hotjar.com", "hotjar.io",
            "fullstory.com", "mixpanel.com", "amplitude.com", "segment.io",
            "segment.com", "optimizely.com", "scorecardresearch.com", "demdex.net",
            "omtrdc.net", "2o7.net", "newrelic.com", "nr-data.net",
            "bugsnag.com", "sentry.io", "sentry-cdn.com", "appsflyer.com",
            "adjust.com", "branch.io", "kochava.com", "moat.com",
            "adsafeprotected.com", "doubleverify.com", "imrworldwide.com"
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
            "exoclick", "juicyads", "adsterra", "fbevents", "facebook.com/tr"
        };

        public static bool ShouldBlock(string url)
        {
            if (!Enabled || string.IsNullOrEmpty(url)) return false;
            Uri u;
            try { u = new Uri(url); }
            catch { return false; }
            if (u.Scheme != Uri.UriSchemeHttp && u.Scheme != Uri.UriSchemeHttps) return false;

            string host = u.Host.ToLowerInvariant();
            for (int i = 0; i < Hosts.Length; i++)
                if (host == Hosts[i] || host.EndsWith("." + Hosts[i])) return true;

            if (Strict)
            {
                for (int i = 0; i < StrictHosts.Length; i++)
                    if (host == StrictHosts[i] || host.EndsWith("." + StrictHosts[i])) return true;
            }

            string hay = (u.Host + u.PathAndQuery).ToLowerInvariant();
            for (int i = 0; i < Patterns.Length; i++)
                if (hay.Contains(Patterns[i])) return true;

            return false;
        }

        public static void Attach(CoreWebView2 core)
        {
            core.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            core.WebResourceRequested += delegate(object sender, CoreWebView2WebResourceRequestedEventArgs e)
            {
                if (ShouldBlock(e.Request.Uri))
                {
                    e.Response = core.Environment.CreateWebResourceResponse(
                        new MemoryStream(new byte[0]), 200, "OK", "Content-Type: text/plain");
                }
            };
        }
    }
}
