# 00 — Método

## Pipeline

1. Fingerprint del motor (`docs/02`, `scripts/scan-steam.ps1`).
2. Instalar el loader del motor y confirmar que carga un mod vacío.
3. Encontrar las clases del jugador, los enemigos y la salud (dnSpy/ILSpy, Il2CppDumper, dump de UE4SS).
4. Implementar los seis hooks de `IGameAdapter` en el orden de PLAYBOOK §3, con un log que verifique cada uno.
5. Forzar encuentros con el comando de debug (`anyrpg_battle`, F9) antes de activar la detección automática.
6. Verificar contra el juego sin mod: un kill por turnos deja el mismo estado que un kill normal.

## Reglas de evidencia

Etiquetas en negrita, en línea:

- **observed**: reproducido con un comando, un log o un archivo en disco.
- **documented**: lo dice una fuente externa (`docs/11`) y no lo reprodujimos.
- **inferred**: razonamiento.
- **unknown**: no sabemos.

Un hecho **documented** que un test confirma pasa a **observed**. Si un test lo contradice queda como
**differs**, con los dos resultados.

## Fail loud

Todo lo que el adapter no puede probar aborta con un mensaje que lo nombra: clase no encontrada, campo
de HP nulo, enemigo sin componente de salud, un handle nativo de tipo desconocido. Así "el mod se comporta raro"
pasa a ser "el mod se detiene acá". Las plantillas lanzan `NotImplementedException` / `error()` en cada hook
sin implementar, así el primer arranque lista lo que falta.

## Capas de test

| Capa | Dónde | Qué prueba |
|---|---|---|
| Core | `template/core/AnyRpg.Core.Tests` | turnos, daño, victoria/derrota/huida, director con adapter falso |
| Adapter | log en el juego | cada hook de PLAYBOOK §3 |
| Equivalencia | juego | kill por turnos = kill normal (drop, XP, quest) |
| Soak | juego | 50 encuentros seguidos con input aleatorio |
