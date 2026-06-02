# Estado del Proyecto — Simulador de Bacterias
**Entrega:** Sábado (quedan pocos días)

---

## ¿Qué hay hecho?

### Escenas
- **MenuPrincipal** — botones Nueva Simulación, Tutorial, Cargar Partida, Salir del Juego
- **GamePlay** — simulación completa funcionando
- **Tutorial** — tutorial guiado con BacteriaTutorial en el centro, ~19 pasos

### Sistemas principales
- Bacterias con genes: velocidad, visión, tamaño, consumo, energía máx, tiempo de vida, coste reproducción, cooldown reproducción
- Reproducción asexual con mutaciones aleatorias
- Depredación (bacteria grande devora pequeña)
- Pool de bacterias y comida (BacteriasMuertas, PoolComida) — DontDestroyOnLoad, limpian al cambiar escena
- GestorLinajes — seguimiento de linajes y evolución
- EvolutionTracker — gráficas de evolución en tiempo real
- Sistema de guardado/carga de partidas (JSON en persistentDataPath)
- Placa Petri circular (LineRenderer + EdgeCollider2D)

### UI / Navegación
- Menú de pausa (Escape) con guardar, cargar, volver al menú
- ControladorTiempo (T = 0x / 1x / 5x) — bloqueado durante tutorial salvo cuando se pide
- Panel inspector de bacteria (draggable)
- Gráficas de evolución (draggables) — se mencionan en el tutorial
- Botón música 🔊/🔇 con sprites (esquina inferior derecha)
- Botón ajustes ⚙ (sprite gear) — panel con pantalla completa y volumen, guarda con PlayerPrefs
- AudioManager DontDestroyOnLoad con shuffle de playlist (Resources/Audio/)
- GestorEscenas — navega entre escenas, pasa datos (PartidaACargar, ModoTutorial)

### Tutorial (~19 pasos)
0. Bienvenida
1. Control del tiempo (T) → secuencia 4s viendo bacteria moverse
2. Cámara WASD
3. Zoom
4. Explicación genes (velocidad, visión, tamaño, consumo, energía, tiempo de vida, coste reproducción, cooldown)
5. Reproducción y evolución
6. Click en bacteria → secuencia 5s viendo inspector en tiempo real
7. Herramienta Menú (explicación)
8. Herramienta Mover (explicación)
9. Herramienta Lupa (explicación)
10. Herramienta + Laboratorio (explicación)
10b. Crear bacteria → detecta cuando el jugador coloca una en la placa
11. Herramienta Estrella comida/spawner (explicación)
11b. Dar de comer → detecta cuando coloca comida
11c. Spawner automático → detecta cuando coloca spawner
12. Herramienta Casa centrar cámara (explicación)
13. Gráficas de evolución movibles
14. Guardar y Cargar
15. Fin → carga GamePlay vacío

### Bugs corregidos (historial)
- ControladorMenuPausa reseteaba timeScale sobreescribiendo el tutorial
- juegoPausado (static) se contaminaba entre sesiones
- SecuenciaBacteriaMoviendose no actualizaba fixedDeltaTime (lag permanente a 5x)
- BacteriasMuertas y PoolComida tenían referencias destruidas entre escenas
- Bacterias se podían crear sin especie seleccionada
- Escape abría el menú durante el tutorial
- float.TryParse en CartaSpawnerUI usaba locale del sistema (coma vs punto) → corregido con InvariantCulture
- Spawner radio por defecto era 1.5 → cambiado a 10

---

## PENDIENTE para próxima sesión

### BUGS / PENDIENTE URGENTE
- [ ] **Amarillo en cartas seleccionadas NO FUNCIONA** — el sistema de evento estático (BandejaEspeciesUI.OnCartaSeleccionada) está implementado pero el color no cambia visualmente. Revisar si el GetComponent<Image>() en Awake() obtiene la imagen correcta (puede que sea un hijo, no el root). Puede que necesite encontrar el hijo "FondoBlanco" en lugar del root.

### TUTORIAL — Cosas a pulir
- [ ] **Colocar 3 bacterias en tutorial** — cambiar paso 10n para que requiera colocar exactamente 3 bacterias. Añadir contador `_bacteriasColocadasCount` en TutorialManager, incrementar en cada `OnBacteriaColocada`, no avanzar hasta llegar a 3. Actualizar mensaje: "Coloca <b>3 bacterias</b> en la placa (X/3)". Además, eliminar la BacteriaTutorial del escenario en el `onEntrar` de ese paso (buscar por tag o nombre y llamar `Purga()` o `SetActive(false)`).
- [ ] **Highlights sección comida** — hacer lo mismo que en la sección de crear bacteria: Canvas override sorting (sortingOrder=999) en cada campo de CartaComidaUI y CartaSpawnerUI para que se vean por encima del overlay oscuro. Exponer RectTransforms necesarios desde esos scripts y conectarlos a los pasos 11b y 11c del TutorialManager con `obtenerTarget`.
- [ ] Revisar que los pasos interactivos (10b, 11b, 11c) funcionen bien en la práctica
- [ ] El paso de "crear bacteria" puede ser confuso — quizás mostrar primero que el laboratorio está abierto antes de pedir que coloquen una
- [ ] Considerar añadir flechas o highlights visuales apuntando a los botones mientras se explica cada herramienta
- [ ] El texto de algunos pasos puede ser demasiado largo para el panel — revisar tamaño y scroll si hace falta
- [ ] Cuando termina el tutorial y carga GamePlay, empieza completamente vacío — considerar si poner 1-2 bacterias de ejemplo
- [ ] El paso de inspeccionar bacteria dura 5 segundos fijos — quizás hacer que el jugador pueda pulsar "Siguiente" en lugar de esperar
- [ ] Revisar que la BacteriaTutorial tenga un color visible (ahora tiene colorLinaje = negro/transparente por defecto)

### GENERAL
- [ ] Comentar el código para la defensa (prioridad: GestorGuardado, GestorLinajes, SistemaVida, GestorEntorno, TutorialManager)
- [ ] El botón "Ajustes" del menú principal no hace nada todavía
- [ ] Probar build completo con amigos en itch.io y recopilar feedback
- [ ] Revisar si la placa Petri se ve bien con el LineRenderer (color azul claro añadido)
- [ ] Añadir icono/logo al juego en Player Settings para el .exe

### SONIDOS — Pendiente añadir
- [ ] Crear un SoundManager (similar al AudioManager, DontDestroyOnLoad) que gestione los SFX
- [ ] Sonido al reproducirse una bacteria
- [ ] Sonido al morir una bacteria
- [ ] Sonido al colocar comida (pincel)
- [ ] Sonido al colocar un spawner
- [ ] Sonido al colocar una bacteria (sintetizar/poner en placa)
- [ ] Sonido al abrir/cerrar el laboratorio
- [ ] Sonido al guardar partida (éxito)
- [ ] Sonido al cambiar velocidad (T — 0x/1x/5x)
- [ ] Sonido de click genérico para botones del menú
- [ ] El usuario tiene que añadir los archivos de audio SFX a Assets/Resources/SFX/ (o similar)

### IDEAS OPCIONALES
- [ ] Indicador visual de velocidad actual (0x/1x/5x) siempre visible
- [ ] Botón "Volver al centro" también en GamePlay fuera del menú de pausa
- [ ] Tooltip al hover de cada botón de la barra lateral explicando qué hace
- [ ] Contador de bacteria vivas visible en HUD
- [ ] Mini-mapa o vista general de la placa

---

## Estructura de carpetas clave
```
Assets/
  _Scripts/
    Bacteria/         — BacteriasMuertas, SistemaVida, MovimientoAleatorio, SensorBacteria, DatosGenéticos, GestorLinajes
    Guardado/         — GestorGuardado, ControladorMenuPausa, ControladorMenuPrincipal, GestorEscenas, SaveData
    ModulosDeControl/ — GestorEntorno, PoolComida, CamaraControlador, ControladorInteraccion, ...
    UI/               — TutorialManager, ControladorTiempo, AudioManager, SalirDelJuego, BotonVolverAlMenu,
                        CartaEspecieUI, CartaComidaUI, CartaSpawnerUI, BandejaEspeciesUI, ...
  Scenes/             — MenuPrincipal, GamePlay, Tutorial
  Resources/
    Audio/            — 5 canciones synthwave/vaporwave (.mp3)
    musicOn.png, musicOff.png, gear.png
  ProyectoInfo/       — este archivo
```

---

## Para retomar
Pega este archivo al inicio de la conversación y di en qué quieres trabajar.
El asistente puede leer todos los scripts del proyecto para ponerse al día.
