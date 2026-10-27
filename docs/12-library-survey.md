# 12 — Relevamiento de la biblioteca local

Escaneo de `C:\Program Files (x86)\Steam\steamapps\common` del 2026-10-27 con `scripts/scan-steam.ps1`.
Los motores son **observed** (archivos presentes en disco). El género y el tipo de combate son
**inferred** salvo que se diga otra cosa.

## Prioridad 1: pilotos (combate en tiempo real + loader maduro)

| Juego | Motor (observed) | Loader | Combate original | Notas |
|---|---|---|---|---|
| Stardew Valley | MonoGame / .NET 6 (`Stardew Valley.dll`, `runtimeconfig` net6.0, `0Harmony.dll`) | SMAPI | melee en tiempo real en las minas | **Piloto.** Adapter en `template/adapters/monogame-smapi`. SMAPI no está instalado todavía (observed) |
| Moonlighter 2 | Unity IL2CPP | BepInEx 6 IL2CPP | ARPG en tiempo real | buen caso de dungeon con salas cerradas = encuentro natural |
| Hogwarts Legacy | Unreal (iostore) | UE4SS | acción mágica en tiempo real | caso grande de Unreal, comunidad de UE4SS activa (**documented**) |

## Prioridad 2: combate en tiempo real, más trabajo

| Juego | Motor | Loader | Notas |
|---|---|---|---|
| FINAL FANTASY VII REBIRTH | Unreal (iostore, 159 GB) | UE4SS | híbrido ATB/acción; el ATB ya existe y se puede volver por turnos puro |
| PRAGMATA | RE Engine (`.pak`, sin `-Shipping.exe`) | REFramework (Lua) | shooter; `docs/06` |
| StarRupture | UE5 (`++Earth20+Neon`) | UE4SS | shooter/supervivencia; posible co-op: solo modo solo |
| Steel Soul Shaper Playtest | Unity IL2CPP | BepInEx 6 | playtest: cambia seguido y se rompe el interop |
| FRD (`BlackJacket.exe`) | Unity Mono | BepInEx 5 | género **unknown** |
| EverRail | Unreal (iostore) | UE4SS | género y combate **unknown** |
| Farever | motor propio (`res/*.pak`, `D3D12/`) | ninguno conocido | **observed**: no es Unreal. Haría falta ingeniería inversa del exe; última prioridad |

## Ya son por turnos (sirven de referencia o para variantes)

| Juego | Motor | Uso en AnyTurnBased |
|---|---|---|
| Clair Obscur: Expedition 33 | UE5 (`SandFall-Win64-Shipping`) | referencia de UX por turnos en Unreal |
| FINAL FANTASY IX | Unity Mono (port de Memoria) | variante: pasar de ATB a CTB puro (tipo FFX) |
| SteamWorld Heist 2 | motor propio | ya es táctico por turnos |
| Slay the Spire 2 | Godot .NET (`0Harmony.dll`, data_sts2) | cartas por turnos; referencia del adapter Godot .NET |
| Balatro | LÖVE 11 (`love.dll`, `lua51.dll`) | por turnos; referencia de motores Lua (Lovely/Steamodded) |
| Rune Dice, Into The Grid | Unity | ya por turnos |
| Skyhill | Unity 5.5.0f3, x86, Mono perfil .NET 3.5 (**observed**) | combate por turnos con apuntado por partes del cuerpo. No es objetivo de conversión. Si algún día se usa para validar Unity viejo, el core necesita un target `net35` |
| Dispatch | Unreal | narrativo/gestión, no aplica |

## Sin combate (fuera de alcance por ahora)

ASTRONEER, The Planet Crafter, Oxygen Not Included, TCG Card Shop Simulator, Unpacking, AdVenture Capitalist,
inKonbini, Restory, Assemble with Care, Sort Them Ducks, Leaf it Alone, Moon Sweepers, RACCOIN,
Tiny Ramen Shop, Supermarket Chaos, Spirit City, Librarian, Infinity Sweeper (Godot), Scrap Collector, Aerolicious.

Necesitarían un "encuentro sintético", es decir inventar enemigos. Es una decisión de diseño de la persona
(PLAYBOOK §7). Aerolicious tiene pinta de shoot'em up (**unknown**).

## Excluidos

| Juego | Motivo |
|---|---|
| War Thunder, Albion Online | online / anti-cheat |
| Aniimo | trae `NEPCleaner.exe` y `KernelDumpAnalyzer.exe`, anti-cheat de NetEase (**observed**) |
| Wardogs | `WardogsClient-Win64-Shipping.exe`: cliente online (**inferred**) |
| SteamVR, Steamworks Shared, Steam Controller Configs | no son juegos |

## No instalados (0 GB)

A Game About Digging A Hole, Duskers, Forever Skies, inZOI, Lightyear Frontier, Motorsport Manager,
No Man's Sky, SlayTheSpire (1), Solarpunk Demo.
