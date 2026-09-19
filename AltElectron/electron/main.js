/* SoundCloud Desktop Alt 3.2.0 - experimental Electron build.
   Radix shell (nav, sidebar, bottom bar) around a WebContentsView with
   soundcloud.com, plus player popup, settings, tray, hotkeys and extras. */
'use strict';
const { app, BrowserWindow, Tray, Menu, ipcMain, dialog, globalShortcut, session, shell, WebContentsView, Notification } = require('electron');
const path = require('path');
const fs = require('fs');

const APP_VERSION = '3.4.0';
const HOME_URL = 'https://soundcloud.com/';
const TOP_H = 52, BOTTOM_H = 46, SIDE_W = 210;

/* ---------------- logging ---------------- */
function logLine(m) {
  try { fs.appendFileSync(path.join(app.getPath('userData'), 'alt.log'), new Date().toISOString() + ' ' + m + '\n'); }
  catch (e) { /* ignore */ }
}

/* ---------------- store ---------------- */
const STORE_FILE = () => path.join(app.getPath('userData'), 'settings.json');
function defaultStore() {
  return {
    adblock: false, anims: true, redesign: true,
    rd: { rdCards: true, rdButtons: true, rdHeader: true, rdPlayer: true, rdComments: true, rdSidebar: true, rdInputs: true, rdPopups: true },
    playerPopup: true, trayHide: true, autostart: false, sidebarOpen: true,
    playAfterClose: true,
    extensions: [], lastUrl: HOME_URL, volume: 0.8, muted: false,
    speed: 1, repeatOne: false, notify: false, zoom: 1,
    recent: [], links: [], startMin: false, bossKey: false, alwaysOnTop: false,
    shellTheme: { appearance: 'dark', accent: 'orange' },
    sleep: 0, stats: { tracks: 0, seconds: 0 },
  };
}
function loadStore() {
  try {
    const raw = fs.readFileSync(STORE_FILE(), 'utf8');
    const s = Object.assign(defaultStore(), JSON.parse(raw));
    s.rd = Object.assign(defaultStore().rd, s.rd || {});
    s.shellTheme = Object.assign(defaultStore().shellTheme, s.shellTheme || {});
    s.stats = Object.assign(defaultStore().stats, s.stats || {});
    if (!Array.isArray(s.extensions)) s.extensions = [];
    if (!Array.isArray(s.recent)) s.recent = [];
    if (!Array.isArray(s.links)) s.links = [];
    return s;
  } catch (e) { return defaultStore(); }
}
function saveStore() {
  try { fs.writeFileSync(STORE_FILE(), JSON.stringify(store, null, 2)); }
  catch (e) { /* ignore */ }
}
let store = defaultStore();
let blockCount = 0;
let sleepLeft = 0;
let memSec = 0;

/* ---------------- site scripts (mirror of the WPF build) ---------------- */
const POLL_JS = `(function(){var t=0,d=0,playing=false,title='',art='',artist='';
try{var list=document.querySelectorAll('audio');
for(var i=0;i<list.length;i++){var m=list[i];
var dd=Math.floor(m.duration||0);
if(dd>d){d=dd;t=Math.floor(m.currentTime||0);}
if(!m.paused&&!m.ended&&m.currentTime>0)playing=true;}}catch(e){}
try{title=document.title||'';}catch(e){}
try{var ti=document.querySelector('.playbackSoundBadge__titleLink');
if(ti&&(ti.title||ti.textContent))title=ti.title||ti.textContent;}catch(e){}
try{var ar=document.querySelector('.playbackSoundBadge__lightLink');
if(ar&&(ar.title||ar.textContent))artist=ar.title||ar.textContent;}catch(e){}
try{var im=document.querySelector('.playbackSoundBadge__avatar img')||document.querySelector('.playControls__soundBadge img');
if(im&&im.src)art=im.src;}catch(e){}
return t+'|'+d+'|'+encodeURIComponent(title)+'|'+(playing?'1':'0')+'|'+encodeURIComponent(art)+'|'+encodeURIComponent(artist);})()`;

const TOGGLE_JS = `(function(){var b=document.querySelector('button[aria-label="Pause"]')
||document.querySelector('button[aria-label="Play"]')
||document.querySelector('.playControls__play');
if(!b)return 'no-btn';b.click();
var a=document.querySelector('audio');
return (a&&!a.paused)?'playing':'paused';})()`;

const NEXT_JS = `(function(){var b=document.querySelector('button[aria-label="Next track"]')
||document.querySelector('.skipControl__next');
if(!b)return 'no-btn';b.click();return 'ok';})()`;

const PREV_JS = `(function(){var b=document.querySelector('button[aria-label="Previous track"]')
||document.querySelector('.skipControl__previous');
if(!b)return 'no-btn';b.click();return 'ok';})()`;

const PAUSE_JS = `(function(){try{var a=document.querySelector('audio');if(a&&!a.paused){a.pause();return 'paused';}}catch(e){}return 'already';})()`;

const LIKE_JS = `(function(){try{var b=document.querySelector('.sc-button-like:not(.liked)')||document.querySelector('button[title="Like"]');if(!b)return 'no-btn';b.click();return 'ok';}catch(e){return 'err';}})()`;

const REPEAT_JS = `(function(){try{var a=document.querySelector('audio');if(a&&a.ended){a.currentTime=0;a.play();return 'looped';}}catch(e){}return 'no';})()`;

function speedJs(v) {
  return `(function(v){try{window.__scSpeed=v;document.querySelectorAll('audio,video').forEach(function(m){try{m.playbackRate=v;}catch(e){}});}catch(e){}})(${v})`;
}

function volumeJs(v) {
  return `(function(v){
window.__scVol=v;
try{document.querySelectorAll('audio,video').forEach(function(m){try{m.volume=v;}catch(e){}});}catch(e){}
function setSlider(val){
var w=document.querySelector('.volume__sliderWrapper');if(!w)return 'no-slider';
var t=w.querySelector('.volume__sliderBackground')||w;
var r=t.getBoundingClientRect();if(!r||r.height<2)return 'no-rect';
var x=r.left+r.width/2,y=r.top+r.height*(1-val);
function ev(type,el){try{el.dispatchEvent(new PointerEvent(type,{bubbles:true,cancelable:true,clientX:x,clientY:y,pointerId:1,isPrimary:true,buttons:1}));}catch(e){}}
ev('pointerover',w);ev('pointerenter',w);ev('pointerdown',w);ev('pointermove',document);ev('pointerup',document);
return 'ok';}
var r1=setSlider(v);
return r1;})(${v})`;
}

const PROMO_JS = `(function(){if(window.__scPromoKiller)return;window.__scPromoKiller=true;
var PH=['Uploading tracks just got way easier','Get heard by up to 100 listeners','Now available: Get heard'];
var BS=["[class*='banner']","[class*='Banner']","[class*='upsell']","[class*='Upsell']","[class*='promo']","[class*='Promo']","[class*='notice']","[class*='Notice']","[class*='callout']","[class*='Callout']"].join(',');
function hasAuth(el){try{return el.querySelector&&el.querySelector('input[type=password],input[type=email],input[type=text][autocomplete*=email],form[action*=login],form[action*=signin]');}catch(e){return null;}}
function hideRoot(el){var cur=el,g=0;
while(cur&&cur.parentElement&&g<8){var p=cur.parentElement;
if(p===document.body)break;
var tag=(p.tagName||'').toLowerCase();
if(tag!=='div'&&tag!=='section'&&tag!=='aside'&&tag!=='li')break;
var txt='';try{txt=p.textContent||'';}catch(e){}
if(txt.length>600)break;
if(hasAuth(p))break;
cur=p;g++;}
try{cur.style.setProperty('display','none','important');}catch(e){}}
function hasPhrase(el){var t='';try{t=el.textContent||'';}catch(e){}if(!t)return false;
for(var i=0;i<PH.length;i++){if(t.indexOf(PH[i])>=0)return true;}return false;}
function sweepText(){try{
var w=document.createTreeWalker(document.body,NodeFilter.SHOW_TEXT,null,false);
var n,found=[];
while(n=w.nextNode()){var t=n.nodeValue;if(!t)continue;
for(var i=0;i<PH.length;i++){if(t.indexOf(PH[i])>=0){found.push(n);break;}}}
for(var k=0;k<found.length;k++){if(found[k].parentElement)hideRoot(found[k].parentElement);}
}catch(e){}}
function sweepBoxes(){try{
var els=document.querySelectorAll(BS);
for(var i=0;i<els.length;i++){if(hasPhrase(els[i])&&!hasAuth(els[i]))hideRoot(els[i]);}
}catch(e){}}
function sweep(){if(document.hidden)return;sweepText();sweepBoxes();}
var t=null;function sch(){if(t||document.hidden)return;t=setTimeout(function(){t=null;sweep();},800);}
try{new MutationObserver(sch).observe(document.documentElement,{childList:true,subtree:true});}catch(e){}
sweep();setInterval(sweep,8000);})()`;

/* ---------------- site CSS fragments (same as the WPF build) ---------------- */
const SCROLL_CSS =
  'html{scrollbar-width:thin;scrollbar-color:rgba(255,255,255,.15) transparent!important}'
  + '::-webkit-scrollbar{width:6px!important;height:6px!important}'
  + '::-webkit-scrollbar-thumb{background:rgba(255,255,255,.15)!important;border-radius:99px!important;border:none!important}'
  + '::-webkit-scrollbar-thumb:hover{background:rgba(255,255,255,.35)!important}'
  + '::-webkit-scrollbar-track{background:transparent!important}';

const ANIM_CSS =
  '@keyframes scFadeUp{from{opacity:0;transform:translateY(10px)}to{opacity:1;transform:none}}'
  + '@keyframes scPopIn{from{opacity:0;transform:scale(.96) translateY(8px)}to{opacity:1;transform:none}}'
  + '@keyframes scDropIn{from{opacity:0;transform:translateY(-6px)}to{opacity:1;transform:none}}'
  + '.soundList__item,.searchItem,.chartTrack,.trackItem,.sound__body,.commentItem{animation:scFadeUp .45s ease both}'
  + 'button,.button,.sc-button{transition:transform .18s ease,background-color .18s ease,box-shadow .18s ease,border-color .18s ease!important}'
  + 'button:hover,.button:hover,.sc-button:hover{transform:translateY(-1px)}'
  + 'button:active,.button:active,.sc-button:active{transform:translateY(0) scale(.97)}'
  + 'a{transition:color .18s ease,opacity .18s ease}'
  + ".modal__modal,.modal,.dialog{animation:scPopIn .25s ease both}"
  + ".dropdownContent,.header__navMenu,[role='menu'],[role='dialog']{animation:scDropIn .2s ease both}"
  + '.playControls__play{transition:transform .15s ease!important}'
  + '.playControls__play:active{transform:scale(.92)!important}'
  + 'input,textarea{transition:border-color .18s ease,box-shadow .18s ease!important}';

const RD = {
  rdHeader:
    'header.header{background:#191919!important;border-bottom:1px solid #2a2a2a!important;box-shadow:0 1px 0 rgba(0,0,0,.4)!important}'
    + '.header__logo{background-size:contain!important}'
    + '.l-nav,.header__navMenuItem{transition:color .18s ease,box-shadow .18s ease!important}'
    + '.g-tabs-link.active,.header__navMenuItem.selected{box-shadow:inset 0 -2px 0 #f76b15!important}'
    + '.profileTabs__link.active,.g-tabs-link.active{color:#eeeeee!important}'
    + "[role='tablist']{border-bottom:1px solid #2a2a2a!important}"
    + "[role='tab']{border-radius:8px 8px 0 0!important;transition:box-shadow .18s ease,background-color .18s ease!important}"
    + "[role='tab']:hover{background:#222222!important}"
    + "[role='tab'][aria-selected='true']{box-shadow:inset 0 -2px 0 #f76b15!important}"
    + '.profileTabs,.tabs,.g-tabs{border-bottom:1px solid #2a2a2a!important}',
  rdCards:
    '.l-container,.l-fixed-top-one-column,.l-fullwidth{max-width:1280px!important}'
    + '.soundList__item,.trackItem,.searchItem,.chartTrack,.sound__content{background:#191919!important;border:1px solid #2a2a2a!important;border-radius:12px!important;padding:12px!important;margin-bottom:10px!important}'
    + '.soundList__item:hover,.trackItem:hover,.searchItem:hover{border-color:#3a3a3a!important;box-shadow:0 6px 20px rgba(0,0,0,.4)!important;transform:translateY(-1px)}'
    + '.soundTitle__title{color:#eeeeee!important}'
    + '.soundTitle__username,.trackItem__username,.soundContext__username{color:#b4b4b4!important}'
    + '.soundStats,.trackItem__stats,.statsList{color:#b4b4b4!important}'
    + '.badgeList__item{background:#222222!important;border:1px solid #2a2a2a!important;border-radius:8px!important}',
  rdButtons:
    'button.sc-button,.button,.sc-button-medium,.sc-button-large{border-radius:999px!important;font-weight:600!important}'
    + '.sc-button-primary,.sc-button-cta{background:#f76b15!important;border-color:#f76b15!important;color:#fff!important}'
    + '.sc-button-primary:hover,.sc-button-cta:hover{background:#ff801f!important;border-color:#ff801f!important}'
    + '.sc-button-secondary,.sc-button-small{background:transparent!important;border:1px solid #3a3a3a!important;color:#eeeeee!important}'
    + '.sc-button-secondary:hover{border-color:#606060!important}'
    + '.sc-button-like.liked,.sc-button-repost.reposted{color:#f76b15!important;border-color:#7e451d!important}',
  rdPlayer:
    '.playControls__bg,.playControls__inner{background:rgba(25,25,25,.94)!important;backdrop-filter:blur(16px)!important;border-top:1px solid #2a2a2a!important}'
    + '.playControls__elements button,.playControls__inner button{background:transparent!important;border:1px solid #3a3a3a!important;border-radius:999px!important;color:inherit!important}'
    + '.playControls__elements button:hover{border-color:#606060!important}'
    + '.playControls__play{background:transparent!important;border:1px solid #3a3a3a!important;border-radius:999px!important}'
    + '.playbackTimeline__progress,.playbackTimeline__progressWrapper .progress{background:#f76b15!important}'
    + '.playbackTimeline__timePassed,.playbackTimeline__duration{color:#b4b4b4!important}'
    + '.volume__sliderBackground,.volume__sliderWrapper{background:#3a3a3a!important;border-radius:99px!important}'
    + '.volume button{background:transparent!important;border:1px solid #3a3a3a!important;border-radius:999px!important}'
    + '.playbackSoundBadge__title{color:#eeeeee!important}'
    + '.playbackSoundBadge__lightLink,.playbackSoundBadge__username{color:#b4b4b4!important}'
    + '.queue__items,.queue{background:#191919!important;border:1px solid #2a2a2a!important;border-radius:12px!important}'
    + '.queueItem:hover,.queue__item:hover{background:#222222!important}'
    + '.queueItem.active,.queue__item.active{background:#331e0b!important;border-radius:8px!important}',
  rdComments:
    '.commentItem,.comments__item{background:#191919!important;border:1px solid #2a2a2a!important;border-radius:12px!important;padding:10px 12px!important;margin-bottom:8px!important}'
    + '.commentItem__avatar,.commentItem img,.comments__avatar{border-radius:50%!important}'
    + '.commentItem__username,.commentItem a{color:#eeeeee!important}'
    + '.commentItem__timestamp,.commentItem time,.timeAgo{color:#7b7b7b!important}'
    + '.commentForm__input,.commentForm textarea{background:#222222!important;border:1px solid transparent!important;border-radius:8px!important;color:#eeeeee!important}'
    + '.commentForm__input:focus,.commentForm textarea:focus{border-color:#f76b15!important;box-shadow:0 0 0 1px #f76b15!important}'
    + '.commentItem__replyButton{background:transparent!important;border:1px solid #3a3a3a!important;border-radius:999px!important}'
    + '.commentItem .commentItem,.comments__item .comments__item{margin-left:16px!important}',
  rdSidebar:
    '.l-sidebar-right aside,.sidebar,.sideNav{background:transparent!important}'
    + '.sidebarModule,.sidebarStats,.relatedTracks,.whoToFollow{background:#191919!important;border:1px solid #2a2a2a!important;border-radius:12px!important;padding:12px!important;margin-bottom:12px!important}'
    + '.sidebarHeader,.sidebarModule h3,.sidebarStats h3{color:#eeeeee!important}'
    + '.sidebarFooter,.footer,.l-footer{color:#7b7b7b!important}'
    + '.relatedTrack:hover,.sidebarTrack:hover{background:#222222!important;border-radius:8px!important}'
    + '.sideNav a:hover,.sidebar a:hover{background:#222222!important;border-radius:8px!important}'
    + '.sc-ministats{color:#b4b4b4!important}',
  rdInputs:
    'input[type=text],input[type=search],input[type=email],input[type=password],.headerSearch__input{background:#222222!important;border:1px solid transparent!important;border-radius:999px!important;color:#eeeeee!important}'
    + 'textarea,select{background:#222222!important;border:1px solid transparent!important;border-radius:8px!important;color:#eeeeee!important}'
    + 'input::placeholder,textarea::placeholder{color:#7b7b7b!important}'
    + 'input:focus,textarea:focus,select:focus{border-color:#f76b15!important;box-shadow:0 0 0 1px #f76b15!important;outline:none!important}'
    + '.searchTitle{color:#eeeeee!important}'
    + '.uploadForm input,.uploadForm textarea,.settingsForm input,.settingsForm textarea{border-radius:8px!important}'
    + '::selection{background:#7e451d!important;color:#fff!important}',
  rdPopups:
    '.modal__modal,.modal,.dialog{background:#222222!important;border:1px solid #2a2a2a!important;border-radius:12px!important;box-shadow:0 20px 60px rgba(0,0,0,.6)!important}'
    + '.modal__title,.dialog h2,.modal h2{color:#eeeeee!important}'
    + '.modalBackground,.modal__overlay{background:rgba(0,0,0,.65)!important}'
    + ".dropdownContent,.header__navMenu,[role='menu'],.contextMenu{background:#222222!important;border:1px solid #2a2a2a!important;border-radius:12px!important;box-shadow:0 12px 32px rgba(0,0,0,.5)!important}"
    + ".dropdownContent a,.header__navMenu a,[role='menuitem']{border-radius:6px!important}"
    + ".dropdownContent a:hover,[role='menuitem']:hover{background:#2a2a2a!important}"
    + '.tooltip,.toast{background:#2a2a2a!important;border:1px solid #3a3a3a!important;border-radius:8px!important;color:#eeeeee!important}',
};

function buildCss() {
  let css = SCROLL_CSS;
  if (store.anims) css += ANIM_CSS;
  if (store.redesign && store.rd) {
    for (const k of Object.keys(RD)) {
      if (store.rd[k] !== false) css += RD[k];
    }
  }
  return css;
}
function cssJs(css) {
  const safe = (css || '').replace(/\\/g, '\\\\').replace(/'/g, "\\'");
  return `(function(){var s=document.getElementById('__scNative');`
    + `if(!s){s=document.createElement('style');s.id='__scNative';`
    + `(document.head||document.documentElement).appendChild(s);}`
    + `s.textContent='${safe}';})()`;
}

/* ---------------- adblock (mirror of the WPF build) ---------------- */
const AD_HOSTS = ['doubleclick.net', 'googlesyndication.com', 'googleadservices.com',
  'googletagservices.com', '2mdn.net', 'adsrvr.org', 'moatads.com', 'criteo.com',
  'criteo.net', 'outbrain.com', 'taboola.com', 'amazon-adsystem.com', 'pubmatic.com',
  'rubiconproject.com', 'openx.net', 'ads.yahoo.com', 'advertising.com', 'smartadserver.com',
  'freewheel.tv', 'fwmrm.net', 'adswizz.com', 'spotxchange.com', 'spotx.tv',
  'springserve.com', 'hilltopads.net', 'popads.net', 'popcash.net', 'propellerads.com',
  'adcash.com', 'exoclick.com', 'adsterra.com', 'teads.tv', 'teads.com',
  'adform.com', 'adform.net', 'sizmek.com', 'quantserve.com', 'adnxs.com'];
const AD_PATTERNS = ['/ads/', '/ads.', '/adserver', '/adservice', 'adsystem', 'pagead',
  'doubleclick', 'googlesyndication', 'adsense', 'prebid', 'criteo', 'taboola',
  'outbrain', 'spotx', 'springserve', 'freewheel', 'fbevents', 'facebook.com/tr',
  'vast', 'vpaid', 'ima3', 'imasdk', 'googleads', 'preroll', 'midroll', 'postroll',
  'adbreak', 'unruly', 'teads'];
const AUTH_HOSTS = ['soundcloud.com', 'api.soundcloud.com', 'sndcdn.com',
  'accounts.google.com', 'apis.google.com', 'ssl.gstatic.com', 'www.gstatic.com',
  'googleusercontent.com', 'gstatic.com', 'accounts.youtube.com',
  'appleid.apple.com', 'id.apple.com',
  'facebook.com', 'facebook.net', 'fbcdn.net', 'connect.facebook.net'];
function isAuthUrl(u) {
  const low = u.toLowerCase();
  if (low.includes('accounts.google.com') || low.includes('appleid.apple.com')
    || low.includes('id.apple.com') || low.includes('apis.google.com')
    || low.includes('googleusercontent.com') || low.includes('gstatic.com')
    || low.includes('connect.facebook') || low.includes('fbcdn.net')
    || low.includes('/login') || low.includes('/signin')
    || low.includes('/signup') || low.includes('/register') || low.includes('/oauth')
    || low.includes('/auth') || low.includes('facebook.com/login')
    || low.includes('facebook.com/dialog')) return true;
  try {
    const host = new URL(u).hostname.toLowerCase();
    return AUTH_HOSTS.some((h) => host === h || host.endsWith('.' + h));
  } catch (e) { return false; }
}
function shouldBlock(u) {
  if (!store.adblock || !u) return false;
  if (isAuthUrl(u)) return false;
  if (!u.startsWith('http://') && !u.startsWith('https://')) return false;
  let host = '';
  try { host = new URL(u).hostname.toLowerCase(); }
  catch (e) { return false; }
  if (AD_HOSTS.some((h) => host === h || host.endsWith('.' + h))) return true;
  const hay = u.toLowerCase();
  return AD_PATTERNS.some((p) => hay.includes(p));
}

/* ---------------- windows ---------------- */
let mainWin = null, siteView = null, playerWin = null, settingsWin = null, tray = null;
let allowExit = false;
let wasPlaying = false, lastTrackKey = '';
let pipWins = [];

const PARTITION = 'persist:scd';
const appDir = (__dirname.endsWith('electron') || __dirname.endsWith('electron/'))
  ? path.join(__dirname, '..') : __dirname;
function asset(name) { return path.join(appDir, 'assets', name); }
function uiFile(name) { return path.join(appDir, 'dist', name); }

function showMain() {
  if (!mainWin) return;
  if (!mainWin.isVisible()) mainWin.show();
  if (mainWin.isMinimized()) mainWin.restore();
  mainWin.focus();
}

function siteWC() { return siteView ? siteView.webContents : null; }

function pushStore() {
  const payload = Object.assign({}, store, { blocks: blockCount });
  for (const w of [mainWin, playerWin, settingsWin]) {
    try { if (w) w.webContents.send('store-changed', payload); } catch (e) { /* ignore */ }
  }
}

function layoutSiteView() {
  if (!mainWin || !siteView) return;
  try {
    const [w, h] = mainWin.getContentSize();
    const x = store.sidebarOpen === false ? 0 : SIDE_W;
    siteView.setBounds({ x, y: TOP_H, width: Math.max(200, w - x), height: Math.max(200, h - TOP_H - BOTTOM_H) });
  } catch (e) { /* ignore */ }
}

function sendNavState() {
  const wc = siteWC();
  if (!wc || !mainWin) return;
  try {
    mainWin.webContents.send('nav-state', {
      url: wc.getURL(), canBack: wc.canGoBack(), canFwd: wc.canGoForward(),
    });
  } catch (e) { /* ignore */ }
}

function injectSite() {
  const wc = siteWC();
  if (!wc) return;
  wc.executeJavaScript(cssJs(buildCss())).catch(() => {});
  wc.executeJavaScript(PROMO_JS).catch(() => {});
  try { wc.setZoomFactor(store.zoom || 1); } catch (e) { /* ignore */ }
  if (store.speed && store.speed !== 1) wc.executeJavaScript(speedJs(store.speed)).catch(() => {});
}

function createMain() {
  mainWin = new BrowserWindow({
    width: 1180, height: 760, minWidth: 860, minHeight: 560,
    title: 'SoundCloud Desktop Alt',
    autoHideMenuBar: true, show: !store.startMin,
    icon: asset('icon.ico'),
    webPreferences: { preload: path.join(__dirname, 'preload-shell.js'), contextIsolation: true },
  });
  mainWin.loadFile(uiFile('shell.html'));
  mainWin.setAlwaysOnTop(!!store.alwaysOnTop);

  siteView = new WebContentsView({ webPreferences: { partition: PARTITION } });
  mainWin.contentView.addChildView(siteView);
  layoutSiteView();
  siteView.webContents.loadURL(store.lastUrl || HOME_URL);

  let lastPopupAt = 0;
  siteView.webContents.setWindowOpenHandler(({ url }) => {
    if (!url.startsWith('http://') && !url.startsWith('https://')) return { action: 'deny' };
    lastPopupAt = Date.now();
    return {
      action: 'allow',
      overrideBrowserWindowOptions: {
        width: 500, height: 680, title: 'Sign in',
        autoHideMenuBar: true,
        icon: asset('icon.ico'),
        webPreferences: { partition: PARTITION },
      },
    };
  });
  app.on('browser-window-created', (_e, win) => {
    if (Date.now() - lastPopupAt > 8000) return;
    try {
      win.webContents.on('did-navigate', (_ev, url) => {
        try {
          const u = new URL(url);
          const host = u.hostname.toLowerCase();
          const isSC = host === 'soundcloud.com' || host.endsWith('.soundcloud.com');
          if (isSC && !url.includes('w.soundcloud.com/player')) {
            try { win.close(); } catch (e) { /* ignore */ }
            const wc = siteWC();
            if (wc) wc.reload();
            showMain();
          }
        } catch (e) { /* ignore */ }
      });
    } catch (e) { /* ignore */ }
  });

  siteView.webContents.on('did-finish-load', () => { injectSite(); applyVolume(); sendNavState(); });
  siteView.webContents.on('did-navigate-in-page', () => { injectSite(); sendNavState(); });
  siteView.webContents.on('did-navigate', () => { sendNavState(); });
  siteView.webContents.on('before-input-event', (e, input) => {
    if (input.control && !input.alt && !input.meta) {
      if (input.key === '=' || input.key === '+' || input.key === 'Add') {
        e.preventDefault(); setZoom((store.zoom || 1) + 0.1);
      } else if (input.key === '-' || input.key === 'Subtract') {
        e.preventDefault(); setZoom((store.zoom || 1) - 0.1);
      } else if (input.key === '0') { e.preventDefault(); setZoom(1); }
    }
  });

  mainWin.on('resize', layoutSiteView);
  mainWin.on('hide', () => setPollMs(4000));
  mainWin.on('show', () => setPollMs(1500));
  mainWin.on('minimize', () => {
    try { if (store.trayHide && mainWin) mainWin.hide(); } catch (e) { /* ignore */ }
    setPollMs(4000);
  });
  mainWin.on('restore', () => setPollMs(1500));
  mainWin.on('close', (e) => {
    const keep = store.playAfterClose !== false;
    if (keep && !allowExit) { e.preventDefault(); mainWin.hide(); }
  });
  mainWin.on('closed', () => { mainWin = null; siteView = null; });
}

function setZoom(z) {
  store.zoom = Math.min(2, Math.max(0.5, Math.round(z * 10) / 10));
  saveStore();
  const wc = siteWC();
  if (wc) { try { wc.setZoomFactor(store.zoom); } catch (e) { /* ignore */ } }
  pushStore();
}

function widgetUrl(pageUrl, autoplay) {
  return 'https://w.soundcloud.com/player/?url=' + encodeURIComponent(pageUrl)
    + '&color=%23f76b15&auto_play=' + (autoplay ? 'true' : 'false')
    + '&hide_related=false&show_comments=true&show_user=true'
    + '&show_reposts=false&show_teaser=true&visual=true';
}

function openPip(pageUrl) {
  let url = (pageUrl || '').trim();
  if (!url) {
    const wc = siteWC();
    if (wc) url = wc.getURL();
  }
  if (!url || !url.startsWith('http')) return;
  try {
    const w = new BrowserWindow({
      width: 440, height: 540, minWidth: 280, minHeight: 300,
      title: 'PiP player', alwaysOnTop: true, autoHideMenuBar: true,
      icon: asset('icon.ico'),
      webPreferences: { partition: PARTITION },
    });
    w.loadURL(widgetUrl(url, true));
    w.on('closed', () => { pipWins = pipWins.filter((x) => x !== w); });
    pipWins.push(w);
  } catch (e) { logLine('pip failed: ' + e.message); }
}

function createPlayer() {
  playerWin = new BrowserWindow({
    width: 400, height: 216, minWidth: 340, minHeight: 190,
    title: 'Now playing', show: false, autoHideMenuBar: true,
    icon: asset('icon.ico'),
    webPreferences: { preload: path.join(__dirname, 'preload-shell.js'), contextIsolation: true },
  });
  playerWin.loadFile(uiFile('player.html'));
  if (store.mini) applyMini();
  playerWin.on('close', (e) => { e.preventDefault(); playerWin.hide(); });
}

function applyMini() {
  if (!playerWin) return;
  try {
    if (store.mini) { playerWin.setSize(360, 132); playerWin.setAlwaysOnTop(true); }
    else { playerWin.setSize(400, 216); playerWin.setAlwaysOnTop(false); }
  } catch (e) { /* ignore */ }
}

function openSettings() {
  if (settingsWin) { settingsWin.show(); settingsWin.focus(); return; }
  settingsWin = new BrowserWindow({
    width: 460, height: 720, minWidth: 380, minHeight: 520,
    title: 'Settings', autoHideMenuBar: true,
    icon: asset('icon.ico'),
    webPreferences: { preload: path.join(__dirname, 'preload-shell.js'), contextIsolation: true },
  });
  settingsWin.loadFile(uiFile('settings.html'));
  settingsWin.on('close', (e) => { e.preventDefault(); settingsWin.hide(); });
}

function setupTray() {
  tray = new Tray(asset('sc-icon.png'));
  tray.setToolTip('SoundCloud Desktop Alt');
  tray.setContextMenu(Menu.buildFromTemplate([
    { label: 'Open', click: showMain },
    { label: 'Now playing', click: () => { if (playerWin) playerWin.show(); showMain(); } },
    { label: 'Settings', click: openSettings },
    { type: 'separator' },
    { label: 'Quit', click: () => { allowExit = true; app.quit(); } },
  ]));
  tray.on('double-click', showMain);
}

function applyAutostart() {
  try { app.setLoginItemSettings({ openAtLogin: !!store.autostart, path: process.execPath }); }
  catch (e) { /* ignore */ }
}

function applyVolume() {
  const wc = siteWC();
  if (!wc) return;
  const v = store.muted ? 0 : store.volume;
  try { wc.setAudioMuted(!!store.muted); } catch (e) { /* ignore */ }
  wc.executeJavaScript(volumeJs(v)).catch(() => {});
}

function fmtTime(sec) {
  sec = Math.max(0, (parseInt(sec, 10) || 0));
  const h = (sec / 3600) | 0, m = ((sec % 3600) / 60) | 0, s = sec % 60;
  const p = (n) => (n < 10 ? '0' + n : '' + n);
  return h > 0 ? h + ':' + p(m) + ':' + p(s) : m + ':' + p(s);
}

async function clickPlayer(js) {
  const wc = siteWC();
  if (!wc) return 'no-btn';
  for (let i = 0; i < 3; i++) {
    try {
      const r = await wc.executeJavaScript(js);
      if (r === 'playing' || r === 'paused' || r === 'ok') return r;
    } catch (e) { /* ignore */ }
    await new Promise((r) => setTimeout(r, 300));
  }
  return 'no-btn';
}

function trackTooltip(title, playing) {
  if (!tray) return;
  try {
    const t = ((playing ? 'Playing: ' : 'Paused: ') + (title || 'SoundCloud Desktop Alt')).slice(0, 120);
    tray.setToolTip(t);
  } catch (e) { /* ignore */ }
}

function notifyTrack(title, artist) {
  try {
    if (Notification.isSupported()) new Notification({ title, body: artist || 'SoundCloud' }).show();
  } catch (e) { /* ignore */ }
}

function pushRecent(title, artist, url) {
  const key = title + '||' + artist;
  store.recent = (store.recent || []).filter((r) => (r.title + '||' + r.artist) !== key);
  store.recent.unshift({ title, artist, url, at: Date.now() });
  store.recent = store.recent.slice(0, 20);
  saveStore();
  updateJumpList();
  pushStore();
}

let pollTimer = null;
let pollMs = 1500;
function setPollMs(ms) {
  if (ms === pollMs) return;
  pollMs = ms;
  if (pollTimer) { clearInterval(pollTimer); pollTimer = null; }
  pollTimer = setInterval(pollOnce, pollMs);
}
function startPoll() {
  setPollMs(mainWin && !mainWin.isVisible() ? 4000 : 1500);
}
async function pollOnce() {
    const wc = siteWC();
    if (!wc) return;
    try {
      const raw = await wc.executeJavaScript(POLL_JS);
      const p = String(raw).split('|');
      if (p.length < 6) return;
      const cur = parseInt(p[0], 10) || 0, dur = parseInt(p[1], 10) || 0;
      const title = decodeURIComponent(p[2] || '');
      const playing = p[3] === '1';
      const art = decodeURIComponent(p[4] || '');
      const artist = decodeURIComponent(p[5] || '');
      const time = fmtTime(cur) + ' / ' + fmtTime(dur);
      const key = title + '||' + artist;
      const info = { title, artist, art, time, playing, cur, dur };
      if (playerWin) playerWin.webContents.send('track', info);
      if (mainWin) mainWin.webContents.send('track', info);
      trackTooltip(title, playing);
      if (playing) {
        memSec += pollMs / 1000;
        if (store.repeatOne) wc.executeJavaScript(REPEAT_JS).catch(() => {});
        if (store.sleep > 0) {
          sleepLeft -= pollMs / 1000;
          if (sleepLeft <= 0) {
            store.sleep = 0; sleepLeft = 0; saveStore();
            wc.executeJavaScript(PAUSE_JS).catch(() => {});
            pushStore();
          }
        }
      }
      if (playing && key !== lastTrackKey && title) {
        store.stats.tracks = (store.stats.tracks || 0) + 1;
        store.stats.seconds = (store.stats.seconds || 0) + memSec;
        memSec = 0;
        saveStore();
        applyVolume();
        if (store.speed && store.speed !== 1) wc.executeJavaScript(speedJs(store.speed)).catch(() => {});
        pushRecent(title, artist, wc.getURL());
        if (store.notify) notifyTrack(title, artist);
        if (store.playerPopup && playerWin && !playerWin.isVisible()) playerWin.show();
      }
      wasPlaying = playing;
      lastTrackKey = key;
    } catch (e) { /* page not ready */ }
}

function updateJumpList() {
  try {
    const items = (store.recent || []).slice(0, 5).map((r) => ({
      type: 'task', program: process.execPath,
      args: '--open-url=' + r.url,
      title: String(r.title || 'Unknown').slice(0, 60),
      description: String(r.artist || '').slice(0, 120),
      iconPath: process.execPath, iconIndex: 0,
    }));
    app.setJumpList([{ type: 'custom', name: 'Recently played', items }]);
  } catch (e) { /* not on Windows or failed */ }
}

function openUrlArg(argv) {
  for (const a of argv || []) {
    if (a.startsWith('--open-url=')) return a.slice(11);
  }
  return null;
}

function setupShortcuts() {
  try {
    globalShortcut.register('num1', () => clickPlayer(PREV_JS));
    globalShortcut.register('num2', () => clickPlayer(TOGGLE_JS));
    globalShortcut.register('num3', () => clickPlayer(NEXT_JS));
    globalShortcut.register('num4', () => {
      store.volume = Math.max(0, store.volume - 0.05); saveStore(); applyVolume(); pushStore();
    });
    globalShortcut.register('num5', () => {
      store.volume = Math.min(1, store.volume + 0.05); saveStore(); applyVolume(); pushStore();
    });
    if (store.bossKey) {
      globalShortcut.register('F9', () => {
        for (const w of [mainWin, playerWin, settingsWin, ...pipWins]) {
          try { if (w) w.hide(); } catch (e) { /* ignore */ }
        }
      });
    }
  } catch (e) { logLine('hotkeys failed: ' + e.message); }
}

function refreshShortcuts() {
  try {
    globalShortcut.unregisterAll();
    setupShortcuts();
  } catch (e) { /* ignore */ }
}

async function loadExtensions() {
  const ses = session.fromPartition(PARTITION);
  for (const dir of store.extensions || []) {
    try {
      if (dir && fs.existsSync(dir)) await ses.loadExtension(dir, { allowFileAccess: true });
    } catch (e) { logLine('ext failed ' + dir + ': ' + e.message); }
  }
}

function setupAdblock() {
  const ses = session.fromPartition(PARTITION);
  try {
    ses.setUserAgent('Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36');
  } catch (e) { /* ignore */ }
  ses.webRequest.onBeforeRequest({ urls: ['*://*/*'] }, adblockListener);
}
function adblockListener(details, callback) {
  try {
    if (shouldBlock(details.url)) { blockCount++; return callback({ cancel: true }); }
  } catch (e) { /* ignore */ }
  callback({});
}

function cmpVer(a, b) {
  const pa = String(a).replace(/^v/i, '').split('.').map((x) => parseInt(x, 10) || 0);
  const pb = String(b).replace(/^v/i, '').split('.').map((x) => parseInt(x, 10) || 0);
  for (let i = 0; i < Math.max(pa.length, pb.length); i++) {
    const d = (pa[i] || 0) - (pb[i] || 0);
    if (d !== 0) return d;
  }
  return 0;
}

async function checkUpdates() {
  try {
    const r = await fetch('https://api.github.com/repos/ToraScriptCopy/soundcloud-desktop/releases/latest', {
      headers: { 'User-Agent': 'scd-alt', Accept: 'application/vnd.github+json' },
    });
    if (!r.ok) return { ok: false, msg: 'Check failed.' };
    const j = await r.json();
    const newer = cmpVer(j.tag_name, APP_VERSION) > 0;
    return { ok: true, hasUpdate: newer, tag: j.tag_name, url: j.html_url, msg: newer ? ('New version ' + j.tag_name + ' is out.') : 'You are on the latest version.' };
  } catch (e) { return { ok: false, msg: 'Check failed.' }; }
}

/* ---------------- IPC ---------------- */
ipcMain.handle('store-get', () => Object.assign({}, store, { blocks: blockCount }));
ipcMain.handle('store-set', (_e, patch) => {
  Object.assign(store, patch || {});
  if (patch && typeof patch.sleep !== 'undefined' && store.sleep > 0 && sleepLeft <= 0) {
    sleepLeft = store.sleep * 60;
  }
  saveStore();
  if (patch && (patch.redesign || patch.rd || patch.anims)) injectSite();
  if (patch && typeof patch.autostart !== 'undefined') applyAutostart();
  if (patch && typeof patch.alwaysOnTop !== 'undefined' && mainWin) {
    try { mainWin.setAlwaysOnTop(!!store.alwaysOnTop); } catch (e) { /* ignore */ }
  }
  if (patch && typeof patch.bossKey !== 'undefined') refreshShortcuts();
  pushStore();
  return true;
});
ipcMain.handle('shell-cmd', async (e, name, arg) => {
  const wc = siteWC();
  if (name === 'toggle') return clickPlayer(TOGGLE_JS);
  if (name === 'next') return clickPlayer(NEXT_JS);
  if (name === 'prev') return clickPlayer(PREV_JS);
  if (name === 'like') { if (wc) return wc.executeJavaScript(LIKE_JS).catch(() => 'err'); return 'no-btn'; }
  if (name === 'volume' && arg) {
    store.volume = Math.min(1, Math.max(0, arg.volume));
    store.muted = !!arg.muted;
    saveStore(); applyVolume(); return true;
  }
  if (name === 'to-tray') {
    if (playerWin) playerWin.hide();
    if (mainWin) mainWin.hide();
    return true;
  }
  if (name === 'player-hide') { if (playerWin) playerWin.hide(); return true; }
  if (name === 'mini-toggle') {
    store.mini = !store.mini; saveStore(); applyMini(); pushStore(); return !!store.mini;
  }
  if (name === 'layout' && arg) {
    store.sidebarOpen = arg.sidebarOpen !== false;
    saveStore(); layoutSiteView(); return true;
  }
  if (name === 'nav-back') { if (wc && wc.canGoBack()) wc.goBack(); return true; }
  if (name === 'nav-fwd') { if (wc && wc.canGoForward()) wc.goForward(); return true; }
  if (name === 'nav-reload') { if (wc) wc.reload(); return true; }
  if (name === 'nav-go' && typeof arg === 'string' && wc) { wc.loadURL(arg); return true; }
  if (name === 'pip-open') { openPip(typeof arg === 'string' ? arg : ''); return true; }
  if (name === 'copy-link') {
    try {
      const { clipboard } = require('electron');
      if (wc) clipboard.writeText(wc.getURL());
    } catch (err) { /* ignore */ }
    return true;
  }
  if (name === 'open-ext') {
    if (wc) { try { await shell.openExternal(wc.getURL()); } catch (err) { /* ignore */ } }
    return true;
  }
  if (name === 'open-settings') { openSettings(); return true; }
  if (name === 'link-add' && arg && arg.url) {
    store.links = store.links || [];
    if (!store.links.some((l) => l.url === arg.url)) {
      store.links.push({ label: arg.label || arg.url, url: arg.url });
      saveStore(); pushStore();
    }
    return store.links;
  }
  if (name === 'link-remove') {
    store.links = (store.links || []).filter((l) => l.url !== arg);
    saveStore(); pushStore(); return store.links;
  }
  if (name === 'recent-clear') {
    store.recent = []; saveStore(); pushStore();
    try { app.setJumpList([]); } catch (err) { /* ignore */ }
    return true;
  }
  if (name === 'pin-start') {
    try {
      const dir = path.join(app.getPath('appData'), 'Microsoft', 'Windows', 'Start Menu', 'Programs');
      const file = path.join(dir, 'SoundCloud Desktop Alt.url');
      if (arg) {
        fs.mkdirSync(dir, { recursive: true });
        const target = 'file:///' + process.execPath.replace(/\\/g, '/');
        fs.writeFileSync(file, '[InternetShortcut]\r\nURL=' + target + '\r\nIconFile=' + process.execPath + '\r\nIconIndex=0\r\n');
      } else if (fs.existsSync(file)) {
        fs.rmSync(file, { force: true });
      }
      store.pinStart = fs.existsSync(file);
      saveStore(); pushStore();
      return store.pinStart;
    } catch (e) { return !!store.pinStart; }
  }
  if (name === 'ext-add') {
    const r = await dialog.showOpenDialog({ properties: ['openDirectory'] });
    if (r.canceled || !r.filePaths[0]) return { msg: 'Cancelled.' };
    const dir = r.filePaths[0];
    try {
      await session.fromPartition(PARTITION).loadExtension(dir, { allowFileAccess: true });
      if (!store.extensions.includes(dir)) store.extensions.push(dir);
      saveStore();
      return { list: store.extensions, msg: 'Extension installed.' };
    } catch (err) { return { msg: 'Failed: ' + err.message }; }
  }
  if (name === 'ext-remove') {
    store.extensions = (store.extensions || []).filter((d) => d !== arg);
    saveStore();
    return { list: store.extensions, msg: 'Removed. Restart the app to unload it.' };
  }
  if (name === 'cache-clear') {
    try { await session.fromPartition(PARTITION).clearStorageData(); return { msg: 'Cache cleared.' }; }
    catch (err) { return { msg: 'Failed.' }; }
  }
  if (name === 'check-updates') return checkUpdates();
  if (name === 'settings-export') {
    const r = await dialog.showSaveDialog({ defaultPath: 'scd-alt-settings.json', filters: [{ name: 'JSON', extensions: ['json'] }] });
    if (r.canceled || !r.filePath) return { msg: 'Cancelled.' };
    try { fs.writeFileSync(r.filePath, JSON.stringify(store, null, 2)); return { msg: 'Settings exported.' }; }
    catch (err) { return { msg: 'Failed: ' + err.message }; }
  }
  if (name === 'settings-import') {
    const r = await dialog.showOpenDialog({ properties: ['openFile'], filters: [{ name: 'JSON', extensions: ['json'] }] });
    if (r.canceled || !r.filePaths[0]) return { msg: 'Cancelled.' };
    try {
      const incoming = JSON.parse(fs.readFileSync(r.filePaths[0], 'utf8'));
      Object.assign(store, incoming);
      saveStore(); applyAutostart();
      if (mainWin) { try { mainWin.setAlwaysOnTop(!!store.alwaysOnTop); } catch (err) { /* ignore */ } }
      refreshShortcuts(); injectSite(); applyVolume(); pushStore();
      return { msg: 'Settings imported.' };
    } catch (err) { return { msg: 'Failed: ' + err.message }; }
  }
  if (name === 'data-open') {
    try { await shell.openPath(app.getPath('userData')); } catch (err) { /* ignore */ }
    return true;
  }
  if (name === 'data-reset') {
    try { await session.fromPartition(PARTITION).clearStorageData(); } catch (err) { /* ignore */ }
    const keepLang = null;
    store = defaultStore();
    saveStore(); applyAutostart(); refreshShortcuts();
    injectSite(); applyVolume(); pushStore();
    return { msg: 'Data cleared. Settings are back to defaults.' };
  }
  if (name === 'stats-reset') {
    store.stats = { tracks: 0, seconds: 0 }; memSec = 0; saveStore(); pushStore(); return true;
  }
  return false;
});

/* ---------------- boot ---------------- */
function openArgUrl(u) {
  if (!u) return;
  showMain();
  const wc = siteWC();
  if (wc) wc.loadURL(u);
}

const gotLock = app.requestSingleInstanceLock();
if (!gotLock) { app.quit(); }
else {
  app.on('second-instance', (_e, argv) => {
    logLine('second instance, restoring main window');
    const u = openUrlArg(argv);
    if (u) openArgUrl(u);
    else showMain();
  });
  app.whenReady().then(async () => {
    store = loadStore();
    logLine('start version=' + APP_VERSION);
    setupAdblock();
    createMain();
    createPlayer();
    setupTray();
    applyAutostart();
    applyVolume();
    await loadExtensions();
    updateJumpList();
    startPoll();
    setupShortcuts();
    const firstUrl = openUrlArg(process.argv);
    if (firstUrl) openArgUrl(firstUrl);
    setTimeout(async () => {
      const r = await checkUpdates();
      logLine('update check: ' + r.msg);
    }, 20000);
    app.on('activate', showMain);
  });
  app.on('window-all-closed', () => { /* tray keeps us alive */ });
  app.on('before-quit', () => {
    allowExit = true;
    try {
      store.stats.seconds = (store.stats.seconds || 0) + memSec;
      memSec = 0;
      const wc = siteWC();
      if (wc && wc.getURL().startsWith('http')) store.lastUrl = wc.getURL();
      saveStore();
    } catch (e) { /* ignore */ }
    globalShortcut.unregisterAll();
  });
}
