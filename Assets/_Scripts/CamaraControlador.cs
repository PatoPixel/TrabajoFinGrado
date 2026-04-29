using System;
using UnityEngine;

public class CamaraControladorPro : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadTeclado = 25f;
    [SerializeField] private float sensibilidadArrastre = 5f;

    [Header("Zoom")]
    [SerializeField] private float zoomMin = 2f;
    [SerializeField] private float zoomMax = 20f;
    [SerializeField] private float sensibilidadZoom = 10f;
    [SerializeField] private float suavizadoZoom = 5f;

    [Header("Límites (Placa Petri)")]
    [SerializeField] private SpriteRenderer objetoLimite; // Arrastra aquí tu fondo/placa

    private Camera _cam;
    private float _targetZoom;
    private Vector3 _ultimaPosicionRaton;

    public static event Action OnIntervencionManual;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _targetZoom = _cam.orthographicSize;
    }

    void LateUpdate() // Usamos LateUpdate para que la cámara se mueva después que las bacterias
    {
        GestionarZoom();
        GestionarMovimiento();
        AplicarLimites();
    }

    private void GestionarZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            _targetZoom -= scroll * sensibilidadZoom;
            _targetZoom = Mathf.Clamp(_targetZoom, zoomMin, zoomMax);
        }

        // Suavizamos el cambio de zoom
        _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetZoom, Time.unscaledDeltaTime * suavizadoZoom);
    }

    private void GestionarMovimiento()
    {
        float multiplicadorZoom = _cam.orthographicSize / zoomMax;

        // --- WASD ---
        Vector3 mov = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);

        // Aplicamos el multiplicador a la velocidad del teclado
        float velocidadRealTeclado = velocidadTeclado * multiplicadorZoom;
        transform.position += mov.normalized * velocidadRealTeclado * Time.unscaledDeltaTime;

        // --- Arrastre Rueda ---
        if (Input.GetMouseButtonDown(2))
        {
            _ultimaPosicionRaton = Input.mousePosition;
        }
        else if (Input.GetMouseButton(2))
        {
            Vector3 diferencia = Input.mousePosition - _ultimaPosicionRaton;

            // Aplicamos el multiplicador a la sensibilidad del ratón
            float sensibilidadReal = sensibilidadArrastre * multiplicadorZoom;

            transform.position += new Vector3(-diferencia.x, -diferencia.y, 0) * sensibilidadReal * Time.unscaledDeltaTime;

            _ultimaPosicionRaton = Input.mousePosition;
        }

        // Si el jugador está usando el teclado o arrastrando, disparamos el evento de intervención manual
        if (mov != Vector3.zero || Input.GetMouseButton(2))
        {
            OnIntervencionManual?.Invoke();
        }
    }

    private void AplicarLimites()
    {
        if (objetoLimite == null) return;

        // 1. Obtenemos los bordes reales del objeto placa petri
        Bounds limitesPlaca = objetoLimite.bounds;

        // 2. Calculamos cuánto "ve" la cámara en vertical y horizontal
        float altoCamara = _cam.orthographicSize;
        float anchoCamara = altoCamara * _cam.aspect;

        // 3. Calculamos los límites permitidos para el CENTRO de la cámara
        // Restamos el tamaño de la cámara a los límites de la placa para que el borde no se salga
        float minX = limitesPlaca.min.x + anchoCamara;
        float maxX = limitesPlaca.max.x - anchoCamara;
        float minY = limitesPlaca.min.y + altoCamara;
        float maxY = limitesPlaca.max.y - altoCamara;

        // 4. Si la placa es más pequeña que el zoom actual, bloqueamos en el centro
        if (minX > maxX) { float centroX = limitesPlaca.center.x; minX = maxX = centroX; }
        if (minY > maxY) { float centroY = limitesPlaca.center.y; minY = maxY = centroY; }

        // 5. Aplicamos el bloqueo
        Vector3 posClamped = transform.position;
        posClamped.x = Mathf.Clamp(posClamped.x, minX, maxX);
        posClamped.y = Mathf.Clamp(posClamped.y, minY, maxY);

        transform.position = posClamped;
    }
}