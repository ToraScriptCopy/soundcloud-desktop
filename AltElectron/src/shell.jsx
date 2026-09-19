import React, { useEffect, useRef, useState } from 'react';
import { createRoot } from 'react-dom/client';
import {
  Theme, Flex, Text, TextField, IconButton, Button, Slider,
} from '@radix-ui/themes';
import {
  ArrowLeftIcon, ArrowRightIcon, ReloadIcon, HomeIcon, GearIcon,
  HamburgerMenuIcon, HeartIcon, BarChartIcon, CopyIcon, ExternalLinkIcon,
  SpeakerLoudIcon, SpeakerOffIcon, VideoIcon, MagnifyingGlassIcon,
} from '@radix-ui/react-icons';
import '@radix-ui/themes/styles.css';
import './shell.css';

const api = window.api;
export const TOP_H = 52;
export const BOTTOM_H = 46;
export const SIDE_W = 210;
const HOME = 'https://soundcloud.com/';
const CHARTS = 'https://soundcloud.com/charts/top';
const LIKES = 'https://soundcloud.com/you/likes';

function smartTarget(text) {
  const t = (text || '').trim();
  if (/^https?:\/\//i.test(t)) return t;
  if (!t) return HOME;
  if (!t.includes(' ') && t.includes('.')) return 'https://' + t;
  return 'https://soundcloud.com/search/sounds?q=' + encodeURIComponent(t);
}

function Shell() {
  const [edit, setEdit] = useState(HOME);
  const [canBack, setCanBack] = useState(false);
  const [canFwd, setCanFwd] = useState(false);
  const [sidebar, setSidebar] = useState(true);
  const [track, setTrack] = useState({ title: '', time: '', playing: false });
  const [vol, setVol] = useState(80);
  const [muted, setMuted] = useState(false);
  const [pipUrl, setPipUrl] = useState('');
  const editing = useRef(false);

  useEffect(() => {
    api.get().then((s) => {
      const open = s.sidebarOpen !== false;
      setSidebar(open);
      setVol(Math.round((s.volume ?? 0.8) * 100));
      setMuted(!!s.muted);
      api.cmd('layout', { sidebarOpen: open });
    });
    const offs = [
      api.on('nav-state', (st) => {
        if (!editing.current) setEdit(st.url);
        setCanBack(!!st.canBack);
        setCanFwd(!!st.canFwd);
      }),
      api.on('track', (t) => setTrack(t)),
    ];
    return () => offs.forEach((f) => f());
  }, []);

  const go = () => api.cmd('nav-go', smartTarget(edit));
  const pushVol = (v, m) => api.cmd('volume', { volume: v / 100, muted: m });
  const toggleSidebar = () => {
    const open = !sidebar;
    setSidebar(open);
    api.cmd('layout', { sidebarOpen: open });
  };

  return (
    <Theme accentColor="orange" grayColor="gray" radius="large" appearance="dark">
      <Flex direction="column" style={{ height: '100vh', background: '#111113' }}>
        {/* Top bar */}
        <Flex align="center" gap="2" px="2" style={{ height: TOP_H, flexShrink: 0, borderBottom: '1px solid #2a2a2a' }}>
          <IconButton variant="ghost" onClick={toggleSidebar} title="Sidebar">
            <HamburgerMenuIcon />
          </IconButton>
          <IconButton variant="ghost" disabled={!canBack} onClick={() => api.cmd('nav-back')} title="Back">
            <ArrowLeftIcon />
          </IconButton>
          <IconButton variant="ghost" disabled={!canFwd} onClick={() => api.cmd('nav-fwd')} title="Forward">
            <ArrowRightIcon />
          </IconButton>
          <IconButton variant="ghost" onClick={() => api.cmd('nav-reload')} title="Reload">
            <ReloadIcon />
          </IconButton>
          <IconButton variant="ghost" onClick={() => api.cmd('nav-go', HOME)} title="Home">
            <HomeIcon />
          </IconButton>
          <div style={{ flex: 1 }}>
            <TextField.Root
              value={edit}
              placeholder="URL or search SoundCloud…"
              onChange={(e) => setEdit(e.target.value)}
              onFocus={() => { editing.current = true; }}
              onBlur={() => { editing.current = false; }}
              onKeyDown={(e) => { if (e.key === 'Enter') go(); }}
            >
              <TextField.Slot>
                <MagnifyingGlassIcon />
              </TextField.Slot>
            </TextField.Root>
          </div>
          <Button variant="solid" onClick={go}>Go</Button>
          <IconButton variant="ghost" onClick={() => api.cmd('open-settings')} title="Settings">
            <GearIcon />
          </IconButton>
        </Flex>

        {/* Content */}
        <Flex style={{ flex: 1, minHeight: 0 }}>
          {sidebar ? (
            <Flex
              direction="column"
              gap="2"
              p="2"
              className="row-enter"
              style={{ width: SIDE_W, flexShrink: 0, borderRight: '1px solid #2a2a2a', overflowY: 'auto' }}
            >
              <Button variant="soft" style={{ justifyContent: 'flex-start' }} onClick={() => api.cmd('nav-go', HOME)}>
                <HomeIcon /> Home
              </Button>
              <Button variant="soft" style={{ justifyContent: 'flex-start' }} onClick={() => api.cmd('nav-go', CHARTS)}>
                <BarChartIcon /> Charts
              </Button>
              <Button variant="soft" style={{ justifyContent: 'flex-start' }} onClick={() => api.cmd('nav-go', LIKES)}>
                <HeartIcon /> My likes
              </Button>
              <Text size="1" weight="bold" color="gray" mt="3">PiP PLAYER</Text>
              <TextField.Root
                size="1"
                value={pipUrl}
                placeholder="Playlist or track link…"
                onChange={(e) => setPipUrl(e.target.value)}
                onKeyDown={(e) => { if (e.key === 'Enter') api.cmd('pip-open', pipUrl); }}
              />
              <Button size="2" variant="solid" onClick={() => api.cmd('pip-open', pipUrl)}>
                <VideoIcon /> Open PiP
              </Button>
              <Text size="1" color="gray">Opens bottom right. Drag it by the title bar.</Text>
              <Flex gap="2" mt="2">
                <IconButton variant="soft" onClick={() => api.cmd('copy-link')} title="Copy link">
                  <CopyIcon />
                </IconButton>
                <IconButton variant="soft" onClick={() => api.cmd('open-ext')} title="Open in browser">
                  <ExternalLinkIcon />
                </IconButton>
              </Flex>
            </Flex>
          ) : null}
          <div id="site-slot" style={{ flex: 1, background: '#111113' }} />
        </Flex>

        {/* Bottom bar */}
        <Flex align="center" gap="2" px="3" style={{ height: BOTTOM_H, flexShrink: 0, borderTop: '1px solid #2a2a2a' }}>
          <Text size="2" truncate style={{ flex: 1 }}>
            {track.title ? (track.time ? track.time + '  -  ' : '') + track.title : 'Nothing playing'}
          </Text>
          <IconButton variant="ghost" onClick={() => api.cmd('pip-open', '')} title="PiP for current page">
            <VideoIcon />
          </IconButton>
          <IconButton
            variant="ghost"
            title={muted ? 'Unmute' : 'Mute'}
            onClick={() => { const m = !muted; setMuted(m); pushVol(vol, m); }}
          >
            {muted ? <SpeakerOffIcon /> : <SpeakerLoudIcon />}
          </IconButton>
          <div style={{ width: 110 }}>
            <Slider
              value={[muted ? 0 : vol]}
              max={100}
              onValueChange={([v]) => {
                setVol(v);
                if (v > 0 && muted) { setMuted(false); pushVol(v, false); }
                else pushVol(v, muted);
              }}
            />
          </div>
          <Text size="2" color="gray" style={{ width: 36 }}>{muted ? '0%' : vol + '%'}</Text>
        </Flex>
      </Flex>
    </Theme>
  );
}

createRoot(document.getElementById('root')).render(<Shell />);
