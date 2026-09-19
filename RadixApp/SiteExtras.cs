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

        // Radix design system restyle. Real @radix-ui/colors tokens,
        // dark appearance, Radix radius scale. On by default in this build.
        public const string RedesignCss =
            ":root{--r1:#111111;--r2:#191919;--r3:#222222;--r4:#2a2a2a;--r6:#3a3a3a;--r11:#b4b4b4;--r12:#eeeeee;--acc:#f76b15;--acc-hi:#ff801f;--acc-tx:#ffa057}"
            + ".l-container,.l-fixed-top-one-column,.l-fullwidth{max-width:1280px!important}"
            + "body{background:var(--r1)!important;color:var(--r12)!important}"
            + "header.header{background:var(--r2)!important;border-bottom:1px solid var(--r4)!important;border-radius:0 0 12px 12px!important}"
            + ".soundList__item,.trackItem,.searchItem,.chartTrack,.commentItem,.sound__content{background:var(--r2)!important;border:1px solid var(--r4)!important;border-radius:12px!important;padding:12px!important;margin-bottom:10px!important}"
            + ".soundList__item:hover,.trackItem:hover,.searchItem:hover{border-color:var(--r6)!important;box-shadow:0 6px 20px rgba(0,0,0,.4)!important;transform:translateY(-1px)}"
            + "button.sc-button,.button,.sc-button-medium,.sc-button-large{border-radius:999px!important;font-weight:600!important}"
            + ".sc-button-primary,.sc-button-cta,.playControls__play{background:var(--acc)!important;border-color:var(--acc)!important;color:#fff!important}"
            + ".sc-button-primary:hover,.sc-button-cta:hover{background:var(--acc-hi)!important;border-color:var(--acc-hi)!important}"
            + "a{color:var(--acc-tx)!important}"
            + ".image__full,.sound__coverArt,.trackItem__image,.visualSound__artwork,.soundBadge__avatar{border-radius:8px!important;overflow:hidden!important}"
            + ".playControls__bg,.playControls__inner{background:rgba(25,25,25,.94)!important;backdrop-filter:blur(16px)!important;border-top:1px solid var(--r4)!important}"
            + ".header__navMenu,.dropdownContent,.modal__modal{background:var(--r3)!important;border-radius:12px!important;border:1px solid var(--r4)!important}"
            + "input[type=text],input[type=search],.headerSearch__input{background:var(--r3)!important;border-radius:999px!important;border:1px solid transparent!important;color:var(--r12)!important}"
            + "input[type=text]:focus,input[type=search]:focus{border-color:var(--acc)!important;box-shadow:0 0 0 1px var(--acc)!important}"
            + ".badgeList,.statsList{border-radius:8px!important}"
            + "::-webkit-scrollbar{width:10px;height:10px}::-webkit-scrollbar-thumb{background:#3a3a3a;border-radius:99px;border:2px solid transparent;background-clip:content-box}::-webkit-scrollbar-track{background:transparent}";
    }
}
