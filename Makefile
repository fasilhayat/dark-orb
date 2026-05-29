.PHONY: help build build-no-cache up down restart reset logs api-logs db-logs clean clean-logs test test-coverage build-local demo build-demo sync-instructions

help:
	@cmd /C "echo Usage:"
	@cmd /C "echo 	make build          	- Build the Docker containers"
	@cmd /C "echo 	make build-no-cache 	- Build the Docker containers without cache"
	@cmd /C "echo 	make up             	- Build images and start the containers"
	@cmd /C "echo 	make down           	- Stop the containers (preserves database files)"
	@cmd /C "echo 	make restart        	- Restart the containers"
	@cmd /C "echo 	make reset          	- Rebuild and restart containers (preserves database files)"
	@cmd /C "echo 	make logs           	- Show logs from all containers"
	@cmd /C "echo 	make api-logs       	- Show logs from the API container"
	@cmd /C "echo 	make db-logs        	- Show logs from the Database container"
	@cmd /C "echo 	make clean          	- Remove all containers and delete database files"
	@cmd /C "echo 	make clean-logs     	- Delete all files in the combat-logs/ folder"
	@cmd /C "echo 	make test           	- Run unit tests"
	@cmd /C "echo 	make test-coverage  	- Run unit tests with coverage"
	@cmd /C "echo 	make build-local    	- Build the .NET solution locally"
	@cmd /C "echo 	make build-demo     	- Build the demo Docker image"
	@cmd /C "echo 	make demo           	- Build images and run the console demo"
	@cmd /C "echo 	make sync-instructions	- Sync AGENTS.md to .github/copilot-instructions.md"

build: publish
	@echo Building Docker containers...
	docker compose build

build-no-cache: publish
	@echo Building Docker containers without cache...
	docker compose build --no-cache

up: publish
	@echo Building images and starting containers...
	docker compose up -d --build

down:
	@echo Stopping the containers...
	docker compose down

restart: down up
	@echo Restarted the containers.

reset: down publish
	docker compose up -d --build
	@echo Containers rebuilt and restarted.

logs:
	docker compose logs -f

api-logs:
	docker compose logs -f battle-arena-api

db-logs:
	docker compose logs -f battle-arena-db

clean: clean-logs
	@echo Stopping containers and removing database files...
	docker compose down -v
	@echo Removing database files...
	powershell -Command "if (Test-Path '.containers/postgres') { Remove-Item -Recurse -Force '.containers/postgres'; Write-Host 'Removed .containers/postgres' } else { Write-Host 'No database files to remove.' }"
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
	docker compose build battle-arena-demo

demo: publish build-demo
	docker compose --profile demo up -d --build
	docker compose --profile demo run --rm battle-arena-demo

sync-instructions:
	@echo Syncing AGENTS.md to .github/copilot-instructions.md...
	pwsh scripts/sync-instructions.ps1
