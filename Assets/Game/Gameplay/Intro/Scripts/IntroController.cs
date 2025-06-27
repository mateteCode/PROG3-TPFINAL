using UnityEngine;
using System; // Necesario para Action

public class IntroController : MonoBehaviour
{
  [SerializeField] private PlayerController playerController = null;
  [SerializeField] private GameplayUI gameplayUI = null;
  [SerializeField] private Dog dog = null;
  [SerializeField] private AudioEvent musicEvent = null;
  [SerializeField] private Door door = null; // Asigna tu objeto Door aquí en el Inspector

  // Propiedades para el estado de los ítems
  public bool HasItem1 { get; private set; }
  public bool HasItem2 { get; private set; }

  private void Start()
  {
    playerController.Init(ToggleOnPause, gameplayUI.UpdatePlayerHealth, null);
    gameplayUI.Init(ToggleTimeScale, ToggleOffPause);
    //dog.onTriggered = () => gameplayUI.OpenDialog();
    dog.onTriggered = HandleDogTriggered;
    dog.onUntriggered = () => gameplayUI.CloseDialog();

    GameManager.Instance.AudioManager.PlayAudio(musicEvent);


    // Asegúrate de que la puerta esté inicialmente deshabilitada
    if (door != null)
    {
      door.gameObject.SetActive(false);
    }

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
  }

  // Método para recolectar Item1
  public void CollectItem1()
  {
    HasItem1 = true;
    Debug.Log("Item 1 recolectado.");
    CheckItemsAndDoor();
  }

  // Método para recolectar Item2
  public void CollectItem2()
  {
    HasItem2 = true;
    Debug.Log("Item 2 recolectado.");
    CheckItemsAndDoor();
  }

  private void CheckItemsAndDoor()
  {
    if (HasItem1 && HasItem2)
    {
      Debug.Log("Ambos ítems recolectados. Habilitando puerta.");
      if (door != null)
      {
        door.gameObject.SetActive(true);
      }
    }
  }

  private void HandleDogTriggered()
  {
    // Lógica para determinar qué mensaje mostrar
    string message = "";
    if (!HasItem1 && !HasItem2)
    {
      message = "Hola, forastero. Buscás el Cristal de la Vida Eterna, ¿verdad? Primero, tocá el Plasma Resonante. Así sabremos si tu cuerpo resiste los portales. Otros lo  intentaron… y no volvieron.";
    }
    else if (HasItem1 && !HasItem2)
    {
      message = "Increíble… eres el primero que regresa. Pero antes de avanzar, ve por la Poción de Lumbre. Sin ella, las visiones te romperán la mente cuando cruces al otro mundo.";
    }
    else if (!HasItem1 && HasItem2)
    {
      message = "Veo que encontraste la poción… Entonces llegó el momento: toca el Plasma Resonante y enfrenta tu destino.";
    }
    else if (HasItem1 && HasItem2)
    {
      message = "Impresionante, solo los valientes cruzan esta puerta… pero no digas que no te avisé sobre lo que te espera.";
    }

    // Llamar directamente a un método en GameplayUI para mostrar el mensaje
    gameplayUI.SetDialogMessageAndOpen(message);
  }

  private void ToggleOnPause()
  {
    gameplayUI.TogglePause(true);
    ToggleTimeScale(false);
  }

  private void ToggleOffPause()
  {
    ToggleTimeScale(true);
    playerController.TogglePause(false);
  }

  private void ToggleTimeScale(bool status)
  {
    Time.timeScale = status ? 1f : 0f;
  }
}
