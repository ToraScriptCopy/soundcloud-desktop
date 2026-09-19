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
    const st = fs.statSync(s);
    if (st.isDirectory()) copyDir(s, d);
    else fs.copyFileSync(s, d);
  }
}

if (!fs.existsSync(dist)) {
  console.error('Electron dist not found. Run npm install first.');
  process.exit(1);
}
if (fs.existsSync(out)) fs.rmSync(out, { recursive: true, force: true });

// 1. Electron binaries
copyDir(dist, out);
// 2. Drop the default app, ours takes its place
for (const n of ['default_app.asar']) {
  const p = path.join(out, 'resources', n);
  if (fs.existsSync(p)) fs.rmSync(p, { force: true });
}
// 3. Rename launcher. Detect the TARGET platform from the dist contents,
// not from the build machine, so cross-packing works.
const hasWinExe = fs.existsSync(path.join(out, 'electron.exe'));
const isWinTarget = hasWinExe;
const srcExe = path.join(out, isWinTarget ? 'electron.exe' : 'electron');
const dstExe = path.join(out, isWinTarget ? 'SoundCloudDeskAlt.exe' : 'SoundCloudDeskAlt');
fs.renameSync(srcExe, dstExe);
if (!isWinTarget) fs.chmodSync(dstExe, 0o755);
// 4. App files (main process + built UI + assets, no node_modules needed at runtime)
fs.mkdirSync(appOut, { recursive: true });
fs.copyFileSync(path.join(root, 'package.json'), path.join(appOut, 'package.json'));
copyDir(path.join(root, 'electron'), path.join(appOut, 'electron'));
copyDir(path.join(root, 'dist'), path.join(appOut, 'dist'));
copyDir(path.join(root, 'assets'), path.join(appOut, 'assets'));

console.log('portable ready: ' + out);
