import React, { useEffect, useState } from 'react';
import { createRoot } from 'react-dom/client';
import {
  Theme, Flex, Text, Heading, Button, Switch, Separator, Card, Badge, Callout,
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

function Settings() {
  const [s, setS] = useState(null);
  const [msg, setMsg] = useState('');

  useEffect(() => {
    api.get().then(setS);
  }, []);

  if (!s) return <Theme appearance="dark"><div style={{ padding: 16 }}><Text>Loading…</Text></div></Theme>;

  const set = (patch) => {
    const next = { ...s, ...patch };
    setS(next);
    api.set(patch);
  };
  const setRd = (key, v) => {
    const rd = { ...(s.rd || {}), [key]: v };
    set({ rd });
  };

  const flash = (t) => { setMsg(t); setTimeout(() => setMsg(''), 2500); };

  return (
    <Theme accentColor="orange" grayColor="gray" radius="large" appearance="dark">
      <Flex direction="column" gap="3" p="4" className="shell-enter">
        <Flex align="center" gap="2">
          <Heading size="4">Settings</Heading>
          <Badge color="orange" variant="soft">Alt UI</Badge>
        </Flex>

        <Card>
          <Flex direction="column">
            <Row index={0} label="Minimize to tray" hint="Closing the window keeps music playing"
              checked={s.trayHide} onChange={(v) => set({ trayHide: v })} />
            <Separator size="4" />
            <Row index={1} label="Start with Windows" checked={s.autostart} onChange={(v) => set({ autostart: v })} />
            <Separator size="4" />
            <Row index={2} label="Player window on track start" checked={s.playerPopup} onChange={(v) => set({ playerPopup: v })} />
          </Flex>
        </Card>

        <Card>
          <Flex direction="column">
            <Row index={3} label="Ad blocking" hint="Off by default. Never touches login windows"
              checked={s.adblock} onChange={(v) => set({ adblock: v })} />
            <Separator size="4" />
            <Row index={4} label="Site animations" hint="Simple fades and hovers on site elements"
              checked={s.anims} onChange={(v) => set({ anims: v })} />
            <Separator size="4" />
            <Row index={5} label="ReDesign" hint="Full Radix restyle of the site"
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
            <Flex gap="2">
              <Button size="2" variant="soft" onClick={() => api.cmd('ext-add').then((r) => { if (r.list) set({ extensions: r.list }); flash(r.msg); })}>
                Add from folder…
              </Button>
              <Button size="2" variant="ghost" onClick={() => api.cmd('cache-clear').then((r) => flash(r.msg))}>
                Clear cache
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

        <Text size="1" color="gray">SoundCloud Desktop Alt 1.9.0. Hotkeys: numpad 1/2/3 tracks, 4/5 volume.</Text>
      </Flex>
    </Theme>
  );
}

createRoot(document.getElementById('root')).render(<Settings />);
