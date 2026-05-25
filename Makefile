.PHONY: help build build-no-cache up down restart reset logs api-logs db-logs clean test test-coverage build-local

help:
	@cmd /C "echo Usage:"
	@cmd /C "echo 	make build          	- Build the Docker containers"
	@cmd /C "echo 	make build-no-cache 	- Build the Docker containers without cache"
	@cmd /C "echo 	make up             	- Start the containers"
	@cmd /C "echo 	make down           	- Stop the containers (preserves database files)"
	@cmd /C "echo 	make restart        	- Restart the containers"
	@cmd /C "echo 	make reset          	- Rebuild and restart containers (preserves database files)"
	@cmd /C "echo 	make logs           	- Show logs from all containers"
	@cmd /C "echo 	make api-logs       	- Show logs from the API container"
	@cmd /C "echo 	make db-logs        	- Show logs from the Database container"
	@cmd /C "echo 	make clean          	- Remove all containers and delete database files"
	@cmd /C "echo 	make test           	- Run unit tests"
	@cmd /C "echo 	make test-coverage  	- Run unit tests with coverage"
	@cmd /C "echo 	make build-local    	- Build the .NET solution locally"

build:
	@echo Building Docker containers...
	docker compose build

build-no-cache:
	@echo Building Docker containers without cache...
	docker compose build --no-cache

up:
	@echo Starting the containers...
	docker compose up -d

down:
	@echo Stopping the containers...
	docker compose down

restart: down up
	@echo Restarted the containers.

reset: down
	docker compose up -d --build
	@echo Containers rebuilt and restarted.

logs:
	docker compose logs -f

api-logs:
	docker compose logs -f battle-arena-api

db-logs:
	docker compose logs -f battle-arena-db

clean:
	@echo Stopping containers and removing database files...
	docker compose down -v
	@echo Removing database files...
	powershell -Command "if (Test-Path '.containers/postgres') { Remove-Item -Recurse -Force '.containers/postgres'; Write-Host 'Removed .containers/postgres' } else { Write-Host 'No database files to remove.' }"
	@echo Clean complete.

test:
	dotnet test BattleArena.sln

test-coverage:
	dotnet test BattleArena.sln /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

build-local:
	dotnet build BattleArena.sln
