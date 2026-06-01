using UnityEngine;

// Obliga a Unity a a�adir un LineRenderer y un EdgeCollider2D 
// en cuanto le pegas el script al GameObject, es imposible que se te olviden.
[RequireComponent(typeof(EdgeCollider2D))]
[RequireComponent(typeof(LineRenderer))]
public class GestorEntorno : MonoBehaviour
{
    private static GestorEntorno _instance;
    public static GestorEntorno Instance
    {
        get
        {
            if (_instance == null) _instance = FindFirstObjectByType<GestorEntorno>();
            return _instance;
        }
    }

    [Header("Configuraci�n de la Placa de Petri")]
    [Tooltip("Radio de la placa circular. Las bacterias no podr�n salir de este radio.")]
    public float radioPlaca = 200f;

    [Tooltip("N�mero de segmentos para formar el c�rculo. A m�s segmentos, m�s perfecto es el c�rculo.")]
    [Range(30, 120)] public int segmentosCirculo = 60;

    // Los dejamos p�blicos solo para el script, pero no hace falta arrastrarlos en el inspector
    private EdgeCollider2D _muroCircular;
    private LineRenderer _lineaVisualBorde;

    private void Awake()
    {
       _instance = this;
        // Al usar [RequireComponent], esto ya nunca jam�s va a devolver null
        _muroCircular = GetComponent<EdgeCollider2D>();
        _lineaVisualBorde = GetComponent<LineRenderer>();

        GenerarPlacaPetri();
    }

    // Por si acaso Unity hace cosas raras en el editor antes de darle al Play
    private void OnValidate()
    {
        _muroCircular = GetComponent<EdgeCollider2D>();
        _lineaVisualBorde = GetComponent<LineRenderer>();
        GenerarPlacaPetri();
    }

    public void CambiarRadioPlaca(float nuevoRadio)
    {
        radioPlaca = nuevoRadio;
        GenerarPlacaPetri();
    }

    private void GenerarPlacaPetri()
    {
        if (_muroCircular == null || _lineaVisualBorde == null) return;

        Vector2[] puntosMuro = new Vector2[segmentosCirculo + 1];
        Vector3[] puntosVisuales = new Vector3[segmentosCirculo + 1];

        float pasoAngulo = (2f * Mathf.PI) / segmentosCirculo;

        for (int i = 0; i <= segmentosCirculo; i++)
        {
            float angulo = i * pasoAngulo;

            float x = Mathf.Cos(angulo) * radioPlaca;
            float y = Mathf.Sin(angulo) * radioPlaca;

            puntosMuro[i] = new Vector2(x, y);
            puntosVisuales[i] = new Vector3(x, y, 0f);
        }

        // 1. Aplicamos f�sicas (EdgeCollider)
        _muroCircular.points = puntosMuro;

        // 2. Aplicamos visuales (LineRenderer)
        _lineaVisualBorde.positionCount = puntosVisuales.Length;
        _lineaVisualBorde.SetPositions(puntosVisuales);

        _lineaVisualBorde.useWorldSpace = false; // Fuerza al dibujo a alinearse con el colisionador
        _lineaVisualBorde.loop = false; // Evita el "pico" gr�fico al sobreponerse el �ltimo y primer punto
        _lineaVisualBorde.numCapVertices = 5; // Suaviza las puntas de la l�nea
        _lineaVisualBorde.numCornerVertices = 5; // Suaviza las esquinas de la l�nea

        // Grosor uniforme y color visible
        _lineaVisualBorde.startWidth = 1.5f;
        _lineaVisualBorde.endWidth = 1.5f;
        _lineaVisualBorde.startColor = new Color(0.6f, 0.85f, 1f, 1f); // azul claro
        _lineaVisualBorde.endColor   = new Color(0.6f, 0.85f, 1f, 1f);
    }
}