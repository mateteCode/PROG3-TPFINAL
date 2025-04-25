using Unity.VisualScripting;
using UnityEngine;

public class DoorController : MonoBehaviour
{
  GameObject door;
  void Awake()
  {
    door = GameObject.Find("door");
  }
  public void OpenDoor()
  {
    Debug.Log("Door opened!");
    door.GetComponent<Animator>().SetTrigger("OpenDoor");
  }
}
