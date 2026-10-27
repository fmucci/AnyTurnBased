<#
.SYNOPSIS
  Fingerprints the engine and mod loader of installed Steam games (docs/02).
.EXAMPLE
  ./scripts/scan-steam.ps1
  ./scripts/scan-steam.ps1 -Game "Stardew Valley"
  ./scripts/scan-steam.ps1 -Out docs/scan.md
#>
param(
    [string]$Game,
    [string]$Out
)

function Get-SteamLibraries {
    $steam = $null
    try { $steam = (Get-ItemProperty 'HKCU:\Software\Valve\Steam' -ErrorAction Stop).SteamPath } catch {}
    if (-not $steam) { $steam = 'C:\Program Files (x86)\Steam' }
    $vdf = Join-Path $steam 'steamapps\libraryfolders.vdf'
    if (-not (Test-Path $vdf)) { throw "[AnyTurnBased] libraryfolders.vdf not found under $steam" }
    Select-String -Path $vdf -Pattern '"path"\s+"([^"]+)"' | ForEach-Object {
        Join-Path ($_.Matches[0].Groups[1].Value -replace '\\\\', '\') 'steamapps\common'
    } | Where-Object { Test-Path $_ } | Select-Object -Unique
}

function Find-First($dir, $filter, $depth) {
    Get-ChildItem $dir -Recurse -Depth $depth -Filter $filter -ErrorAction SilentlyContinue | Select-Object -First 1
}

function Get-Fingerprint($dir) {
    $r = [ordered]@{ Engine = 'unknown'; Loader = '-'; Detail = ''; Excluded = '' }

    if (Find-First $dir 'NEPCleaner.exe' 3)      { $r.Excluded = 'anti-cheat (NEP)' }
    if (Find-First $dir 'EasyAntiCheat*' 3)      { $r.Excluded = 'anti-cheat (EAC)' }
    if (Find-First $dir 'BattlEye*' 3)           { $r.Excluded = 'anti-cheat (BattlEye)' }
    if (Find-First $dir '*Client-Win64-Shipping.exe' 4) { $r.Excluded = 'online client' }

    $shipping = Find-First $dir '*-Win64-Shipping.exe' 4
    # Some Unreal games rename the shipping exe (ff7rebirth_.exe); Engine/ + <Project>/Content/Paks still gives them away.
    $unrealLayout = (Test-Path (Join-Path $dir 'Engine')) -and
        (Get-ChildItem $dir -Directory | Where-Object { Test-Path (Join-Path $_.FullName 'Content\Paks') } | Select-Object -First 1)
    if (Find-First $dir 'GameAssembly.dll' 3) {
        $r.Engine = 'unity-il2cpp'; $r.Loader = 'bepinex6-il2cpp'
    } elseif (Find-First $dir 'Assembly-CSharp.dll' 4) {
        $r.Engine = 'unity-mono'; $r.Loader = 'bepinex5'
        if (-not (Find-First $dir 'UnityPlayer.dll' 2)) { $r.Detail = 'old Unity (<2017)' }
    } elseif ($shipping -or $unrealLayout) {
        $r.Engine = 'unreal'; $r.Loader = 'ue4ss'
        $v = if ($shipping) { $shipping.VersionInfo.FileVersion }
        if ($v -and $v -ne 'Unknown-CL-0') { $r.Detail = $v }
        if (Find-First $dir '*.utoc' 5) { $r.Detail = ($r.Detail + ' iostore').Trim() }
    } elseif ((Find-First $dir 'love.dll' 1) -and (Find-First $dir 'lua51.dll' 1)) {
        $r.Engine = 'love'; $r.Loader = 'lovely'
    } elseif (Find-First $dir 'libGodot*.dll' 2) {
        $r.Engine = 'godot'
        if (Find-First $dir '0Harmony.dll' 3) { $r.Engine = 'godot-dotnet'; $r.Loader = 'harmony' }
    } elseif (Find-First $dir '*.pck' 2) {
        $r.Engine = 'godot'
    } elseif (Find-First $dir '*.runtimeconfig.json' 1) {
        $r.Engine = 'dotnet'; $r.Loader = 'harmony'
        if (Test-Path (Join-Path $dir 'Stardew Valley.dll')) { $r.Engine = 'monogame'; $r.Loader = 'smapi' }
    } elseif (Find-First $dir 'data.win' 2) {
        $r.Engine = 'gamemaker'; $r.Loader = 'undertalemodtool'
    } elseif (Test-Path (Join-Path $dir 're_chunk_000.pak')) {
        $r.Engine = 're-engine'; $r.Loader = 'reframework'
    } elseif (Find-First $dir '*.pak' 2) {
        $r.Engine = 'custom'; $r.Detail = 'pak archives, proprietary engine'
    }
    [pscustomobject]$r
}

$rows = foreach ($lib in Get-SteamLibraries) {
    Get-ChildItem $lib -Directory | Where-Object { -not $Game -or $_.Name -eq $Game } | ForEach-Object {
        $files = Get-ChildItem $_.FullName -Recurse -File -ErrorAction SilentlyContinue
        $gb = [math]::Round((($files | Measure-Object Length -Sum).Sum) / 1GB, 1)
        $fp = if ($gb -gt 0) { Get-Fingerprint $_.FullName } else { [pscustomobject]@{ Engine = 'not installed'; Loader = '-'; Detail = ''; Excluded = '' } }
        [pscustomobject]@{ Game = $_.Name; GB = $gb; Engine = $fp.Engine; Loader = $fp.Loader; Detail = $fp.Detail; Excluded = $fp.Excluded; Path = $_.FullName }
    }
}

if ($Game -and -not $rows) { throw "[AnyTurnBased] game folder not found: $Game" }

if ($Out) {
    $md = @('| Game | GB | Engine | Loader | Detail | Excluded |', '|---|---|---|---|---|---|')
    $md += $rows | ForEach-Object { "| $($_.Game) | $($_.GB) | $($_.Engine) | $($_.Loader) | $($_.Detail) | $($_.Excluded) |" }
    $md | Out-File -Encoding utf8 $Out
    "wrote $Out ($($rows.Count) games)"
} else {
    $rows
}
