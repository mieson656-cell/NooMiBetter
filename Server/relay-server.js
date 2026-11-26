'use strict';

// Minimal room relay for NooMiBetter. No accounts or personal data are stored.
// For internet use, put this process behind HTTPS (for example Caddy or nginx).
const http = require('node:http');
const crypto = require('node:crypto');

const PORT = Number(process.env.PORT || 8787);
const rooms = new Map();
const limits = new Map();
const ROOM_TTL_MS = 60 * 60 * 1000;
const MAX_PLAYERS = 8;
const MAX_EVENTS = 300;
const MAX_BODY = 16 * 1024;

function send(res, status, data) {
  const body = JSON.stringify(data);
  res.writeHead(status, {
    'content-type': 'application/json; charset=utf-8',
    'content-length': Buffer.byteLength(body),
    'cache-control': 'no-store',
    'access-control-allow-origin': '*',
    'access-control-allow-headers': 'content-type',
    'access-control-allow-methods': 'GET, POST, OPTIONS'
  });
  res.end(body);
}
function readJson(req) {
  return new Promise((resolve, reject) => {
    let size = 0; let raw = '';
    req.on('data', chunk => {
      size += chunk.length;
      if (size > MAX_BODY) { reject(new Error('body too large')); req.destroy(); return; }
      raw += chunk;
    });
    req.on('end', () => {
      try { resolve(raw ? JSON.parse(raw) : {}); }
      catch { reject(new Error('invalid JSON')); }
    });
  });
}
function rateLimited(ip) {
  const now = Date.now(); const row = limits.get(ip) || { start: now, count: 0 };
  if (now - row.start > 10_000) { row.start = now; row.count = 0; }
  row.count += 1; limits.set(ip, row);
  return row.count > 80;
}
function newCode() {
  const alphabet = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
  let code;
  do { code = Array.from({ length: 6 }, () => alphabet[crypto.randomInt(alphabet.length)]).join(''); }
  while (rooms.has(code));
  return code;
}
function prune() {
  const now = Date.now();
  for (const [code, room] of rooms) if (now - room.updatedAt > ROOM_TTL_MS) rooms.delete(code);
  for (const [ip, row] of limits) if (now - row.start > 60_000) limits.delete(ip);
}
setInterval(prune, 60_000).unref();

const server = http.createServer(async (req, res) => {
  if (req.method === 'OPTIONS') return send(res, 204, {});
  const ip = req.socket.remoteAddress || 'unknown';
  if (rateLimited(ip)) return send(res, 429, { error: 'rate limit' });
  const url = new URL(req.url, `http://${req.headers.host || 'localhost'}`);
  if (req.method === 'GET' && url.pathname === '/health') return send(res, 200, { ok: true, rooms: rooms.size });

  try {
    if (req.method === 'POST' && url.pathname === '/api/lobbies') {
      const body = await readJson(req);
      const playerId = typeof body.playerId === 'string' ? body.playerId.slice(0, 64) : crypto.randomUUID();
      const code = newCode();
      rooms.set(code, { createdAt: Date.now(), updatedAt: Date.now(), cursor: 0, members: new Set([playerId]), events: [] });
      return send(res, 201, { code, playerId });
    }

    const join = url.pathname.match(/^\/api\/lobbies\/([A-Z0-9]+)\/join$/i);
    if (req.method === 'POST' && join) {
      const code = join[1].toUpperCase(); const room = rooms.get(code);
      if (!room) return send(res, 404, { error: 'room not found' });
      const body = await readJson(req);
      const playerId = typeof body.playerId === 'string' ? body.playerId.slice(0, 64) : crypto.randomUUID();
      if (!room.members.has(playerId) && room.members.size >= MAX_PLAYERS) return send(res, 409, { error: 'room full' });
      room.members.add(playerId); room.updatedAt = Date.now();
      return send(res, 200, { code, playerId });
    }

    const events = url.pathname.match(/^\/api\/lobbies\/([A-Z0-9]+)\/events$/i);
    if (events) {
      const code = events[1].toUpperCase(); const room = rooms.get(code);
      if (!room) return send(res, 404, { error: 'room not found' });
      room.updatedAt = Date.now();
      if (req.method === 'POST') {
        const body = await readJson(req);
        const playerId = typeof body.playerId === 'string' ? body.playerId : '';
        if (!room.members.has(playerId)) return send(res, 403, { error: 'not a room member' });
        const event = {
          cursor: ++room.cursor, playerId,
          x: Number(body.x) || 0, y: Number(body.y) || 0, z: Number(body.z) || 0,
          qx: Number(body.qx) || 0, qy: Number(body.qy) || 0, qz: Number(body.qz) || 0, qw: Number(body.qw) || 1
        };
        room.events.push(event);
        if (room.events.length > MAX_EVENTS) room.events.splice(0, room.events.length - MAX_EVENTS);
        return send(res, 202, { ok: true, cursor: event.cursor });
      }
      if (req.method === 'GET') {
        const after = Math.max(0, Number(url.searchParams.get('after')) || 0);
        const playerId = url.searchParams.get('playerId') || '';
        if (!room.members.has(playerId)) return send(res, 403, { error: 'not a room member' });
        const batch = room.events.filter(e => e.cursor > after).slice(-80);
        return send(res, 200, { cursor: room.cursor, events: batch });
      }
    }
    return send(res, 404, { error: 'not found' });
  } catch (error) {
    return send(res, 400, { error: error.message || 'bad request' });
  }
});

server.listen(PORT, '0.0.0.0', () => {
  console.log(`NooMiBetter relay listening on 0.0.0.0:${PORT}`);
});
