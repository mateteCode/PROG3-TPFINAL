// Archivo: Assets/Scripts/MirroredCameraControl.cs

using UnityEngine;

public class MirroredCameraControl : MonoBehaviour
{
  [Header("Referencias de Cámaras y Puntos")]
  [Tooltip("La cámara principal de la escena (normalmente la del jugador).")]
  public Camera mainCamera;

  [Tooltip("Un punto de referencia en el mundo asociado a la ubicación de la cámara principal. " +
           "La posición relativa de la cámara principal a este punto se usará para el cálculo.")]
  public Transform mainCameraReferencePoint;

  [Tooltip("Un punto de referencia en el mundo que servirá como ancla para la posición de esta cámara secundaria. " +
           "La posición relativa calculada se aplicará a este punto.")]
  public Transform secondaryCameraReferencePoint;

  private bool initializedForActivation = false; // Flag para saber si los parámetros de espejo están listos

  void Awake()
  {
    //Debug.Log($"<color=orange>MirroredCameraControl:</color> Awake called for {gameObject.name}. (Tiempo: {Time.time})");

    // Intentamos encontrar la cámara principal si no se asigna manualmente.
    // Solo para depuración en Awake, la verificación real se hace en InitializeMirroring().
    if (mainCamera == null)
    {
      mainCamera = Camera.main;
      if (mainCamera == null)
      {
        Debug.LogError("<color=red>MirroredCameraControl:</color> ERROR en Awake: mainCamera es NULL. Asegúrate de que una cámara tenga el tag 'MainCamera'.");
      }
    }
    else
    {
      //Debug.Log("<color=green>MirroredCameraControl:</color> mainCamera ya estaba asignada en Awake.");
    }

    initializedForActivation = false; // Aseguramos que no se mueva hasta que InitializeMirroring() sea llamado.
    //Debug.Log("<color=orange>MirroredCameraControl:</color> Awake completado. Script esperando InitializeMirroring().");
    InitializeMirroring();
  }

  /// <summary>
  /// Método para inicializar el control de espejo.
  /// DEBE ser llamado por el script que activa esta cámara secundaria (ej. CameraVisionEffect).
  /// </summary>
  public void InitializeMirroring()
  {
    //Debug.Log($"<color=cyan>MirroredCameraControl:</color> InitializeMirroring() ha sido llamado. (Tiempo: {Time.time})");

    // --- Verificación Crucial de Referencias ---
    bool referencesValid = true;
    if (mainCamera == null)
    {
      Debug.LogError("<color=red>MirroredCameraControl:</color> FALLO en InitializeMirroring: 'mainCamera' es NULL. ¡Asígnala!");
      referencesValid = false;
    }
    if (mainCameraReferencePoint == null)
    {
      Debug.LogError("<color=red>MirroredCameraControl:</color> FALLO en InitializeMirroring: 'mainCameraReferencePoint' es NULL. ¡Asígnalo!");
      referencesValid = false;
    }
    if (secondaryCameraReferencePoint == null)
    {
      Debug.LogError("<color=red>MirroredCameraControl:</color> FALLO en InitializeMirroring: 'secondaryCameraReferencePoint' es NULL. ¡Asígnalo!");
      referencesValid = false;
    }

    if (!referencesValid)
    {
      initializedForActivation = false; // No se puede inicializar si faltan referencias
      Debug.LogError("<color=red>MirroredCameraControl:</color> Inicialización fallida debido a referencias nulas. La cámara NO SE MOVERÁ.");
      return;
    }

    initializedForActivation = true; // Si todas las referencias son válidas, la cámara está lista para espejar.
    //Debug.Log("<color=lime>MirroredCameraControl:</color> ¡Inicialización de espejado EXITOSA! (initializedForActivation = TRUE)");

    // Establece la posición y rotación inicial de la cámara secundaria inmediatamente
    // para que no haya un frame donde esté en (0,0,0) antes del primer LateUpdate.
    transform.rotation = mainCamera.transform.rotation;
    Vector3 relativePositionFromMainRef = mainCamera.transform.position - mainCameraReferencePoint.position;
    transform.position = secondaryCameraReferencePoint.position + relativePositionFromMainRef;
    //Debug.Log($"<color=lime>MirroredCameraControl:</color> Posición inicial secundaria establecida en: {transform.position}. Rotación: {transform.rotation.eulerAngles}");
  }

  void LateUpdate()
  {
    // Esto se ejecuta cada frame que el GameObject está activo y el script está habilitado.
    // ¡Descomentar esta línea SOLO para ver si LateUpdate se invoca! CUIDADO, es mucho spam.
    // Debug.Log("<color=blue>MirroredCameraControl:</color> LateUpdate running."); 

    // --- Condición para Mover la Cámara ---
    if (!initializedForActivation)
    {
      // Debug.Log("<color=red>MirroredCameraControl:</color> LateUpdate saltado: initializedForActivation es FALSE."); // Si esto se imprime, el problema es que InitializeMirroring no se llamó o falló.
      return;
    }

    // Si initializedForActivation es TRUE, pero las referencias se volvieron nulas después (poco probable, pero posible).
    if (mainCamera == null || mainCameraReferencePoint == null || secondaryCameraReferencePoint == null)
    {
      Debug.LogError("<color=red>MirroredCameraControl:</color> ERROR crítico en LateUpdate: Referencias nulas DESPUÉS de inicialización. Esto NO debería pasar. Desactivando movimiento.");
      initializedForActivation = false; // Desactivar el movimiento para evitar más errores
      return;
    }

    // --- Lógica de Movimiento y Rotación ---
    // 1. Mantener la misma orientación (rotación) que la cámara principal
    transform.rotation = mainCamera.transform.rotation;

    // 2. Calcular la posición relativa de la cámara principal con respecto a su punto de referencia
    Vector3 relativePositionFromMainRef = mainCamera.transform.position - mainCameraReferencePoint.position;

    // 3. Aplicar este mismo desplazamiento al punto de referencia de la cámara secundaria.
    Vector3 newSecondaryPosition = secondaryCameraReferencePoint.position + relativePositionFromMainRef;
    transform.position = newSecondaryPosition;

    // --- Debugging Logs para ver los valores exactos ---
    // Descomentar estas líneas SOLO para depurar y ver los valores en la consola.
    // Comentarlas de nuevo una vez que la depuración haya terminado.
    /*
    Debug.Log($"<color=grey>Main Cam Pos:</color> {mainCamera.transform.position} | <color=grey>Main Ref Pos:</color> {mainCameraReferencePoint.position} | <color=grey>Relative Pos from Main Ref:</color> {relativePositionFromMainRef}");
    Debug.Log($"<color=grey>Secondary Ref Pos:</color> {secondaryCameraReferencePoint.position} | <color=purple>Calculated Secondary Pos:</color> {newSecondaryPosition}");
    Debug.Log($"<color=purple>Current Secondary Cam Pos:</color> {transform.position} | <color=purple>Current Rot:</color> {transform.rotation.eulerAngles}");
    */
  }

  // Opcional: OnDrawGizmos para depuración visual en la escena.
  void OnDrawGizmos()
  {
    // Dibujar puntos de referencia (siempre visibles en el editor si existen)
    if (mainCameraReferencePoint != null)
    {
      Gizmos.color = Color.red;
      Gizmos.DrawSphere(mainCameraReferencePoint.position, 0.4f);
      Gizmos.DrawWireSphere(mainCameraReferencePoint.position, 0.5f);
      Gizmos.DrawIcon(mainCameraReferencePoint.position + Vector3.up * 0.8f, "SettingsIcon.png", true); // Usamos un icono genérico
    }
    if (secondaryCameraReferencePoint != null)
    {
      Gizmos.color = Color.blue;
      Gizmos.DrawSphere(secondaryCameraReferencePoint.position, 0.4f);
      Gizmos.DrawWireSphere(secondaryCameraReferencePoint.position, 0.5f);
      Gizmos.DrawIcon(secondaryCameraReferencePoint.position + Vector3.up * 0.8f, "Camera Icon.png", true);
    }

    // Dibujar líneas de relación solo si el script está en ejecución y listo para moverse
    // o si estamos en el editor y los puntos de referencia existen.
    if (Application.isPlaying)
    {
      if (initializedForActivation && mainCamera != null && mainCameraReferencePoint != null && secondaryCameraReferencePoint != null)
      {
        // Línea entre la cámara principal y su referencia
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(mainCamera.transform.position, mainCameraReferencePoint.position);

        // Línea entre la cámara secundaria y su referencia
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, secondaryCameraReferencePoint.position);

        // Línea que conecta las dos referencias para visualizar su desplazamiento base
        Gizmos.color = Color.green;
        Gizmos.DrawLine(mainCameraReferencePoint.position, secondaryCameraReferencePoint.position);
      }
    }
    else // En el editor (no en Play Mode), dibujar una línea básica entre las referencias si existen
    {
      if (mainCameraReferencePoint != null && secondaryCameraReferencePoint != null)
      {
        Gizmos.color = Color.grey;
        Gizmos.DrawLine(mainCameraReferencePoint.position, secondaryCameraReferencePoint.position);
      }
    }
  }
}