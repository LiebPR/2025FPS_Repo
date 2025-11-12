using UnityEngine;
using System.Collections;

public class AutoDespawnVFX : MonoBehaviour
{
    [SerializeField] float lifeTime = 2f; // Duración del VFX antes de despawnear
    [SerializeField] string poolName = "EnemyDeathVFX"; // Nombre de la pool

    private void OnEnable()
    {
        StartCoroutine(DespawnAfterTime());
    }

    IEnumerator DespawnAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Despawn(poolName, gameObject);
        }
    }
}
