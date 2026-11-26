# Relay deployment

The relay is a prototype and should not be exposed directly to the public internet.

1. Install Docker Engine and the Compose plugin on a Linux host.
2. Clone this repository and enter `Server/`.
3. Run `docker compose up -d --build`.
4. Put a reverse proxy with automatic TLS (for example Caddy or nginx with a valid certificate) in front of `127.0.0.1:8787`.
5. In the game, enter the resulting HTTPS base URL in the Relay Server field and press SET SERVER.

Health check: `GET /health`.

The relay currently stores room state in process memory. Restarting the container clears all rooms. It has no account system, persistent database, anti-cheat authority, or production-grade abuse protection. Do not treat room membership IDs as secure user authentication. Keep it behind HTTPS and consider additional authentication, per-room secrets, stricter validation, and persistent storage before public release.
