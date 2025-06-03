using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random; // Para evitar conflictos si usas System.Random también

public class GhostManager : MonoBehaviour
{
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


  private List<GameObject> ghostPool;
  private int currentGhostIndex = 0; // Índice para recorrer el pool
  private Coroutine spawnCoroutine;

  private List<Transform> allStartPoints = new List<Transform>();
  private List<Transform> allIntermediatePoints = new List<Transform>();
  private List<Transform> allFinalPoints = new List<Transform>();

  void Awake()
  {
    CachePoints(); // Cargar los puntos de las jerarquías
    InitializeGhostPool();
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
    Debug.LogWarning("Pool de fantasmas vacío. Considera aumentar el 'Pool Size' o espera a que uno termine.");
    return null;
  }


  IEnumerator SpawnGhostsRoutine()
  {
    while (true)
    {
      yield return new WaitForSeconds(spawnInterval);

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
      ReturnGhostToPool(ghost); // Devuelve el fantasma si la ruta es inválida
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

    // Seleccionar un número aleatorio de puntos intermedios (entre 0 y el máximo disponible)
    // Se cambió a 0 como mínimo para permitir rutas directas si no hay intermedios o se elige 0.
    int numIntermediatePoints = Random.Range(0, allIntermediatePoints.Count + 1);

    // Ordenar los puntos intermedios por cercanía al punto final
    // Esto crea una "línea" de puntos hacia el destino, ayudando a la lógica de "siempre llegue a destino"
    List<Transform> sortedIntermediatePoints = new List<Transform>(allIntermediatePoints);
    sortedIntermediatePoints.Sort((a, b) =>
        Vector3.Distance(a.position, finalPoint).CompareTo(Vector3.Distance(b.position, finalPoint)));

    // Añadir los puntos intermedios seleccionados (los 'numIntermediatePoints' más cercanos al destino)
    for (int i = 0; i < numIntermediatePoints && i < sortedIntermediatePoints.Count; i++)
    {
      path.Add(sortedIntermediatePoints[i].position);
    }

    // Añadir el punto final a la ruta
    path.Add(finalPoint);

    return path;
  }

  public void ReturnGhostToPool(GameObject ghost)
  {
    ghost.SetActive(false);
  }

  void OnDisable()
  {
    if (spawnCoroutine != null)
    {
      StopCoroutine(spawnCoroutine);
    }
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
  }
}