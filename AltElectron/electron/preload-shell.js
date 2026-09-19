const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('api', {
  get: () => ipcRenderer.invoke('store-get'),
  set: (patch) => ipcRenderer.invoke('store-set', patch),
  cmd: (name, arg) => ipcRenderer.invoke('shell-cmd', name, arg),
  on: (ch, cb) => {
    const h = (_e, d) => cb(d);
    ipcRenderer.on(ch, h);
    return () => ipcRenderer.removeListener(ch, h);
  },
});
