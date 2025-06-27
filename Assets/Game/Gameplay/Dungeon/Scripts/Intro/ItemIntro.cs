using UnityEngine;

public class ItemIntro : MonoBehaviour
{
  public enum ItemType { Item1, Item2 }
  public ItemType itemType;

  private IntroController introController; // Referencia al IntroController

  private void Awake()
  {
    // Buscar el IntroController en la escena
    introController = FindObjectOfType<IntroController>();
    if (introController == null)
    {
      Debug.LogError("IntroController no encontrado en la escena. Asegúrate de que esté presente.");
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    // Asumiendo que el Player tiene el tag "Player"
    if (other.CompareTag("Player"))
    {
      if (introController != null)
      {
        if (itemType == ItemType.Item1)
        {
          introController.CollectItem1();
        }
        else if (itemType == ItemType.Item2)
        {
          introController.CollectItem2();
        }
      }
      // Desactiva el ítem después de ser recolectado
      gameObject.SetActive(false);
    }
  }
}
