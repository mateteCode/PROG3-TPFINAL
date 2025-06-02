using UnityEngine;

public class GhostMovement : MonoBehaviour
{
  private Vector3 targetPosition;
  private float moveSpeed;
  private GhostManager manager; // Referencia al manager para devolver el fantasma
  public int Health { get; set; } // Propiedad para la vida del fantasma

  public void SetMovementParameters(Vector3 target, float speed, GhostManager ghostManager)
  {
    targetPosition = target;
    moveSpeed = speed;
    manager = ghostManager;
  }

  void Update()
  {
    // Mover el fantasma hacia el objetivo
    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

    // Si el fantasma ha llegado a su destino
    if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
    {
      // Opcional: Podrías hacer que el fantasma desaparezca o se desactive después de un tiempo aquí
      // Por ahora, lo devolvemos al pool inmediatamente.
      manager.ReturnGhostToPool(this.gameObject);
    }

    // Ejemplo simple de cómo la vida podría ser usada (aunque no hay daño implementado aquí)
    if (Health <= 0)
    {
      // Si la vida llega a 0, devolver al pool (asumiendo que algo le resta vida)
      manager.ReturnGhostToPool(this.gameObject);
    }
  }
}
