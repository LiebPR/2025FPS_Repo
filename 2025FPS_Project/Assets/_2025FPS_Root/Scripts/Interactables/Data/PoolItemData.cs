using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;




/// <summary>
/// PoolItemData: Scriptable Object que almacena la configuración global de pools.
/// Permite definir y reutilizar distintas configuraciones por escena o contexto.
/// </summary>
[CreateAssetMenu(fileName = "PoolItemConfiguration", menuName = "Scriptable Objects/Pool Config")]
public class PoolItemData : ScriptableObject
{
    #region Serializable
    [System.Serializable]
    public class PoolItem
    {
        public string poolName; //identificador lógico de la pool 
        public GameObject prefab; //prefab a instanciar
        public int initialSize = 10; //cantidad inicial de instancias
        public bool canExpand = true; //permitir expenasión dinámica
    }
    #endregion

    [Header("Pool Definitions")]
    public List<PoolItem> pools = new List<PoolItem>(); //lsita de las configuraciones disponibles
}
