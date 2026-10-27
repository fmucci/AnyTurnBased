<#
.SYNOPSIS
  Checks that mechanics/*.json reference only ids that exist in catalog.json,
  and optionally validates a game's battle.json the same way (docs/13).
.EXAMPLE
  ./scripts/check-mechanics.ps1
  ./scripts/check-mechanics.ps1 -BattleConfig ..\anyturnbased-moonlighter2\game\battle.json
#>
param([string]$BattleConfig)

$ErrorActionPreference = 'Stop'
$dir = Join-Path (Split-Path $PSScriptRoot -Parent) 'mechanics'
function Load($name) { Get-Content (Join-Path $dir $name) -Raw -Encoding UTF8 | ConvertFrom-Json }

$catalog = Load 'catalog.json'
$games = Load 'games.json'
$presets = Load 'presets.json'
$errors = New-Object System.Collections.Generic.List[string]

$byId = @{}
foreach ($m in $catalog.mechanics) {
    if ($byId.ContainsKey($m.id)) { $errors.Add("catalog: duplicated id $($m.id)") }
    $byId[$m.id] = $m
    if (-not $m.id.StartsWith("$($m.category)."))  { $errors.Add("catalog: $($m.id) does not start with its category '$($m.category)'") }
    if (-not ($catalog.coreStatus.PSObject.Properties.Name -contains $m.core)) { $errors.Add("catalog: $($m.id) has unknown core status '$($m.core)'") }
}
$pickOne = @($catalog.categories | Where-Object { $_.pick -eq 'one' } | ForEach-Object { $_.id })

function Check-Selection($where, $time, $ids, $params) {
    if (-not $byId.ContainsKey($time) -or $byId[$time].category -ne 'time') { $errors.Add("${where}: time '$time' is not a time.* mechanic") }
    foreach ($id in $ids) { if (-not $byId.ContainsKey($id)) { $errors.Add("${where}: unknown mechanic '$id'") } }
    foreach ($cat in $pickOne) {
        $n = @($ids | Where-Object { $_ -like "$cat.*" }).Count
        if ($cat -ne 'time' -and $n -gt 1) { $errors.Add("${where}: category '$cat' allows one mechanic, got $n") }
    }
    if ($params) {
        foreach ($p in $params.PSObject.Properties) {
            if ($p.Name -ne $time -and -not ($ids -contains $p.Name)) { $errors.Add("${where}: params for '$($p.Name)', which is not selected") }
            elseif ($byId.ContainsKey($p.Name)) {
                $known = $byId[$p.Name].params.PSObject.Properties.Name
                foreach ($k in $p.Value.PSObject.Properties.Name) { if (-not ($known -contains $k)) { $errors.Add("${where}: '$($p.Name)' has no param '$k'") } }
            }
        }
    }
}

foreach ($g in $games.games) {
    foreach ($id in $g.mechanics.PSObject.Properties.Name) { if (-not $byId.ContainsKey($id)) { $errors.Add("games/$($g.id): unknown mechanic '$id'") } }
}
$gameIds = @($games.games | ForEach-Object { $_.id })
foreach ($p in $presets.presets) {
    Check-Selection "presets/$($p.id)" $p.time $p.mechanics $p.params
    foreach ($b in $p.basedOn) { if (-not ($gameIds -contains $b)) { $errors.Add("presets/$($p.id): unknown game '$b'") } }
}

if ($BattleConfig) {
    $cfg = Get-Content $BattleConfig -Raw -Encoding UTF8 | ConvertFrom-Json
    if ($cfg.version -ne 1) { $errors.Add("battle.json: version must be 1") }
    Check-Selection 'battle.json' $cfg.time $cfg.mechanics $cfg.params
    $pending = @(@($cfg.time) + @($cfg.mechanics) | Where-Object { $byId.ContainsKey($_) -and $byId[$_].core -ne 'supported' })
    if ($pending.Count) { "not in the core yet: " + (($pending | ForEach-Object { "$_ ($($byId[$_].core))" }) -join ', ') }
}

if ($errors.Count) { $errors | ForEach-Object { "ERROR $_" }; exit 1 }
"ok: $($catalog.mechanics.Count) mechanics, $($games.games.Count) games, $($presets.presets.Count) presets"
