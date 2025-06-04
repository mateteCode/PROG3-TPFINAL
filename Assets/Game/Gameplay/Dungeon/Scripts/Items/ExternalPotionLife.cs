using UnityEngine;

public class ExternalPotionLife : MonoBehaviour
{
  [Header("Configuración de la Poción de Vida")]
  [Tooltip("Cantidad de vida que la poción restaurará al jugador.")]
  public int lifeRestored = 16;
  [Tooltip("Tiempo en segundos que la poción estará activa.")]
  public float potionDuration = 20.0f;

  void OnTriggerEnter(Collider other)
  {
    if (!other.CompareTag("Player")) return;
    PlayerController playerController = other.GetComponent<PlayerController>();
    if (playerController == null) return;
    playerController.ConsumeExternalPotionLife(lifeRestored);
    // TODO: Sonido de recogida de poción
    Destroy(gameObject);
  }

  void Update()
  {
    if (potionDuration > 0) potionDuration -= Time.deltaTime;
    else Destroy(gameObject);
  }
}
