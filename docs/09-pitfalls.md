# 09 — Pitfalls

1. **Borrar la entidad en vez de matarla.** No hay drop, ni XP, ni avance de quest. Hay que usar la función
   de muerte o de daño del juego.
2. **HP 0 escrito sin pasar por la muerte.** Algunos juegos solo chequean la muerte dentro de `TakeDamage`
   y dejan un enemigo inmortal con 0 HP. `ApplyHp` escribe como mínimo 1, y `Kill` hace el resto.
3. **Re-entrar en batalla en loop.** El enemigo que sobrevivió por huida sigue en rango. Por eso existe el cooldown,
   y conviene alejarlo o darle invulnerabilidad al jugador.
4. **Daño de contacto al reanudar.** Después de la batalla el enemigo puede estar tocando al jugador. Hay que dar
   i-frames (Stardew: `temporarilyInvincible`).
5. **Divergencia entre el core C# y el port Lua.** Todo cambio de fórmula se hace en los dos lados, con una entrada
   en `CHANGELOG.md`.
6. **Interop IL2CPP desactualizado** después de un update del juego.
7. **Encuentros durante cutscenes o diálogos.** `CanStartEncounter` tiene que excluirlos.
