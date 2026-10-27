# PLAYBOOK - pasar el combate de un juego de PC a turnos, como agente, sin un humano

## 0. Reglas que no se doblan

1. **Ningún archivo del juego en git.** DLL, assets, saves y dumps viven en `game-files/`, `work/`, `build/`
   y `dist/`, todos ignorados.
2. **Solo single-player/offline.** Si el juego tiene anti-cheat (EAC, BattlEye, NEP, Vanguard) o combate
   online, frená y reportá. No se intenta evadir nada.
3. **Fail loud.** Si el adapter no encuentra una clase, un campo o un enemigo esperado, aborta con un mensaje
   que lo nombra. No inventa valores ni sigue con defaults silenciosos (`docs/00`).
4. **El resultado se aplica con la ruta propia del juego.** Para matar enemigos se usa la función de daño o
   de muerte del juego, no se borra la entidad, para que drops, XP, logros y quests sigan funcionando.
5. **Cada afirmación lleva etiqueta** (observed / inferred / unknown / documented).
6. **Commits chicos**, solo con los tests del core en verde.

## 1. Prerrequisitos (verificá e instalá lo que falte, no preguntes)

- .NET SDK 8 (`dotnet --list-sdks` no debe salir vacío).
- git.
- El loader del motor (`docs/03`–`06`): SMAPI, BepInEx 5/6, UE4SS o Lovely, según el fingerprint.
- Para IL2CPP: Il2CppDumper o el interop que genera BepInEx 6. Para Unreal: el dumper SDK de UE4SS.

## 2. Crear el repositorio del juego

```powershell
./scripts/scan-steam.ps1 -Game "<carpeta en steamapps/common>"   # fingerprint (docs/02)
./scripts/new-game.ps1   -Game "<carpeta>"                         # copia template/ a ~/work/anyturnbased-<slug>
```

`new-game.ps1` escribe `game/game.json` con el motor detectado y deja solo el adapter que corresponde.
**Done:** el repo existe, `game.json` tiene `engine`, `loader` e `installPath`, y el loader está instalado
en el juego.

Si hay una persona disponible, corré la skill `anytb-design` para que elija las mecánicas: escribe
`game/battle.json` y lo valida con `scripts/check-mechanics.ps1` (`docs/13`). Sin persona, el default es el
preset `ctb-classic`, que es lo único que hoy está completo en el core.

## 3. Bring-up: los seis hooks

Implementá `IGameAdapter` en este orden. Cada paso tiene un log verificable.

| # | Hook | Done cuando el log muestra… |
|---|---|---|
| 1 | Cargar el mod | `[AnyTurnBased] loaded <game> <version>` |
| 2 | `GetPlayerParty` | HP y stats del jugador, iguales a los del HUD |
| 3 | `FindNearbyEnemies` | lista de enemigos con HP al acercarse a uno |
| 4 | `FreezeWorld` / `ResumeWorld` | enemigos y proyectiles quietos, timers de juego quietos (`docs/07`) |
| 5 | `ApplyDamage` / `Kill` | HP nativo baja; al morir hay drop/XP como en combate normal |
| 6 | `IBattleUi` | menú visible, se navega con teclado y con gamepad |

## 4. Triage (síntoma → dónde mirar)

| Síntoma | Causa probable | Doc |
|---|---|---|
| El mundo sigue moviéndose congelado | timers con tiempo real o físicas aparte | 07 |
| Re-entra en batalla en loop | falta cooldown o el enemigo sigue en rango | 01 |
| Enemigo muere sin drop | se borró la entidad en vez de usar la muerte nativa | 09 |
| Daño absurdo (1 o 9999) | escala de stats mal mapeada | 08 |
| Crash al cargar en IL2CPP | interop desactualizado tras un patch | 03 |
| UE4SS no inyecta | versión de UE o build no soportada | 04 |

## 5. Verificar sin un humano

1. Tests del core en verde (`dotnet test template/core`).
2. Encuentro forzado por comando de debug: batalla completa con victoria y con derrota.
3. La victoria deja el mismo estado que un kill normal (drop, XP, contador de quest).
4. La derrota usa la ruta de muerte del jugador del juego.
5. Huir restaura el mundo sin daño y con cooldown.
6. Guardar y cargar después de una batalla no corrompe nada.
7. Soak: 50 encuentros seguidos con input aleatorio, cero excepciones.

## 6. Devolver al template

Todo lo genérico (un helper de freeze para Unity, un mapeo de stats nuevo) vuelve a `template/` con un test.
Lo que aprendiste del motor va al doc que corresponda, con etiqueta y entrada en `CHANGELOG.md`.

## 7. Cuándo preguntarle a la persona (solo esto)

- El juego tiene anti-cheat o componentes online.
- Hay que comprar o descargar algo que no sea open source.
- Una decisión de diseño cambia el juego: qué cuenta como "encuentro" en un juego sin combate.

## 8. Condiciones de corte y reporte final

Cortá cuando §5 esté en verde, o cuando el mismo error resista tres intentos basados en evidencia.
Escribí `docs/report.md` en el repo del juego con lo que funciona, lo que no y las etiquetas de cada afirmación.
