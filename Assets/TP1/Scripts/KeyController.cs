using UnityEngine;

public class KeyController : MonoBehaviour
{
  public GameObject doorToOpen;
  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      DoorController door = doorToOpen.GetComponent<DoorController>();
      if (door != null)
      {
        door.OpenDoor();
      }
      Destroy(gameObject);
    }
  }
}
