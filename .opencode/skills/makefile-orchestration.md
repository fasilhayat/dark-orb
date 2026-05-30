---
name: makefile-orchestration
description: Use when orchestrating Docker builds, running the demo, running tests, or managing containers via the Makefile. Not for editing the Makefile itself.
---

# Makefile Orchestration

## Environments

| Mode | DB | API | Demo | Ports exposed |
|------|----|-----|------|---------------|
| `up-local` | Docker | Docker | host (`make demo-local`) | 5432, 8585 |
| `up-dev` | Docker | Docker | Docker (interactive) | 5432, 8585 |
| `up-test` | Docker | Docker | Docker (interactive) | — |
| `up-preprod` | Docker | Docker | — | — |
| `up-prod` | Docker | Docker | — | — |

## Quick reference

| Goal | Command |
|------|---------|
| Start DB + API (demo on host) | `make up-local` |
| Start DB + API + demo (ports exposed) | `make up-dev` |
| Start DB + API + demo (no ports) | `make up-test` |
| Start DB + API (no demo, no ports) | `make up-preprod` / `make up-prod` |
| Run demo locally against up-local | `make demo-local` |
| Stop all containers | `make down` |
| Nuke everything (volumes + publish output) | `make clean` |
| Run tests | `make test` |
| Clear combat log files | `make clean-logs` |

## How it works

- `up-local` runs `publish` to build the API, then starts DB + API containers.
- `up-dev` / `up-test` run `publish` + `publish-demo`, then start DB + API containers and launch the demo interactively.
- `up-preprod` / `up-prod` run only `publish` and start DB + API (no demo).
- `up-local` exposes ports on the host for the demo to connect via `make demo-local`.

## Docker caching

Docker `COPY` layers use content checksums (not timestamps). Any change to a `.cs` file changes the checksum, which invalidates the cache for that layer and everything that follows.

For a full clean rebuild, use `make clean` then `make up-local` (or `make up-dev`).
