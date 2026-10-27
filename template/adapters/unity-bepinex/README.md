# Adapter Unity (BepInEx)

Plantilla para juegos Unity. Ver `docs/03`.

| Backend | Loader | Cómo se ven las clases del juego |
|---|---|---|
| Mono (`<Game>_Data/Managed/Assembly-CSharp.dll`) | BepInEx 5 | referencia directa a `Assembly-CSharp.dll` |
| IL2CPP (`GameAssembly.dll`) | BepInEx 6 (IL2CPP) | ensamblados interop que BepInEx genera en el primer arranque |

## Los seis hooks en Unity

| Hook | Implementación habitual |
|---|---|
| `FindNearbyEnemies` | `Object.FindObjectsOfType<EnemyClass>()` filtrado por distancia. La clase sale de dnSpy/ILSpy (Mono) o del dump de Il2CppDumper |
| `FreezeWorld` | `Time.timeScale = 0`. El menú usa `Time.unscaledDeltaTime`. Si la IA usa tiempo real, parchear su `Update` con Harmony (`docs/07`) |
| `ApplyHp` / `Kill` | método de daño del juego (`TakeDamage`, `ApplyDamage`, `Hit`); nunca `Destroy(gameObject)` |
| `IBattleUi` | IMGUI (`OnGUI`) para empezar: anda con `timeScale = 0` y no depende de los assets del juego |

`Plugin.cs` contiene el esqueleto Mono (BepInEx 5). Para IL2CPP se cambia la base a `BasePlugin`
y se registra el `MonoBehaviour` con `ClassInjector`.
