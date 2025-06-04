using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
//using UnityEngine.Rendering.PostProcessing; // Importante para Post Processing

public class Portal : MonoBehaviour
{
  [Header("Configuración del Portal")]
  [Tooltip("La nueva posición a la que el jugador será teletransportado.")]
  public Transform destinationPoint;
  [Tooltip("Tiempo que dura la transición a través del portal en segundos.")]
  public float transitionDuration = 2.0f;
  [Tooltip("La capa del jugador. Asegúrate de que tu jugador está en esta capa.")]
  public LayerMask playerLayer;

  [Header("Configuración de Daño")]
  [Tooltip("Cantidad de daño que el jugador recibirá al atravesar el portal.")]
  public int damageAmount = 8;
  [Tooltip("Tiempo de espera antes de aplicar el daño al jugador después de la transición.")]
  public float damageDelay = 0.75f;

  public Volume PortalVolume;

  //[Header("Efectos de Post-Procesado")]
  //[Tooltip("El perfil de Post-Procesado a aplicar durante la transición.")]
  //public PostProcessProfile portalEffectProfile;
  //[Tooltip("El PostProcessVolume global de tu cámara principal.")]
  //public PostProcessVolume cameraPostProcessVolume;

  private bool isTransitioning = false; // Bandera para evitar múltiples activaciones

  private void Start()
  {
    PortalVolume = GameObject.Find("PortalPostProcessingPlayerVolume").GetComponent<Volume>();
  }

  void OnTriggerEnter(Collider other)
  {
    // Verifica si el objeto que entró es el jugador y si no estamos ya en transición
    if (((1 << other.gameObject.layer) & playerLayer) != 0 && !isTransitioning)
    {
      // Opcional: Desactiva el collider del jugador para evitar que re-entre durante la transición
      // Esto dependerá de cómo quieras que se comporte tu jugador durante el viaje.
      // if (other.GetComponent<Collider>() != null) other.GetComponent<Collider>().enabled = false;

      StartCoroutine(PortalTransition(other.transform));
    }
  }

  IEnumerator PortalTransition(Transform playerTransform)
  {
    isTransitioning = true;
    PortalVolume.enabled = true; // Activa el volumen de post-procesado del portal

    // Guarda el perfil original para restaurarlo después
    //PostProcessProfile originalProfile = cameraPostProcessVolume.profile;

    // Aplica el perfil de post-procesado del portal
    //cameraPostProcessVolume.profile = portalEffectProfile;

    // Bloquea el movimiento del jugador (opcional, dependiendo de tu controlador)
    // Esto asume que tienes un script de controlador de jugador que puedes deshabilitar
    // o un Rigidbody al que puedes establecer kinematic.
    // Ejemplo:
    // PlayerController playerController = playerTransform.GetComponent<PlayerController>();
    // if (playerController != null) playerController.enabled = false;
    // Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
    // if (playerRb != null) playerRb.isKinematic = true;

    // Espera durante la duración de la transición
    yield return new WaitForSeconds(transitionDuration / 2);

    // Teletransporta al jugador
    if (destinationPoint != null)
    {
      playerTransform.position = destinationPoint.position;
      // Opcional: Ajustar la rotación del jugador a la del punto de destino
      // playerTransform.rotation = destinationPoint.rotation;
    }
    else
    {
      Debug.LogWarning("¡Punto de destino del portal no asignado! El jugador no será teletransportado.");
    }

    // Restaura el perfil de post-procesado original de la cámara
    //cameraPostProcessVolume.profile = originalProfile;

    // Vuelve a habilitar el movimiento del jugador y el collider (si los deshabilitaste)
    // if (playerController != null) playerController.enabled = true;
    // if (playerRb != null) playerRb.isKinematic = false;
    // if (playerTransform.GetComponent<Collider>() != null) playerTransform.GetComponent<Collider>().enabled = true;

    isTransitioning = false;

    yield return new WaitForSeconds(transitionDuration / 2);
    playerTransform.GetComponent<PlayerController>().Damage(damageAmount);
    PortalVolume.enabled = false;
  }

  IEnumerator ApplyDamage(Transform playerTransform)
  {
    yield return new WaitForSeconds(transitionDuration);
    playerTransform.GetComponent<PlayerController>().Damage(14);
  }

  void OnDrawGizmos()
  {
    // Dibujar un gizmo para el punto de destino para facilitar la configuración en el editor
    if (destinationPoint != null)
    {
      Gizmos.color = Color.blue;
      Gizmos.DrawSphere(destinationPoint.position, 0.5f);
      Gizmos.DrawLine(transform.position, destinationPoint.position);
    }
    else
    {
      Gizmos.color = Color.red;
      Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one);
      UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, "DESTINO NO ASIGNADO");
    }

    // Dibujar el collider del trigger del portal
    Gizmos.color = Color.yellow;
    Collider portalCollider = GetComponent<Collider>();
    if (portalCollider != null && portalCollider.isTrigger)
    {
      if (portalCollider is BoxCollider box)
      {
        Gizmos.DrawWireCube(transform.position + box.center, box.size);
      }
      else if (portalCollider is SphereCollider sphere)
      {
        Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
      }
      // Agrega más tipos de collider si los usas
    }
  }
}