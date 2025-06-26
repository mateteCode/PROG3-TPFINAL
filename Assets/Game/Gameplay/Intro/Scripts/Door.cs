using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer = default;

    private void OnTriggerEnter(Collider other)
    {
        if (Utils.CheckLayerInMask(playerLayer, other.gameObject.layer))
        {
            GameManager.Instance.ChangeScene(SceneGame.Gameplay);
        }
    }
}
