using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PoolManager: Sistema de manejo e pools que utiliza una configuración externa (ScriptableObject).
/// Puede usarse como singleton global o como instancia independiente.
/// </summary>
public class PoolManager : MonoBehaviour
{
    #region Fields
    [SerializeField] PoolItemData poolData; //ref al scriptable que contiene la configuración de las pools.

    //Diccionario interno que mantiene las colas de objetos activos e interactivos por pool
    Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    bool initialized = false; //indica si las pools ya han sido inicializadas

    // Instancia Singleton
    public static PoolManager Instance { get; private set; } // Propiedad estática
    #endregion

    // Asegurarse de que solo haya una instancia de PoolManager
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Si ya hay una instancia, destruir esta
            return;
        }

        Instance = this; // Asignar la instancia
        DontDestroyOnLoad(gameObject); // Mantener esta instancia en todas las escenas
    }

    //Inicializa todas las pools declaradas en poolData
    public void Initialize()
    {
        if (initialized || poolData == null) return; //evita inicilización más de una vez o si no hay configuración

        poolDictionary.Clear(); //limpia cualquier pool anterior

        foreach (var pool in poolData.pools)
        {
            if (pool.prefab == null) continue;

            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false); // inicialmente inactivos
                objectPool.Enqueue(obj);
            }
            poolDictionary[pool.poolName] = objectPool; //guarda la cola en el diccionario
        }

        initialized = true;
    }

    #region Spawn & Despawn
    //Spawnea desde la pool indicada, colocándolo en la posición y rotación dadas.
    public GameObject Spawn(string poolName, Vector3 pos, Quaternion rot)
    {
        if (!poolDictionary.ContainsKey(poolName)) return null;

        var config = poolData.pools.Find(poolDictionary => poolDictionary.poolName == poolName); //obtiene la configuración de la pool
        var objects = poolDictionary[poolName]; //cola de objetos

        GameObject obj = null;

        if (objects.Count == 0 && config.canExpand)
        {
            // Instanciación dinámica si se permite expandir 
            obj = Instantiate(config.prefab, pos, rot);
        }
        else if (objects.Count > 0)
        {
            obj = objects.Dequeue(); // Saca el objeto de la cola

            if (obj != null) // Verifica si el objeto es válido antes de intentar usarlo
            {
                obj.transform.SetPositionAndRotation(pos, rot); // Reposiciona
            }
            else
            {
                // Si el objeto ya ha sido destruido, simplemente instanciamos uno nuevo
                obj = Instantiate(config.prefab, pos, rot);
            }
        }

        if (obj != null)
        {
            obj.SetActive(true); // Activa el objeto
        }

        return obj;
    }

    //Devuelve un objeto a su pool correspondiente
    public void Despawn(string poolName, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Destroy(obj); // Si la pool no existe, destruimos el objeto
            return;
        }

        if (obj == null) // Verifica que el objeto no sea nulo antes de procesarlo
        {
            return;
        }

        if (obj.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
        {
            // Llamamos al método ResetMuzzleFlash solo si el objeto es de tipo GunSystem
            GunSystem gunSystem = FindAnyObjectByType<GunSystem>();
            if (gunSystem != null)
            {
                gunSystem.ResetMuzzleFlash(obj);  // Aquí se invoca el método correctamente
            }
        }

        obj.SetActive(false); // Desactiva el objeto

        if (!obj.Equals(null)) // Verifica que el objeto no haya sido destruido antes de devolverlo al pool
        {
            poolDictionary[poolName].Enqueue(obj); // Lo devuelve a la cola
        }
    }
    #endregion

    #region Utilities
    //Comprueba si existe uan pool registrada con el nombre indicado
    public bool HasPool(string poolName) => poolDictionary.ContainsKey(poolName);

    //Limpia todas las pools y destruye los objetos instaciados
    public void Clear()
    {
        foreach (var kvp in poolDictionary)
        {
            foreach (var obj in kvp.Value)
            {
                if (obj != null)
                {
                    Destroy(obj); // Solo destruye los objetos si no son null
                }
            }
        }

        poolDictionary.Clear();
        initialized = false;
    }
    #endregion
}
