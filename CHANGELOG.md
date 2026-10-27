# Changelog

## 0.2.0 — 2026-10-27

- Renombrado de AnyRPG a **AnyTurnBased**: el proyecto pasa el combate a turnos, no convierte juegos en RPGs.
  Namespaces `AnyTurnBased.*`, comando de debug `anytb_battle`, mod de SMAPI `fmucci.AnyTurnBased`,
  repos por juego `anyturnbased-<slug>`.
- `docs/12`: Skyhill pasa a "ya son por turnos" (Unity 5.5.0f3, x86, Mono .NET 3.5).

## 0.1.0 — 2026-10-27

- Esqueleto inicial con el formato de AnyPS1: `docs/` 00–12, `template/`, `PLAYBOOK.md`, `scripts/`.
- Core: `BattleEngine` CTB, acciones base, `SimpleEnemyAi`, `EncounterDirector`, `StatMapper`, tests xunit.
- Adapters: Stardew/SMAPI (piloto), Unity/BepInEx (plantilla + IMGUI), Unreal/UE4SS (port Lua del core).
- `docs/12`: relevamiento de 55 carpetas de la biblioteca local.
- Core compilado con .NET SDK 8.0.427: 0 warnings, 5/5 tests en verde.
- Adapter Stardew: compila contra 1.6.15 con SMAPI 4.5.2 (faltaba `EnableHarmony`). Carga en el juego
  (PLAYBOOK §3 paso 1). Pasos 2–6 sin verificar: hace falta cargar una partida y bajar a las minas.
- `scan-steam.ps1`: detecta Unreal con exe renombrado (`Engine/` + `Content/Paks`). RE Engine solo con
  `re_chunk_000.pak`. Otros `.pak` se marcan como motor propio.
- `new-game.ps1`: copia el adapter a `adapters/<nombre>` para que la referencia al core resuelva.
