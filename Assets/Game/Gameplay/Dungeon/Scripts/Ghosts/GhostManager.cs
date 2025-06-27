using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System;

public class GhostManager : MonoBehaviour
{
  public static GhostManager Instance;

  [Header("Configuración del Pool")]
  [Tooltip("Prefab del fantasma. Asegúrate de que tiene un MeshRenderer y un Collider si lo necesitas para otras interacciones.")]
  public GameObject ghostPrefab;
  [Tooltip("Tamaño inicial del pool de fantasmas. No se crearán más allá de este número.")]
  public int poolSize = 10;

  [Header("Configuración de Fantasmas")]
  [Tooltip("Vida inicial de cada fantasma generado.")]
  public int ghostHealth = 100;
  [Tooltip("Velocidad de movimiento de los fantasmas.")]
  public float ghostSpeed = 2f;
  [Tooltip("Tiempo de espera entre la aparición de fantasmas.")]
  public float spawnInterval = 3f;

  [Header("Configuración de Ruta")]
  [Tooltip("GameObject padre que contiene las posibles posiciones de inicio para los fantasmas (hijos).")]
  public Transform startPositionsParent;
  [Tooltip("GameObject padre que contiene las posibles posiciones intermedias para los fantasmas (hijos).")]
  public Transform intermediatePositionsParent;
  [Tooltip("GameObject padre que contiene las posibles posiciones de destino para los fantasmas (hijos).")]
  public Transform finalPositionsParent;
  [Tooltip("El punto fijo global al que regresa el fantasma rojo antes de desaparecer.")]
  public Transform redGhostGlobalReturnPoint;
  [Tooltip("Prefab que el fantasma blanco dejará al desaparecer.")]
  public GameObject whiteGhostDropPrefab;

  // --- NUEVAS VARIABLES PARA EL ESTADO DE VICTORIA ---
  [Header("Configuración de Victoria")]
  [Tooltip("Punto de referencia desde el cual se generarán las posiciones de victoria aleatorias.")]
  public Transform victoryReferencePoint;
  [Tooltip("Radio mínimo desde el punto de referencia para las posiciones de victoria.")]
  public float minVictoryRadius = 5f;
  [Tooltip("Radio máximo desde el punto de referencia para las posiciones de victoria.")]
  public float maxVictoryRadius = 15f;
  [Tooltip("Altura del segmento Y (centrado en el punto de referencia Y) para las posiciones de victoria (ej: 1f para 1 metro).")]
  public float yVictorySegmentHeight = 1f;
  [Tooltip("Velocidad de movimiento de los fantasmas cuando están lejos de su destino de victoria.")]
  public float victoryFastSpeed = 10f; // ¡Nueva velocidad aumentada!
  [Tooltip("Velocidad de movimiento de los fantasmas cuando están cerca de su destino de victoria.")]
  public float victoryNormalSpeed = 2f; // Velocidad normal cerca del destino
  [Tooltip("Distancia en la que los fantasmas reducen su velocidad a 'normal' durante el estado de victoria.")]
  public float victorySlowDownDistance = 10f; // ¡Nueva distancia de reducción!

  [Header("Validación de Posición de Victoria")]
  [Tooltip("Distancia mínima que debe haber entre la posición de victoria generada y el jugador.")]
  public float minDistanceToPlayerForVictoryPos = 5f; // ¡NUEVO! Distancia mínima al jugador
  [Tooltip("Número máximo de intentos para generar una posición de victoria válida antes de rendirse.")]
  public int maxVictoryPosGenerationAttempts = 10; // ¡NUEVO! Intentos para evitar bucles infinitos


  private List<GameObject> ghostPool;
  private int currentGhostIndex = 0;
  private Coroutine spawnCoroutine;

  private List<Transform> allStartPoints = new List<Transform>();
  private List<Transform> allIntermediatePoints = new List<Transform>();
  private List<Transform> allFinalPoints = new List<Transform>();

  private List<GhostMovement> activeGhosts = new List<GhostMovement>();

  private int ghostsAtVictoryPositionCount = 0; // ¡Nuevo contador!
  private bool victoryDeclared = false; // Bandera para asegurar que la victoria solo se declare una vez

  private Transform playerTransform; // ¡NUEVO! Referencia al transform del jugador

  private Action onWinAnimationEnd = null;

  public void Init(Action onWinAnimationEnd)
  {
    this.onWinAnimationEnd = onWinAnimationEnd;

  }
  void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    else
    {
      Destroy(gameObject);
    }

    CachePoints();
    InitializeGhostPool();

    // Obtener la referencia al jugador
    GameObject player = GameObject.FindWithTag("Player");
    if (player != null)
    {
      playerTransform = player.transform;
    }
    else
    {
      Debug.LogWarning("GhostManager: Jugador con tag 'Player' no encontrado al inicio. Asegúrate de que tu jugador tiene el tag 'Player'.");
    }
  }

  void Start()
  {
    spawnCoroutine = StartCoroutine(SpawnGhostsRoutine());
  }

  void CachePoints()
  {
    if (startPositionsParent != null)
    {
      foreach (Transform child in startPositionsParent) { allStartPoints.Add(child); }
    }
    else { Debug.LogWarning("GhostManager: Start Positions Parent no asignado. No se generarán rutas de inicio."); }

    if (intermediatePositionsParent != null)
    {
      foreach (Transform child in intermediatePositionsParent) { allIntermediatePoints.Add(child); }
    }
    else { Debug.LogWarning("GhostManager: Intermediate Positions Parent no asignado. Las rutas serán directas."); }

    if (finalPositionsParent != null)
    {
      foreach (Transform child in finalPositionsParent) { allFinalPoints.Add(child); }
    }
    else { Debug.LogWarning("GhostManager: Final Positions Parent no asignado. No se generarán rutas de destino."); }
  }

  void InitializeGhostPool()
  {
    ghostPool = new List<GameObject>();
    for (int i = 0; i < poolSize; i++)
    {
      GameObject ghost = Instantiate(ghostPrefab, transform);
      ghost.SetActive(false);
      ghostPool.Add(ghost);
    }
  }

  GameObject GetGhostFromPool()
  {
    for (int i = 0; i < ghostPool.Count; i++)
    {
      int index = (currentGhostIndex + i) % ghostPool.Count;
      if (!ghostPool[index].activeInHierarchy)
      {
        currentGhostIndex = index;
        return ghostPool[index];
      }
    }
    return null;
  }

  IEnumerator SpawnGhostsRoutine()
  {
    while (true)
    {
      yield return new WaitForSeconds(spawnInterval);

      if (victoryDeclared) // No spawnear más fantasmas si la victoria ya fue declarada
      {
        yield break;
      }

      GameObject ghost = GetGhostFromPool();
      if (ghost != null)
      {
        ConfigureAndActivateGhost(ghost);
      }
    }
  }

  void ConfigureAndActivateGhost(GameObject ghost)
  {
    GhostType randomType = (GhostType)Random.Range(0, System.Enum.GetValues(typeof(GhostType)).Length);

    List<Vector3> route = GenerateRandomPath();
    if (route.Count == 0)
    {
      Debug.LogError("Ruta generada vacía para el fantasma " + randomType + ". No se puede configurar. Devolviendo al pool.");
      ReturnGhostToPool(ghost);
      return;
    }

    ghost.SetActive(true);

    GhostMovement ghostMovement = ghost.GetComponent<GhostMovement>();
    if (ghostMovement == null)
    {
      Debug.LogWarning("El fantasma " + ghost.name + " no tiene un componente GhostMovement. Añadiéndolo...");
      ghostMovement = ghost.AddComponent<GhostMovement>();
    }

    ghostMovement.SetMovementParameters(randomType, route, ghostSpeed, this, whiteGhostDropPrefab, redGhostGlobalReturnPoint);
    ghostMovement.Health = ghostHealth;

    if (!activeGhosts.Contains(ghostMovement))
    {
      activeGhosts.Add(ghostMovement);
    }
  }

  List<Vector3> GenerateRandomPath()
  {
    List<Vector3> path = new List<Vector3>();

    if (allStartPoints.Count == 0 || allFinalPoints.Count == 0)
    {
      Debug.LogError("No hay suficientes puntos de inicio o destino configurados para generar una ruta.");
      return path;
    }

    Vector3 startPoint = allStartPoints[Random.Range(0, allStartPoints.Count)].position;
    path.Add(startPoint);

    Vector3 finalPoint = allFinalPoints[Random.Range(0, allFinalPoints.Count)].position;

    int numIntermediatePoints = Random.Range(0, allIntermediatePoints.Count + 1);

    List<Transform> sortedIntermediatePoints = new List<Transform>(allIntermediatePoints);
    sortedIntermediatePoints.Sort((a, b) =>
        Vector3.Distance(a.position, finalPoint).CompareTo(Vector3.Distance(b.position, finalPoint)));

    for (int i = 0; i < numIntermediatePoints && i < sortedIntermediatePoints.Count; i++)
    {
      path.Add(sortedIntermediatePoints[i].position);
    }

    path.Add(finalPoint);

    return path;
  }

  public void ReturnGhostToPool(GameObject ghost)
  {
    ghost.SetActive(false);
    GhostMovement ghostMovement = ghost.GetComponent<GhostMovement>();
    if (ghostMovement != null && activeGhosts.Contains(ghostMovement))
    {
      activeGhosts.Remove(ghostMovement);

      // Si un fantasma regresa al pool después de la victoria, y no estaba en el estado Idle,
      // podría significar que murió antes de llegar. Debemos considerar esto para el contador.
      // O solo nos importan los que llegaron VIVOS a la victoria.
      // Para este caso, solo incrementamos el contador si el fantasma llegó al destino de victoria.
    }
  }

  // --- NUEVO MÉTODO: Notificación de fantasma llegado a la posición de victoria ---
  public void GhostReachedVictoryPosition()
  {
    ghostsAtVictoryPositionCount++;
    Debug.Log($"Fantasma llegó a la posición de victoria. Total llegados: {ghostsAtVictoryPositionCount} de {activeGhosts.Count}");

    if (victoryDeclared && ghostsAtVictoryPositionCount >= activeGhosts.Count)
    {
      Debug.Log("¡Todos los fantasmas han llegado a su posición de victoria! Disparando evento.");
      onWinAnimationEnd?.Invoke();
    }
  }

  void OnDisable()
  {
    if (spawnCoroutine != null)
    {
      StopCoroutine(spawnCoroutine);
    }
  }

  public void DeclareVictory()
  {
    if (victoryDeclared) return; // Evitar llamar múltiples veces
    victoryDeclared = true;

    if (victoryReferencePoint == null)
    {
      Debug.LogError("GhostManager: ¡El punto de referencia de victoria (Victory Reference Point) no está asignado en el editor! No se puede ejecutar la función de victoria.");
      return;
    }

    StopAllCoroutines(); // Detener el spawn de nuevos fantasmas

    // Reiniciar el contador de fantasmas en posición de victoria
    ghostsAtVictoryPositionCount = 0;

    // Crear una copia temporal de la lista para evitar errores si la lista cambia durante la iteración
    List<GhostMovement> ghostsToProcess = new List<GhostMovement>(activeGhosts);

    if (ghostsToProcess.Count == 0)
    {
      Debug.LogWarning("No hay fantasmas activos para procesar en el estado de victoria.");
      onWinAnimationEnd?.Invoke(); // Si no hay fantasmas, la animación de victoria termina inmediatamente.
      return;
    }


    foreach (GhostMovement ghost in ghostsToProcess)
    {
      if (ghost != null && ghost.gameObject.activeInHierarchy)
      {
        Vector3 destination = GenerateRandomVictoryPosition();
        // Pasar las nuevas velocidades y la distancia de desaceleración al fantasma
        ghost.SetVictoryDestination(destination, victoryFastSpeed, victoryNormalSpeed, victorySlowDownDistance);
      }
      else
      {
        // Si un fantasma ya no está activo (ej. fue destruido antes de la victoria),
        // incrementamos el contador para que no impida la notificación final.
        ghostsAtVictoryPositionCount++;
      }
    }
    Debug.Log($"Victoria declarada! {activeGhosts.Count} fantasmas se dirigen a sus posiciones finales.");
  }

  // --- FUNCIÓN MODIFICADA: GenerateRandomVictoryPosition ---
  private Vector3 GenerateRandomVictoryPosition()
  {
    Vector3 generatedPosition = Vector3.zero;
    bool isValidPosition = false;
    int attempts = 0;

    while (!isValidPosition && attempts < maxVictoryPosGenerationAttempts) //
    {
      // Generar un punto aleatorio en un círculo (plano XZ)
      Vector2 randomCirclePoint = Random.insideUnitCircle.normalized * Random.Range(minVictoryRadius, maxVictoryRadius); //

      float x = victoryReferencePoint.position.x + randomCirclePoint.x; //
      float z = victoryReferencePoint.position.z + randomCirclePoint.y; // Usamos 'y' del Vector2 para el eje Z

      // Generar un punto aleatorio en el segmento Y
      float yMin = victoryReferencePoint.position.y - (yVictorySegmentHeight / 2f); //
      float yMax = victoryReferencePoint.position.y + (yVictorySegmentHeight / 2f); //
      float y = Random.Range(yMin, yMax); //

      generatedPosition = new Vector3(x, y, z); //

      // Verificar distancia al jugador
      if (playerTransform != null) //
      {
        if (Vector3.Distance(generatedPosition, playerTransform.position) >= minDistanceToPlayerForVictoryPos) //
        {
          isValidPosition = true; //
        }
      }
      else
      {
        // Si no hay jugador, la posición es válida por defecto (o podrías querer otro criterio)
        isValidPosition = true; //
      }

      attempts++; //
    }

    if (!isValidPosition)
    {
      Debug.LogWarning($"GhostManager: No se pudo encontrar una posición de victoria válida después de {maxVictoryPosGenerationAttempts} intentos. Usando la última posición generada."); //
    }

    return generatedPosition; //
  }

  void OnDrawGizmos()
  {
    Gizmos.color = Color.green;
    if (startPositionsParent != null)
    {
      foreach (Transform child in startPositionsParent)
      {
        Gizmos.DrawSphere(child.position, 0.5f);
        Gizmos.DrawWireSphere(child.position, 0.6f);
      }
    }

    Gizmos.color = Color.yellow;
    if (intermediatePositionsParent != null)
    {
      foreach (Transform child in intermediatePositionsParent)
      {
        Gizmos.DrawSphere(child.position, 0.4f);
      }
    }

    Gizmos.color = Color.red;
    if (finalPositionsParent != null)
    {
      foreach (Transform child in finalPositionsParent)
      {
        Gizmos.DrawCube(child.position, Vector3.one * 0.5f);
        Gizmos.DrawWireCube(child.position, Vector3.one * 0.6f);
      }
    }

    Gizmos.color = Color.magenta;
    if (redGhostGlobalReturnPoint != null)
    {
      Gizmos.DrawWireSphere(redGhostGlobalReturnPoint.position, 0.7f);
    }

    Gizmos.color = Color.cyan;
    if (victoryReferencePoint != null)
    {
      Gizmos.DrawSphere(victoryReferencePoint.position, 0.3f);
      Gizmos.DrawWireSphere(victoryReferencePoint.position, minVictoryRadius);
      Gizmos.DrawWireSphere(victoryReferencePoint.position, maxVictoryRadius);

      Vector3 yLower = new Vector3(victoryReferencePoint.position.x, victoryReferencePoint.position.y - (yVictorySegmentHeight / 2f), victoryReferencePoint.position.z);
      Vector3 yUpper = new Vector3(victoryReferencePoint.position.x, victoryReferencePoint.position.y + (yVictorySegmentHeight / 2f), victoryReferencePoint.position.z);
      Gizmos.DrawLine(yLower, yUpper);

      // ¡NUEVO GIZMO para la distancia mínima al jugador!
      if (playerTransform != null)
      {
        Gizmos.color = Color.yellow; // Puedes elegir otro color
        Gizmos.DrawWireSphere(playerTransform.position, minDistanceToPlayerForVictoryPos);
        Gizmos.DrawSphere(playerTransform.position, 0.1f); // Pequeña esfera para el jugador
      }
    }
  }
}