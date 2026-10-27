# 06 — Motores con Lua: LÖVE y RE Engine

## LÖVE (Balatro)

- `love.dll` + `lua51.dll` (**observed**). Los mods se inyectan con Lovely, que parchea el Lua del juego
  al cargarlo (**documented**).
- Balatro ya es por turnos y queda solo como referencia. Para otro juego LÖVE se reusaría
  `anyrpg_core.lua`, pero LuaJIT/5.1 no tiene `//`: hay que reemplazarlo por `math.floor(a / b)`
  (**inferred**).

## RE Engine (PRAGMATA)

- Trae `re_chunk_000.pak` (**observed**), así que es RE Engine (**inferred**).
- REFramework permite scripts Lua con acceso a los tipos del motor vía `sdk.find_type_definition` (**documented**).
- Congelar: el time scale global del motor (`via.Application` / `via.Scene`). El nombre exacto es **unknown**.
- Si PRAGMATA es muy nuevo, REFramework puede no soportarlo todavía (**unknown**).
