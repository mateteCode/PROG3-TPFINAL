using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GhostMovement : MonoBehaviour
{
  private Vector3 currentTargetPosition;
  private float moveSpeed; // Velocidad base para rutas normales
  private GhostManager manager = null;
  public int Health { get; set; }

  private GhostType ghostType;
  private List<Vector3> currentPath;
  private int pathIndex;

  [Header("Behavior Configuration")]
  [Tooltip("Distancia mínima al jugador para que el fantasma reaccione en estado Idle.")]
  public float playerDetectionRange = 1.5f;
  [Tooltip("Prefab del objeto que deja el fantasma blanco al desaparecer.")]
  public GameObject whiteGhostDropPrefab;
  [Tooltip("Posición fija a la que regresa el fantasma rojo antes de desaparecer.")]
  public Transform redGhostFixedReturnPoint;
  [Tooltip("Distancia a la que el fantasma considera que ha llegado a su destino de victoria.")]
  public float stoppingDistance = 0.1f;

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
    MovingToWaypoint,
    IdleLookingAtPlayer,
    ReturningToOrigin,
    ReturningToFixedPoint,
    VictoryMove,
    Idle,
    Disappearing
  }
  private GhostState currentState;

  private Vector3 victoryTargetPosition;
  private float victoryCurrentSpeed; // Velocidad actual durante el estado de victoria
  private float victoryFastSpeedRef; // Referencia a la velocidad rápida
  private float victoryNormalSpeedRef; // Referencia a la velocidad normal
  private float victorySlowDownDistanceRef; // Referencia a la distancia de desaceleración

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

    if (manager == null)
    {
      ghostType = GhostType.White;
      setMaterial(ghostType);
      currentState = GhostState.IdleLookingAtPlayer;
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
    moveSpeed = speed; // Esta es la velocidad normal para rutas
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
      HandleDisappearance();
    }
  }

  // --- MÉTODO para establecer el destino de victoria (ahora con velocidades) ---
  public void SetVictoryDestination(Vector3 destination, float fastSpeed, float normalSpeed, float slowDownDistance)
  {
    victoryTargetPosition = destination;
    victoryFastSpeedRef = fastSpeed;
    victoryNormalSpeedRef = normalSpeed;
    victorySlowDownDistanceRef = slowDownDistance;

    // Establecer la velocidad inicial para el estado de victoria como la velocidad rápida
    victoryCurrentSpeed = victoryFastSpeedRef;

    currentState = GhostState.VictoryMove;
    Debug.Log($"{gameObject.name} (Tipo: {ghostType}) está moviéndose a su posición de victoria: {victoryTargetPosition} con velocidad inicial {victoryCurrentSpeed}");
  }

  void Update()
  {
    switch (currentState)
    {
      case GhostState.MovingToWaypoint:
        MoveTowardsCurrentTarget();
        break;
      case GhostState.IdleLookingAtPlayer:
        LookAtPlayer();
        CheckPlayerDistanceAndTriggerBehavior();
        break;
      case GhostState.ReturningToOrigin:
      case GhostState.ReturningToFixedPoint:
        MoveTowardsCurrentTarget();
        break;
      case GhostState.VictoryMove:
        MoveToVictoryDestination();
        break;
      case GhostState.Idle:
        // El fantasma está inactivo
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
          currentState = GhostState.IdleLookingAtPlayer;
        }
      }
      else if (currentState == GhostState.ReturningToOrigin || currentState == GhostState.ReturningToFixedPoint)
      {
        StartCoroutine(Disappear());
      }
    }
  }

  // --- MÉTODO para moverse al destino de victoria (con velocidad adaptativa) ---
  void MoveToVictoryDestination()
  {
    float distanceToTarget = Vector3.Distance(transform.position, victoryTargetPosition);

    // Ajustar velocidad basada en la distancia al objetivo
    if (distanceToTarget > victorySlowDownDistanceRef)
    {
      victoryCurrentSpeed = victoryFastSpeedRef; // Velocidad rápida si está lejos
    }
    else
    {
      // Interpolar velocidad para una transición suave o simplemente usar la normal
      victoryCurrentSpeed = Mathf.Lerp(victoryNormalSpeedRef, victoryFastSpeedRef, distanceToTarget / victorySlowDownDistanceRef);
      // O si quieres que la velocidad sea DIRECTAMENTE normal al pasar el umbral:
      // victoryCurrentSpeed = victoryNormalSpeedRef;
    }

    // Mover hacia la posición final de victoria
    transform.position = Vector3.MoveTowards(transform.position, victoryTargetPosition, victoryCurrentSpeed * Time.deltaTime);

    // Si el fantasma ha llegado (o está muy cerca) de su destino
    if (distanceToTarget < stoppingDistance)
    {
      transform.position = victoryTargetPosition; // Asegurarse de que esté exactamente en el punto
      currentState = GhostState.Idle; // Cambiar a estado Idle
      Debug.Log($"{gameObject.name} (Tipo: {ghostType}) ha llegado a su posición de victoria y ahora está INACTIVO.");
      // Notificar al GhostManager que este fantasma ha llegado a su destino de victoria
      if (manager != null)
      {
        manager.GhostReachedVictoryPosition();
      }
    }
  }

  void CheckPlayerDistanceAndTriggerBehavior()
  {
    if (playerTransform == null) return;

    float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

    if (distanceToPlayer <= playerDetectionRange)
    {
      TriggerSpecificGhostBehavior();
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
        if (whiteGhostDropPrefab != null)
        {
          Instantiate(whiteGhostDropPrefab, transform.position, Quaternion.identity);
        }
        StartCoroutine(Disappear());
        break;

      case GhostType.Blue:
        if (currentPath != null && currentPath.Count > 0)
        {
          currentTargetPosition = currentPath[0];
          currentState = GhostState.ReturningToOrigin;
        }
        else
        {
          Debug.LogWarning("Fantasma Azul: Ruta de origen no disponible. Desapareciendo.");
          StartCoroutine(Disappear());
        }
        break;

      case GhostType.Red:
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
    yield return null;
    HandleDisappearance();
  }

  private void HandleDisappearance()
  {
    if (manager != null)
    {
      manager.ReturnGhostToPool(this.gameObject);
    }
    else
    {
      Destroy(gameObject);
    }
  }
}