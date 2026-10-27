# 13 — Base de mecánicas por turnos

Relevamiento del 2026-10-27 de los juegos por turnos de la biblioteca local, convertido en un catálogo que
se usa para elegir cómo va a ser el combate de cada juego convertido.

## Archivos

| Archivo | Qué tiene |
|---|---|
| `mechanics/catalog.json` | 48 mecánicas en 11 categorías, con parámetros, estado en el core y cómo se adaptan a un juego en tiempo real |
| `mechanics/games.json` | 9 juegos relevados: qué mecánicas usa cada uno y con qué evidencia |
| `mechanics/presets.json` | combinaciones listas para usar: CTB, FF9, Expedition 33, cartas, Skyhill, rondas |
| `mechanics/battle-config.schema.json` | esquema de `game/battle.json`, lo que se elige para un juego |
| `scripts/check-mechanics.ps1` | valida que todo use ids del catálogo; con `-BattleConfig` valida un juego y dice qué falta en el core |
| `.claude/skills/anytb-design` | la entrevista: pregunta a la persona qué quiere y escribe `game/battle.json` |

`new-game.ps1` copia todo esto al repo de cada juego.

## Juegos relevados

| Juego | Estilo | Evidencia principal |
|---|---|---|
| Final Fantasy IX | ATB, grupo de 4, Trance, filas | **observed**: `battle_start_type_tags`, `continuous_status_tags`, `command_status` en Assembly-CSharp |
| Clair Obscur: Expedition 33 | CTB con línea de turnos, PA, esquivar/parar, golpes con timing, quiebre | **documented** (UE5; no se abrieron los `.pak`) |
| Final Fantasy VII Rebirth | acción con ATB para comandos | **documented** |
| Slay the Spire 2 | cartas por fases, energía, intenciones, escudo | **observed**: `MonsterMoves.Intents.*`, `NEnergyCounter`, `BlockProps`, `Models.Powers.*`, `Models.Orbs.*` en `sts2.dll` |
| Into The Grid | cartas por fases, recurso Vim, intenciones | **observed**: `VimManager`, `EnemyIntentTemplate`, `CardSlot`, `StatusFactory` |
| SteamWorld Heist II | táctico en grilla, mover y disparar, apuntado manual, cobertura | **documented** (motor propio) |
| Skyhill | duelo 1 a 1 con zonas del cuerpo | **observed**: `FightingManager`, `AttackZones`, `WeaponsControl` |
| Balatro | manos limitadas contra un puntaje | **documented** |
| Rune Dice | **unknown** (IL2CPP sin interop; "dados" se deduce del nombre) | — |

Fuera de esta lista: Dispatch (narrativo) y el resto de la biblioteca, que no es por turnos (`docs/12`).

## Categorías

| Categoría | Se elige | Ejemplos |
|---|---|---|
| Modelo de tiempo | una | rondas, fases por bando, CTB, ATB (espera/activo), acción + ATB, turnos limitados |
| Economía del turno | varias | una acción, PA acumulables, energía por turno, mover y actuar, maná, medidor de límite |
| Acciones | varias | menú, objetos, cartas, dados, combinaciones, mecánica por personaje |
| Habilidad en tiempo real | varias | esquivar/parar, golpes con timing, apuntado libre, zonas del cuerpo |
| Información del enemigo | una | sin aviso, orden de turnos visible, intenciones visibles |
| Daño y defensa | varias | escudo temporal, debilidades, quiebre, probabilidad de acierto, cobertura |
| Estados | varias | con duración, acumulables, modificadores pasivos |
| Grupo | una | un personaje, grupo |
| Posición | varias | filas, grilla, invocaciones |
| Inicio del combate | varias | por cercanía, con una tecla, golpe inicial |
| Victoria | una | derrotar a todos, puntaje, sobrevivir |

## Estado en el core

Cada mecánica tiene un campo `core`:

- **supported (12)**: CTB, una acción por turno, maná, menú, defender, huir, sin aviso, orden visible,
  un personaje, por cercanía, con tecla, derrotar a todos.
- **partial (2)**: objetos (la poción es una habilidad, no usa el inventario real) y grupo (el motor acepta
  varios, los adapters devuelven solo al jugador).
- **planned (24)**: no dependen del juego y se pueden programar en el core. Por ejemplo ATB, rondas, fases, PA,
  energía, cartas, intenciones, esquivar/parar con una señal propia en la UI, quiebre y estados.
- **research (8)**: dependen de cada juego, como apuntado libre, cobertura, grilla, mover y actuar, o acción + ATB.
  Necesitan la cámara, la geometría o la navegación del juego estando congelado.
- **reference (2)**: encuentros aleatorios y enemigos visibles; existen en los juegos relevados pero no
  aplican a una conversión.

## Lo que esto no resuelve todavía

- `battle.json` no lo lee nadie: el core sigue jugando CTB fijo. El siguiente paso es que `BattleEngine` reciba
  un modelo de tiempo (`ITurnScheduler`: CTB, rondas, fases, ATB) y que las mecánicas `planned` entren de a una.
- Las mecánicas de Expedition 33 y FF7 Rebirth salen del diseño público del juego, no de sus archivos.
