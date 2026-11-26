'use strict';
const assert = require('node:assert/strict');
const { spawn } = require('node:child_process');

(async () => {
  const port = 18000 + Math.floor(Math.random() * 1000);
  const child = spawn(process.execPath, ['relay-server.js'], {
    cwd: __dirname, env: { ...process.env, PORT: String(port) }, stdio: 'ignore'
  });
  const base = `http://127.0.0.1:${port}`;
  const wait = ms => new Promise(r => setTimeout(r, ms));
  async function request(path, options) {
    const response = await fetch(base + path, options);
    return { status: response.status, body: await response.json() };
  }
  try {
    let health;
    for (let i = 0; i < 30; i++) {
      try { health = await request('/health'); if (health.status === 200) break; } catch {}
      await wait(100);
    }
    assert.equal(health.status, 200, 'health endpoint');
    const created = await request('/api/lobbies', {
      method: 'POST', headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ playerId: 'host' })
    });
    assert.equal(created.status, 201);
    const { code } = created.body;
    const joined = await request(`/api/lobbies/${code}/join`, {
      method: 'POST', headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ playerId: 'guest' })
    });
    assert.equal(joined.status, 200);
    const sent = await request(`/api/lobbies/${code}/events`, {
      method: 'POST', headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ playerId: 'host', x: 1.25, y: 2, z: 3 })
    });
    assert.equal(sent.status, 202);
    const received = await request(`/api/lobbies/${code}/events?after=0&playerId=guest`);
    assert.equal(received.status, 200);
    assert.equal(received.body.events.length, 1);
    assert.equal(received.body.events[0].x, 1.25);
    const denied = await request(`/api/lobbies/${code}/events?after=0&playerId=intruder`);
    assert.equal(denied.status, 403);
    console.log('PASS: health, room create/join, state relay, membership enforcement');
  } finally {
    child.kill();
  }
})().catch(error => { console.error(error); process.exitCode = 1; });
