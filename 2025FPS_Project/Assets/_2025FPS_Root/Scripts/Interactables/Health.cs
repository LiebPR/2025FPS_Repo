using System;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Player UI")]
    [SerializeField] Image healthBar;
    #endregion

    #region Events
    public event Action OnDeath; //informa de la muerte del enemigo
    public event Action<Vector3> OnHit; //informa de que le han impactado
    #endregion

    #region Referencias
    EnemyStateMachine fsm;
    #endregion

    private void Awake()
    {
        enemyRend = GetComponent<MeshRenderer>();
        health = maxHealth;
        if (!isPlayer)
        {
            baseMat = enemyRend.material;
            fsm = GetComponent<EnemyStateMachine>();
        }
        else
        {
            //Inicializar barra de vida del jugador
            if (healthBar != null)
                healthBar.fillAmount = 1f;
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
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if(isPlayer && healthBar != null)
        {
            //Actualizar barra de vida
            healthBar.fillAmount = (float) currentHealth / maxHealth;
        }

        if (!isPlayer)
        {
            //Solo lanza el evento OnHit si NO esta en Chase ni Alert
            if(fsm != null && fsm.currentState != EnemyState.Chase && fsm.currentState != EnemyState.Alert)
            {
                OnHit?.Invoke(transform.position);
            }
            
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
