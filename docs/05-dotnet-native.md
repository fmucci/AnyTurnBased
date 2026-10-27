# 05 — .NET nativo: MonoGame (Stardew) y Godot .NET

## Stardew Valley / SMAPI

- Stardew 1.6 corre en .NET 6 (**observed**: `Stardew Valley.runtimeconfig.json`, tfm net6.0, runtime 6.0.32).
- SMAPI reemplaza el arranque y carga mods de `Mods/<Mod>/` con `manifest.json` (**documented**).
- `Pathoschild.Stardew.ModBuildConfig` encuentra el juego y copia el build a `Mods/` (**documented**).
- Instalado: SMAPI 4.5.2 sobre Stardew Valley 1.6.15 build 24356 (**observed**, log de SMAPI).
- Instalación no interactiva: `SMAPI.Installer.exe --install --game-path "<ruta>" --no-prompt`. Necesita una
  consola real (con la salida redirigida falla en `Console.Clear`) y una ruta de extracción corta, porque las
  rutas de más de 260 caracteres dan "directory name is invalid" (**observed**).
- ModBuildConfig necesita `<EnableHarmony>true</EnableHarmony>` para referenciar `0Harmony` (**observed**).
- Miembros usados en el adapter: `Monster.Health/MaxHealth/DamageToFarmer/resilience/Speed`,
  `Farmer.health/maxHealth/buffs.Defense/Stamina`, `GameLocation.damageMonster(...)`. Que existen con esas
  firmas está **observed** (compila contra 1.6.15). Que se comporten como espera el adapter (XP y drops en
  `damageMonster`) sigue **documented** hasta PLAYBOOK §3 paso 5.
- PLAYBOOK §3 paso 1 (el mod carga): `[AnyTurnBased] loaded stardew-valley 1.6.15` (**observed**).
- Congelar: un prefix de Harmony sobre `Monster.update` + `Game1.freezeControls`. El reloj del día
  sigue corriendo mientras dure la batalla (**inferred**, `docs/10`).

## Godot .NET (Slay the Spire 2)

- Trae `0Harmony.dll` y `data_sts2_windows_x86_64/` (**observed**). Es Godot con C# (**inferred**).
- Ya es por turnos. Sirve para validar que `AnyTurnBased.Core` carga en Godot .NET. No es objetivo de conversión.
