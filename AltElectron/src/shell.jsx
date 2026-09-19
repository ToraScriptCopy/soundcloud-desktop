import React, { useEffect, useRef, useState } from 'react';
import { createRoot } from 'react-dom/client';
import {
  Theme, Flex, Text, TextField, IconButton, Button, Slider,
} from '@radix-ui/themes';
import {
  ArrowLeftIcon, ArrowRightIcon, ReloadIcon, HomeIcon, GearIcon,
  HamburgerMenuIcon, HeartIcon, BarChartIcon, CopyIcon, ExternalLinkIcon,
  SpeakerLoudIcon, SpeakerOffIcon, VideoIcon, MagnifyingGlassIcon,
  Cross2Icon,
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

function useTheme() {
  const [theme, setTheme] = useState({ appearance: 'dark', accent: 'orange' });
  useEffect(() => {
    api.get().then((s) => { if (s.shellTheme) setTheme(s.shellTheme); });
    const off = api.on('store-changed', (s) => { if (s.shellTheme) setTheme(s.shellTheme); });
    return off;
  }, []);
  return theme;
}

function Shell() {
  const theme = useTheme();
  const [edit, setEdit] = useState(HOME);
  const [canBack, setCanBack] = useState(false);
  const [canFwd, setCanFwd] = useState(false);
  const [sidebar, setSidebar] = useState(true);
  const [track, setTrack] = useState({ title: '', time: '', playing: false, cur: 0, dur: 0 });
  const [vol, setVol] = useState(80);
  const [muted, setMuted] = useState(false);
  const [pipUrl, setPipUrl] = useState('');
  const [recent, setRecent] = useState([]);
  const [links, setLinks] = useState([]);
  const [linkLabel, setLinkLabel] = useState('');
  const [page, setPage] = useState(HOME);
  const editing = useRef(false);

  useEffect(() => {
    api.get().then((s) => {
      const open = s.sidebarOpen !== false;
      setSidebar(open);
      setVol(Math.round((s.volume ?? 0.8) * 100));
      setMuted(!!s.muted);
      setRecent(s.recent || []);
      setLinks(s.links || []);
      api.cmd('layout', { sidebarOpen: open });
    });
    const offs = [
      api.on('nav-state', (st) => {
        if (!editing.current) setEdit(st.url);
        setPage(st.url || HOME);
        setCanBack(!!st.canBack);
        setCanFwd(!!st.canFwd);
      }),
      api.on('track', (t) => setTrack(t)),
      api.on('store-changed', (s) => {
        if (s.sidebarOpen !== undefined) setSidebar(s.sidebarOpen !== false);
        setRecent(s.recent || []);
        setLinks(s.links || []);
      }),
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
  const progress = track.dur > 0 ? Math.min(100, (track.cur / track.dur) * 100) : 0;

  return (
    <Theme accentColor={theme.accent} grayColor="gray" radius="large" appearance={theme.appearance}>
      <Flex direction="column" style={{ height: '100vh', background: theme.appearance === 'light' ? '#ffffff' : '#111113' }}>
        {/* Top bar */}
        <Flex align="center" gap="2" px="2" style={{ height: TOP_H, flexShrink: 0, borderBottom: '1px solid #2a2a2a' }}>
          <IconButton variant="ghost" onClick={toggleSidebar}>
            <HamburgerMenuIcon />
          </IconButton>
          <IconButton variant="ghost" disabled={!canBack} onClick={() => api.cmd('nav-back')}>
            <ArrowLeftIcon />
          </IconButton>
          <IconButton variant="ghost" disabled={!canFwd} onClick={() => api.cmd('nav-fwd')}>
            <ArrowRightIcon />
          </IconButton>
          <IconButton variant="ghost" onClick={() => api.cmd('nav-reload')}>
            <ReloadIcon />
          </IconButton>
          <IconButton variant="ghost" onClick={() => api.cmd('nav-go', HOME)}>
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
          <IconButton variant="ghost" onClick={() => api.cmd('open-settings')}>
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
              <Button variant={page === HOME ? 'solid' : 'soft'} style={{ justifyContent: 'flex-start' }} onClick={() => api.cmd('nav-go', HOME)}>
                <HomeIcon /> Home
              </Button>
              <Button variant={page.indexOf('/charts') >= 0 ? 'solid' : 'soft'} style={{ justifyContent: 'flex-start' }} onClick={() => api.cmd('nav-go', CHARTS)}>
                <BarChartIcon /> Charts
              </Button>
              <Button variant={page.indexOf('/you/likes') >= 0 ? 'solid' : 'soft'} style={{ justifyContent: 'flex-start' }} onClick={() => api.cmd('nav-go', LIKES)}>
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
              {recent.length > 0 ? (
                <React.Fragment>
                  <Text size="1" weight="bold" color="gray" mt="3">RECENTLY PLAYED</Text>
                  {recent.slice(0, 8).map((r, i) => (
                    <Button key={i} size="1" variant="ghost" style={{ justifyContent: 'flex-start' }}
                      onClick={() => api.cmd('nav-go', r.url)}>
                      <Text size="1" truncate>{r.title}</Text>
                    </Button>
                  ))}
                  <Button size="1" variant="ghost" color="gray" onClick={() => api.cmd('recent-clear')}>
                    Clear history
                  </Button>
                </React.Fragment>
              ) : null}
              <Text size="1" weight="bold" color="gray" mt="3">QUICK LINKS</Text>
              {links.map((l, i) => (
                <Flex key={i} align="center" gap="1">
                  <Button size="1" variant="ghost" style={{ justifyContent: 'flex-start', flex: 1 }}
                    onClick={() => api.cmd('nav-go', l.url)}>
                    <Text size="1" truncate>{l.label}</Text>
                  </Button>
                  <IconButton size="1" variant="ghost" color="gray" onClick={() => api.cmd('link-remove', l.url)}>
                    <Cross2Icon />
                  </IconButton>
                </Flex>
              ))}
              <TextField.Root
                size="1"
                value={linkLabel}
                placeholder="Label for current page…"
                onChange={(e) => setLinkLabel(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' && linkLabel.trim()) {
                    api.cmd('link-add', { label: linkLabel.trim(), url: edit });
                    setLinkLabel('');
                  }
                }}
              />
              <Flex gap="2" mt="2">
                <IconButton variant="soft" onClick={() => api.cmd('copy-link')}>
                  <CopyIcon />
                </IconButton>
                <IconButton variant="soft" onClick={() => api.cmd('open-ext')}>
                  <ExternalLinkIcon />
                </IconButton>
              </Flex>
            </Flex>
          ) : null}
          <div id="site-slot" style={{ flex: 1, background: theme.appearance === 'light' ? '#ffffff' : '#111113' }} />
        </Flex>

        {/* Bottom bar */}
        <div style={{ position: 'relative', flexShrink: 0 }}>
          <div style={{
            position: 'absolute', top: 0, left: 0, height: 2, zIndex: 5,
            width: progress + '%', background: '#f76b15', transition: 'width 1s linear',
          }} />
          <Flex align="center" gap="2" px="3" style={{ height: BOTTOM_H, borderTop: '1px solid #2a2a2a' }}>
            <Text size="2" truncate style={{ flex: 1 }}>
              {track.title ? (track.time ? track.time + '  -  ' : '') + track.title : 'Nothing playing'}
            </Text>
            <IconButton variant="ghost" onClick={() => api.cmd('like')}>
              <HeartIcon />
            </IconButton>
            <IconButton variant="ghost" onClick={() => api.cmd('pip-open', '')}>
              <VideoIcon />
            </IconButton>
            <IconButton
              variant="ghost"
             
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
        </div>
      </Flex>
    </Theme>
  );
}

createRoot(document.getElementById('root')).render(<Shell />);
