using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] int width = 100;
    [SerializeField] int height = 100;
    [SerializeField] SpriteRenderer mapRenderer;

    // Colores para pintar (Configurables desde el Inspector si quisieras)
    Color colorEmpty = Color.black; // O transparente
    Color colorSand = new Color(0.9f, 0.8f, 0.5f); // Amarillo arena
    Color colorWater = new Color(0.2f, 0.4f, 1f);  // Azul agua
    Color colorStone = Color.gray;

    Cell[,] grid;
    Texture2D mapTexture;

    // Temporizador para la simulación (para que no vaya demasiado rápido si quieres)
    float timer;
    float tickRate = 0.05f; // Actualiza la física cada 0.05 segundos

    void Start()
    {
        GenerateGrid();
        InitializeTexture();
    }

    void Update()
    {
        // 1. INPUT: Pintar con el ratón
        HandleInput();

        // 2. FÍSICA: Ejecutar la simulación
        // Usamos un acumulador de tiempo para controlar la velocidad
        timer += Time.deltaTime;
        if (timer >= tickRate)
        {
            Simulate();
            UpdateMapVisuals(); // Solo repintamos cuando la simulación cambia
            timer = 0f;
        }
    }

    void HandleInput()
    {
        // Click Izquierdo (0) -> Pinta ARENA
        if (Input.GetMouseButton(0))
        {
            PaintCell(ElementType.Sand);
        }
        // Click Derecho (1) -> Pinta PIEDRA (Muro)
        else if (Input.GetMouseButton(1))
        {
            PaintCell(ElementType.Stone);
        }
        // Click Rueda (2) -> Borra (Pone EMPTY)
        else if (Input.GetMouseButton(2))
        {
            PaintCell(ElementType.Empty);
        }
    }

    void PaintCell(ElementType type)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x = Mathf.FloorToInt(worldPos.x);
        int y = Mathf.FloorToInt(worldPos.y);

        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            // PINCEL: Pintamos un radio de 3 para que sea más fácil ver
            int brushSize = 2;
            for (int i = -brushSize; i <= brushSize; i++)
            {
                for (int j = -brushSize; j <= brushSize; j++)
                {
                    int px = x + i;
                    int py = y + j;
                    // Comprobamos límites otra vez para el pincel
                    if (px >= 0 && px < width && py >= 0 && py < height)
                    {
                        // Solo pintamos si está vacío o si estamos borrando
                        // Esto evita que borres piedra con arena accidentalmente
                        if (grid[px, py].Type == ElementType.Empty || type == ElementType.Empty || type == ElementType.Stone)
                        {
                            grid[px, py].Type = type;
                        }
                    }
                }
            }
        }
    }

    // --- EL CORAZÓN DE LA BESTIA: LA SIMULACIÓN ---
    void Simulate()
    {
        // Recorremos el mapa para aplicar reglas.
        // TRUCO IMPORTANTE: Para simular gravedad (cosas que caen),
        // es mejor recorrer de ABAJO hacia ARRIBA (y = 0 a y = height).
        // ¿Por qué? Si recorres de arriba abajo, un grano de arena podría teletransportarse
        // hasta el suelo en un solo frame.

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];

                // REGLAS DE LA ARENA
                if (cell.Type == ElementType.Sand)
                {
                    // Miramos abajo (y - 1)
                    if (y > 0) // Solo si no estamos en el suelo del todo
                    {
                        Cell cellBelow = grid[x, y - 1];

                        // Si abajo hay VACÍO o AGUA, la arena cae
                        if (cellBelow.Type == ElementType.Empty || cellBelow.Type == ElementType.Water)
                        {
                            // INTERCAMBIO: La arena baja, lo de abajo sube
                            ElementType typeBelow = cellBelow.Type;

                            cellBelow.Type = ElementType.Sand; // Abajo ponemos arena
                            cell.Type = typeBelow;             // Arriba ponemos lo que había (vacío o agua)
                        }
                        // EXTRA: Física de resbalar (hace montañitas)
                        // Si abajo está bloqueado, intenta caer en diagonal izquierda o derecha
                        else
                        {
                            int direction = Random.Range(0, 2) == 0 ? -1 : 1; // Izquierda o Derecha al azar
                            int diagX = x + direction;

                            if (diagX >= 0 && diagX < width)
                            {
                                Cell cellDiag = grid[diagX, y - 1];
                                if (cellDiag.Type == ElementType.Empty)
                                {
                                    cellDiag.Type = ElementType.Sand;
                                    cell.Type = ElementType.Empty;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void UpdateMapVisuals()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];
                Color pixelColor = colorEmpty;

                switch (cell.Type)
                {
                    case ElementType.Sand: pixelColor = colorSand; break;
                    case ElementType.Water: pixelColor = colorWater; break;
                    case ElementType.Stone: pixelColor = colorStone; break;
                    case ElementType.Empty: pixelColor = colorEmpty; break;
                }
                mapTexture.SetPixel(x, y, pixelColor);
            }
        }
        mapTexture.Apply();
    }

    // --- SETUP INICIAL (Casi igual que antes) ---
    void GenerateGrid()
    {
        grid = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Cell(x, y);
            }
        }
    }

    void InitializeTexture()
    {
        mapTexture = new Texture2D(width, height);
        mapTexture.filterMode = FilterMode.Point;
        Rect rect = new Rect(0, 0, width, height);
        mapRenderer.sprite = Sprite.Create(mapTexture, rect, Vector2.zero, 1f); // Pivote (0,0)
    }
}