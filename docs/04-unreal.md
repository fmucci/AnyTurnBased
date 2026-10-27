# 04 — Unreal (UE4/UE5)

- UE4SS se inyecta con un proxy `dwmapi.dll` en `<Game>/Binaries/Win64/` (**documented**).
- Los mods en Lua van en `Binaries/Win64/Mods/<Mod>/Scripts/main.lua` y se habilitan en `mods.txt`.
- El dumper de UE4SS genera los headers con las clases del juego: de ahí salen el jugador, los enemigos
  y el componente de salud.
- La salud suele estar en un `UActorComponent` propio, o en un atributo de GAS (`UAbilitySystemComponent`)
  en juegos grandes (**inferred**). Con GAS, aplicar daño con un `GameplayEffect` del juego mantiene los
  efectos secundarios.
- Congelar: `SetGlobalTimeDilation`. Valores extremos se clampean (`docs/07`).
- Versiones observadas: Librarian `++UE5+Release-5.5`. Las demás no exponen la versión en `FileVersion`.
  Hay que leerla del exe con un string search de `++UE` (**unknown** hasta hacerlo).
- El core está portado a Lua (`template/adapters/unreal-ue4ss/Scripts/anyturnbased_core.lua`). Hay que mantener las
  fórmulas iguales a las de C# (`docs/09`).
