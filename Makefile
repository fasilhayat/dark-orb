.PHONY: help up-local up-dev up-test up-preprod up-prod down clean clean-logs redo-local demo-local test test-coverage build-local sync-instructions publish publish-demo

help:
	@cmd /C "echo  BattleArena -- available make targets"
	@cmd /C "echo  ======================================="
	@echo.
	@cmd /C "echo  QUICK START"
	@cmd /C "echo    make up-local       Start DB + API containers (ports exposed). Demo runs on host:"
	@cmd /C "echo                          make demo-local"
	@cmd /C "echo    make up-dev         Build demo in Release mode, start DB + API + demo in Docker"
	@cmd /C "echo                          (interactive demo container)"
	@cmd /C "echo    make up-test        Build demo in Release mode, start DB + API + demo in Docker"
	@cmd /C "echo                          (no host ports)"
	@cmd /C "echo    make up-preprod     Start DB + API only (no host ports)"
	@cmd /C "echo    make up-prod        Start DB + API only (no host ports)"
	@echo.
	@cmd /C "echo    make demo-local     Run demo on host against up-local (DOTNET_ENVIRONMENT=LocalDev)"
	@cmd /C "echo    make down           Tear down all containers"
	@cmd /C "echo    make clean          Tear down everything, wipe volumes and publish output"
	@echo.
	@cmd /C "echo  BUILD ^& TEST"
	@cmd /C "echo    make build-local    Publish the API locally (output to ./publish)"
	@cmd /C "echo    make test           Run all unit and acceptance tests"
	@cmd /C "echo    make test-coverage  Run tests with code coverage (opencover format)"
	@echo.
	@cmd /C "echo  OTHER"
	@cmd /C "echo    make sync-instructions  Copy AGENTS.md to .github/copilot-instructions.md"
	@cmd /C "echo    make clean-logs     Delete generated combat-logs/ files"

# --- Environment targets ------------------------------------------------

up-local: publish
	@echo Starting local stack (DB + API)...
	docker compose -p dark-orb-localdev -f docker-compose.yml -f docker-compose.localdev.yml up -d --build

up-dev: publish publish-demo
	@echo Building demo in Release mode and starting everything in Docker...
	docker compose -p dark-orb-dev -f docker-compose.yml -f docker-compose.dev.yml up -d --build
	docker compose -p dark-orb-dev -f docker-compose.yml -f docker-compose.dev.yml --profile demo run --rm battle-arena-demo

up-test: publish publish-demo
	@echo Building demo in Release mode and starting test stack...
	docker compose -p dark-orb-test -f docker-compose.yml -f docker-compose.test.yml up -d --build
	docker compose -p dark-orb-test -f docker-compose.yml -f docker-compose.test.yml --profile demo run --rm battle-arena-demo

up-preprod: publish
	@echo Starting preprod stack (DB + API)...
	docker compose -p dark-orb-preprod -f docker-compose.yml -f docker-compose.preprod.yml up -d --build

up-prod: publish
	@echo Starting production stack (DB + API)...
	docker compose -p dark-orb-prod -f docker-compose.yml -f docker-compose.prod.yml up -d --build

down:
	pwsh -NoProfile -Command "'localdev','dev','test','preprod','prod' | ForEach-Object { docker compose -p dark-orb-$$_ down 2>$$null }"

clean: clean-logs
	pwsh -NoProfile -Command "'localdev','dev','test','preprod','prod' | ForEach-Object { docker compose -p dark-orb-$$_ down -v 2>$$null }"
	pwsh -NoProfile -Command "if (Test-Path 'publish') { Remove-Item -Recurse -Force 'publish' }; if (Test-Path 'publish-demo') { Remove-Item -Recurse -Force 'publish-demo' }"

clean-logs:
	@echo Deleting combat logs...
	@powershell -Command "Get-ChildItem -Path 'combat-logs' -File | Where-Object { $$_.Name -ne '.gitkeep' } | Remove-Item -Force; Write-Host 'combat-logs/ cleared.'"

redo-local:
	@echo Clean + build + up-local + demo...
	dotnet clean BattleArena.sln
	dotnet build BattleArena.sln
	@$(MAKE) up-local
	dotnet run --project BattleArena.Demo/BattleArena.Demo.csproj

demo-local:
	@echo Starting BattleArena Demo locally (DOTNET_ENVIRONMENT=LocalDev)...
	cmd /C "set DOTNET_ENVIRONMENT=LocalDev && dotnet run --project BattleArena.Demo/BattleArena.Demo.csproj"

test:
	dotnet test BattleArena.sln

test-coverage:
	dotnet test BattleArena.sln /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

build-local: publish

publish:
	pwsh -NoProfile -Command "if (Test-Path 'publish') { Remove-Item -Recurse -Force 'publish' }"
	dotnet publish BattleArena.Api/BattleArena.Api.csproj -c Release -o ./publish

publish-demo:
	pwsh -NoProfile -Command "if (Test-Path 'publish-demo') { Remove-Item -Recurse -Force 'publish-demo' }"
	dotnet publish BattleArena.Demo/BattleArena.Demo.csproj -c Release -o ./publish-demo

sync-instructions:
	@echo Syncing AGENTS.md to .github/copilot-instructions.md...
	pwsh scripts/sync-instructions.ps1
