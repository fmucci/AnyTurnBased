# AnyRPG

Base de conocimiento y toolchain listo para instanciar que convierte juegos de PC en **RPGs por turnos**,
sin importar su combate original (melee en tiempo real, shooter, físicas). Sigue el modelo de AnyPS1.

No existe un mod que funcione sobre cualquier ejecutable. Lo que sí es universal es el **patrón**:
detectar un encuentro, congelar el mundo, resolver la pelea con un motor por turnos y escribir el
resultado de vuelta en las entidades reales del juego. AnyRPG separa ese patrón, que es genérico,
de la conexión con cada motor, que es específica.

## Tres partes

- **`docs/`**: el conocimiento por motor (fingerprints, cómo congelar el tiempo, cómo leer HP, pitfalls).
  Cada afirmación lleva una etiqueta: **observed**, **inferred**, **unknown** o **documented** (ver `docs/00`).
- **`template/`**: el toolchain genérico.
  - `core/AnyRpg.Core`: motor por turnos en netstandard2.0 (sirve en Unity Mono, .NET 6+ y Godot .NET).
  - `adapters/<motor>`: plantillas de adapter (SMAPI/MonoGame, Unity+BepInEx, Unreal+UE4SS).
  - `game/game.json`: configuración del juego.
- **`PLAYBOOK.md`**: el procedimiento que sigue un agente para convertir un juego nuevo. El resultado es
  un **repositorio aparte por juego**.

## Uso

1. Darle a un agente esta carpeta y la ruta de instalación del juego.
2. Indicarle: *"Seguí PLAYBOOK.md para convertir `<juego>` en RPG por turnos."*
3. `scripts/new-game.ps1` crea `~/work/anyrpg-<juego>` a partir de `template/`. Los archivos del juego
   nunca se commitean ni se suben.

```powershell
./scripts/scan-steam.ps1 -Out docs/12-library-survey.md   # re-escanear la biblioteca
./scripts/new-game.ps1 -Game "Stardew Valley"              # instanciar un repo de juego
```

## Docs

| # | Tema |
|---|---|
| 00 | Método, etiquetas de evidencia, política fail-loud |
| 01 | Modelo por turnos (CTB, stats, acciones, máquina de estados del encuentro) |
| 02 | Fingerprints de motor (cómo saber con qué está hecho un juego) |
| 03 | Unity (Mono e IL2CPP) con BepInEx |
| 04 | Unreal (UE4/UE5) con UE4SS |
| 05 | .NET nativo: MonoGame/SMAPI y Godot .NET |
| 06 | Motores Lua: LÖVE y RE Engine |
| 07 | Congelar el mundo |
| 08 | Mapear stats nativos a stats RPG |
| 09 | Pitfalls |
| 10 | Preguntas abiertas |
| 11 | Fuentes |
| 12 | Relevamiento de la biblioteca local |

## Exclusiones deliberadas

Nada de binarios, DLL, assets, saves, dumps ni código de terceros. Los adapters referencian las DLL
del juego instalado al compilar. Solo juegos **single-player/offline**: nada con anti-cheat ni online.

## Convenciones

- Un cambio en un hecho de motor va al doc correspondiente, con etiqueta y entrada en `CHANGELOG.md`.
- Lo que es específico de un juego (offsets, nombres de clases, hooks) va en el repo de ese juego.
- Las mejoras genéricas que surjan en un juego vuelven a `template/` (PLAYBOOK §7).

## Estado

- Core: compila (netstandard2.0, 0 warnings) y pasan los 5 tests con .NET SDK 8.0.427 (**observed**).
- Adapter Stardew: compila y carga en Stardew 1.6.15 + SMAPI 4.5.2 (**observed**). Combate en juego sin probar.
- Adapters Unity y Unreal: plantillas, se completan con un juego concreto.

```powershell
dotnet test template/core/AnyRpg.Core.Tests
```
