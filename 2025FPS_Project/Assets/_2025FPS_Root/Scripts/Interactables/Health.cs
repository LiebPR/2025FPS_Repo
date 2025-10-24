using System;
using UnityEngine;

/// <summary>
/// EnemyHealth: Sistema de salud de enemigo. Gestiona daño, feedback visual y evento de muerte.
/// </summary>
public class Health : MonoBehaviour
{
    #region General Variables
    [Header("Health System Management")]
    [SerializeField] bool isPlayer;
    [SerializeField] int maxHealth = 100; //Vida máxima del enemigo
    [SerializeField] int health; //Vida actual del enemigo

    int currentHealth;

    [Header("Feedback Configuration")]
    [SerializeField] Material damagedMat; //Material feedback de daño
    Material baseMat; //Material base del enemigo
    MeshRenderer enemyRend; //Referencia al MeshRenderer propio
    #endregion

    #region Events
    public event Action OnDeath; //informa de la muerte del enemigo
    #endregion


    private void Awake()
    {
        enemyRend = GetComponent<MeshRenderer>();
        health = maxHealth;
        if (!isPlayer)
        {
            baseMat = enemyRend.material;
        }
    }

    //Restaura la salud total del enemigo 
    public void ResetHealth()
    {
        if (!isPlayer)
        {
            enemyRend.material = baseMat;
        }
        currentHealth = maxHealth;
    }

    //Aplica daño al enemigo y gestiona el feedback visual.
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (!isPlayer)
        {
            enemyRend.material = damagedMat; //feedback visual de impacto
            Invoke(nameof(ResetDamageMat), 0.1f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    //Restaura el material original tras el feedback de daño
    void ResetDamageMat()
    {
        if (isPlayer) return;
        enemyRend.material = baseMat;
    }

    //Gestiona la muerte del enemigo y ejecuta el evento correspondiente
    void Die()
    {
        currentHealth = 0;
        if (!isPlayer)
        {
            OnDeath?.Invoke(); //invoca el evento antes de apagaer el objeto
            gameObject.SetActive(false); //desactivar el enemigo (vuelve a la pool)
        }
        else
        {
            //LOSESCENE
        }

        
    }

    //Método llamado automáticamente al activar el objeto
    private void OnEnable()
    {
        ResetHealth(); //Restauramos la salud y materiales al reaparecer
    }
}
