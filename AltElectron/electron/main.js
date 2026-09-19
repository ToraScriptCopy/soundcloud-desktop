/* SoundCloud Desktop Alt 2.0 - Electron shell with real Radix Themes UI.
   Main window is a Radix shell (nav, sidebar, bottom bar) around a
   WebContentsView that loads soundcloud.com with the shared ReDesign CSS/JS. */
'use strict';
const { app, BrowserWindow, Tray, Menu, ipcMain, dialog, globalShortcut, session, shell, WebContentsView } = require('electron');
const path = require('path');
const fs = require('fs');

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
    extensions: [], lastUrl: HOME_URL, volume: 0.8, muted: false,
  };
}
function loadStore() {
  try {
    const raw = fs.readFileSync(STORE_FILE(), 'utf8');
    const s = Object.assign(defaultStore(), JSON.parse(raw));
    s.rd = Object.assign(defaultStore().rd, s.rd || {});
    if (!Array.isArray(s.extensions)) s.extensions = [];
    return s;
  } catch (e) { return defaultStore(); }
}
function saveStore() {
  try { fs.writeFileSync(STORE_FILE(), JSON.stringify(store, null, 2)); }
  catch (e) { /* ignore */ }
}
let store = defaultStore();

/* ---------------- site scripts (mirror of the WPF build) ---------------- */
const POLL_JS = `(function(){var t=0,d=0,playing=false,title='',art='',artist='';
try{var a=document.querySelector('audio');
if(a){t=Math.floor(a.currentTime||0);d=Math.floor(a.duration||0);playing=!a.paused;}}catch(e){}
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
try{if(!window.__scVolObs){window.__scVolObs=new MutationObserver(function(muts){muts.forEach(function(mu){if(!mu.addedNodes)return;for(var i=0;i<mu.addedNodes.length;i++){var nd=mu.addedNodes[i];if(!nd||!nd.querySelectorAll)continue;try{nd.querySelectorAll('audio,video').forEach(function(m){try{m.volume=window.__scVol;}catch(e){}});}catch(e){}}});});window.__scVolObs.observe(document.documentElement,{childList:true,subtree:true});}}catch(e){}
return r1;})(${v})`;
}

const PROMO_JS = `(function(){if(window.__scPromoKiller)return;window.__scPromoKiller=true;
var PH=['Uploading tracks just got way easier','Get heard by up to 100 listeners','Now available: Get heard'];
function sweep(){try{
var w=document.createTreeWalker(document.body,NodeFilter.SHOW_TEXT,null,false);
var n,found=[];
while(n=w.nextNode()){var t=n.nodeValue;if(!t)continue;
for(var i=0;i<PH.length;i++){if(t.indexOf(PH[i])>=0){found.push(n);break;}}}
for(var k=0;k<found.length;k++){var el=found[k].parentElement,g=0;
while(el&&el!==document.body&&g<5){
if(el.querySelector&&el.querySelector('input[type=password],input[type=email]'))break;
var tag=(el.tagName||'').toLowerCase();
if(tag==='div'||tag==='section'||tag==='aside'||tag==='li'){el.style.setProperty('display','none','important');break;}
el=el.parentElement;g++;}}
}catch(e){}}
var t=null;function sch(){if(t)return;t=setTimeout(function(){t=null;sweep();},300);}
try{new MutationObserver(sch).observe(document.documentElement,{childList:true,subtree:true});}catch(e){}
sweep();setInterval(sweep,3000);})()`;

/* ---------------- site CSS fragments (same as the WPF build) ---------------- */
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
    + '.profileTabs__link.active,.g-tabs-link.active{color:#eeeeee!important}',
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
    + '.playControls__elements button,.playControls__inner button{background:transparent!important;border:1px solid #3a3a3a!important;border-radius:999px!important}'
    + '.playControls__elements button:hover{border-color:#606060!important}'
    + '.playControls__play{background:#f76b15!important;border-color:#f76b15!important;color:#fff!important}'
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
    + '.commentItem__replyButton{background:transparent!important;border:1px solid #3a3a3a!important;border-radius:999px!important}',
  rdSidebar:
    '.l-sidebar-right aside,.sidebar,.sideNav{background:transparent!important}'
    + '.sidebarModule,.sidebarStats,.relatedTracks,.whoToFollow{background:#191919!important;border:1px solid #2a2a2a!important;border-radius:12px!important;padding:12px!important;margin-bottom:12px!important}'
    + '.sidebarHeader,.sidebarModule h3,.sidebarStats h3{color:#eeeeee!important}'
    + '.sidebarFooter,.footer,.l-footer{color:#7b7b7b!important}'
    + '.relatedTrack:hover,.sidebarTrack:hover{background:#222222!important;border-radius:8px!important}'
    + '.sc-ministats{color:#b4b4b4!important}',
  rdInputs:
    'input[type=text],input[type=search],input[type=email],input[type=password],.headerSearch__input{background:#222222!important;border:1px solid transparent!important;border-radius:999px!important;color:#eeeeee!important}'
    + 'textarea,select{background:#222222!important;border:1px solid transparent!important;border-radius:8px!important;color:#eeeeee!important}'
    + 'input::placeholder,textarea::placeholder{color:#7b7b7b!important}'
    + 'input:focus,textarea:focus,select:focus{border-color:#f76b15!important;box-shadow:0 0 0 1px #f76b15!important;outline:none!important}'
    + '.searchTitle{color:#eeeeee!important}'
    + '.uploadForm input,.uploadForm textarea,.settingsForm input,.settingsForm textarea{border-radius:8px!important}',
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
  let css = '';
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
  'appleid.apple.com', 'id.apple.com'];
function isAuthUrl(u) {
  const low = u.toLowerCase();
  if (low.includes('accounts.google.com') || low.includes('appleid.apple.com')
    || low.includes('id.apple.com') || low.includes('/login') || low.includes('/signin')
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
}

function createMain() {
  mainWin = new BrowserWindow({
    width: 1180, height: 760, minWidth: 860, minHeight: 560,
    title: 'SoundCloud Desktop Alt',
    autoHideMenuBar: true,
    icon: asset('icon.ico'),
    webPreferences: { preload: path.join(__dirname, 'preload-shell.js'), contextIsolation: true },
  });
  mainWin.loadFile(uiFile('shell.html'));

  siteView = new WebContentsView({ webPreferences: { partition: PARTITION } });
  mainWin.contentView.addChildView(siteView);
  layoutSiteView();
  siteView.webContents.loadURL(store.lastUrl || HOME_URL);

  siteView.webContents.setWindowOpenHandler(({ url }) => {
    if (url.startsWith('http://') || url.startsWith('https://')) openAuth(url);
    return { action: 'deny' };
  });

  siteView.webContents.on('did-finish-load', () => { injectSite(); applyVolume(); sendNavState(); });
  siteView.webContents.on('did-navigate-in-page', () => { injectSite(); sendNavState(); });
  siteView.webContents.on('did-navigate', () => { sendNavState(); });

  mainWin.on('resize', layoutSiteView);
  mainWin.on('close', (e) => {
    if (store.trayHide && !allowExit) { e.preventDefault(); mainWin.hide(); }
  });
  mainWin.on('closed', () => { mainWin = null; siteView = null; });
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

function openAuth(url) {
  const w = new BrowserWindow({
    width: 480, height: 640, title: 'SoundCloud',
    autoHideMenuBar: true, parent: mainWin || undefined,
    icon: asset('icon.ico'),
    webPreferences: { partition: PARTITION },
  });
  w.loadURL(url);
  w.webContents.on('page-title-updated', (_e, title) => { if (title) w.setTitle(title); });
  w.webContents.on('did-finish-load', () => {
    w.webContents.executeJavaScript(PROMO_JS).catch(() => {});
  });
}

function createPlayer() {
  playerWin = new BrowserWindow({
    width: 400, height: 216, minWidth: 340, minHeight: 190,
    title: 'Now playing', show: false, autoHideMenuBar: true,
    icon: asset('icon.ico'),
    webPreferences: { preload: path.join(__dirname, 'preload-shell.js'), contextIsolation: true },
  });
  playerWin.loadFile(uiFile('player.html'));
  playerWin.on('close', (e) => { e.preventDefault(); playerWin.hide(); });
}

function openSettings() {
  if (settingsWin) { settingsWin.show(); settingsWin.focus(); return; }
  settingsWin = new BrowserWindow({
    width: 440, height: 700, minWidth: 380, minHeight: 520,
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

function startPoll() {
  setInterval(async () => {
    const wc = siteWC();
    if (!wc) return;
    try {
      const raw = await wc.executeJavaScript(POLL_JS);
      const p = String(raw).split('|');
      if (p.length < 6) return;
      const title = decodeURIComponent(p[2] || '');
      const playing = p[3] === '1';
      const art = decodeURIComponent(p[4] || '');
      const artist = decodeURIComponent(p[5] || '');
      const time = fmtTime(p[0]) + ' / ' + fmtTime(p[1]);
      const key = title + '||' + artist;
      const info = { title, artist, art, time, playing };
      if (playerWin) playerWin.webContents.send('track', info);
      if (mainWin) mainWin.webContents.send('track', info);
      if (playing && store.playerPopup && (!wasPlaying || key !== lastTrackKey)) {
        if (playerWin && !playerWin.isVisible()) playerWin.show();
      }
      wasPlaying = playing;
      lastTrackKey = key;
    } catch (e) { /* page not ready */ }
  }, 1500);
}

function setupShortcuts() {
  try {
    globalShortcut.register('num1', () => clickPlayer(PREV_JS));
    globalShortcut.register('num2', () => clickPlayer(TOGGLE_JS));
    globalShortcut.register('num3', () => clickPlayer(NEXT_JS));
    globalShortcut.register('num4', () => {
      store.volume = Math.max(0, store.volume - 0.05); saveStore(); applyVolume();
    });
    globalShortcut.register('num5', () => {
      store.volume = Math.min(1, store.volume + 0.05); saveStore(); applyVolume();
    });
  } catch (e) { logLine('hotkeys failed: ' + e.message); }
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
  ses.webRequest.onBeforeRequest({ urls: ['*://*/*'] }, (details, callback) => {
    try {
      if (shouldBlock(details.url)) return callback({ cancel: true });
    } catch (e) { /* ignore */ }
    callback({});
  });
}

/* ---------------- IPC ---------------- */
ipcMain.handle('store-get', () => store);
ipcMain.handle('store-set', (_e, patch) => {
  Object.assign(store, patch || {});
  saveStore();
  if (patch && (patch.redesign || patch.rd || patch.anims)) injectSite();
  if (patch && typeof patch.autostart !== 'undefined') applyAutostart();
  return true;
});
ipcMain.handle('shell-cmd', async (e, name, arg) => {
  const wc = siteWC();
  if (name === 'toggle') return clickPlayer(TOGGLE_JS);
  if (name === 'next') return clickPlayer(NEXT_JS);
  if (name === 'prev') return clickPlayer(PREV_JS);
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
  return false;
});

/* ---------------- boot ---------------- */
const gotLock = app.requestSingleInstanceLock();
if (!gotLock) { app.quit(); }
else {
  app.on('second-instance', showMain);
  app.whenReady().then(async () => {
    store = loadStore();
    logLine('start version=2.0.0');
    setupAdblock();
    createMain();
    createPlayer();
    setupTray();
    applyAutostart();
    applyVolume();
    await loadExtensions();
    startPoll();
    setupShortcuts();
    app.on('activate', showMain);
  });
  app.on('window-all-closed', () => { /* tray keeps us alive */ });
  app.on('before-quit', () => {
    allowExit = true;
    try {
      const wc = siteWC();
      if (wc && wc.getURL().startsWith('http')) {
        store.lastUrl = wc.getURL();
        saveStore();
      }
    } catch (e) { /* ignore */ }
    globalShortcut.unregisterAll();
  });
}
