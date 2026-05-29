---
name: makefile-orchestration
description: Use when orchestrating Docker builds, running the demo, running tests, or managing containers via the Makefile. Not for editing the Makefile itself.
---

# Makefile Orchestration

## Quick reference

| Goal | Command |
|------|---------|
| Launch the demo (builds + runs) | `make demo` |
| Start API + DB in background | `make up` |
| Stop containers (preserves DB) | `make down` |
| Rebuild + restart everything | `make reset` |
| Full clean rebuild (nukes DB too) | `make clean` then `make up` |
| Run tests | `make test` |
| Build images without running | `make build` |
| Force no-cache build | `make build-no-cache` |
| Build demo image only | `make build-demo` |
| View container logs | `make logs`, `make api-logs`, `make db-logs` |
| Clear combat log files | `make clean-logs` |

## How it works

All build commands (`up`, `build`, `build-no-cache`, `demo`, `reset`) depend on `make publish` which runs `dotnet publish` to produce fresh binaries under `publish/`. Docker then detects content changes via file checksums and only rebuilds the affected layers.

- `up` runs `docker compose up -d --build` — always rebuilds before starting
- `demo` runs `publish` → builds demo image → starts API + DB → runs demo container
- `reset` is `down` → `publish` → `docker compose up -d --build`

## Docker caching

Docker `COPY` layers use content checksums (not timestamps). Any change to a `.cs` file changes the checksum, which invalidates the cache for that layer and everything that follows. No manual `--no-cache` is needed for normal code changes.

Use `make build-no-cache` only when you suspect the Docker layer cache is corrupted or stale in an unexpected way.
