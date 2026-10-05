# WebGL optimization audit

Fecha: 2026-10-05

## Resumen ejecutivo

El problema principal no parece ser una sola opcion de Player Settings, sino la forma en que los assets se estan empaquetando para WebGL. El proyecto tiene muchos assets pesados en `Assets/`, pero Unity solo incluye en el build lo que esta referenciado por escenas, `Resources`, `StreamingAssets`, plugins del target y contenido incluido por Addressables. En este proyecto los riesgos mas claros para la primera carga son:

- `Assets/Resources/CARTASAVESUNITY` tenia ~64 MB de imagenes dentro de `Resources`; ya fue movido a `Assets/AddressableContent/CARTASAVESUNITY`.
- `Assets/StreamingAssets/Master.bank`: ~6.11 MB que WebGL debe publicar como archivo de runtime.
- 15 escenas habilitadas en `ProjectSettings/EditorBuildSettings.asset`.
- FMOD HTML5 activo: `Assets/Plugins/FMOD/platforms/html5/lib/3.1.39/libfmodstudio.a`.
- Addressables instalado pero practicamente sin uso real: el grupo `Escenas` esta vacio.

## Cambios aplicados

- `ProjectSettings/ProjectSettings.asset`: `webGLAnalyzeBuildSize` paso de `0` a `1` para generar informacion de tamano de build WebGL.
- `ProjectSettings/QualitySettings.asset`: WebGL paso del perfil `High` al perfil `Low` como calidad inicial.
- `Assets/AddressableAssetsData/AddressableAssetSettings.asset`: `m_OptimizeCatalogSize` paso a `1`.
- `Assets/AddressableAssetsData/AddressableAssetSettings.asset`: `m_BuildAddressablesWithPlayerBuild` paso a `1`.
- `Assets/AddressableAssetsData/AddressableAssetSettings.asset`: `m_BuildRemoteCatalog` paso a `1`.
- `ProjectSettings/ProjectSettings.asset`: `webGLNameFilesAsHashes` paso a `1` para facilitar cache inmutable en servidor.
- `Assets/Resources/CARTASAVESUNITY` fue movido a `Assets/AddressableContent/CARTASAVESUNITY` preservando archivos `.meta`.
- Se agregaron utilidades runtime en `Assets/Scripts/Optimization`.
- Se agrego `Assets/Editor/WebGLOptimizationTools.cs` para configurar Addressables, texturas y audio desde Unity.
- Se agregaron ejemplos de headers/configuracion en `webgl-server/` para Nginx, Apache, IIS y hosts compatibles con `_headers`, mas checklist en `docs/WEBGL_DEPLOYMENT.md`.

Estos cambios son reversibles y no eliminan assets.

## Hallazgos principales

### 1. `Resources` estaba inflando el build inicial

`Assets/Resources` pesaba aproximadamente 64.12 MB. La mayor parte eran cartas que ahora viven en `Assets/AddressableContent/CARTASAVESUNITY`:

- `Assets/AddressableContent/CARTASAVESUNITY/TINGUA PICO VERDE.png`: ~3.21 MB
- `Assets/AddressableContent/CARTASAVESUNITY/AGUILA PESCADORA.png`: ~3.17 MB
- `Assets/AddressableContent/CARTASAVESUNITY/CARPINTERO AHUMADO.png`: ~3.14 MB
- Muchas otras cartas estan entre 2.9 MB y 3.1 MB.

Ademas, hay copias muy parecidas en `Assets/ASSETS/00.CARTAS`, por lo que hay duplicacion en el repositorio y riesgo de duplicacion si ambas rutas son referenciadas por escenas.

Estado:

- `CARTASAVESUNITY` ya salio de `Resources`.
- `Assets/Resources` queda en ~1.79 MB.
- El grupo Addressables remoto se prepara desde `Tools > WebGL Optimization > Prepare Project For WebGL`.

Recomendacion pendiente:

- Cargar cartas por demanda en la escena `Cartas`, no durante la carga inicial del juego.
- Bajar import settings WebGL de esas imagenes a 1024 px o menos si se muestran como cartas UI.
- Activar `Crunch Compression` o usar formatos WebGL adecuados segun calidad visual requerida.

### 2. Addressables esta configurado, pero no se esta usando

El grupo `Assets/AddressableAssetsData/AssetGroups/Escenas.asset` existe, pero `m_SerializeEntries` esta vacio. Eso significa que Addressables no esta reduciendo la primera descarga todavia.

Recomendacion:

- Crear grupos por dominio: `Cards`, `Monsters`, `Dungeons`, `Audio`.
- Mantener en escenas iniciales solo UI minima y assets necesarios para el menu.
- Cargar el resto despues del primer frame o al entrar a cada modo.

### 3. Audio WAV duplicado y pesado

`Assets/ImaginatioSound` pesa ~325.49 MB. Hay muchos WAV duplicados en la raiz y en `Assets/ImaginatioSound/Assets`.

Ejemplos:

- `Imaginatio sound_CavernSound01.wav`: ~30.27 MB duplicado.
- `Imaginatio sound_DungeonExplorer.wav`: ~22.98 MB duplicado.
- `Imaginatio sound_CavernSound02.wav`: ~20.07 MB duplicado.
- `Imaginatio sound_PrincipalMusic.wav`: ~18.20 MB duplicado.
- `Imaginatio sound_SecondMusic.wav`: ~16.97 MB duplicado.

Los `.meta` revisados usan `compressionFormat: 1` y `preloadAudioData: 0`, lo cual ayuda, pero `loadInBackground: 0` puede causar pausas al cargar clips grandes.

Recomendacion:

- Eliminar duplicados solo despues de confirmar referencias en escenas/prefabs.
- Para musica larga: `Load Type = Streaming`, `Load In Background = true`, Vorbis con calidad moderada.
- Para SFX cortos: mantener comprimido en memoria o decompress on load segun uso.
- Si FMOD ya maneja el audio final, evitar mantener los WAV fuente dentro de carpetas referenciadas por escenas.

### 4. Modelos 3D pesados

`Assets/Modelos Bichitos` pesa ~331.38 MB. Hay FBX individuales de 6 MB a 15 MB y texturas de 2K/4K aprox.

Ejemplos:

- Tarrasque FBX: ~15.61 MB.
- Ent FBX: ~14.44 MB.
- Pixie FBX: ~11.21 MB.
- Marid/Gnoll/Myconid: ~10 MB cada uno.

Recomendacion:

- Usar LODs o versiones WebGL simplificadas.
- Desactivar importacion de animaciones si no se usan.
- Reducir normales/texturas a 1024 o 512 cuando se ven pequenos en pantalla.
- Mover monstruos a Addressables por dificultad y cargar solo el enemigo requerido.

### 5. Calidad WebGL estaba en `High`

`ProjectSettings/QualitySettings.asset` tenia `WebGL: 3`, que corresponde a `High`. Ese perfil activa sombras, reflection probes y mas presupuesto de particulas que `Low` o `Medium`.

Cambio aplicado:

- WebGL ahora inicia en `Low` (`WebGL: 1`).

Si la diferencia visual es demasiado fuerte, probar `Medium` (`WebGL: 2`) como equilibrio.

### 6. Plugins y SDKs ocupan mucho en repo

Tamanos por carpeta:

- `Assets/Plugins`: ~373.20 MB.
- `Assets/Firebase`: ~62.53 MB.
- `Assets/Mapbox`: ~55.35 MB.
- `Assets/Photon`: ~34.51 MB.
- `Assets/TextMesh Pro`: ~13.85 MB.

Esto no entra completo al build si los import settings estan correctos. Verificacion puntual: solo `Assets/Plugins/FMOD/platforms/html5/lib/3.1.39/libfmodstudio.a` aparece habilitado para WebGL dentro de los plugins FMOD.

Recomendacion:

- Borrar demos/ejemplos de SDKs solo si no estan referenciados.
- Mantener los SDKs reales, pero excluir samples y assets demo.
- Revisar `Mapbox/Examples`, `Photon/.../Demos` y `TextMesh Pro/Examples & Extras`.

## Siguiente paso recomendado

1. Hacer un build WebGL con `Development Build` desactivado y `Compression Format = Brotli`.
2. Revisar el Build Report / WebGL build size analyzer.
3. Priorizar los 20 assets mas grandes que realmente aparezcan en el `.data`.
4. Ejecutar `Tools > WebGL Optimization > Prepare Project For WebGL` dentro de Unity para registrar las cartas como Addressables y reimportar texturas/audio.
5. Luego migrar monstruos/audio por escena o por dificultad.

## Meta inicial

Para una experiencia WebGL razonable, buscar:

- Primer paquete inicial: idealmente menos de 30-50 MB comprimidos.
- Contenido pesado: Addressables descargados despues del menu.
- Menu inicial: escena pequena, sin cartas completas, sin monstruos 3D, sin bancos o musica que no suene inmediatamente.
