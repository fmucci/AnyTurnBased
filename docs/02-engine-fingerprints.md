# 02 — Fingerprints de motor

Todos **observed** en la biblioteca local salvo que se diga otra cosa. `scripts/scan-steam.ps1` aplica esta tabla.

| Señal en disco | Motor | Loader |
|---|---|---|
| `UnityPlayer.dll` + `*_Data/Managed/Assembly-CSharp.dll` | Unity Mono | BepInEx 5 |
| `UnityPlayer.dll` + `GameAssembly.dll` | Unity IL2CPP | BepInEx 6 IL2CPP |
| `Assembly-CSharp.dll` sin `UnityPlayer.dll` | Unity viejo (< 2017) Mono | BepInEx 5 |
| `*-Win64-Shipping.exe` + `Content/Paks/*.pak` | Unreal 4/5 | UE4SS |
| …y además `*.utoc`/`*.ucas` | Unreal con IoStore (UE4.26+ / UE5) | UE4SS (los mods de assets necesitan retoc) |
| `FileVersion` del Shipping exe `++UE5+Release-5.x` | versión exacta de UE | — |
| `*.pck` o `data_*_windows_x86_64/` + `libGodot*.dll` | Godot | Harmony si es .NET |
| `<Game>.dll` + `*.runtimeconfig.json` (net6.0) + MonoGame | MonoGame / .NET | SMAPI (Stardew) o Harmony |
| `love.dll` + `lua51.dll` | LÖVE | Lovely injector |
| `Engine/` + `<Proyecto>/Content/Paks/` sin `-Shipping.exe` (exe renombrado, ej. FF7 Rebirth, Hogwarts) | Unreal | UE4SS |
| `re_chunk_000.pak` | RE Engine (PRAGMATA) | REFramework (**documented**) |
| otros `.pak` sueltos (Farever `res/*.pak`, SteamWorld Heist 2 `Game.pak`) | motor propio | ninguno |
| `data.win` | GameMaker | UndertaleModTool (**documented**) |

## Señales de exclusión

`EasyAntiCheat/`, `BattlEye/`, `NEPCleaner.exe`, `*Client-Win64-Shipping.exe`, launchers con login. Ante
cualquiera de estas, no se modea (PLAYBOOK §0.2).
