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

  /* ---------- releases ---------- */
  function loadReleases() {
    var box = document.getElementById('relList');
    if (!box) return;
    fetch(API + '/releases?per_page=20')
      .then(function (r) { if (!r.ok) throw new Error('http ' + r.status); return r.json(); })
      .then(function (rels) {
        if (!rels.length) { box.innerHTML = '<p class="loading">No releases yet.</p>'; return; }
        box.innerHTML = rels.map(function (rel, i) {
          var assets = (rel.assets || []).map(function (a) {
            return '<a href="' + esc(a.browser_download_url) + '">'
              + '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 3v12"/><path d="M7 10l5 5 5-5"/><path d="M4 21h16"/></svg>'
              + esc(a.name) + ' <span style="opacity:.6">' + fmtSize(a.size) + '</span></a>';
          }).join('');
          var notes = esc((rel.body || '').slice(0, 400));
          return '<div class="rel" style="animation-delay:' + Math.min(i * 60, 300) + 'ms">'
            + '<div class="rel-head"><strong>' + esc(rel.name || rel.tag_name) + '</strong>'
            + '<time>' + fmtDate(rel.published_at) + '</time></div>'
            + (notes ? '<div class="rel-notes">' + notes + '</div>' : '')
            + '<div class="rel-assets">' + assets + '</div></div>';
        }).join('');
      })
      .catch(function () {
        box.innerHTML = '<p class="loading">Could not reach GitHub API. '
          + '<a style="color:#f76b15" href="https://github.com/' + REPO + '/releases">Open the releases page directly.</a></p>';
      });
  }

  /* ---------- source tree + editor ---------- */
  var TEXT_EXT = ['cs', 'xaml', 'csproj', 'js', 'jsx', 'html', 'css', 'json', 'py', 'md', 'xml', 'txt', 'yml', 'gitignore'];
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
            && n.size < 300000;
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

  var editor = null, editorReady = null;
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
            editor = monaco.editor.create(document.getElementById('editor'), {
              value: '// pick a file on the left',
              language: 'plaintext', theme: 'vs-dark', readOnly: true,
              minimap: { enabled: false }, fontSize: 13, scrollBeyondLastLine: false,
            });
            resolve(editor);
          }, function () { resolve(null); });
        } catch (e) { resolve(null); }
      }
    });
    return editorReady;
  }
  function openFile(path) {
    ensureEditor().then(function (ed) {
      var box = document.getElementById('editor');
      fetch(RAW + path)
        .then(function (r) { if (!r.ok) throw new Error('http ' + r.status); return r.text(); })
        .then(function (text) {
          if (ed) {
            var model = monaco.editor.createModel(text.slice(0, 200000), langOf(path));
            ed.setModel(model);
          } else if (box) {
            box.innerHTML = '<pre style="padding:16px;overflow:auto;font-size:12.5px">' + esc(text.slice(0, 20000)) + '</pre>';
          }
        })
        .catch(function () {
          if (box && !ed) box.innerHTML = '<p class="loading">Could not load this file.</p>';
        });
    });
  }

  /* ---------- verify ---------- */
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
          var vt = hash ? 'https://www.virustotal.com/gui/file/' + hash : null;
          return '<div class="scan-row">'
            + '<span class="icon-badge"><svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 3l8 3v6c0 5-3.5 8-8 9-4.5-1-8-4-8-9V6l8-3z"/><path d="M9 12l2 2 4-4"/></svg></span>'
            + '<div style="flex:1;min-width:0"><strong>' + esc(a.name) + '</strong>'
            + '<span style="color:#a8a8a8;font-size:13px"> - ' + fmtSize(a.size) + '</span>'
            + (hash ? '<code class="hash">SHA-256: ' + esc(hash) + '</code>' : '')
            + (vt ? '<a class="vt" href="' + vt + '" target="_blank" rel="noopener">Open VirusTotal report</a>' : '')
            + '</div></div>';
        }).join('');
      })
      .catch(function () {
        box.innerHTML = '<p class="loading">Could not reach GitHub API.</p>';
      });
  }

  document.addEventListener('DOMContentLoaded', function () {
    loadReleases();
    loadTree();
    loadHashes();
  });
})();
