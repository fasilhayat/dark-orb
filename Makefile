.PHONY: help build build-no-cache up down restart reset logs api-logs db-logs clean clean-logs test test-coverage build-local demo-local demo-dev demo-test build-demo sync-instructions up-local up-dev up-test up-preprod up-prod down-local down-dev down-test down-preprod down-prod

-include .env
export ENV
export COMPOSE_PROJECT_NAME

ENV                  ?= prod
COMPOSE_PROJECT_NAME  = dark-orb-$(ENV)
COMPOSE               = docker compose -f docker-compose.yml -f docker-compose.$(ENV).yml

help:
	@cmd /C "echo  BattleArena -- available make targets"
	@cmd /C "echo  ======================================="
	@echo.
	@cmd /C "echo  QUICK START"
	@cmd /C "echo    make up-local          Start DB+API only (ports exposed). Run demo with: make demo-local"
	@cmd /C "echo    make up-dev            Start DB+API + demo (dev, ports exposed)"
	@cmd /C "echo    make up-test           Start DB+API + demo (test, no host ports)"
	@cmd /C "echo    make up-preprod        Start DB+API only, no host ports"
	@cmd /C "echo    make up-prod           Start DB+API only, no host ports"
	@echo.
	@cmd /C "echo    make demo-local        Run demo on host (DOTNET_ENVIRONMENT=LocalDev -> localhost:8585)"
	@cmd /C "echo    make demo-dev          Relaunch demo container only (dev, DB+API must already be running)"
	@cmd /C "echo    make demo-test         Relaunch demo container only (test, DB+API must already be running)"
	@echo.
	@cmd /C "echo    make down-local        Stop local environment"
	@cmd /C "echo    make down-dev          Stop dev environment"
	@cmd /C "echo    make down-test         Stop test environment"
	@cmd /C "echo    make down-preprod      Stop preprod environment"
	@cmd /C "echo    make down-prod         Stop prod environment"
	@echo.
	@cmd /C "echo  GENERIC TARGETS  (read ENV from .env or pass ENV=localdev^|dev^|test^|preprod^|prod)"
	@cmd /C "echo    make up                Build images and start DB+API  (e.g. make up ENV=dev)"
	@cmd /C "echo    make down              Stop containers               (e.g. make down ENV=dev)"
	@cmd /C "echo    make restart           down then up"
	@cmd /C "echo    make reset             down, republish, rebuild, start fresh"
	@cmd /C "echo    make clean             Stop ALL environments and wipe all volumes + publish output"
	@cmd /C "echo    make build             Build Docker images for current ENV"
	@cmd /C "echo    make build-no-cache    Build Docker images without layer cache"
	@cmd /C "echo    make build-demo        Build only the demo Docker image"
	@cmd /C "echo    make logs              Stream logs from all containers"
	@cmd /C "echo    make api-logs          Stream API container logs"
	@cmd /C "echo    make db-logs           Stream database container logs"
	@cmd /C "echo    make clean-logs        Delete generated combat-logs/ files"
	@echo.
	@cmd /C "echo  BUILD ^& TEST"
	@cmd /C "echo    make build-local       Publish the API locally (output to ./publish)"
	@cmd /C "echo    make test              Run all unit and acceptance tests"
	@cmd /C "echo    make test-coverage     Run tests with code coverage (opencover format)"
	@echo.
	@cmd /C "echo  OTHER"
	@cmd /C "echo    make sync-instructions Copy AGENTS.md to .github/copilot-instructions.md"

# --- Named environment targets -------------------------------------------

up-local: publish
	@echo Starting local stack (DB + API)...
	docker compose -f docker-compose.yml -f docker-compose.localdev.yml up -d --build

up-dev: publish publish-demo
	@echo Starting dev stack (DB + API) then launching demo...
	docker compose -f docker-compose.yml -f docker-compose.dev.yml up -d --build
	docker compose -f docker-compose.yml -f docker-compose.dev.yml --profile demo run --rm --build battle-arena-demo

demo-dev: publish-demo
	@echo Launching demo (dev)...
	docker compose -f docker-compose.yml -f docker-compose.dev.yml --profile demo run --rm --build battle-arena-demo

up-test: publish publish-demo
	@echo Starting test stack (DB + API) then launching demo...
	docker compose -f docker-compose.yml -f docker-compose.test.yml up -d --build
	docker compose -f docker-compose.yml -f docker-compose.test.yml --profile demo run --rm --build battle-arena-demo

demo-test: publish-demo
	@echo Launching demo (test)...
	docker compose -f docker-compose.yml -f docker-compose.test.yml --profile demo run --rm --build battle-arena-demo

up-preprod: publish
	@echo Starting preprod stack (DB + API)...
	docker compose -f docker-compose.yml -f docker-compose.preprod.yml up -d --build

up-prod: publish
	@echo Starting production stack (DB + API)...
	docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build

down-local:
	docker compose -f docker-compose.yml -f docker-compose.localdev.yml down

down-dev:
	docker compose -f docker-compose.yml -f docker-compose.dev.yml down

down-test:
	docker compose -f docker-compose.yml -f docker-compose.test.yml down

down-preprod:
	docker compose -f docker-compose.yml -f docker-compose.preprod.yml down

down-prod:
	docker compose -f docker-compose.yml -f docker-compose.prod.yml down

# --- Generic targets (read ENV from .env or ENV= override) ---------------

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
	@echo Stopping all environments and removing all volumes...
	-docker compose -f docker-compose.yml -f docker-compose.localdev.yml -p dark-orb-localdev down -v
	-docker compose -f docker-compose.yml -f docker-compose.dev.yml -p dark-orb-dev down -v
	-docker compose -f docker-compose.yml -f docker-compose.test.yml -p dark-orb-test down -v
	-docker compose -f docker-compose.yml -f docker-compose.preprod.yml -p dark-orb-preprod down -v
	-docker compose -f docker-compose.yml -f docker-compose.prod.yml -p dark-orb-prod down -v
	powershell -Command "if (Test-Path 'publish') { Remove-Item -Recurse -Force 'publish'; Write-Host 'Removed publish/' } else { Write-Host 'No publish output to remove.' }"
	powershell -Command "if (Test-Path 'publish-demo') { Remove-Item -Recurse -Force 'publish-demo'; Write-Host 'Removed publish-demo/' } else { Write-Host 'No publish-demo output to remove.' }"
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

publish-demo:
	dotnet publish BattleArena.Demo/BattleArena.Demo.csproj -c Release -o ./publish-demo

build-demo:
	$(COMPOSE) build battle-arena-demo

demo-local:
	@echo Starting BattleArena Demo locally (DOTNET_ENVIRONMENT=LocalDev)...
	cmd /C "set DOTNET_ENVIRONMENT=LocalDev && dotnet run --project BattleArena.Demo/BattleArena.Demo.csproj"

sync-instructions:
	@echo Syncing AGENTS.md to .github/copilot-instructions.md...
	pwsh scripts/sync-instructions.ps1
