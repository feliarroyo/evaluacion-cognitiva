# Aplicación móvil que presenta un juego diseñado para facilitar la evaluación de la memoria visual a corto plazo

Desarrollada por Felipe Arroyo y Melisa Messa, bajo la dirección de Virginia Yannibelli, en el contexto de Proyecto Final de la carrera de Ingeniería de Sistemas en UNICEN. La aplicación combina la portabilidad de los dispositivos móviles con la naturaleza interactiva de los entornos virtuales, simulando una actividad de la vida cotidiana en un entorno del hogar controlado para medir métricas cognitivas con alta validez ecológica.

En conjunto con esta aplicación, se desarrolló una [aplicación web](https://github.com/feliarroyo/evaluacion-cognitiva-web) para la consulta y modificación de los datos de los usuarios, incluyendo sus resultados completos y la configuración de sus niveles.

## Aclaraciones para la instalación del proyecto
- Utilizar Unity 2021 LTS (2021.3.9f1)
- Utilizar Git LFS para clonar modelos y otros archivos de gran tamaño
- Agregar layers 6 a 9: (6. Ground - 7. Furniture - 8. Items - 9. Drawer) para un funcionamiento adecuado

## Stack técnico y estructura

*   **Motor de Desarrollo:** Unity 2021 LTS (versión estable 2021.3.9f1).
*   **Lenguaje de Programación:** C#.
*   **Integración en la Nube:** SDK de Firebase para Unity (Firebase Authentication y Realtime Database).
*   **Arquitectura de Escenas:** El flujo dinámico se organiza en tres escenas principales cargadas de forma asíncrona (`PreloadScene`) junto con un panel de carga interactivo:
    1.  `MainMenu`: Inicialización del estado de sesión (`Firebase Auth Manager`), sincronización de datos locales y panel del historial (`History Loader`).
    2.  `House`: Entorno tridimensional interactivo donde transcurre la evaluación y la lógica del temporizador.
    3.  `Results`: Escena de procesamiento local de métricas de desempeño y sincronización final con la nube.

## Mecánicas de juego e interacción

El juego implementa una navegación en primera persona optimizada para pantallas táctiles:
*   **Controles simultáneos:** Desplazamiento por el mapa mediante un joystick virtual (`Fixed Joystick` y `Player Movement`) combinado con el control de rotación de cámara (`Fixed Touch Field` y `Camera Control`).
*   **Sistema de interacción completo:** Un componente `Interactable Detection` adjunto al jugador proyecta un Collider esférico invisible para identificar objetos o muebles cercanos. Los elementos interactuables adquieren un borde blanco dinámicamente.
*   **Observación y manipulación 3D:** Al tocar un objeto interactuable, este se traslada mediante Raycasts y cálculos con su Collider hacia la cámara (`HeldItem` e interfaz `IElementBehaviour`). El usuario puede rotarlo libremente en cualquier eje para memorizar sus detalles, confirmando su elección al tocarlo nuevamente (añadiéndose al `Inventory Display` mediante un patrón Singleton) o devolviéndolo a su posición con el botón de soltar.
*   **Muebles dinámicos:** Puertas (`OpenDoor`) y cajones (`OpenDrawer`) ejecutan rotaciones o desplazamientos físicos personalizados. Los cajones trasladan de forma coordinada los objetos contenidos en sus Spawns internos.

## Progresión del juego y flujo de evaluación

La evaluación se ejecuta de manera secuencial y local a través de un archivo JSON descargado automáticamente al iniciar sesión, estructurado con las clases nativas `PatientData`, `LevelConfig` y `SpawnInfo`. El ciclo de vida de la actividad está controlado por el script `GameStatus` utilizando el enumerador `GamePhase`:

1.  **Waiting:** El usuario inicia fuera de la casa.
2.  **Before Memorizing:** Se activa al abrir la puerta de entrada por cuestiones de auditoría de logs.
3.  **Memorizing (etapa de memorización):** Se activa al cruzar un Trigger de entrada. Enciende automáticamente las luces inteligentes del Hall e inicia el cronómetro regresivo con el tiempo personalizado (`timeMem`). El usuario debe memorizar los estímulos 3D expuestos en los estantes. Una pared invisible impide avanzar en el pasillo durante esta fase.
4.  **Before Search (etapa de retención):** Al expirar el tiempo del Hall, las luces de esta sección se apagan, se desactiva la interacción con objetos, se habilitan las luces del pasillo y desaparece la pared invisible. El usuario recorre el pasillo simulando el tiempo de retención de información.
5.  **Search (Etapa de Identificación):** Al cruzar el Trigger del Living, se apagan las luces del pasillo y se encienden las del Living. El cronómetro se inicializa con el tiempo de búsqueda (`timeSearch`) y se vuelve a activar la interacción. El usuario debe buscar e identificar los objetos del Hall mezclados entre el amoblamiento y múltiples elementos distractores.
6.  **Search Over:** Al terminar el tiempo o cruzar el Trigger de salida, las luces se apagan por completo y se realiza la transición a la escena de resultados.

## Sistema de logging y cálculo de métricas

### Registro secuencial de estados
El componente `Logging` captura detalladamente los eventos del usuario en tiempo real mediante un formato estructurado por punto y coma (`;`), recopilando marcas de tiempo, coordenadas de posición y vectores de movimiento, orientación de la cámara, objetos interactuables en pantalla y el estado preciso del inventario.

### Procesamiento de métricas de desempeño
Al concluir la sesión, el script `Results` invoca localmente a las clases auxiliares `MetricsCalculator` y `LogAnalyzer` para calcular de manera automatizada las siguientes ecuaciones matemáticas fundamentales antes de guardarlas en el nodo `results` de Firebase.
