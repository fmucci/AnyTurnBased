# 07 — Congelar el mundo

El paso más frágil. Un freeze es correcto cuando, durante la batalla, no se mueve ningún enemigo, proyectil,
timer de juego, reloj de día/noche ni física.

| Motor | Mecanismo | Lo que se escapa |
|---|---|---|
| Unity | `Time.timeScale = 0` | código que usa `Time.unscaledDeltaTime`, `Time.realtimeSinceStartup`, corutinas con `WaitForSecondsRealtime`, animators en modo `UnscaledTime` |
| Unreal | `SetGlobalTimeDilation(w, ~0)` | timers con tiempo real, actores con `CustomTimeDilation` propio, audio. Valores muy chicos se clampean (**documented**) |
| MonoGame/Stardew | prefix de Harmony sobre `update` de monstruos + congelar controles | reloj del día (`Game1.timeOfDay`) y proyectiles (`GameLocation.projectiles`) (**inferred**) |
| RE Engine | time scale de la escena | **unknown** |

Reglas:
- La UI y el director corren con tiempo sin escalar.
- Guardar el time scale previo y restaurarlo: el juego puede estar en cámara lenta.
- Verificar con un enemigo que dispara: si el proyectil avanza durante la batalla, el freeze no alcanza.
