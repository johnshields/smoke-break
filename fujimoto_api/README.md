#  fujimoto API

A Cloudflare Workers API for saving and loading player data, backed by D1 (SQLite).

# Run

```bash
$ cd fujimoto_api
$ npx wrangler dev
```

# Deploy

```bash
$ npx wrangler deploy
```

# Seed DB

```bash
$ npx wrangler d1 execute fujimoto --file=sql/schema.sql --remote
```
