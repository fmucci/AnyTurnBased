# Adapter Unreal (UE4SS)

Plantilla para juegos UE4/UE5. Ver `docs/04`.

UE4SS se inyecta reemplazando `dwmapi.dll` junto a `<Game>-Win64-Shipping.exe` y carga mods en Lua desde
`Binaries/Win64/Mods/<Mod>/Scripts/main.lua`. El core en C# no corre ahí. Este adapter es un **port en Lua**
de `BattleEngine` y `EncounterDirector` (`Scripts/anyturnbased_core.lua`), con las mismas fórmulas. Cuando se
cambia una fórmula en C#, se cambia también acá (`docs/09`).

| Hook | Implementación habitual en Unreal |
|---|---|
| `FindNearbyEnemies` | `FindAllOf("<EnemyClass>")` + `K2_GetActorLocation()` y distancia |
| `FreezeWorld` | `UGameplayStatics:SetGlobalTimeDilation(world, 0.0001)`; 0 exacto rompe algunos juegos (`docs/07`) |
| `ApplyHp` / `Kill` | el componente de salud del juego (ver el dump del SDK de UE4SS); se evita `K2_DestroyActor` |
| UI | Hecha en UE4SS con ImGui o con un `UUserWidget` del juego. Por ahora usa mensajes en pantalla y teclas |

Los nombres de clases salen del **dumper de objetos de UE4SS** (`Keybinds: Ctrl+Numpad7` dump CXX headers).
