using UnityEngine;

// Controla la velocidad global del juego para acelerar la simulación.
// - Método público `ToggleVelocidad` para enlazar a un botón UI.
// - También responde a la tecla T para alternar rápidamente.
// - Ajusta Time.fixedDeltaTime para que la física siga comportándose correctamente.
public class ControladorTiempo : MonoBehaviour
{
 [SerializeField] private float velocidadNormal =1f;
 [SerializeField] private float velocidadRapida =5f;

 private bool rapido = false;
 private float baseFixedDeltaTime;

 void Awake()
 {
 baseFixedDeltaTime = Time.fixedDeltaTime;
 }

 void OnEnable()
 {
 AplicarEscala(velocidadNormal);
 }

 void Update()
 {
 // Tecla rápida para alternar (útil en el editor)
 if (Input.GetKeyDown(KeyCode.T))
 {
 ToggleVelocidad();
 }
 }

 // Método público para enlazar desde un botón UI (OnClick)
 public void ToggleVelocidad()
 {
 rapido = !rapido;
 AplicarEscala(rapido ? velocidadRapida : velocidadNormal);
 }

 // Aplicar la escala de tiempo y ajustar fixedDeltaTime
 public void AplicarEscala(float escala)
 {
 Time.timeScale = escala;
 Time.fixedDeltaTime = baseFixedDeltaTime * escala;
 }

 // Opcional: restaurar al valor normal al deshabilitar el componente
 void OnDisable()
 {
 AplicarEscala(velocidadNormal);
 }
}
