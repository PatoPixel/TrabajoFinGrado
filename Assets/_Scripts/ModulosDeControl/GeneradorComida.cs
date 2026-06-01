using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SpawnerComida : MonoBehaviour
{
    private float _minEnergia;
    private float _maxEnergia;
    private float _intervalo;
    private float _radioSpawn;
    private float _cronometro;

    private CircleCollider2D _miColisionadorFisico;
    private LineRenderer _lineRenderer;

    [Header("Ajustes Visuales del Círculo")]
    [SerializeField] private int segmentosCirculo = 36;
    [SerializeField] private float grosorLinea = 0.05f;
    [SerializeField] private Color colorCirculo = new Color(0f, 1f, 1f, 0.4f);

    // NUEVO: Getters públicos para que el GestorGuardado pueda leer estos valores al serializar
    public float MinEnergia => _minEnergia;
    public float MaxEnergia => _maxEnergia;
    public float Intervalo => _intervalo;
    public float RadioSpawn => _radioSpawn;

    public void Inicializar(float min, float max, float intervalo, float radio)
    {
        _minEnergia = min;
        _maxEnergia = max;
        _intervalo = intervalo;
        _radioSpawn = radio;
        _cronometro = 0f;

        if (TryGetComponent(out CircleCollider2D collider))
        {
            _miColisionadorFisico = collider;
            _miColisionadorFisico.radius = _radioSpawn;
            _miColisionadorFisico.isTrigger = true;
        }

        _lineRenderer = GetComponent<LineRenderer>();
        ConfigurarVisualCirculo();
    }

    void Update()
    {
        _cronometro += Time.deltaTime;
        if (_cronometro >= _intervalo)
        {
            _cronometro = 0f;
            SpawnearComida();
        }
    }

    private void SpawnearComida()
    {
        if (PoolComida.Instance != null)
        {
            Vector2 desplazamientoAleatorio = Random.insideUnitCircle * _radioSpawn;
            Vector2 posicionFinal = (Vector2)transform.position + desplazamientoAleatorio;

            GameObject nuevaComida = PoolComida.Instance.GetComida(posicionFinal, 0.5f);
            if (nuevaComida != null && nuevaComida.TryGetComponent(out Comida scriptComida))
            {
                float energiaAleatoria = Random.Range(_minEnergia, _maxEnergia);
                scriptComida.EstablecerEnergiaManual(energiaAleatoria);
            }
        }
    }

    private void ConfigurarVisualCirculo()
    {
        if (_lineRenderer == null) return;
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.startWidth = grosorLinea;
        _lineRenderer.endWidth = grosorLinea;
        _lineRenderer.positionCount = segmentosCirculo + 1;
        _lineRenderer.loop = true;

        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = colorCirculo;
        _lineRenderer.endColor = colorCirculo;

        float deltaAngulo = (2f * Mathf.PI) / segmentosCirculo;
        float angulo = 0f;

        for (int i = 0; i <= segmentosCirculo; i++)
        {
            float x = Mathf.Cos(angulo) * _radioSpawn;
            float y = Mathf.Sin(angulo) * _radioSpawn;
            _lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
            angulo += deltaAngulo;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        Gizmos.DrawSphere(transform.position, _radioSpawn > 0.1f ? _radioSpawn : 1.5f);
    }
}