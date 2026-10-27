---
name: anytb-design
description: Entrevista a la persona para elegir cómo va a ser el combate por turnos de un juego (modelo de tiempo, economía, reacciones, enemigos, defensa, grupo, inicio y victoria) y escribe game/battle.json. Usar cuando alguien quiere convertir un juego con AnyTurnBased y hay que decidir las mecánicas, o pide "que sea como Expedition 33 / FF9 / Slay the Spire".
---

# Diseñar el combate de un juego

El resultado es `game/battle.json` (esquema en `mechanics/battle-config.schema.json`). Las opciones salen solo de
`mechanics/catalog.json`; no inventes ids. Lo que la persona pida y no esté en el catálogo va a `notes` con sus palabras.

## 0. Leer antes de preguntar

- `mechanics/catalog.json`: categorías (`pick: one|many`, con su pregunta) y mecánicas con `core` y `retrofit`.
- `mechanics/presets.json`: puntos de partida.
- `mechanics/games.json`: qué hace cada juego de referencia, para explicar con ejemplos.
- `game/game.json` si existe: el juego a convertir (motor, adapter). Si no existe, preguntá qué juego es.

Si ya hay un `game/battle.json`, mostrá lo elegido y preguntá qué quiere cambiar en lugar de empezar de cero.

## 1. Punto de partida

Una pregunta: "¿A qué se tiene que parecer el combate?". Opciones: los 3 presets que mejor encajen con el juego
y "Armar desde cero" (con CTB clásico como base). Describí cada preset con su `summary` y el juego de referencia.
Si la persona nombra un juego que no está en presets, buscalo en `games.json`; si tampoco está, describí sus
mecánicas con lo que sepas, decí que es de memoria, y mapealas a ids del catálogo.

## 2. Modelo de tiempo

Es la decisión más importante: preguntala sola, aunque el preset ya traiga una. Opciones de `category: time`,
cada una con un ejemplo de `games.json` (FF9 → ATB, Expedition 33 → CTB con línea de turnos, Slay the Spire → fases).
Si elige `time.atb`, preguntá el modo: Espera (el tiempo se para en los submenús) o Activo (nunca se para).

## 3. El resto, por categoría

Recorré las categorías en el orden del catálogo, partiendo de lo que trae el preset:

- Hasta 4 preguntas por llamada a AskUserQuestion; `multiSelect` cuando la categoría es `pick: many`.
- Máximo 4 opciones por pregunta: dejá afuera las `core: reference` y, si siguen sobrando, partí la categoría
  en dos preguntas o poné primero las que trae el preset.
- En la descripción de cada opción decí si ya funciona (`supported`) o si hay que programarla
  (`partial`/`planned`/`research`). En `research` sumá el `retrofit`: es la parte que depende del juego.
- Si el preset ya resolvió una categoría y la persona no mostró interés, no la preguntes: confirmala en un
  resumen al final.

Parámetros: preguntá solo los que cambian la sensación del juego y traducilos a valores:

| Mecánica | Pregunta | Valores |
|---|---|---|
| `reactive.dodge-parry` | dificultad de los reflejos | fácil 260/130, normal 180/90, difícil 120/60 (`windowMs`/`parryWindowMs`) |
| `reactive.timed-hits` | dificultad | fácil 220, normal 150, difícil 100 (`windowMs`) |
| `time.atb` | velocidad | lento 6, normal 4, rápido 2.5 (`fillSeconds`) |
| `economy.overdrive` | cómo se carga | `damage-taken` / `damage-dealt` / `both` |

El resto queda con el default del catálogo.

## 4. Escribir y validar

1. Escribí `game/battle.json`:

   ```json
   {
     "$schema": "../mechanics/battle-config.schema.json",
     "version": 1,
     "game": "<slug>",
     "preset": "<id del preset o se omite>",
     "time": "time.<...>",
     "mechanics": ["..."],
     "params": { "<id>": { "<param>": "<valor>" } },
     "notes": "<lo que no entró en el catálogo>"
   }
   ```

2. Corré `scripts/check-mechanics.ps1 -BattleConfig game/battle.json`. Si falla, corregí y volvé a correr.
3. Contale a la persona, en este orden:
   - lo que eligió, en una lista corta;
   - qué funciona hoy y qué falta programar en el core (lo imprime el script: "not in the core yet");
   - las `research`: qué depende del juego y por qué puede no salir.

No prometas mecánicas que no estén `supported`: decí que quedan anotadas y qué trabajo implica cada una.
