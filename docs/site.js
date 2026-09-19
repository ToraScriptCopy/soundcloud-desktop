/* Live GitHub data: releases, source tree with editor, hashes. No mocks. */
(function () {
  'use strict';
  var REPO = 'ToraScriptCopy/soundcloud-desktop';
  var API = 'https://api.github.com/repos/' + REPO;
  var RAW = 'https://raw.githubusercontent.com/' + REPO + '/main/';

  function t(key) {
    try {
      var lang = localStorage.getItem('scd-lang') || 'en';
      var d = (window.SCD_I18N && (window.SCD_I18N[lang] || window.SCD_I18N.en)) || {};
      return d[key] || key;
    } catch (e) { return key; }
  }

  function fmtSize(n) {
    if (!n && n !== 0) return '';
    if (n > 1048576) return (n / 1048576).toFixed(1) + ' MB';
    return Math.max(1, Math.round(n / 1024)) + ' KB';
  }
  function fmtDate(s) {
    try { return new Date(s).toLocaleDateString(); } catch (e) { return s || ''; }
  }
  function esc(s) {
    return String(s == null ? '' : s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
  }

  /* ---------- theme (dark default) ---------- */
  var SUN = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><circle cx="12" cy="12" r="4.5"/><path d="M12 2v2.5M12 19.5V22M2 12h2.5M19.5 12H22M4.9 4.9l1.8 1.8M17.3 17.3l1.8 1.8M19.1 4.9l-1.8 1.8M6.7 17.3l-1.8 1.8"/></svg>';
  var MOON = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 14.5A8.5 8.5 0 0 1 9.5 4 8.5 8.5 0 1 0 20 14.5z"/></svg>';
  function applyThemeIcon() {
    var b = document.getElementById('themeBtn');
    if (!b) return;
    var light = document.documentElement.getAttribute('data-theme') === 'light';
    b.innerHTML = light ? MOON : SUN;
    b.setAttribute('title', light ? t('theme_dark') : t('theme_light'));
    b.setAttribute('aria-label', light ? t('theme_dark') : t('theme_light'));
  }
  function initTheme() {
    var saved = null;
    try { saved = localStorage.getItem('scd-theme'); } catch (e) { /* ignore */ }
    if (saved === 'light') document.documentElement.setAttribute('data-theme', 'light');
    applyThemeIcon();
    var b = document.getElementById('themeBtn');
    if (b) b.addEventListener('click', function () {
      var cur = document.documentElement.getAttribute('data-theme') === 'light';
      if (cur) document.documentElement.removeAttribute('data-theme');
      else document.documentElement.setAttribute('data-theme', 'light');
      try { localStorage.setItem('scd-theme', cur ? 'dark' : 'light'); } catch (e) { /* ignore */ }
      applyThemeIcon();
    });
    document.addEventListener('scd-lang', applyThemeIcon);
  }

  /* ---------- reveal on scroll ---------- */
  function initReveal() {
    if (!('IntersectionObserver' in window)) return;
    var els = document.querySelectorAll('.card, .feature, .shot, .scan-row');
    els.forEach(function (el, i) {
      el.classList.add('rv');
      el.style.transitionDelay = Math.min((i % 8) * 45, 300) + 'ms';
    });
    var io = new IntersectionObserver(function (entries) {
      entries.forEach(function (en) {
        if (en.isIntersecting) { en.target.classList.add('in'); io.unobserve(en.target); }
      });
    }, { threshold: 0.08 });
    els.forEach(function (el) { io.observe(el); });
  }

  /* ---------- back to top ---------- */
  function initTop() {
    var b = document.getElementById('toTop');
    if (!b) return;
    window.addEventListener('scroll', function () {
      b.classList.toggle('show', window.scrollY > 600);
    }, { passive: true });
    b.addEventListener('click', function () { window.scrollTo({ top: 0, behavior: 'smooth' }); });
  }

  /* ---------- releases + live stats ---------- */
  function loadReleases() {
    var box = document.getElementById('relList');
    if (!box) return;
    fetch(API + '/releases?per_page=20')
      .then(function (r) { if (!r.ok) throw new Error('http ' + r.status); return r.json(); })
      .then(function (rels) {
        if (!rels.length) { box.innerHTML = '<p class="loading">No releases yet.</p>'; return; }
        var files = 0, bytes = 0;
        rels.forEach(function (rel) {
          (rel.assets || []).forEach(function (a) { files++; bytes += a.size || 0; });
        });
        var stats = document.getElementById('statsLine');
        if (stats) {
          stats.textContent = rels.length + ' ' + t('stats_releases') + ' · '
            + files + ' ' + t('stats_files') + ' · ' + fmtSize(bytes);
        }
        box.innerHTML = rels.map(function (rel) {
          var assets = (rel.assets || []).map(function (a) {
            return '<a href="' + esc(a.browser_download_url) + '">'
              + '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 3v12"/><path d="M7 10l5 5 5-5"/><path d="M4 21h16"/></svg>'
              + esc(a.name) + ' <span style="opacity:.6">' + fmtSize(a.size) + '</span></a>';
          }).join('');
          var notes = esc((rel.body || '').slice(0, 400));
          return '<div class="rel">'
            + '<div class="rel-head"><strong>' + esc(rel.name || rel.tag_name) + '</strong>'
            + '<time>' + fmtDate(rel.published_at) + '</time></div>'
            + (notes ? '<div class="rel-notes">' + notes + '</div>' : '')
            + '<div class="rel-assets">' + assets + '</div></div>';
        }).join('');
        initReveal();
      })
      .catch(function () {
        box.innerHTML = '<p class="loading">Could not reach GitHub API. '
          + '<a style="color:#f76b15" href="https://github.com/' + REPO + '/releases">Open the releases page directly.</a></p>';
      });
  }

  /* ---------- source tree + editor ---------- */
  var TEXT_EXT = ['cs', 'xaml', 'csproj', 'js', 'jsx', 'html', 'css', 'json', 'py', 'md', 'xml', 'txt', 'yml', 'gitignore'];
  var IMG_EXT = ['png', 'jpg', 'jpeg', 'gif', 'webp', 'bmp', 'ico', 'svg'];
  function isText(path) {
    var e = (path.split('.').pop() || '').toLowerCase();
    return TEXT_EXT.indexOf(e) >= 0;
  }
  function isImage(path) {
    var e = (path.split('.').pop() || '').toLowerCase();
    return IMG_EXT.indexOf(e) >= 0;
  }
  function langOf(path) {
    var e = (path.split('.').pop() || '').toLowerCase();
    var map = { cs: 'csharp', xaml: 'xml', csproj: 'xml', js: 'javascript', jsx: 'javascript', html: 'html', css: 'css', json: 'json', py: 'python', md: 'markdown', xml: 'xml', yml: 'yaml', txt: 'plaintext' };
    return map[e] || 'plaintext';
  }
  function loadTree() {
    var box = document.getElementById('fileTree');
    if (!box) return;
    fetch(API + '/git/trees/main?recursive=1')
      .then(function (r) { if (!r.ok) throw new Error('http ' + r.status); return r.json(); })
      .then(function (j) {
        var files = (j.tree || []).filter(function (n) {
          return n.type === 'blob'
            && n.path.indexOf('node_modules/') !== 0
            && n.path.indexOf('bin/') === -1 && n.path.indexOf('obj/') === -1
            && n.size < 3000000;
        });
        var groups = {};
        files.forEach(function (f) {
          var parts = f.path.split('/');
          var top = parts.length > 1 ? parts[0] : '/';
          (groups[top] = groups[top] || []).push(f.path);
        });
        var html = '';
        Object.keys(groups).sort().forEach(function (g) {
          html += '<div class="dir">' + esc(g) + '</div>';
          groups[g].sort().forEach(function (p) {
            var name = p.split('/').pop();
            html += '<button data-path="' + esc(p) + '">'
              + '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M6 2h9l5 5v15H6z"/><path d="M14 2v6h6"/></svg>'
              + '<span style="overflow:hidden;text-overflow:ellipsis;white-space:nowrap">' + esc(name) + '</span></button>';
          });
        });
        box.innerHTML = html || '<p class="loading">Empty.</p>';
        var btns = box.querySelectorAll('button');
        btns.forEach(function (b) {
          b.addEventListener('click', function () {
            btns.forEach(function (x) { x.classList.remove('active'); });
            b.classList.add('active');
            openFile(b.getAttribute('data-path'));
          });
        });
        var first = files.filter(function (f) { return /readme\.md$/i.test(f.path); })[0] || files[0];
        if (first) {
          var btn = box.querySelector('button[data-path="' + first.path.replace(/"/g, '') + '"]');
          if (btn) btn.classList.add('active');
          openFile(first.path);
        }
      })
      .catch(function () {
        box.innerHTML = '<p class="loading">Could not reach GitHub API.</p>';
      });
  }

  var editor = null, editorReady = null, editorMode = '';
  function editorHost() {
    var host = document.getElementById('editorInner');
    if (host) return host;
    var box = document.getElementById('editor');
    if (!box) return null;
    box.innerHTML = '<div id="editorInner" style="height:100%"></div>';
    return document.getElementById('editorInner');
  }
  function setCrumb(path) {
    var c = document.getElementById('crumb');
    if (c) c.textContent = path || '';
  }
  function ensureEditor() {
    if (editorReady) return editorReady;
    editorReady = new Promise(function (resolve) {
      if (typeof require === 'undefined') {
        var s = document.createElement('script');
        s.src = 'https://cdn.jsdelivr.net/npm/monaco-editor@0.52.2/min/vs/loader.js';
        s.onload = boot;
        s.onerror = function () { resolve(null); };
        document.head.appendChild(s);
      } else boot();
      function boot() {
        try {
          require.config({ paths: { vs: 'https://cdn.jsdelivr.net/npm/monaco-editor@0.52.2/min/vs' } });
          require(['vs/editor/editor.main'], function () {
            var host = editorHost();
            if (!host) { resolve(null); return; }
            editor = monaco.editor.create(host, {
              value: '// pick a file on the left',
              language: 'plaintext', theme: 'vs-dark', readOnly: true,
              minimap: { enabled: false }, fontSize: 13, scrollBeyondLastLine: false,
              lineNumbers: 'on', renderLineHighlight: 'all', smoothScrolling: true,
            });
            resolve(editor);
          }, function () { resolve(null); });
        } catch (e) { resolve(null); }
      }
    });
    return editorReady;
  }
  function openFile(path) {
    var box = document.getElementById('editor');
    setCrumb(path);
    if (isImage(path)) {
      editorMode = 'img';
      if (box) {
        box.innerHTML = '<div style="display:flex;align-items:center;justify-content:center;height:100%;min-height:300px;background:#141414">'
          + '<img src="' + RAW + esc(path) + '" alt="' + esc(path) + '" style="max-width:92%;max-height:480px;object-fit:contain;border-radius:8px" />'
          + '</div>';
      }
      return;
    }
    if (!isText(path)) {
      editorMode = 'bin';
      box.innerHTML = '<p class="loading">Binary file, preview is not available. Open it on GitHub instead.</p>';
      return;
    }
    if (editorMode !== 'code') {
      editorMode = 'code';
      editor = null;
      editorReady = null;
      box.innerHTML = '<div id="editorInner" style="height:100%"></div>';
    }
    ensureEditor().then(function (ed) {
      fetch(RAW + path)
        .then(function (r) { if (!r.ok) throw new Error('http ' + r.status); return r.text(); })
        .then(function (text) {
          if (ed) {
            var model = monaco.editor.createModel(text.slice(0, 200000), langOf(path));
            ed.setModel(model);
            try { ed.layout(); } catch (e) { /* ignore */ }
          } else {
            var b2 = document.getElementById('editor');
            if (b2) b2.innerHTML = '<pre style="padding:16px;overflow:auto;font-size:12.5px">' + esc(text.slice(0, 20000)) + '</pre>';
          }
        })
        .catch(function () {
          var b3 = document.getElementById('editor');
          if (b3) b3.innerHTML = '<p class="loading">Could not load this file.</p>';
        });
    });
  }

  function toggleFullscreen() {
    var sec = document.getElementById('sourceSec');
    var btn = document.getElementById('fsBtn');
    if (!sec) return;
    if (document.fullscreenElement) {
      document.exitFullscreen().catch(function () { /* ignore */ });
    } else if (sec.requestFullscreen) {
      sec.requestFullscreen().catch(function () { /* ignore */ });
    }
    setTimeout(function () {
      try { if (editor) editor.layout(); } catch (e) { /* ignore */ }
      if (btn) btn.textContent = document.fullscreenElement ? t('fs_close') : t('fs_open');
    }, 150);
  }

  /* ---------- verify: hashes with copy buttons, real report links ---------- */
  var VERDICTS = {
    'f10a235a902decdd2528a1ac9ea7e38f6f3f0ca1a32affe4e0ccd64be32e8904': { m: 0, total: 67 },
    '74ca47b850a7bd87eca1c6318e2522e9cfe306037c1e6426b8d2c0c978023a27': { m: 0, total: 66 },
    '2d1a67fe6f40738fd8dbd74b76a8c6dccc720cb3b00e255fec7184d2bfecfb00': { m: 1, total: 68 },
    '68208049d5ce7575752b53249cbab5df58daaa4ea6d4c39f1bd74ae6f48fae4d': { m: 8, total: 70 }
  };
  function loadHashes() {
    var box = document.getElementById('scanList');
    if (!box) return;
    fetch(API + '/releases/latest')
      .then(function (r) { if (!r.ok) throw new Error('http ' + r.status); return r.json(); })
      .then(function (rel) {
        var assets = (rel.assets || []).filter(function (a) {
          return /\.(exe|zip|tar\.gz)$/i.test(a.name);
        });
        if (!assets.length) { box.innerHTML = '<p class="loading">No files found.</p>'; return; }
        box.innerHTML = assets.map(function (a) {
          var hash = (a.digest || '').replace(/^sha256:/i, '');
          var v = hash ? VERDICTS[hash.toLowerCase()] : null;
          var verdict = v
            ? '<span style="color:#a8a8a8;font-size:13px">Detections: ' + v.m + '/' + v.total + '</span>'
              + ' <a class="vt" href="https://www.virustotal.com/gui/file/' + esc(hash) + '" target="_blank" rel="noopener">VirusTotal report</a>'
            : '';
          return '<div class="scan-row">'
            + '<span class="icon-badge"><svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 3l8 3v6c0 5-3.5 8-8 9-4.5-1-8-4-8-9V6l8-3z"/><path d="M9 12l2 2 4-4"/></svg></span>'
            + '<div style="flex:1;min-width:0"><strong>' + esc(a.name) + '</strong>'
            + '<span style="color:#a8a8a8;font-size:13px"> - ' + fmtSize(a.size) + '</span>'
            + (hash ? '<code class="hash">SHA-256: ' + esc(hash) + '</code>' : '')
            + (verdict ? '<div>' + verdict + '</div>' : '')
            + (hash ? '<button class="copy-btn" data-hash="' + esc(hash) + '">'
              + '<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="9" y="9" width="12" height="12" rx="2"/><path d="M5 15V5a2 2 0 0 1 2-2h10"/></svg>'
              + esc(t('copied_default')) + '</button>' : '')
            + '</div></div>';
        }).join('');
        box.querySelectorAll('.copy-btn').forEach(function (b) {
          b.addEventListener('click', function () {
            var h = b.getAttribute('data-hash');
            var done = function () {
              b.classList.add('done');
              var label = b.querySelector('span');
              if (label) label.textContent = t('copied');
              setTimeout(function () {
                b.classList.remove('done');
                if (label) label.textContent = t('copied_default');
              }, 1600);
            };
            if (navigator.clipboard && navigator.clipboard.writeText) {
              navigator.clipboard.writeText(h).then(done, done);
            } else done();
          });
        });
        initReveal();
      })
      .catch(function () {
        box.innerHTML = '<p class="loading">Could not reach GitHub API.</p>';
      });
  }

  /* ---------- screenshot lightbox with zoom and pan ---------- */
  var zoomLevel = 1, panX = 0, panY = 0, dragOn = false, dragSX = 0, dragSY = 0;
  function zoomApply() {
    var img = document.getElementById('zoomImg');
    var label = document.getElementById('zoomLabel');
    if (!img) return;
    img.style.transform = 'translate(' + panX + 'px,' + panY + 'px) scale(' + zoomLevel + ')';
    if (label) label.textContent = Math.round(zoomLevel * 100) + '%';
  }
  function zoomSet(z) {
    zoomLevel = Math.min(6, Math.max(0.4, z));
    if (zoomLevel <= 1) { panX = 0; panY = 0; }
    zoomApply();
  }
  function initLightbox() {
    var box = document.getElementById('lightbox');
    var img = document.getElementById('zoomImg');
    var stage = document.getElementById('zoomStage');
    if (!box || !img || !stage) return;
    document.querySelectorAll('.shot img').forEach(function (th) {
      th.style.cursor = 'zoom-in';
      th.addEventListener('click', function () {
        img.src = th.src;
        zoomLevel = 1; panX = 0; panY = 0;
        zoomApply();
        box.classList.add('open');
      });
    });
    document.getElementById('zoomIn').addEventListener('click', function () { zoomSet(zoomLevel * 1.25); });
    document.getElementById('zoomOut').addEventListener('click', function () { zoomSet(zoomLevel / 1.25); });
    document.getElementById('zoomReset').addEventListener('click', function () { zoomSet(1); });
    var close = function () { box.classList.remove('open'); };
    document.getElementById('zoomClose').addEventListener('click', close);
    box.addEventListener('click', function (e) { if (e.target === box || e.target === stage) close(); });
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') close(); });
    stage.addEventListener('wheel', function (e) {
      e.preventDefault();
      zoomSet(zoomLevel * (e.deltaY < 0 ? 1.12 : 0.9));
    }, { passive: false });
    stage.addEventListener('pointerdown', function (e) {
      if (zoomLevel <= 1) return;
      dragOn = true; dragSX = e.clientX - panX; dragSY = e.clientY - panY;
      try { stage.setPointerCapture(e.pointerId); } catch (err) { /* ignore */ }
    });
    stage.addEventListener('pointermove', function (e) {
      if (!dragOn) return;
      panX = e.clientX - dragSX; panY = e.clientY - dragSY;
      zoomApply();
    });
    stage.addEventListener('pointerup', function () { dragOn = false; });
    stage.addEventListener('pointercancel', function () { dragOn = false; });
  }

  document.addEventListener('DOMContentLoaded', function () {
    initTheme();
    initTop();
    initLightbox();
    loadReleases();
    loadTree();
    loadHashes();
    initReveal();
    var fsBtn = document.getElementById('fsBtn');
    if (fsBtn) fsBtn.addEventListener('click', toggleFullscreen);
    document.addEventListener('fullscreenchange', function () {
      var b = document.getElementById('fsBtn');
      if (b) b.textContent = document.fullscreenElement ? t('fs_close') : t('fs_open');
      setTimeout(function () { try { if (editor) editor.layout(); } catch (e) { /* ignore */ } }, 150);
    });
    document.addEventListener('scd-lang', function () {
      var b = document.getElementById('fsBtn');
      if (b && !document.fullscreenElement) b.textContent = t('fs_open');
    });
  });
})();
