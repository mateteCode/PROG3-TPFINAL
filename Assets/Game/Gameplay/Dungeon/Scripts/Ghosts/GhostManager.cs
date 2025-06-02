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

  [Header("Posiciones de Spawneo y Destino")]
  [Tooltip("Lista de posibles posiciones de inicio para los fantasmas.")]
  public List<Transform> startPositions = new List<Transform>();
  [Tooltip("Lista de posibles posiciones de destino para los fantasmas.")]
  public List<Transform> targetPositions = new List<Transform>();

  [Header("Materiales de Fantasmas")]
  [Tooltip("Lista de materiales que se asignarán aleatoriamente a los fantasmas.")]
  public List<Material> ghostMaterials = new List<Material>();

  private List<GameObject> ghostPool;
  private int currentGhostIndex = 0; // Índice para recorrer el pool
  private Coroutine spawnCoroutine;

  void Awake()
  {
    InitializeGhostPool();
  }

  void Start()
  {
    // Inicia el proceso de spawneo y movimiento
    spawnCoroutine = StartCoroutine(SpawnGhostsRoutine());
  }

  /// <summary>
  /// Inicializa el pool de fantasmas, desactivándolos y agregándolos a la lista.
  /// </summary>
  void InitializeGhostPool()
  {
    ghostPool = new List<GameObject>();
    for (int i = 0; i < poolSize; i++)
    {
      GameObject ghost = Instantiate(ghostPrefab, transform); // Instancia como hijo del GhostManager para organizar
      ghost.SetActive(false); // Desactiva el fantasma inmediatamente
      ghostPool.Add(ghost);
    }
  }

  /// <summary>
  /// Obtiene un fantasma del pool.
  /// </summary>
  /// <returns>Un GameObject de fantasma si hay uno disponible, de lo contrario null.</returns>
  GameObject GetGhostFromPool()
  {
    // Busca un fantasma inactivo en el pool, empezando desde el último índice usado para una mejor distribución.
    for (int i = 0; i < ghostPool.Count; i++)
    {
      int index = (currentGhostIndex + i) % ghostPool.Count;
      if (!ghostPool[index].activeInHierarchy)
      {
        currentGhostIndex = index; // Actualiza el índice para la próxima búsqueda
        return ghostPool[index];
      }
    }
    // Si todos los fantasmas están activos, no hay ninguno disponible.
    Debug.LogWarning("Pool de fantasmas vacío. Considera aumentar el 'Pool Size'.");
    return null;
  }

  /// <summary>
  /// Corutina que maneja el spawneo periódico de fantasmas.
  /// </summary>
  IEnumerator SpawnGhostsRoutine()
  {
    while (true)
    {
      yield return new WaitForSeconds(spawnInterval); // Espera el tiempo configurado

      GameObject ghost = GetGhostFromPool();
      if (ghost != null)
      {
        ConfigureAndActivateGhost(ghost);
      }
    }
  }

  /// <summary>
  /// Configura y activa un fantasma obtenido del pool.
  /// </summary>
  /// <param name="ghost">El GameObject del fantasma a configurar.</param>
  void ConfigureAndActivateGhost(GameObject ghost)
  {
    // 1. Asignar posición de inicio aleatoria
    Vector3 startPos = GetRandomStartPosition();
    ghost.transform.position = startPos;

    // 2. Asignar material aleatorio
    AssignRandomMaterial(ghost);

    // 3. Activar el fantasma
    ghost.SetActive(true);

    // 4. Iniciar movimiento del fantasma
    // Asegúrate de que tu prefab de fantasma tenga un script que maneje su movimiento.
    // Aquí asumimos que tienes un script llamado 'GhostMovement' o similar.
    // Si no tienes uno, tendrás que crearlo o manejar el movimiento directamente aquí
    // (lo cual puede hacer este script muy grande).
    GhostMovement ghostMovement = ghost.GetComponent<GhostMovement>();
    if (ghostMovement == null)
    {
      Debug.LogWarning("El fantasma " + ghost.name + " no tiene un componente GhostMovement. Añadiéndolo...");
      ghostMovement = ghost.AddComponent<GhostMovement>();
    }

    // Configura el movimiento del fantasma
    ghostMovement.SetMovementParameters(GetRandomTargetPosition(), ghostSpeed, this); // Pasa una referencia a GhostManager
    ghostMovement.Health = ghostHealth; // Asigna la vida
  }

  /// <summary>
  /// Devuelve una posición de inicio aleatoria de la lista.
  /// </summary>
  Vector3 GetRandomStartPosition()
  {
    if (startPositions == null || startPositions.Count == 0)
    {
      Debug.LogError("No hay posiciones de inicio configuradas. Asegúrate de añadir posiciones en el Inspector.");
      return Vector3.zero;
    }
    return startPositions[Random.Range(0, startPositions.Count)].position;
  }

  /// <summary>
  /// Devuelve una posición de destino aleatoria de la lista.
  /// </summary>
  Vector3 GetRandomTargetPosition()
  {
    if (targetPositions == null || targetPositions.Count == 0)
    {
      Debug.LogError("No hay posiciones de destino configuradas. Asegúrate de añadir posiciones en el Inspector.");
      return Vector3.zero;
    }
    return targetPositions[Random.Range(0, targetPositions.Count)].position;
  }

  /// <summary>
  /// Asigna un material aleatorio al MeshRenderer del fantasma.
  /// </summary>
  /// <param name="ghost">El GameObject del fantasma.</param>
  void AssignRandomMaterial(GameObject ghost)
  {
    if (ghostMaterials == null || ghostMaterials.Count == 0)
    {
      Debug.LogWarning("No hay materiales de fantasma configurados. El fantasma usará su material por defecto.");
      return;
    }

    MeshRenderer meshRenderer = ghost.GetComponentInChildren<MeshRenderer>(); // Busca en hijos también
    if (meshRenderer != null)
    {
      meshRenderer.material = ghostMaterials[Random.Range(0, ghostMaterials.Count)];
    }
    else
    {
      Debug.LogWarning("El fantasma " + ghost.name + " no tiene un MeshRenderer para asignar el material.");
    }
  }

  /// <summary>
  /// Método para "devolver" un fantasma al pool cuando termina su vida útil o llega a su destino.
  /// </summary>
  /// <param name="ghost">El GameObject del fantasma a devolver.</param>
  public void ReturnGhostToPool(GameObject ghost)
  {
    ghost.SetActive(false); // Desactiva el fantasma
                            // Cualquier otra lógica de reinicio del fantasma (ej. resetear vida, estado, etc.)
                            // Si el fantasma tuviera componentes de física o partículas, es un buen lugar para resetearlos.
  }

  void OnDisable()
  {
    // Detiene la corutina cuando el objeto se desactiva o se destruye
    if (spawnCoroutine != null)
    {
      StopCoroutine(spawnCoroutine);
    }
  }

  void OnDrawGizmos()
  {
    // Dibujar las posiciones de inicio
    Gizmos.color = Color.green;
    foreach (Transform pos in startPositions)
    {
      Gizmos.DrawSphere(pos.position, 0.5f);
      Gizmos.DrawWireSphere(pos.position, 0.6f);
    }

    // Dibujar las posiciones de destino
    Gizmos.color = Color.red;
    foreach (Transform pos in targetPositions)
    {
      Gizmos.DrawCube(pos.position, Vector3.one * 0.5f);
      Gizmos.DrawWireCube(pos.position, Vector3.one * 0.6f);
    }
  }
}
