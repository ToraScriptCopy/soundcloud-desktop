import React, { useEffect, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { Theme, Flex, Text, Heading, Button, Slider, IconButton } from '@radix-ui/themes';
import {
  PlayIcon, PauseIcon, TrackPreviousIcon, TrackNextIcon,
  SpeakerLoudIcon, SpeakerOffIcon, Cross2Icon, ArrowDownIcon,
} from '@radix-ui/react-icons';
import '@radix-ui/themes/styles.css';
import './shell.css';

const api = window.api;

function Player() {
  const [track, setTrack] = useState({ title: '', artist: '', art: '', time: '', playing: false });
  const [vol, setVol] = useState(80);
  const [muted, setMuted] = useState(false);

  useEffect(() => {
    let off = () => {};
    api.get().then((s) => {
      setVol(Math.round((s.volume ?? 0.8) * 100));
      setMuted(!!s.muted);
    });
    off = api.on('track', (t) => setTrack(t));
    return off;
  }, []);

  const pushVol = (v, m) => api.cmd('volume', { volume: v / 100, muted: m });

  return (
    <Theme accentColor="orange" grayColor="gray" radius="large" appearance="dark">
      <Flex direction="column" gap="3" p="3" className="shell-enter">
        <Flex gap="3" align="center">
          {track.art
            ? <img className="artwork" src={track.art} width="92" height="92" alt="" />
            : <div className="artwork" style={{ width: 92, height: 92 }} />}
          <Flex direction="column" gap="1" style={{ minWidth: 0, flex: 1 }}>
            <Heading size="3" truncate>{track.title || 'Nothing playing'}</Heading>
            <Text size="2" color="gray" truncate>{track.artist}</Text>
            <Text size="2" color="gray">{track.time}</Text>
          </Flex>
        </Flex>
        <Flex gap="2" align="center">
          <IconButton variant="soft" onClick={() => api.cmd('prev')} title="Previous track">
            <TrackPreviousIcon />
          </IconButton>
          <Button variant="solid" onClick={() => api.cmd('toggle')} title="Play / pause">
            {track.playing ? <PauseIcon /> : <PlayIcon />}
          </Button>
          <IconButton variant="soft" onClick={() => api.cmd('next')} title="Next track">
            <TrackNextIcon />
          </IconButton>
          <IconButton
            variant="ghost"
            title={muted ? 'Unmute' : 'Mute'}
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
          <IconButton variant="ghost" onClick={() => api.cmd('to-tray')} title="Minimize to tray">
            <ArrowDownIcon />
          </IconButton>
          <IconButton variant="ghost" color="gray" onClick={() => api.cmd('player-hide')} title="Close">
            <Cross2Icon />
          </IconButton>
        </Flex>
      </Flex>
    </Theme>
  );
}

createRoot(document.getElementById('root')).render(<Player />);
