# 03 — Unity (Mono e IL2CPP)

## Mono

- BepInEx 5 se instala copiando `winhttp.dll` + `BepInEx/` junto al exe (**documented**).
- Las clases del juego están en `<Game>_Data/Managed/Assembly-CSharp.dll`. Se leen con ILSpy/dnSpy y se
  referencian directo al compilar (sin commitear la DLL).
- `AnyTurnBased.Core` es netstandard2.0: carga en Unity 2018.1+ (**documented**). Skyhill usa un Unity anterior
  (sin `UnityPlayer.dll`, **observed**) y puede necesitar compilar el core para net35 (**unknown**).

## IL2CPP

- BepInEx 6 (bleeding edge, IL2CPP) genera ensamblados interop en el primer arranque. Con be.791 funciona
  sobre Unity 6000.3.18f1 con metadata IL2CPP v39: Cpp2IL más el interop tardan ~75 s, y descarga las libs base
  de Unity desde unity.bepinex.dev (**observed**, Moonlighter 2).
- Los plugins corren en el runtime .NET 6 que trae BepInEx (`dotnet/`). Se compilan para `net6.0` y cargan
  `AnyTurnBased.Core` (netstandard2.0) sin problemas (**observed**).
- Para encontrar clases sin decompilador alcanza con leer la metadata del `Assembly-CSharp.dll` del interop
  con `System.Reflection.Metadata`: nombres de tipos, propiedades y firmas de métodos (**observed**).
- Que algo sea estático no se ve en los nombres: `Debug_KillWill()` resultó estático recién al compilar (**observed**).
- Juegos con Unity 6 suelen usar el Input System nuevo: se lee con `Keyboard.current.<tecla>.wasPressedThisFrame`.
  En Moonlighter 2 `GUI.Box/Label/Button` sobreviven al stripping (**observed**).
- Los `MonoBehaviour` propios se registran con `ClassInjector.RegisterTypeInIl2Cpp` antes de usarse.
- Cada patch del juego puede invalidar el interop: borrar `BepInEx/interop` y relanzar (PLAYBOOK §4).

## Hooks típicos

| Hook | Patrón |
|---|---|
| Enemigos | `Object.FindObjectsOfType<T>()` cada N frames, no en cada frame |
| Salud | buscar campos `health/hp/currentHealth` y métodos `TakeDamage/Damage/Hit/Die` |
| Congelar | `Time.timeScale = 0` (`docs/07`) |
| UI | IMGUI en `OnGUI`; no depende de los assets y funciona con `timeScale = 0` |
