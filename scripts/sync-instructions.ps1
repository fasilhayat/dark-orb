# sync-instructions.ps1
# Copies AGENTS.md → .github/copilot-instructions.md, replacing the
# AGENTS-specific preamble block with the GitHub Copilot-specific one.
#
# Run from the repository root:
#   pwsh scripts/sync-instructions.ps1
#   make sync-instructions

$repoRoot = Split-Path $PSScriptRoot -Parent
$source   = Join-Path $repoRoot "AGENTS.md"
$target   = Join-Path $repoRoot ".github" "copilot-instructions.md"

# Walk line-by-line to find where the shared content starts:
# skip the title (#), the blank line, the blockquote (>) preamble, and the following blank line.
$lines = Get-Content $source
$i = 0
while ($i -lt $lines.Count -and $lines[$i] -match '^#\s') { $i++ }   # skip title
while ($i -lt $lines.Count -and $lines[$i] -eq '')         { $i++ }   # skip blank
while ($i -lt $lines.Count -and $lines[$i] -match '^>')    { $i++ }   # skip blockquote
while ($i -lt $lines.Count -and $lines[$i] -eq '')         { $i++ }   # skip blank

$header = @(
    "# BattleArena — AI Assistant Instructions (GitHub Copilot)",
    "",
    "> **Mirrored file.** The canonical source is ``AGENTS.md`` at the repository root (read by OpenCode).",
    "> This copy exists solely because GitHub Copilot reads ``.github/copilot-instructions.md``.",
    "> Edit ``AGENTS.md``, then run ``make sync-instructions`` to update this file.",
    ""
)

($header + $lines[$i..($lines.Count - 1)]) | Set-Content $target -Encoding UTF8

Write-Host "Synced: AGENTS.md -> .github/copilot-instructions.md"
