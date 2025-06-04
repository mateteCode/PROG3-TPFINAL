using UnityEngine;
using System.Collections;

public class CameraVisionEffect : MonoBehaviour
{
  [Header("Configuración de la Cámara de Visión")]
  [Tooltip("El Tag de la cámara de visión (p.ej. 'VisionCamera'). Esta cámara debe estar en una zona oculta de la misma escena.")]
  public string visionCameraTag = "VisionCamera";

  [Header("Configuración del Efecto de Visión")]
  [Tooltip("Duración total del efecto de visión en segundos.")]
  public float effectDuration = 2.0f;
  [Tooltip("Intervalo entre cada 'parpadeo' o alteración de la cámara.")]
  public float flickerInterval = 0.1f;
  [Tooltip("Magnitud máxima de la alteración aleatoria de la posición de la cámara.")]
  public float maxPositionShake = 0.5f;
  [Tooltip("Magnitud máxima de la alteración aleatoria de la rotación de la cámara (en grados).")]
  public float maxRotationShake = 5.0f;

  private Camera mainCamera; // La cámara principal de la escena (la del jugador)
  private Camera visionCamera; // La cámara en la zona oculta de la misma escena
  private bool effectActive = false; // Bandera para evitar que el efecto se active múltiples veces

  void Awake()
  {
    // Busca la cámara principal (debe tener el tag "MainCamera")
    mainCamera = Camera.main;
    if (mainCamera == null)
    {
      Debug.LogError("CameraVisionEffect: No se encontró la cámara principal (con el tag 'MainCamera'). Asegúrate de tener una.");
      enabled = false; // Desactiva este script si no hay cámara principal
      return;
    }

    // Busca la cámara de visión en la escena actual usando su Tag
    GameObject visionCamGO = GameObject.FindWithTag(visionCameraTag);
    if (visionCamGO != null)
    {
      visionCamera = visionCamGO.GetComponent<Camera>();
      if (visionCamera != null)
      {
        visionCamera.gameObject.SetActive(false); // ¡Asegúrate de que la cámara de visión esté DESACTIVADA al inicio!
        Debug.Log("CameraVisionEffect: Cámara de visión encontrada y lista.");
      }
      else
      {
        Debug.LogError($"CameraVisionEffect: El GameObject con el Tag '{visionCameraTag}' no tiene un componente Camera.");
        enabled = false;
      }
    }
    else
    {
      Debug.LogError($"CameraVisionEffect: No se encontró ningún GameObject con el Tag '{visionCameraTag}' en la escena actual. Asegúrate de que existe y tiene el Tag correcto.");
      enabled = false;
    }
  }


  /// Activa el efecto de visión. Este método es llamado por scripts externos (como DoorTriggerEffect).
  public void TriggerVisionEffect()
  {
    // Evita activar el efecto si ya está activo o si las cámaras no están configuradas
    if (effectActive || visionCamera == null || mainCamera == null)
    {
      Debug.LogWarning("CameraVisionEffect: El efecto ya está activo o las cámaras no están listas.");
      return;
    }
    StartCoroutine(VisionEffectRoutine());
  }

  private IEnumerator VisionEffectRoutine()
  {
    effectActive = true; // Marca el efecto como activo
    visionCamera.gameObject.SetActive(true); // Activa la cámara de visión
    mainCamera.gameObject.SetActive(false); // Desactiva la cámara principal (cambia la vista del jugador)

    float timer = 0f;
    while (timer < effectDuration) // Bucle principal del efecto
    {
      // Sincroniza la posición y rotación de la cámara de visión con la principal
      // Esto asegura que la visión siempre se base en la perspectiva actual del jugador
      visionCamera.transform.position = mainCamera.transform.position;
      visionCamera.transform.rotation = mainCamera.transform.rotation;

      // Aplica offsets aleatorios para simular la distorsión de la visión
      Vector3 randomPosOffset = new Vector3(
          Random.Range(-maxPositionShake, maxPositionShake),
          Random.Range(-maxPositionShake, maxPositionShake),
          Random.Range(-maxPositionShake, maxPositionShake)
      );
      Quaternion randomRotOffset = Quaternion.Euler(
          Random.Range(-maxRotationShake, maxRotationShake),
          Random.Range(-maxRotationShake, maxRotationShake),
          Random.Range(-maxRotationShake, maxRotationShake)
      );

      visionCamera.transform.position += randomPosOffset; // Añade el offset de posición
      visionCamera.transform.rotation *= randomRotOffset; // Multiplica para aplicar el offset de rotación

      yield return new WaitForSeconds(flickerInterval); // Espera antes del siguiente "parpadeo"
      timer += flickerInterval; // Incrementa el temporizador
    }

    // Al finalizar el efecto
    visionCamera.gameObject.SetActive(false); // Desactiva la cámara de visión
    mainCamera.gameObject.SetActive(true); // Reactiva la cámara principal (devuelve la vista normal al jugador)
    effectActive = false; // Marca el efecto como inactivo
    Debug.Log("CameraVisionEffect: Efecto de visión finalizado.");
  }
}