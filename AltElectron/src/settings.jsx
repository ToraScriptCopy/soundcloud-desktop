import React, { useEffect, useState } from 'react';
import { createRoot } from 'react-dom/client';
import {
  Theme, Flex, Text, Heading, Button, Switch, Separator, Card, Badge, Callout,
  Select, SegmentedControl,
} from '@radix-ui/themes';
import { InfoCircledIcon } from '@radix-ui/react-icons';
import '@radix-ui/themes/styles.css';
import './shell.css';

const api = window.api;

const RD_FLAGS = [
  ['rdCards', 'Track cards'],
  ['rdButtons', 'Buttons'],
  ['rdHeader', 'Site header'],
  ['rdPlayer', 'Bottom player'],
  ['rdComments', 'Comments'],
  ['rdSidebar', 'Sidebar'],
  ['rdInputs', 'Inputs'],
  ['rdPopups', 'Popups and menus'],
];
const ACCENTS = [
  ['orange', '#f76b15'], ['crimson', '#e93d82'], ['violet', '#8e4ec6'],
  ['blue', '#0090ff'], ['green', '#46a758'], ['amber', '#ffb224'],
];
const SPEEDS = ['0.75', '1', '1.25', '1.5', '2'];
const SLEEPS = [['0', 'Off'], ['15', '15 min'], ['30', '30 min'], ['60', '1 hour'], ['90', '90 min']];

function Row({ label, hint, checked, onChange, index }) {
  return (
    <Flex
      align="center"
      justify="between"
      gap="3"
      py="2"
      className="row-enter"
      style={{ animationDelay: Math.min(index * 25, 200) + 'ms' }}
    >
      <Flex direction="column" gap="1" style={{ flex: 1 }}>
        <Text size="2" weight="medium">{label}</Text>
        {hint ? <Text size="1" color="gray">{hint}</Text> : null}
      </Flex>
      <Switch checked={!!checked} onCheckedChange={onChange} />
    </Flex>
  );
}

function fmtDur(sec) {
  sec = Math.round(sec || 0);
  const h = Math.floor(sec / 3600), m = Math.floor((sec % 3600) / 60);
  if (h > 0) return h + 'h ' + m + 'm';
  return m + 'm';
}

function Settings() {
  const [s, setS] = useState(null);
  const [msg, setMsg] = useState('');

  useEffect(() => {
    api.get().then(setS);
    const off = api.on('store-changed', (next) => setS((prev) => ({ ...prev, ...next })));
    return off;
  }, []);

  if (!s) return <Theme appearance="dark"><div style={{ padding: 16 }}><Text>Loading…</Text></div></Theme>;
  const theme = s.shellTheme || { appearance: 'dark', accent: 'orange' };

  const set = (patch) => {
    const next = { ...s, ...patch };
    setS(next);
    api.set(patch);
  };
  const setRd = (key, v) => set({ rd: { ...(s.rd || {}), [key]: v } });
  const setTheme = (patch) => set({ shellTheme: { ...theme, ...patch } });
  const flash = (t) => { setMsg(t); setTimeout(() => setMsg(''), 3000); };

  return (
    <Theme accentColor={theme.accent} grayColor="gray" radius="large" appearance={theme.appearance}>
      <Flex direction="column" gap="3" p="4" className="shell-enter">
        <Flex align="center" gap="2">
          <Heading size="4">Settings</Heading>
          <Badge color="orange" variant="soft">Alt UI</Badge>
          <Badge color="gray" variant="soft">experimental</Badge>
        </Flex>

        <Card>
          <Flex direction="column" gap="2">
            <Text size="2" weight="bold">Shell theme</Text>
            <SegmentedControl.Root
              value={theme.appearance}
              onValueChange={(v) => setTheme({ appearance: v })}
            >
              <SegmentedControl.Item value="dark">Dark</SegmentedControl.Item>
              <SegmentedControl.Item value="light">Light</SegmentedControl.Item>
              <SegmentedControl.Item value="inherit">System</SegmentedControl.Item>
            </SegmentedControl.Root>
            <Flex gap="2" mt="1">
              {ACCENTS.map(([name, color]) => (
                <button
                  key={name}
                  title={name}
                  onClick={() => setTheme({ accent: name })}
                  style={{
                    width: 26, height: 26, borderRadius: 99, cursor: 'pointer',
                    background: color, border: theme.accent === name ? '2px solid #fff' : '2px solid transparent',
                  }}
                />
              ))}
            </Flex>
          </Flex>
        </Card>

        <Card>
          <Flex direction="column">
            <Row index={0} label="Minimize to tray" hint="Closing the window keeps music playing"
              checked={s.trayHide} onChange={(v) => set({ trayHide: v })} />
            <Separator size="4" />
            <Row index={1} label="Start with Windows" checked={s.autostart} onChange={(v) => set({ autostart: v })} />
            <Separator size="4" />
            <Row index={2} label="Start minimized" checked={s.startMin} onChange={(v) => set({ startMin: v })} />
            <Separator size="4" />
            <Row index={3} label="Always on top" checked={s.alwaysOnTop} onChange={(v) => set({ alwaysOnTop: v })} />
            <Separator size="4" />
            <Row index={4} label="Boss key (F9)" hint="Instantly hides every window"
              checked={s.bossKey} onChange={(v) => set({ bossKey: v })} />
            <Separator size="4" />
            <Row index={5} label="Player window on track start" checked={s.playerPopup} onChange={(v) => set({ playerPopup: v })} />
            <Separator size="4" />
            <Row index={6} label="Track notifications" checked={s.notify} onChange={(v) => set({ notify: v })} />
          </Flex>
        </Card>

        <Card>
          <Flex direction="column" gap="2">
            <Text size="2" weight="bold">Playback</Text>
            <Flex align="center" justify="between">
              <Text size="2">Speed</Text>
              <Select.Root value={String(s.speed ?? 1)} onValueChange={(v) => set({ speed: parseFloat(v) })}>
                <Select.Trigger />
                <Select.Content>
                  {SPEEDS.map((x) => <Select.Item key={x} value={x}>{x}x</Select.Item>)}
                </Select.Content>
              </Select.Root>
            </Flex>
            <Flex align="center" justify="between">
              <Text size="2">Sleep timer</Text>
              <Select.Root value={String(s.sleep ?? 0)} onValueChange={(v) => set({ sleep: parseInt(v, 10) })}>
                <Select.Trigger />
                <Select.Content>
                  {SLEEPS.map(([v, l]) => <Select.Item key={v} value={v}>{l}</Select.Item>)}
                </Select.Content>
              </Select.Root>
            </Flex>
            <Row index={7} label="Repeat one" checked={s.repeatOne} onChange={(v) => set({ repeatOne: v })} />
          </Flex>
        </Card>

        <Card>
          <Flex direction="column">
            <Row index={8} label="Ad blocking" hint={'Off by default. Blocked this session: ' + (s.blocks ?? 0)}
              checked={s.adblock} onChange={(v) => set({ adblock: v })} />
            <Separator size="4" />
            <Row index={9} label="Site animations" hint="Simple fades and hovers on site elements"
              checked={s.anims} onChange={(v) => set({ anims: v })} />
            <Separator size="4" />
            <Row index={10} label="ReDesign" hint="Full Radix restyle of the site"
              checked={s.redesign} onChange={(v) => set({ redesign: v })} />
          </Flex>
        </Card>

        {s.redesign ? (
          <Card>
            <Flex direction="column" gap="1">
              <Text size="2" weight="bold" mb="1">ReDesign parts</Text>
              {RD_FLAGS.map(([key, label], i) => (
                <React.Fragment key={key}>
                  {i > 0 ? <Separator size="4" /> : null}
                  <Row index={i} label={label} checked={s.rd && s.rd[key] !== false}
                    onChange={(v) => setRd(key, v)} />
                </React.Fragment>
              ))}
            </Flex>
          </Card>
        ) : null}

        <Card>
          <Flex direction="column" gap="2">
            <Text size="2" weight="bold">Listening stats</Text>
            <Text size="2" color="gray">
              {s.stats ? s.stats.tracks + ' tracks, ' + fmtDur(s.stats.seconds) + ' listened' : '…'}
            </Text>
            <Button size="2" variant="ghost" onClick={() => api.cmd('stats-reset')}>Reset stats</Button>
          </Flex>
        </Card>

        <Card>
          <Flex direction="column" gap="2">
            <Text size="2" weight="bold">Browser extensions</Text>
            {(s.extensions || []).length === 0
              ? <Text size="2" color="gray">None installed. Pick an unpacked extension folder.</Text>
              : s.extensions.map((d, i) => (
                <Flex key={i} align="center" justify="between" gap="2">
                  <Text size="1" color="gray" truncate style={{ flex: 1 }}>{d}</Text>
                  <Button size="1" variant="soft" color="red"
                    onClick={() => api.cmd('ext-remove', d).then((r) => { set({ extensions: r.list }); flash(r.msg); })}>
                    Remove
                  </Button>
                </Flex>
              ))}
            <Button size="2" variant="soft" onClick={() => api.cmd('ext-add').then((r) => { if (r.list) set({ extensions: r.list }); flash(r.msg); })}>
              Add from folder…
            </Button>
          </Flex>
        </Card>

        <Card>
          <Flex direction="column" gap="2">
            <Text size="2" weight="bold">Updates and data</Text>
            <Flex gap="2">
              <Button size="2" variant="soft" onClick={() => api.cmd('check-updates').then((r) => flash(r.msg))}>
                Check for updates
              </Button>
              <Button size="2" variant="ghost" onClick={() => api.cmd('cache-clear').then((r) => flash(r.msg))}>
                Clear cache
              </Button>
            </Flex>
            <Flex gap="2">
              <Button size="2" variant="ghost" onClick={() => api.cmd('settings-export').then((r) => flash(r.msg))}>
                Export settings
              </Button>
              <Button size="2" variant="ghost" onClick={() => api.cmd('settings-import').then((r) => { flash(r.msg); api.get().then(setS); })}>
                Import settings
              </Button>
            </Flex>
            <Flex gap="2">
              <Button size="2" variant="ghost" onClick={() => api.cmd('data-open')}>Open data folder</Button>
              <Button size="2" variant="ghost" color="red" onClick={() => api.cmd('data-reset').then((r) => { flash(r.msg); api.get().then(setS); })}>
                Reset everything
              </Button>
            </Flex>
            {msg ? (
              <Callout.Root size="1" color="orange">
                <Callout.Icon><InfoCircledIcon /></Callout.Icon>
                <Callout.Text>{msg}</Callout.Text>
              </Callout.Root>
            ) : null}
          </Flex>
        </Card>

        <Text size="1" color="gray">SoundCloud Desktop Alt 3.0.0, experimental. Hotkeys: numpad 1/2/3 tracks, 4/5 volume.</Text>
      </Flex>
    </Theme>
  );
}

createRoot(document.getElementById('root')).render(<Settings />);
