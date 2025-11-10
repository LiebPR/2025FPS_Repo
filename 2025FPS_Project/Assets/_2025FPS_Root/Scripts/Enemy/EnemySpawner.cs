using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemySpawner: Spawn de enemigos que mantiene un número constante en escena usando
/// PoolManager y zona de spawn definidas con colliders.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    #region General Variables
    [Header("PoolManager")]
    [SerializeField] PoolManager poolManager;
    [SerializeField] string poolName = "Enemy"; //nombre de la pool a spawnear

    [Header("Spawner Settings")]
    [SerializeField] int maxEnemies = 10; //número máximo de enemigos
    [SerializeField] float spawnDelayInitial = 0.2f; //delay entre spawn iniciales
    [SerializeField] float spawnDelayRespawn = 1f; //delay tras la muerte de un enemigo

    [Header("Spawn Zones")]
    [SerializeField] Collider spawnZone; //colliders que definen las áreas de spawn

    List<GameObject> activeEnemies = new List<GameObject>(); //lista de enemigos activos en escena

    bool playerInsideZone;
    #endregion

    private void Start()
    {
        if (poolManager == null) return;

        poolManager.Initialize();
        StartCoroutine(SpawnInitialEnemies());
    }

    public void SetPlayerInside(bool state)
    {
        playerInsideZone = state;
    }

    #region Spawn Inicial
    //Spawnea progresivamente los enemigos al inicio para evitar caídas de FPS.
    IEnumerator SpawnInitialEnemies()
    {
        int spawned = 0;

        while(spawned < maxEnemies)
        {
            if (playerInsideZone)
            {
                SpawnEnemy();
                spawned++;
            }
            yield return new WaitForSeconds(spawnDelayInitial);
        }
    }
    #endregion

    #region Spawn

    void SpawnEnemy()
    {
        if (!playerInsideZone) return;
        if (spawnZone == null) return;

        Vector3 spawnPos = GetRandomPointInsideCollider(spawnZone);

        GameObject enemy = poolManager.Spawn(poolName, spawnPos, Quaternion.identity);
        if (enemy == null) return;

        activeEnemies.Add(enemy);

        Health hp = enemy.GetComponent<Health>();
        if (hp != null)
        {
            hp.OnDeath -= () => OnEnemyDeath(enemy);
            hp.OnDeath += () => OnEnemyDeath(enemy);
        }
    }
    

    //Devuelve una posición aleatoria dentro de un collider
    Vector3 GetRandomPointInsideCollider(Collider col)
    {
        return new Vector3(Random.Range(col.bounds.min.x, col.bounds.max.x), Random.Range(col.bounds.min.y, col.bounds.max.y), Random.Range(col.bounds.min.z, col.bounds.max.z));
    }
    #endregion

    #region Respawn
    //Llamado cuando un enemigo se desactiva en la PoolManager.
    //Espera un tiempo y respawnea otro enemigo si hay huecos en la pool
    void OnEnemyDeath(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        StartCoroutine(RespawnAfterDelay());
    }

    //Esperar el delay antes de intentar reactivar un enemigo desactivado de al pool
    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(spawnDelayRespawn);

        //Solo respawneamos si hay menos enemigos activos que el máximo
        if(activeEnemies.Count < maxEnemies)
        {
            SpawnEnemy();
        }
    }
    #endregion
}
