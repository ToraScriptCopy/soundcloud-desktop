namespace WpfApp1
{
    // Extra CSS/JS injected into soundcloud.com. The redesign is split
    // into flags so every part can be toggled from the ReDesign window.
    // Text colors are left alone on purpose, only surfaces, borders,
    // buttons and accents change.
    public static class SiteExtras
    {
        public static string BuildCss(AppState s, bool adblock)
        {
            string css = ScrollCss;
            if (s.HideHeader)
            {
                css += "header.header{display:none!important;}";
            }
            else if (s.ReDesign && s.RdHeader)
            {
                css += RdHeader;
            }
            if (adblock) css += AdBlock.CosmeticCss;
            if (s.SiteAnims) css += AnimCss;
            if (s.ReDesign)
            {
                if (s.RdCards) css += RdCards;
                if (s.RdButtons) css += RdButtons;
                if (s.RdPlayer) css += RdPlayer;
                if (s.RdComments) css += RdComments;
                if (s.RdSidebar) css += RdSidebar;
                if (s.RdInputs) css += RdInputs;
                if (s.RdPopups) css += RdPopups;
            }
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

        // Barely visible scrollbars, always on. Thumb sits at 15% opacity.
        public const string ScrollCss =
            "html{scrollbar-width:thin;scrollbar-color:rgba(255,255,255,.15) transparent!important}"
            + "::-webkit-scrollbar{width:6px!important;height:6px!important}"
            + "::-webkit-scrollbar-thumb{background:rgba(255,255,255,.15)!important;border-radius:99px!important;border:none!important}"
            + "::-webkit-scrollbar-thumb:hover{background:rgba(255,255,255,.35)!important}"
            + "::-webkit-scrollbar-track{background:transparent!important}";

        // Simple pretty animations. Transform and opacity only, so the
        // page stays fast. No artwork rounding here on purpose.
        public const string AnimCss =
            "@keyframes scFadeUp{from{opacity:0;transform:translateY(10px)}to{opacity:1;transform:none}}"
            + "@keyframes scPopIn{from{opacity:0;transform:scale(.96) translateY(8px)}to{opacity:1;transform:none}}"
            + "@keyframes scDropIn{from{opacity:0;transform:translateY(-6px)}to{opacity:1;transform:none}}"
            + ".soundList__item,.searchItem,.chartTrack,.trackItem,.sound__body,.commentItem{animation:scFadeUp .45s ease both}"
            + "button,.button,.sc-button{transition:transform .18s ease,background-color .18s ease,box-shadow .18s ease,border-color .18s ease!important}"
            + "button:hover,.button:hover,.sc-button:hover{transform:translateY(-1px)}"
            + "button:active,.button:active,.sc-button:active{transform:translateY(0) scale(.97)}"
            + "a{transition:color .18s ease,opacity .18s ease}"
            + ".modal__modal,.modal,.dialog{animation:scPopIn .25s ease both}"
            + ".dropdownContent,.header__navMenu,[role='menu'],[role='dialog']{animation:scDropIn .2s ease both}"
            + ".playControls__play{transition:transform .15s ease!important}"
            + ".playControls__play:active{transform:scale(.92)!important}"
            + "input,textarea{transition:border-color .18s ease,box-shadow .18s ease!important}";

        // Promo killer. Hides "become an author" style upsell banners by
        // their exact text, removing the whole banner root so no empty
        // boxes stay behind. Login and signup are never touched.
        // Runs always, independent of the adblock toggle.
        public const string PromoJs =
            "(function(){if(window.__scPromoKiller)return;window.__scPromoKiller=true;"
            + "var PH=['Uploading tracks just got way easier','Get heard by up to 100 listeners','Now available: Get heard'];"
            + "var BS=[\"[class*='banner']\",\"[class*='Banner']\",\"[class*='upsell']\",\"[class*='Upsell']\",\"[class*='promo']\",\"[class*='Promo']\",\"[class*='notice']\",\"[class*='Notice']\",\"[class*='callout']\",\"[class*='Callout']\"].join(',');"
            + "function hasAuth(el){try{return el.querySelector&&el.querySelector('input[type=password],input[type=email],input[type=text][autocomplete*=email],form[action*=login],form[action*=signin]');}catch(e){return null;}}"
            + "function hideRoot(el){var cur=el,g=0;"
            + "while(cur&&cur.parentElement&&g<8){var p=cur.parentElement;"
            + "if(p===document.body)break;"
            + "var tag=(p.tagName||'').toLowerCase();"
            + "if(tag!=='div'&&tag!=='section'&&tag!=='aside'&&tag!=='li')break;"
            + "var txt='';try{txt=p.textContent||'';}catch(e){}"
            + "if(txt.length>600)break;"
            + "if(hasAuth(p))break;"
            + "cur=p;g++;}"
            + "try{cur.style.setProperty('display','none','important');}catch(e){}}"
            + "function hasPhrase(el){var t='';try{t=el.textContent||'';}catch(e){}if(!t)return false;"
            + "for(var i=0;i<PH.length;i++){if(t.indexOf(PH[i])>=0)return true;}return false;}"
            + "function sweepText(){try{"
            + "var w=document.createTreeWalker(document.body,NodeFilter.SHOW_TEXT,null,false);"
            + "var n,found=[];"
            + "while(n=w.nextNode()){var t=n.nodeValue;if(!t)continue;"
            + "for(var i=0;i<PH.length;i++){if(t.indexOf(PH[i])>=0){found.push(n);break;}}}"
            + "for(var k=0;k<found.length;k++){if(found[k].parentElement)hideRoot(found[k].parentElement);}"
            + "}catch(e){}}"
            + "function sweepBoxes(){try{"
            + "var els=document.querySelectorAll(BS);"
            + "for(var i=0;i<els.length;i++){if(hasPhrase(els[i])&&!hasAuth(els[i]))hideRoot(els[i]);}"
            + "}catch(e){}}"
            + "function sweep(){if(document.hidden)return;sweepText();sweepBoxes();}"
            + "var t=null;function sch(){if(t||document.hidden)return;t=setTimeout(function(){t=null;sweep();},800);}"
            + "try{new MutationObserver(sch).observe(document.documentElement,{childList:true,subtree:true});}catch(e){}"
            + "sweep();setInterval(sweep,8000);})()";

        public const string RdHeader =
            "header.header{background:#191919!important;border-bottom:1px solid #2a2a2a!important;box-shadow:0 1px 0 rgba(0,0,0,.4)!important}"
            + ".header__logo{background-size:contain!important}"
            + ".l-nav,.header__navMenuItem{transition:color .18s ease,box-shadow .18s ease!important}"
            + "[role='tablist']{border-bottom:1px solid #2a2a2a!important}"
            + "[role='tab']{border-radius:8px 8px 0 0!important;transition:box-shadow .18s ease,background-color .18s ease!important}"
            + "[role='tab']:hover{background:#222222!important}"
            + "[role='tab'][aria-selected='true']{box-shadow:inset 0 -2px 0 #f76b15!important}"
            + ".g-tabs-link.active,.header__navMenuItem.selected{box-shadow:inset 0 -2px 0 #f76b15!important}"
            + ".profileTabs__link.active,.g-tabs-link.active{color:#eeeeee!important}"
            + ".profileTabs,.tabs,.g-tabs{border-bottom:1px solid #2a2a2a!important}";

        public const string RdCards =
            ".l-container,.l-fixed-top-one-column,.l-fullwidth{max-width:1280px!important}"
            + ".soundList__item,.trackItem,.searchItem,.chartTrack,.sound__content{background:#191919!important;border:1px solid #2a2a2a!important;border-radius:12px!important;padding:12px!important;margin-bottom:10px!important}"
            + ".soundList__item:hover,.trackItem:hover,.searchItem:hover{border-color:#3a3a3a!important;box-shadow:0 6px 20px rgba(0,0,0,.4)!important;transform:translateY(-1px)}"
            + ".soundTitle__title{color:#eeeeee!important}"
            + ".soundTitle__username,.trackItem__username,.soundContext__username{color:#b4b4b4!important}"
            + ".soundStats,.trackItem__stats,.statsList{color:#b4b4b4!important}"
            + ".badgeList__item{background:#222222!important;border:1px solid #2a2a2a!important;border-radius:8px!important}";

        public const string RdButtons =
            "button.sc-button,.button,.sc-button-medium,.sc-button-large{border-radius:999px!important;font-weight:600!important}"
            + ".sc-button-primary,.sc-button-cta{background:#f76b15!important;border-color:#f76b15!important;color:#fff!important}"
            + ".sc-button-primary:hover,.sc-button-cta:hover{background:#ff801f!important;border-color:#ff801f!important}"
            + ".sc-button-secondary,.sc-button-small{background:transparent!important;border:1px solid #3a3a3a!important;color:#eeeeee!important}"
            + ".sc-button-secondary:hover{border-color:#606060!important}"
            + ".sc-button-like.liked,.sc-button-repost.reposted{color:#f76b15!important;border-color:#7e451d!important}";

        public const string RdPlayer =
            ".playControls__bg,.playControls__inner{background:rgba(25,25,25,.94)!important;backdrop-filter:blur(16px)!important;border-top:1px solid #2a2a2a!important}"
            + ".playControls__elements button,.playControls__inner button{background:transparent!important;border:1px solid #3a3a3a!important;border-radius:999px!important;color:inherit!important}"
            + ".playControls__elements button:hover{border-color:#606060!important}"
            + ".playControls__play{background:transparent!important;border:1px solid #3a3a3a!important;border-radius:999px!important}"
            + ".playbackTimeline__progress,.playbackTimeline__progressWrapper .progress{background:#f76b15!important}"
            + ".playbackTimeline__timePassed,.playbackTimeline__duration{color:#b4b4b4!important}"
            + ".volume__sliderBackground,.volume__sliderWrapper{background:#3a3a3a!important;border-radius:99px!important}"
            + ".volume button{background:transparent!important;border:1px solid #3a3a3a!important;border-radius:999px!important}"
            + ".playbackSoundBadge__title{color:#eeeeee!important}"
            + ".playbackSoundBadge__lightLink,.playbackSoundBadge__username{color:#b4b4b4!important}"
            + ".queue__items,.queue{ background:#191919!important;border:1px solid #2a2a2a!important;border-radius:12px!important}"
            + ".queueItem:hover,.queue__item:hover{background:#222222!important}"
            + ".queueItem.active,.queue__item.active{background:#331e0b!important;border-radius:8px!important}";

        public const string RdComments =
            ".commentItem,.comments__item{background:#191919!important;border:1px solid #2a2a2a!important;border-radius:12px!important;padding:10px 12px!important;margin-bottom:8px!important}"
            + ".commentItem__avatar,.commentItem img,.comments__avatar{border-radius:50%!important}"
            + ".commentItem__username,.commentItem a{color:#eeeeee!important}"
            + ".commentItem__timestamp,.commentItem time,.timeAgo{color:#7b7b7b!important}"
            + ".commentForm__input,.commentForm textarea{background:#222222!important;border:1px solid transparent!important;border-radius:8px!important;color:#eeeeee!important}"
            + ".commentForm__input:focus,.commentForm textarea:focus{border-color:#f76b15!important;box-shadow:0 0 0 1px #f76b15!important}"
            + ".commentItem__replyButton{background:transparent!important;border:1px solid #3a3a3a!important;border-radius:999px!important}"
            + ".commentItem .commentItem,.comments__item .comments__item{margin-left:16px!important}";

        public const string RdSidebar =
            ".l-sidebar-right aside,.sidebar,.sideNav{background:transparent!important}"
            + ".sidebarModule,.sidebarStats,.relatedTracks,.whoToFollow{background:#191919!important;border:1px solid #2a2a2a!important;border-radius:12px!important;padding:12px!important;margin-bottom:12px!important}"
            + ".sidebarHeader,.sidebarModule h3,.sidebarStats h3{color:#eeeeee!important}"
            + ".sidebarFooter,.footer,.l-footer{color:#7b7b7b!important}"
            + ".relatedTrack:hover,.sidebarTrack:hover{background:#222222!important;border-radius:8px!important}"
            + ".sideNav a:hover,.sidebar a:hover{background:#222222!important;border-radius:8px!important}"
            + ".sc-ministats{color:#b4b4b4!important}";

        public const string RdInputs =
            "input[type=text],input[type=search],input[type=email],input[type=password],.headerSearch__input{background:#222222!important;border:1px solid transparent!important;border-radius:999px!important;color:#eeeeee!important}"
            + "textarea,select{background:#222222!important;border:1px solid transparent!important;border-radius:8px!important;color:#eeeeee!important}"
            + "input::placeholder,textarea::placeholder{color:#7b7b7b!important}"
            + "input:focus,textarea:focus,select:focus{border-color:#f76b15!important;box-shadow:0 0 0 1px #f76b15!important;outline:none!important}"
            + ".searchTitle{color:#eeeeee!important}"
            + ".uploadForm input,.uploadForm textarea,.settingsForm input,.settingsForm textarea{border-radius:8px!important}"
            + "::selection{background:#7e451d!important;color:#fff!important}";

        public const string RdPopups =
            ".modal__modal,.modal,.dialog{background:#222222!important;border:1px solid #2a2a2a!important;border-radius:12px!important;box-shadow:0 20px 60px rgba(0,0,0,.6)!important}"
            + ".modal__title,.dialog h2,.modal h2{color:#eeeeee!important}"
            + ".modalBackground,.modal__overlay{background:rgba(0,0,0,.65)!important}"
            + ".dropdownContent,.header__navMenu,[role='menu'],.contextMenu{background:#222222!important;border:1px solid #2a2a2a!important;border-radius:12px!important;box-shadow:0 12px 32px rgba(0,0,0,.5)!important}"
            + ".dropdownContent a,.header__navMenu a,[role='menuitem']{border-radius:6px!important}"
            + ".dropdownContent a:hover,[role='menuitem']:hover{background:#2a2a2a!important}"
            + ".tooltip,.toast{background:#2a2a2a!important;border:1px solid #3a3a3a!important;border-radius:8px!important;color:#eeeeee!important}";
    }
}
