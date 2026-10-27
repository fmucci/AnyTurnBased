# 01 — Modelo por turnos

## Orden de turnos: CTB

Conditional Turn Battle, como en FFX. Cada combatiente espera `10000 / Speed` ticks entre turnos. Con speed
200 actúa el doble que con speed 100. Se eligió CTB en vez de rondas fijas porque conserva la diferencia
de velocidad del juego original: un enemigo rápido en tiempo real sigue siendo rápido por turnos (**inferred**).

`BattleEngine.PreviewOrder(n)` da la cola de turnos para mostrarla en la UI.

## Stats

`MaxHp/Hp, MaxMp/Mp, Attack, Defense, Speed`. El HP se mapea 1:1 con el nativo, así escribirlo de vuelta es
exacto. El resto pasa por `StatMapper` (`docs/08`).

## Daño

```
raw  = attack * power - defense * 0.5
dmg  = max(1, round(raw * U(0.9, 1.1)))
si el objetivo se defiende: dmg = max(1, dmg / 2)
```

## Acciones base

| Acción | Objetivo | Efecto |
|---|---|---|
| Atacar | enemigo | daño con power 1.0 |
| Skill | enemigo / aliado / uno mismo | daño con `power`, o cura `MaxHp * 0.1 * power`; cuesta MP |
| Defender | uno mismo | mitad de daño hasta el próximo turno propio |
| Huir | — | 60 % de éxito; termina la batalla sin aplicar muertes |

Cada adapter agrega acciones con el sabor del juego: hechizos en Hogwarts, armas especiales en Stardew, etc.

## Máquina de estados del encuentro

```
Exploring --(enemigo en radio && CanStartEncounter)--> InBattle
InBattle  --(Victory | Defeat | Fled)--> aplicar HP, Kill(), ResumeWorld --> Cooldown
Cooldown  --(CooldownSeconds en tiempo real)--> Exploring
```

El cooldown evita re-entrar en batalla en el mismo frame si un enemigo sobrevivió por huida.
El director corre con **tiempo sin escalar**: el mundo está congelado, pero el menú y la IA enemiga siguen.
