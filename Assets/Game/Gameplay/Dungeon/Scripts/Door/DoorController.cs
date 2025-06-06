using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DoorController : MonoBehaviour
{
  GameObject door;
  [Header("Key Configuration")]
  [Tooltip("Número de llaves necesarias para abrir la puerta.")]
  [SerializeField] private int keysRequired = 1;
  int currentKeys;

  [Header("Door Camera Configuration")]
  [Tooltip("La cámara de la puerta que se activará temporalmente.")]
  public Camera doorCamera;
  [Tooltip("Tiempo en segundos que la cámara de la puerta estará activa.")]
  public float doorCameraViewDuration = 2.0f;

  [Tooltip("Demora en segundos antes de activar la cámara de visión de la puerta.")]
  public float cameraViewActivationDelay = 0.5f;

  [SerializeField] private AudioEvent openDoorSound = null;
  [SerializeField] private AudioEvent doorLockSound = null;

  private Camera mainCamera;

  public bool IsOpen { get { return currentKeys <= 0; } }
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

    mainCamera = Camera.main;
    if (mainCamera == null)
    {
      Debug.LogWarning("DoorController: Main camera not found in the scene! Ensure your main camera is tagged 'MainCamera'.");
    }

    // Ensure the door camera is initially off
    if (doorCamera != null)
    {
      doorCamera.gameObject.SetActive(false);
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
      //Debug.Log($"Door requires {currentKeys} more key(s) to open.");


      if (currentKeys == 0)
      {
        // Si el contador llega a cero, la puerta se puede abrir
        //Debug.Log("All keys collected! Opening door...");

        Animator doorAnimator = door.GetComponent<Animator>();
        if (doorAnimator != null)
        {
          doorAnimator.SetTrigger("OpenDoor");
          GameManager.Instance.AudioManager.PlayAudio(openDoorSound);
        }
        else
        {
          Debug.LogWarning("DoorController: Animator component not found on 'door' GameObject!");
        }
      }
      else
      {
        GameManager.Instance.AudioManager.PlayAudio(doorLockSound);
      }

      if (doorCamera != null)
      {
        StartCoroutine(HandleDoorCameraView());
      }
    }
    else
    {
      // La puerta ya está abierta o ya se activó la condición
      //Debug.Log("Door is already open or condition met.");
      // Opcional: Podrías reproducir un sonido de "ya abierta" o "bloqueada" aquí
    }
    //Debug.Log("Door opened!");
    //door.GetComponent<Animator>().SetTrigger("OpenDoor");
  }

  private IEnumerator HandleDoorCameraView()
  {
    yield return new WaitForSeconds(cameraViewActivationDelay);
    // Ensure main camera exists and is enabled
    if (mainCamera != null)
    {
      mainCamera.gameObject.SetActive(false);
    }

    // Activate the door camera
    doorCamera.gameObject.SetActive(true);

    // Wait for the specified duration
    yield return new WaitForSeconds(doorCameraViewDuration);

    // Deactivate the door camera
    doorCamera.gameObject.SetActive(false);

    // Reactivate the main camera
    if (mainCamera != null)
    {
      mainCamera.gameObject.SetActive(true);
    }
  }
}
