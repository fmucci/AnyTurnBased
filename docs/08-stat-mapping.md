# 08 — Mapear stats nativos

Los juegos de acción equilibran con habilidad del jugador (esquivar, apuntar). Por turnos esa habilidad
desaparece, así que con los números crudos las peleas quedan rotas. `StatMapper.FromNative` normaliza:

| Stat | Fuente nativa | Regla |
|---|---|---|
| HP | HP del juego | 1:1, para escribirlo de vuelta exacto |
| Attack | daño por golpe del arma o del enemigo | `max(daño nativo, HP del par / hitsToKillPeer)`: un par cae en ~4 golpes |
| Defense | armadura, resiliencia, reducción de daño | 1:1, resta `0.5 * def` |
| Speed | velocidad de movimiento | `100 * vel / vel del jugador`, mínimo 10 |
| MP | stamina, maná, munición | por juego. En Stardew `Stamina / 10` |

Validación: un enemigo común debe morir en 2–5 turnos del jugador, y un jefe en 10–20. Si no da eso,
el problema está en el mapeo, no en el core (PLAYBOOK §4).
