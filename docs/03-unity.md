# 03 — Unity (Mono e IL2CPP)

## Mono

- BepInEx 5 se instala copiando `winhttp.dll` + `BepInEx/` junto al exe (**documented**).
- Las clases del juego están en `<Game>_Data/Managed/Assembly-CSharp.dll`. Se leen con ILSpy/dnSpy y se
  referencian directo al compilar (sin commitear la DLL).
- `AnyRpg.Core` es netstandard2.0: carga en Unity 2018.1+ (**documented**). Skyhill usa un Unity anterior
  (sin `UnityPlayer.dll`, **observed**) y puede necesitar compilar el core para net35 (**unknown**).

## IL2CPP

- BepInEx 6 (bleeding edge, IL2CPP) genera ensamblados interop en el primer arranque (**documented**).
- Los `MonoBehaviour` propios se registran con `ClassInjector.RegisterTypeInIl2Cpp` antes de usarse.
- Cada patch del juego puede invalidar el interop: borrar `BepInEx/interop` y relanzar (PLAYBOOK §4).

## Hooks típicos

| Hook | Patrón |
|---|---|
| Enemigos | `Object.FindObjectsOfType<T>()` cada N frames, no en cada frame |
| Salud | buscar campos `health/hp/currentHealth` y métodos `TakeDamage/Damage/Hit/Die` |
| Congelar | `Time.timeScale = 0` (`docs/07`) |
| UI | IMGUI en `OnGUI`; no depende de los assets y funciona con `timeScale = 0` |
