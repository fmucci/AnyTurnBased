# Changelog

## 0.4.0 — 2026-10-27

- El core lee `game/battle.json` (`BattleConfig`, sin dependencias). Modelos de tiempo sin implementar fallan
  fuerte; mecánicas sin implementar se loguean.
- Estilo Expedition 33: `economy.ap` (puntos de acción, `IBattleAction.ApCost`) y `reactive.dodge-parry`
  (`ReactionTimer`, `IReactionUi`, contraataque al parar). Las dos pasan a `supported` en el catálogo.
- Tests: 22/22 (antes 5).
- Moonlighter 2: `battle.json` estilo Expedition 33, Q esquiva y E para, barra de timing en IMGUI. Carga en el
  juego (**observed**); el combate sigue sin jugarse.

## 0.3.0 — 2026-10-27

- `mechanics/`: base de mecánicas por turnos relevada en 9 juegos de la biblioteca (FF9, Expedition 33,
  FF7 Rebirth, Slay the Spire 2, Into The Grid, SteamWorld Heist II, Skyhill, Balatro, Rune Dice).
  48 mecánicas en 11 categorías, 6 presets y el esquema de `game/battle.json` (`docs/13`).
- Skill `anytb-design`: entrevista a la persona para elegir modelo de tiempo y mecánicas, y escribe `battle.json`.
- `scripts/check-mechanics.ps1`: valida ids, categorías de elección única y parámetros; con `-BattleConfig`
  lista lo que falta en el core.
- `new-game.ps1` copia `mechanics/`, el script y la skill al repo del juego.

## 0.2.0 — 2026-10-27

- Renombrado de AnyRPG a **AnyTurnBased**: el proyecto pasa el combate a turnos, no convierte juegos en RPGs.
  Namespaces `AnyTurnBased.*`, comando de debug `anytb_battle`, mod de SMAPI `fmucci.AnyTurnBased`,
  repos por juego `anyturnbased-<slug>`.
- `docs/12`: Skyhill pasa a "ya son por turnos" (Unity 5.5.0f3, x86, Mono .NET 3.5).
- Moonlighter 2: primer juego Unity IL2CPP. BepInEx 6 be.791 funciona en Unity 6000.3 (metadata v39) y el
  plugin carga. Lo aprendido de IL2CPP queda en `docs/03`.
- `new-game.ps1 -Slug`: nombre corto para el repo del juego, para no pasar de 260 caracteres en las rutas de build.

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
