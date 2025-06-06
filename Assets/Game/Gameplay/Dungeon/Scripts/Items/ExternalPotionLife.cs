using UnityEngine;

public class ExternalPotionLife : MonoBehaviour
{
  [Header("Configuración de la Poción de Vida")]
  [Tooltip("Cantidad de vida que la poción restaurará al jugador.")]
  public int lifeRestored = 16;
  [Tooltip("Tiempo en segundos que la poción estará activa.")]
  public float potionDuration = 20.0f;
  [SerializeField] private AudioEvent pickUpSound = null;

  void OnTriggerEnter(Collider other)
  {
    if (!other.CompareTag("Player")) return;
    PlayerController playerController = other.GetComponent<PlayerController>();
    if (playerController == null) return;
    playerController.ConsumeExternalPotionLife(lifeRestored);
    GameManager.Instance.AudioManager.PlayAudio(pickUpSound);
    Destroy(gameObject);
  }

  void Update()
  {
    if (potionDuration > 0) potionDuration -= Time.deltaTime;
    else Destroy(gameObject);
  }
}
