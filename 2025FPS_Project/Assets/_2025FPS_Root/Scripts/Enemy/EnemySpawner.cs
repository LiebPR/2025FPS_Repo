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
    [SerializeField] string poolName = "Enemy"; //Nombre de la pool a spawnear

    [Header("Spawner Settings")]
    [SerializeField] int maxEnemies = 10; //número máximo de enemigos
    [SerializeField] float spawnDelayInitial = 0.2f; //delay entre spawn iniciales
    [SerializeField] float spawnDelayRespawn = 1f; //delay tras la muerte de un enemigo

    [Header("Spawn Zones")]
    [SerializeField] Collider[] spawnZones; //colliders que definen las áreas de spawn

    List<GameObject> activeEnemies = new List<GameObject>();
    #endregion

    private void Start()
    {
        if (poolManager == null) return;

        poolManager.Initialize();
        StartCoroutine(SpawnInitialEnemies());
    }

    #region Spawn Inicial
    //Spawnea progresivamente los enemigos al inicio para evitar caídas de FPS.
    IEnumerator SpawnInitialEnemies()
    {
        int spawned = 0;

        while(spawned < maxEnemies)
        {
            SpawnEnemyAtRandomPosition();
            spawned++;
            yield return new WaitForSeconds(spawnDelayInitial); //delay entre cada spawn
        }
    }
    #endregion

    #region Spawn Aleatorio

    //Spawnea un enemigo en una posición aleatoria dentro de las zonas de spawn.
    void SpawnEnemyAtRandomPosition()
    {
        if (spawnZones.Length == 0) return;

        Collider zone = spawnZones[Random.Range(0, spawnZones.Length)];

        //Obtener posición aleatoria dentro de los bounds del collider
        Vector3 spawnPos = GetRandomPointInsideCollider(zone);

        GameObject enemy = poolManager.Spawn(poolName, spawnPos, Quaternion.identity);

        if(enemy != null)
        {
            activeEnemies.Add(enemy);

            //Registrar callback de muerte (ejemplo, puedes reemplazar con tu sistema de salud
            Health enemyHealth = enemy.GetComponent<Health>();
            if(enemyHealth != null)
            {
                enemyHealth.OnDeath += () => StartCoroutine(RespawnEnemy(enemy));
            }
        }
    }

    //Devuelve una posición aleatoria dentro de un collider
    Vector3 GetRandomPointInsideCollider(Collider col)
    {
        Vector3 point = new Vector3(Random.Range(col.bounds.min.x, col.bounds.max.x), Random.Range(col.bounds.min.y, col.bounds.max.y), Random.Range(col.bounds.min.z, col.bounds.max.z));
        return point;
    }
    #endregion

    #region Respawn Enemigos
    //Respawnea un enemigo tras morir con un delay configurable
    IEnumerator RespawnEnemy(GameObject enemy)
    {
        //Quitar de la lista de activos
        activeEnemies.Remove(enemy);

        //Devolver a la pool
        poolManager.Despawn(poolName, enemy);

        //Esperar el delay antes de reespawnear
        yield return new WaitForSeconds(spawnDelayRespawn);

        //Mantener siempre maxEnemies activos
        if(activeEnemies.Count < maxEnemies)
        {
            SpawnEnemyAtRandomPosition();
        }


    }
    #endregion
}
