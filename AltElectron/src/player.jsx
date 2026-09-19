import React, { useEffect, useState } from 'react';
import { createRoot } from 'react-dom/client';
import {
  Theme, Flex, Text, Heading, Button, Slider, IconButton, Select,
} from '@radix-ui/themes';
import {
  PlayIcon, PauseIcon, TrackPreviousIcon, TrackNextIcon,
  SpeakerLoudIcon, SpeakerOffIcon, Cross2Icon, ArrowDownIcon,
  HeartIcon, LoopIcon, LapTimerIcon,
} from '@radix-ui/react-icons';
import '@radix-ui/themes/styles.css';
import './shell.css';

const api = window.api;
const SPEEDS = ['0.75', '1', '1.25', '1.5', '2'];

function Player() {
  const [theme, setTheme] = useState({ appearance: 'dark', accent: 'orange' });
  const [track, setTrack] = useState({ title: '', artist: '', art: '', time: '', playing: false });
  const [vol, setVol] = useState(80);
  const [muted, setMuted] = useState(false);
  const [speed, setSpeed] = useState('1');
  const [repeat, setRepeat] = useState(false);
  const [mini, setMini] = useState(false);

  useEffect(() => {
    api.get().then((s) => {
      setVol(Math.round((s.volume ?? 0.8) * 100));
      setMuted(!!s.muted);
      setSpeed(String(s.speed ?? 1));
      setRepeat(!!s.repeatOne);
      setMini(!!s.mini);
      if (s.shellTheme) setTheme(s.shellTheme);
    });
    const offs = [
      api.on('track', (t) => setTrack(t)),
      api.on('store-changed', (s) => {
        if (s.shellTheme) setTheme(s.shellTheme);
        setRepeat(!!s.repeatOne);
        setMini(!!s.mini);
        setSpeed(String(s.speed ?? 1));
      }),
    ];
    return () => offs.forEach((f) => f());
  }, []);

  const pushVol = (v, m) => api.cmd('volume', { volume: v / 100, muted: m });

  return (
    <Theme accentColor={theme.accent} grayColor="gray" radius="large" appearance={theme.appearance}>
      <Flex direction="column" gap="3" p="3" className="shell-enter">
        {!mini ? (
          <Flex gap="3" align="center">
            {track.art
              ? <img key={track.art} className="artwork art-swap" src={track.art} width="92" height="92" alt="" />
              : <div className="artwork" style={{ width: 92, height: 92 }} />}
            <Flex direction="column" gap="1" style={{ minWidth: 0, flex: 1 }}>
              <Heading size="3" truncate>{track.title || 'Nothing playing'}</Heading>
              <Text size="2" color="gray" truncate>{track.artist}</Text>
              <Text size="2" color="gray">{track.time}</Text>
            </Flex>
          </Flex>
        ) : (
          <Flex align="center" gap="2" style={{ minWidth: 0 }}>
            <Text size="2" weight="bold" truncate style={{ flex: 1 }}>
              {track.title || 'Nothing playing'}
            </Text>
            <Text size="2" color="gray">{track.time}</Text>
          </Flex>
        )}
        <Flex gap="2" align="center">
          <IconButton variant="soft" onClick={() => api.cmd('prev')}>
            <TrackPreviousIcon />
          </IconButton>
          <Button variant="solid" onClick={() => api.cmd('toggle')}>
            {track.playing ? <PauseIcon /> : <PlayIcon />}
          </Button>
          <IconButton variant="soft" onClick={() => api.cmd('next')}>
            <TrackNextIcon />
          </IconButton>
          <IconButton
            variant={repeat ? 'solid' : 'ghost'}
            onClick={() => api.set({ repeatOne: !repeat })}
           
          >
            <LoopIcon />
          </IconButton>
          <IconButton variant="ghost" onClick={() => api.cmd('like')}>
            <HeartIcon />
          </IconButton>
          {!mini ? (
            <React.Fragment>
              <IconButton
                variant="ghost"
               
                onClick={() => { const m = !muted; setMuted(m); pushVol(vol, m); }}
              >
                {muted ? <SpeakerOffIcon /> : <SpeakerLoudIcon />}
              </IconButton>
              <div style={{ flex: 1, minWidth: 60 }}>
                <Slider
                  value={[muted ? 0 : vol]}
                  max={100}
                  onValueChange={([v]) => { setVol(v); if (v > 0 && muted) { setMuted(false); pushVol(v, false); } else pushVol(v, muted); }}
                />
              </div>
              <Text size="2" color="gray" style={{ width: 38 }}>{muted ? '0%' : vol + '%'}</Text>
            </React.Fragment>
          ) : null}
          <Select.Root
            value={speed}
            onValueChange={(v) => { setSpeed(v); api.set({ speed: parseFloat(v) }); }}
          >
            <Select.Trigger />
            <Select.Content>
              {SPEEDS.map((s) => (
                <Select.Item key={s} value={s}>{s}x</Select.Item>
              ))}
            </Select.Content>
          </Select.Root>
          <IconButton variant="ghost" onClick={() => api.cmd('mini-toggle')}>
            <LapTimerIcon />
          </IconButton>
          <IconButton variant="ghost" onClick={() => api.cmd('to-tray')}>
            <ArrowDownIcon />
          </IconButton>
          <IconButton variant="ghost" color="gray" onClick={() => api.cmd('player-hide')}>
            <Cross2Icon />
          </IconButton>
        </Flex>
      </Flex>
    </Theme>
  );
}

createRoot(document.getElementById('root')).render(<Player />);
