namespace WpfApp1
{
    // Extra CSS injected into soundcloud.com. Both are optional
    // and toggled from settings.
    public static class SiteExtras
    {
        public static string BuildCss(bool hideHeader, bool anims, bool redesign, bool adblock)
        {
            string css = "";
            if (hideHeader) css += "header.header{display:none!important;}";
            if (adblock) css += AdBlock.CosmeticCss;
            if (anims) css += AnimCss;
            if (redesign) css += RedesignCss;
            return css;
        }

        public static string ToJs(string css)
        {
            css = (css ?? "").Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", " ").Replace("\n", " ");
            return "(function(){var s=document.getElementById('__scNative');"
                + "if(!s){s=document.createElement('style');s.id='__scNative';"
                + "(document.head||document.documentElement).appendChild(s);}"
                + "s.textContent='" + css + "';})()";
        }

        // Simple pretty animations for site elements. Transform and opacity
        // only, so the page stays fast.
        public const string AnimCss =
            "@keyframes scFadeUp{from{opacity:0;transform:translateY(10px)}to{opacity:1;transform:none}}"
            + ".soundList__item,.searchItem,.chartTrack,.trackItem,.sound__body,.commentItem{animation:scFadeUp .45s ease both}"
            + "button,.button,.sc-button{transition:transform .18s ease,background-color .18s ease,box-shadow .18s ease!important}"
            + "button:hover,.button:hover,.sc-button:hover{transform:translateY(-1px)}"
            + "button:active,.button:active,.sc-button:active{transform:translateY(0) scale(.98)}"
            + "a{transition:color .18s ease,opacity .18s ease}"
            + ".image__full,.sound__coverArt img,.trackItem__image img,.visualSound__artwork img{transition:transform .25s ease,box-shadow .25s ease!important;border-radius:12px!important}"
            + ".soundList__item:hover .image__full,.trackItem:hover .trackItem__image img{transform:scale(1.03)}"
            + ".playControls__inner,.playControls__elements{transition:opacity .25s ease}"
            + "input,textarea{transition:border-color .18s ease,box-shadow .18s ease!important}";

        // Beta Material Design 3 restyle. Rounded shapes, tonal surfaces,
        // pill buttons. Keeps SoundCloud orange as primary.
        public const string RedesignCss =
            ":root{--m3-primary:#ff5500;--m3-on-primary:#fff;--m3-surface:#1b1b1e;--m3-surface2:#242428;--m3-outline:#3a3a40;--m3-radius:18px}"
            + ".l-container,.l-fixed-top-one-column,.l-fullwidth{max-width:1280px!important}"
            + "header.header{border-radius:0 0 20px 20px!important}"
            + ".soundList__item,.trackItem,.searchItem,.chartTrack,.commentItem,.sound__content{background:rgba(255,255,255,.04)!important;border:1px solid rgba(255,255,255,.08)!important;border-radius:var(--m3-radius)!important;padding:12px!important;margin-bottom:10px!important;box-shadow:0 1px 2px rgba(0,0,0,.25)!important}"
            + ".soundList__item:hover,.trackItem:hover,.searchItem:hover{box-shadow:0 6px 20px rgba(0,0,0,.35)!important;transform:translateY(-1px)}"
            + "button.sc-button,.button,.playControls__play,.sc-button-medium,.sc-button-large{border-radius:999px!important;font-weight:600!important}"
            + ".sc-button-primary,.sc-button-cta{background:var(--m3-primary)!important;border-color:var(--m3-primary)!important;color:var(--m3-on-primary)!important}"
            + ".image__full,.sound__coverArt,.trackItem__image,.visualSound__artwork,.soundBadge__avatar{border-radius:16px!important;overflow:hidden!important}"
            + ".playControls__bg,.playControls__inner{background:rgba(20,20,23,.92)!important;backdrop-filter:blur(16px)!important;border-radius:20px 20px 0 0!important;border-top:1px solid rgba(255,255,255,.1)!important}"
            + ".header__navMenu,.dropdownContent,.modal__modal{background:#242428!important;border-radius:20px!important;border:1px solid rgba(255,255,255,.1)!important}"
            + "input[type=text],input[type=search],.headerSearch__input{background:rgba(255,255,255,.07)!important;border-radius:999px!important;border:1px solid transparent!important}"
            + "input[type=text]:focus,input[type=search]:focus{border-color:var(--m3-primary)!important}"
            + ".badgeList,.statsList{border-radius:12px!important}"
            + "::-webkit-scrollbar{width:10px;height:10px}::-webkit-scrollbar-thumb{background:rgba(255,255,255,.18);border-radius:99px;border:2px solid transparent;background-clip:content-box}::-webkit-scrollbar-track{background:transparent}";
    }
}
