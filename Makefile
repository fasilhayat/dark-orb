.PHONY: help build build-no-cache up down restart reset logs api-logs db-logs clean clean-logs test test-coverage build-local demo demo-local build-demo sync-instructions

-include .env
export ENV
export COMPOSE_PROJECT_NAME

ENV                  ?= prod
COMPOSE_PROJECT_NAME  = battle-arena-$(ENV)
COMPOSE               = docker compose -f docker-compose.yml -f docker-compose.$(ENV).yml

help:
	@cmd /C "echo  BattleArena -- available make targets"
	@cmd /C "echo  ======================================="
	@echo.
	@cmd /C "echo  ENVIRONMENT SETUP"
	@cmd /C "echo    Copy .env.example to .env and set ENV before running any target."
	@cmd /C "echo    Or pass ENV on the command line:  make up ENV=dev"
	@echo.
	@cmd /C "echo    ENV=localdev  DB+API in Docker (ports 5432/8585 exposed). Demo runs on host."
	@cmd /C "echo    ENV=dev       DB+API+Demo in Docker (ports exposed). Use make demo to run demo."
	@cmd /C "echo    ENV=test      DB+API+Demo in Docker (no host ports). Use make demo to run demo."
	@cmd /C "echo    ENV=preprod   DB+API in Docker only, no host ports."
	@cmd /C "echo    ENV=prod      DB+API in Docker only, no host ports.  [default when no .env]"
	@echo.
	@cmd /C "echo  CONTAINERS"
	@cmd /C "echo    make build             Publish API, then build all Docker images for current ENV"
	@cmd /C "echo    make build-no-cache    Same as build but forces a full image rebuild (no layer cache)"
	@cmd /C "echo    make up                Publish API, build images, and start DB+API in the background"
	@cmd /C "echo    make down              Stop and remove containers (volumes and DB data are preserved)"
	@cmd /C "echo    make restart           down then up -- pick up config changes without data loss"
	@cmd /C "echo    make reset             down, republish, rebuild, and start fresh (data preserved)"
	@cmd /C "echo    make clean             Stop containers AND delete all volumes/data for current ENV"
	@echo.
	@cmd /C "echo  DEMO"
	@cmd /C "echo    make demo              Build demo image and run it interactively in Docker"
	@cmd /C "echo                           Requires ENV=dev or ENV=test (demo not in other envs)"
	@cmd /C "echo                           Also starts DB+API if not already running (via make up)"
	@cmd /C "echo    make demo-local        Run the demo directly on the host (no Docker for the demo)"
	@cmd /C "echo                           Sets DOTNET_ENVIRONMENT=LocalDev, connects to localhost:8585"
	@cmd /C "echo                           Requires DB+API already running: make up ENV=localdev"
	@cmd /C "echo    make build-demo        Build only the demo Docker image (no run)"
	@echo.
	@cmd /C "echo  LOGS"
	@cmd /C "echo    make logs              Stream logs from all running containers (Ctrl+C to stop)"
	@cmd /C "echo    make api-logs          Stream logs from the API container only"
	@cmd /C "echo    make db-logs           Stream logs from the database container only"
	@cmd /C "echo    make clean-logs        Delete generated combat log files from combat-logs/"
	@echo.
	@cmd /C "echo  BUILD ^& TEST"
	@cmd /C "echo    make build-local       Publish the API locally (output to ./publish)"
	@cmd /C "echo    make test              Run all unit and acceptance tests with dotnet test"
	@cmd /C "echo    make test-coverage     Run tests and collect code coverage (opencover format)"
	@echo.
	@cmd /C "echo  OTHER"
	@cmd /C "echo    make sync-instructions Copy AGENTS.md to .github/copilot-instructions.md"

build: publish
	@echo Building Docker containers (ENV=$(ENV))...
	$(COMPOSE) build

build-no-cache: publish
	@echo Building Docker containers without cache (ENV=$(ENV))...
	$(COMPOSE) build --no-cache

up: publish
	@echo Building images and starting containers (ENV=$(ENV))...
	$(COMPOSE) up -d --build

down:
	@echo Stopping the containers (ENV=$(ENV))...
	$(COMPOSE) down

restart: down up
	@echo Restarted the containers.

reset: down publish
	$(COMPOSE) up -d --build
	@echo Containers rebuilt and restarted.

logs:
	$(COMPOSE) logs -f

api-logs:
	$(COMPOSE) logs -f battle-arena-api

db-logs:
	$(COMPOSE) logs -f battle-arena-db

clean: clean-logs
	@echo Stopping containers and removing volumes (ENV=$(ENV))...
	$(COMPOSE) down -v
	powershell -Command "if (Test-Path 'publish') { Remove-Item -Recurse -Force 'publish'; Write-Host 'Removed publish/' } else { Write-Host 'No publish output to remove.' }"
	@echo Clean complete.

clean-logs:
	@echo Deleting combat logs...
	powershell -Command "Get-ChildItem -Path 'combat-logs' -File | Where-Object { $$_.Name -ne '.gitkeep' } | Remove-Item -Force; Write-Host 'combat-logs/ cleared.'"

test:
	dotnet test BattleArena.sln

test-coverage:
	dotnet test BattleArena.sln /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

build-local: publish

publish:
	dotnet publish BattleArena.Api/BattleArena.Api.csproj -c Release -o ./publish

build-demo:
	$(COMPOSE) build battle-arena-demo

demo: up
	@echo Starting BattleArena Demo container (ENV=$(ENV))...
	$(COMPOSE) --profile demo run --rm --build battle-arena-demo

demo-local:
	@echo Starting BattleArena Demo locally (DOTNET_ENVIRONMENT=LocalDev)...
	cmd /C "set DOTNET_ENVIRONMENT=LocalDev && dotnet run --project BattleArena.Demo/BattleArena.Demo.csproj"

sync-instructions:
	@echo Syncing AGENTS.md to .github/copilot-instructions.md...
	pwsh scripts/sync-instructions.ps1
