---
name: work-intake
description: Load this skill at the start of any session to find pending bug fixes and feature requests. Looks in the bugs-features/ folder at the repository root.
---

# Work Intake

Pending tasks are listed in `bugs-features/` at the repository root.
Each numbered file (`01-*.md`, `02-*.md`, ...) describes a discrete set of bugs or features to implement.

## Workflow

1. Read the next numbered file in `bugs-features/`.
2. Investigate the codebase to understand the affected systems.
3. Implement fixes/features as described.
4. Run `dotnet test BattleArena.sln` from `src/` to verify nothing is broken.
5. Move the completed file to `bugs-features/done/`.
6. Proceed to the next numbered file (if any).

## Rule

Never skip acceptance criteria. Every scenario in the task file must be satisfied before the file moves to `done/`.
