using UnityEngine;

public class ZonaSpawnerComida : MonoBehaviour
{
    [Header("Configuración del Oasis")]
    [Tooltip("Radio del círculo donde aparecerá la comida")]
    public float radioSpawneo = 10f;

    [Tooltip("Cada cuántos segundos llueve comida")]
    public float tiempoEntreSpawns = 2f;

    [Tooltip("Cuántos trozos de comida caen de golpe")]
    public int cantidadPorSpawn = 1;

    [Tooltip("El tamaño de la comida que se genera")]
    public float tamanoComida = 0.5f;

    private float _temporizador;

    // Esto dibuja una esfera verde transparente en la escena de Unity
    // para que sepas exactamente qué tamaño tiene este spawner sin tener que adivinar.
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
        Gizmos.DrawSphere(transform.position, radioSpawneo);
    }

    private void Update()
    {
        // El temporizador avanza con el tiempo del juego (se pausa si pausas el juego)
        _temporizador += Time.deltaTime;

        if (_temporizador >= tiempoEntreSpawns)
        {
            _temporizador = 0f;
            GenerarComida();
        }
    }

    private void GenerarComida()
    {
        if (PoolComida.Instance == null) return;

        for (int i = 0; i < cantidadPorSpawn; i++)
        {
            // 1. Elegimos un punto aleatorio dentro de nuestro círculo
            Vector2 puntoAleatorio = (Vector2)transform.position + Random.insideUnitCircle * radioSpawneo;

            // 2. Comprobamos que el punto no se haya salido de los muros del mapa
            if (GestorEntorno.Instance != null)
            {
                //float mitadAncho = GestorEntorno.Instance.dimensionesMapa.x / 2f;
                //float mitadAlto = GestorEntorno.Instance.dimensionesMapa.y / 2f;

                // El "Clamp" fuerza a la variable a quedarse dentro de un mínimo y un máximo.
                // Si la comida iba a caer en X = 60, pero el muro está en 50, la deja en 50.
                //puntoAleatorio.x = Mathf.Clamp(puntoAleatorio.x, -mitadAncho, mitadAncho);
                //puntoAleatorio.y = Mathf.Clamp(puntoAleatorio.y, -mitadAlto, mitadAlto);
            }

            // 3. Pedimos al Pool que coloque la comida en ese punto seguro
            PoolComida.Instance.GetComida(puntoAleatorio, tamanoComida);
        }
    }
}