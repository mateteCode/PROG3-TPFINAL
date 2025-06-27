using System;
using UnityEngine;

public class Dog : MonoBehaviour
{
  [SerializeField] private LayerMask playerLayer = default;

  public Action onTriggered = null;
  public Action onUntriggered = null;

  private void OnTriggerEnter(Collider other)
  {
    if (Utils.CheckLayerInMask(playerLayer, other.gameObject.layer))
    {
      onTriggered?.Invoke();
    }
  }


  void OnTriggerExit(Collider other)
  {
    onUntriggered?.Invoke();
  }
}
