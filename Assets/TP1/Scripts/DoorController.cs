using Unity.VisualScripting;
using UnityEngine;

public class DoorController : MonoBehaviour
{
  GameObject door;
  [Header("Key Configuration")]
  [Tooltip("Número de llaves necesarias para abrir la puerta.")]
  [SerializeField] private int keysRequired = 1;
  int currentKeys;
  void Awake()
  {
    currentKeys = keysRequired;
    Transform doorTransform = transform.Find("door");
    if (doorTransform != null)
    {
      door = doorTransform.gameObject;
    }
    else
    {
      Debug.LogWarning("DoorController: 'door' child GameObject not found under " + gameObject.name + "!");
    }
  }
  public void OpenDoor()
  {
    if (door == null)
    {
      Debug.LogWarning("DoorController: No door GameObject assigned. Cannot attempt to open.");
      return;
    }
    if (currentKeys > 0)
    {
      currentKeys--; // Decrementa el contador de llaves
      Debug.Log($"Door requires {currentKeys} more key(s) to open.");

      if (currentKeys == 0)
      {
        // Si el contador llega a cero, la puerta se puede abrir
        Debug.Log("All keys collected! Opening door...");

        Animator doorAnimator = door.GetComponent<Animator>();
        if (doorAnimator != null)
        {
          doorAnimator.SetTrigger("OpenDoor");
        }
        else
        {
          Debug.LogWarning("DoorController: Animator component not found on 'door' GameObject!");
        }
      }
    }
    else
    {
      // La puerta ya está abierta o ya se activó la condición
      Debug.Log("Door is already open or condition met.");
      // Opcional: Podrías reproducir un sonido de "ya abierta" o "bloqueada" aquí
    }
    //Debug.Log("Door opened!");
    //door.GetComponent<Animator>().SetTrigger("OpenDoor");
  }
}
