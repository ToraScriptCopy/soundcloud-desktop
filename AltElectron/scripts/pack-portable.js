/* Assembles a portable folder from the Electron dist + app files.
   Run: npm run ui:build && npm run pkg:portable
   Output: portable/SoundCloudDeskAlt/ */
'use strict';
const fs = require('fs');
const path = require('path');

const root = path.join(__dirname, '..');
const dist = path.join(root, 'node_modules', 'electron', 'dist');
const out = path.join(root, 'portable', 'SoundCloudDeskAlt');
const appOut = path.join(out, 'resources', 'app');

function copyDir(src, dst) {
  fs.mkdirSync(dst, { recursive: true });
  for (const name of fs.readdirSync(src)) {
    const s = path.join(src, name);
    const d = path.join(dst, name);
    try {
      const lst = fs.lstatSync(s);
      if (lst.isSymbolicLink()) {
        try { if (fs.existsSync(d)) fs.rmSync(d, { force: true }); } catch (e) { /* ignore */ }
        fs.symlinkSync(fs.readlinkSync(s), d);
        continue;
      }
      const st = fs.statSync(s);
      if (st.isDirectory()) copyDir(s, d);
      else fs.copyFileSync(s, d);
    } catch (e) {
      console.error('copy skipped ' + s + ': ' + e.message);
    }
  }
}

function electronDistName() {
  const p = process.platform + '-' + process.arch;
  if (p === 'darwin-x64' || p === 'darwin-arm64' || p === 'linux-x64' || p === 'win32-x64') return p;
  throw new Error('unsupported pack target: ' + p);
}

function ensureDist() {
  const marker =
    fs.existsSync(path.join(dist, 'Electron.app')) ? path.join(dist, 'Electron.app') :
    fs.existsSync(path.join(dist, 'electron.exe')) ? path.join(dist, 'electron.exe') :
    fs.existsSync(path.join(dist, 'electron')) ? path.join(dist, 'electron') : null;
  if (marker) {
    console.log('electron runtime found: ' + marker);
    return;
  }
  const ver = require(path.join(root, 'node_modules', 'electron', 'package.json')).version;
  const target = electronDistName();
  const zip = path.join(root, 'electron-' + target + '.zip');
  const url = 'https://github.com/electron/electron/releases/download/v' + ver + '/electron-v' + ver + '-' + target + '.zip';
  console.log('downloading ' + url);
  const { spawnSync } = require('child_process');
  let r = spawnSync('curl', ['-sL', '-o', zip, url], { stdio: 'inherit' });
  if (r.status !== 0) throw new Error('curl download failed');
  const unzip = process.platform === 'win32' ? null : 'unzip';
  if (process.platform === 'win32') {
    const { execSync } = require('child_process');
    execSync('powershell -NoProfile -Command "Expand-Archive -Path \'' + zip + '\' -DestinationPath \'' + dist + '\' -Force"');
  } else {
    r = spawnSync('unzip', ['-q', zip, '-d', dist], { stdio: 'inherit' });
    if (r.status !== 0) throw new Error('unzip failed');
  }
  console.log('electron runtime ready');
}

console.log('pack target platform: ' + process.platform + '-' + process.arch);
ensureDist();
if (fs.existsSync(out)) fs.rmSync(out, { recursive: true, force: true });

copyDir(dist, out);
for (const n of ['default_app.asar']) {
  const p = path.join(out, 'resources', n);
  if (fs.existsSync(p)) fs.rmSync(p, { force: true });
}
const APP_NAME = 'SoundCloudDeskAlt';
if (fs.existsSync(path.join(out, 'Electron.app'))) {
  const appBundle = path.join(out, APP_NAME + '.app');
  fs.renameSync(path.join(out, 'Electron.app'), appBundle);
  const contents = path.join(appBundle, 'Contents');
  const resDir = path.join(contents, 'Resources');
  const macDir = path.join(contents, 'MacOS');
  const appRes = path.join(resDir, 'app');
  const defaultAsar = path.join(resDir, 'default_app.asar');
  if (fs.existsSync(defaultAsar)) fs.rmSync(defaultAsar, { force: true });
  if (fs.existsSync(appRes)) fs.rmSync(appRes, { recursive: true, force: true });
  fs.mkdirSync(appRes, { recursive: true });
  fs.copyFileSync(path.join(root, 'package.json'), path.join(appRes, 'package.json'));
  copyDir(path.join(root, 'electron'), path.join(appRes, 'electron'));
  copyDir(path.join(root, 'dist'), path.join(appRes, 'dist'));
  copyDir(path.join(root, 'assets'), path.join(appRes, 'assets'));
  const plistPath = path.join(contents, 'Info.plist');
  try {
    let plist = fs.readFileSync(plistPath, 'utf8');
    plist = plist
      .replace(/<key>CFBundleExecutable<\/key>\s*<string>[^<]*<\/string>/, '<key>CFBundleExecutable</key><string>' + APP_NAME + '</string>')
      .replace(/<key>CFBundleName<\/key>\s*<string>[^<]*<\/string>/, '<key>CFBundleName</key><string>SoundCloud Desktop Alt</string>')
      .replace(/<key>CFBundleIdentifier<\/key>\s*<string>[^<]*<\/string>/, '<key>CFBundleIdentifier</key><string>com.torascript.soundcloud-alt</string>');
    fs.writeFileSync(plistPath, plist);
  } catch (e) { console.error('Info.plist patch failed: ' + e.message); }
  const srcBin = path.join(macDir, 'Electron');
  const dstBin = path.join(macDir, APP_NAME);
  if (fs.existsSync(srcBin)) fs.renameSync(srcBin, dstBin);
  try { fs.chmodSync(dstBin, 0o755); } catch (e) { /* ignore */ }
  try {
    const { spawnSync } = require('child_process');
    const r = spawnSync('codesign', ['--force', '--deep', '--sign', '-', appBundle], { stdio: 'pipe' });
    if (r.status === 0) console.log('ad-hoc signed');
    else console.log('codesign skipped (not on macOS)');
  } catch (e) { console.log('codesign skipped (not on macOS)'); }
  console.log('portable ready: ' + appBundle);
} else {
const hasWinExe = fs.existsSync(path.join(out, 'electron.exe'));
const isWinTarget = hasWinExe;
const srcExe = path.join(out, isWinTarget ? 'electron.exe' : 'electron');
const dstExe = path.join(out, isWinTarget ? 'SoundCloudDeskAlt.exe' : 'SoundCloudDeskAlt');
fs.renameSync(srcExe, dstExe);
if (!isWinTarget) fs.chmodSync(dstExe, 0o755);
if (isWinTarget) {
  try {
    const { rcedit } = require('rcedit');
    const ver = require(path.join(root, 'package.json')).version + '.0';
    rcedit(dstExe, {
      icon: path.join(root, 'assets', 'icon.ico'),
      'file-version': ver,
      'product-version': ver,
      'version-string': {
        CompanyName: 'SoundCloud Desktop',
        FileDescription: 'SoundCloud Desktop Alt',
        ProductName: 'SoundCloud Desktop Alt',
        InternalName: 'SoundCloudDeskAlt',
        OriginalFilename: 'SoundCloudDeskAlt.exe',
      },
    }).then(
      () => console.log('exe branded'),
      (e) => console.error('rcedit failed: ' + (e && e.message)));
  } catch (e) { console.error('rcedit failed: ' + e.message); }
}
fs.mkdirSync(appOut, { recursive: true });
fs.copyFileSync(path.join(root, 'package.json'), path.join(appOut, 'package.json'));
copyDir(path.join(root, 'electron'), path.join(appOut, 'electron'));
copyDir(path.join(root, 'dist'), path.join(appOut, 'dist'));
copyDir(path.join(root, 'assets'), path.join(appOut, 'assets'));

console.log('portable ready: ' + out);
}
