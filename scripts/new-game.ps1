<#
.SYNOPSIS
  Creates a per-game repository from template/ (PLAYBOOK 2).
.EXAMPLE
  ./scripts/new-game.ps1 -Game "Stardew Valley"
#>
param(
    [Parameter(Mandatory)] [string]$Game,
    [string]$WorkRoot = (Join-Path $HOME 'work')
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$fp = & (Join-Path $PSScriptRoot 'scan-steam.ps1') -Game $Game | Select-Object -First 1

if ($fp.Excluded) { throw "[AnyRPG] $Game excluded: $($fp.Excluded) (PLAYBOOK 0.2)" }

$adapter = switch -Wildcard ($fp.Engine) {
    'unity-*'  { 'unity-bepinex' }
    'unreal'   { 'unreal-ue4ss' }
    'monogame' { 'monogame-smapi' }
    default    { throw "[AnyRPG] no adapter template for engine '$($fp.Engine)' yet (docs/02)" }
}

$slug = ($Game.ToLower() -replace '[^a-z0-9]+', '-').Trim('-')
$dest = Join-Path $WorkRoot "anyrpg-$slug"
if (Test-Path $dest) { throw "[AnyRPG] $dest already exists" }

New-Item -ItemType Directory -Force $dest | Out-Null
Copy-Item (Join-Path $root 'template\core') (Join-Path $dest 'core') -Recurse
# Same layout as template/ so the adapter's ..\..\core project reference still resolves.
New-Item -ItemType Directory -Force (Join-Path $dest 'adapters') | Out-Null
Copy-Item (Join-Path $root "template\adapters\$adapter") (Join-Path $dest "adapters\$adapter") -Recurse
Copy-Item (Join-Path $root '.gitignore') $dest
New-Item -ItemType Directory -Force (Join-Path $dest 'game'), (Join-Path $dest 'docs') | Out-Null

$cfg = Get-Content (Join-Path $root 'template\game\game.json') -Raw | ConvertFrom-Json
$cfg.name = $Game
$cfg.slug = $slug
$cfg.installPath = $fp.Path
$cfg.engine = $fp.Engine
$cfg.loader = $fp.Loader
$cfg.adapter = $adapter
$cfg | ConvertTo-Json -Depth 5 | Out-File -Encoding utf8 (Join-Path $dest 'game\game.json')

@"
# AnyRPG — $Game

Engine: $($fp.Engine) $($fp.Detail) · Loader: $($fp.Loader) · Adapter: $adapter

## Progreso (PLAYBOOK 3)

- [ ] 1 mod carga
- [ ] 2 GetPlayerParty
- [ ] 3 FindNearbyEnemies
- [ ] 4 FreezeWorld / ResumeWorld
- [ ] 5 ApplyHp / Kill
- [ ] 6 IBattleUi
"@ | Out-File -Encoding utf8 (Join-Path $dest 'README.md')

"created $dest ($adapter)"
