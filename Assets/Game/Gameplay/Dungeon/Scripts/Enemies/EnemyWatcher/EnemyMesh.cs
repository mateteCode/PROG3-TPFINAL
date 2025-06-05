using UnityEngine;

public class EnemyMesh : MonoBehaviour
{
    [SerializeField] EnemyWatcherController enemyWatcherController;

    public void EnableAttack(int attackNumber)
    {
        enemyWatcherController.EnableAttack(attackNumber);
    }

    public void DisableAttack()
    {
        enemyWatcherController.DisableAttack();
    }
}
