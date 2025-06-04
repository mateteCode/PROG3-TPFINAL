using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GhostMovement : MonoBehaviour
{
  private Vector3 currentTargetPosition;
  private float moveSpeed;
  private GhostManager manager = null; // Se inicializa a null, el manager lo asignará si viene del pool
  public int Health { get; set; } // Propiedad para la vida del fantasma

  private GhostType ghostType;
  private List<Vector3> currentPath; // La ruta que debe seguir el fantasma
  private int pathIndex; // Índice actual en la ruta

  [Header("Behavior Configuration")]
  [Tooltip("Distancia mínima al jugador para que el fantasma reaccione en estado Idle.")]
  public float playerDetectionRange = 1.5f;
  [Tooltip("Prefab del objeto que deja el fantasma blanco al desaparecer.")]
  public GameObject whiteGhostDropPrefab;
  [Tooltip("Posición fija a la que regresa el fantasma rojo antes de desaparecer.")]
  public Transform redGhostFixedReturnPoint;

  [Header("Material Configuration")]
  [Tooltip("Material para el fantasma Blanco. Asignar en el Inspector del prefab del fantasma.")]
  [SerializeField] Material whiteGhostMaterial;
  [Tooltip("Material para el fantasma Azul. Asignar en el Inspector del prefab del fantasma.")]
  [SerializeField] Material blueGhostMaterial;
  [Tooltip("Material para el fantasma Rojo. Asignar en el Inspector del prefab del fantasma.")]
  [SerializeField] Material redGhostMaterial;

  private Transform playerTransform;

  private enum GhostState
  {
    MovingToWaypoint,        // Moviéndose a lo largo de la ruta inicial
    IdleLookingAtPlayer,     // Detenido, mirando al jugador y esperando que se acerque
    ReturningToOrigin,       // Fantasma Azul: regresando al punto de inicio de su ruta
    ReturningToFixedPoint,   // Fantasma Rojo: regresando a un punto fijo predefinido
    Disappearing             // Fantasma Blanco o final de retorno: en proceso de desactivarse
  }
  private GhostState currentState;

  void Awake()
  {
    Health = 100;

    GameObject player = GameObject.FindWithTag("Player");
    if (player != null)
    {
      playerTransform = player.transform;
    }
    else
    {
      Debug.LogWarning("GhostMovement: Jugador con tag 'Player' no encontrado en la escena. Asegúrate de que tu jugador tiene el tag 'Player'.");
    }

    // Si este fantasma NO es gestionado por el Manager (es decir, manager es null al inicio),
    if (manager == null)
    {
      ghostType = GhostType.White; // O usar un campo serializado
      setMaterial(ghostType);
      currentState = GhostState.IdleLookingAtPlayer; // Empieza en idle si no es del pool
    }
  }

  void setMaterial(GhostType type)
  {
    MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
    if (meshRenderer != null)
    {
      switch (type)
      {
        case GhostType.White:
          meshRenderer.material = whiteGhostMaterial;
          break;
        case GhostType.Blue:
          meshRenderer.material = blueGhostMaterial;
          break;
        case GhostType.Red:
          meshRenderer.material = redGhostMaterial;
          break;
        default:
          //Debug.LogWarning("GhostMovement: Tipo de fantasma no reconocido. Usando material blanco por defecto.");
          meshRenderer.material = whiteGhostMaterial;
          break;
      }
    }
    else
    {
      Debug.LogWarning("GhostMovement: No se encontró un MeshRenderer en el fantasma para asignar el material.");
    }
  }

  public void SetMovementParameters(GhostType type, List<Vector3> path, float speed, GhostManager ghostManager, GameObject whiteGhostDrop, Transform redReturnPoint)
  {
    ghostType = type;
    currentPath = path;
    moveSpeed = speed;
    manager = ghostManager;
    whiteGhostDropPrefab = whiteGhostDrop;
    redGhostFixedReturnPoint = redReturnPoint;

    setMaterial(ghostType);

    pathIndex = 0;
    Health = 100;

    if (currentPath != null && currentPath.Count > 0)
    {
      currentTargetPosition = currentPath[pathIndex];
      currentState = GhostState.MovingToWaypoint;
      transform.position = currentPath[0];
    }
    else
    {
      //Debug.LogError("GhostMovement: Ruta de fantasma vacía o nula! Desactivando fantasma.");
      HandleDisappearance();
    }
  }

  void Update()
  {
    // Debug.Log($"GhostMovement: Estado actual: {currentState}, Tipo de fantasma: {ghostType} para el fantasma {gameObject.name}");
    switch (currentState)
    {
      case GhostState.MovingToWaypoint:
        MoveTowardsCurrentTarget(); // Mover hacia el waypoint actual
        break;
      case GhostState.IdleLookingAtPlayer:
        LookAtPlayer();
        CheckPlayerDistanceAndTriggerBehavior(); // Verificar distancia para activar comportamiento
        break;
      case GhostState.ReturningToOrigin:
      case GhostState.ReturningToFixedPoint:
        MoveTowardsCurrentTarget(); // Mover de regreso al punto final (origen o fijo)
        break;
    }

    if (Health <= 0 && currentState != GhostState.Disappearing)
    {
      StartCoroutine(Disappear());
    }
  }

  void MoveTowardsCurrentTarget()
  {
    if (currentState == GhostState.MovingToWaypoint)
    {
      if (currentPath == null || currentPath.Count == 0 || pathIndex >= currentPath.Count)
      {
        Debug.LogError("GhostMovement: Ruta inicial inválida o finalizada inesperadamente. Desactivando fantasma.");
        HandleDisappearance();
        return;
      }
    }

    transform.position = Vector3.MoveTowards(transform.position, currentTargetPosition, moveSpeed * Time.deltaTime);

    if (Vector3.Distance(transform.position, currentTargetPosition) < 0.1f)
    {
      if (currentState == GhostState.MovingToWaypoint)
      {
        pathIndex++;
        if (pathIndex < currentPath.Count)
        {
          currentTargetPosition = currentPath[pathIndex];
        }
        else
        {
          //Debug.Log($"Fantasma {ghostType}: Llegó al final de su ruta inicial. Entrando en estado Idle.");
          currentState = GhostState.IdleLookingAtPlayer;
        }
      }
      else if (currentState == GhostState.ReturningToOrigin || currentState == GhostState.ReturningToFixedPoint)
      {
        //Debug.Log($"Fantasma {ghostType}: Llegó a su punto de retorno. Desapareciendo.");
        StartCoroutine(Disappear());
      }
    }
  }

  void CheckPlayerDistanceAndTriggerBehavior()
  {
    if (playerTransform == null) return;

    float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

    if (distanceToPlayer <= playerDetectionRange)
    {
      //Debug.Log($"Fantasma {ghostType}: Jugador detectado dentro del rango ({distanceToPlayer:F2}m). Activando comportamiento final.");
      TriggerSpecificGhostBehavior(); // Llama al comportamiento final específico para el tipo de fantasma
    }
  }

  void LookAtPlayer()
  {
    if (playerTransform == null) return;

    Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
    directionToPlayer.y = 0;
    if (directionToPlayer != Vector3.zero)
    {
      Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
      transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * moveSpeed * 2);
    }
  }

  void TriggerSpecificGhostBehavior()
  {
    switch (ghostType)
    {
      case GhostType.White:
        //Debug.Log("Fantasma Blanco: Desapareciendo y dejando objeto.");
        if (whiteGhostDropPrefab != null)
        {
          Instantiate(whiteGhostDropPrefab, transform.position, Quaternion.identity);
        }
        else
        {
          //Debug.LogWarning("Fantasma Blanco: No se asignó un prefab para soltar.");
        }
        StartCoroutine(Disappear());
        break;

      case GhostType.Blue:
        //Debug.Log("Fantasma Azul: Regresando a la posición de origen.");
        if (currentPath != null && currentPath.Count > 0)
        {
          currentTargetPosition = currentPath[0]; // Regresar al primer punto de la ruta
          currentState = GhostState.ReturningToOrigin;
        }
        else
        {
          Debug.LogWarning("Fantasma Azul: Ruta de origen no disponible. Desapareciendo.");
          StartCoroutine(Disappear());
        }
        break;

      case GhostType.Red:
        //Debug.Log("Fantasma Rojo: Volviendo a una posición fija.");
        if (redGhostFixedReturnPoint != null)
        {
          currentTargetPosition = redGhostFixedReturnPoint.position;
          currentState = GhostState.ReturningToFixedPoint;
        }
        else
        {
          Debug.LogWarning("Fantasma Rojo: No se asignó un punto de retorno fijo. Desapareciendo.");
          StartCoroutine(Disappear());
        }
        break;
    }
  }


  private IEnumerator Disappear()
  {
    currentState = GhostState.Disappearing;
    // TODO: Añadir efecto de desaparición visual (ej. sistema de partículas, fade del shader).
    // Y/o un sonido de desaparición.

    yield return null;

    HandleDisappearance(); // Llama al método que decide si devolver al pool o destruir
  }

  private void HandleDisappearance()
  {
    if (manager != null)
    {
      // Si el fantasma fue creado por el manager (y por lo tanto, pertenece al pool)
      manager.ReturnGhostToPool(this.gameObject);
    }
    else
    {
      // Si el manager es nulo, el fantasma probablemente fue instanciado manualmente en la escena y debe ser destruido directamente.
      //Debug.Log($"Fantasma {gameObject.name}: No gestionado por un pool. Destruyendo...");
      Destroy(gameObject);
    }
  }
}