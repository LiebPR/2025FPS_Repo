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
    [SerializeField] Color lowHealthColor = Color.yellow;
    [SerializeField] Color fullHealthColor;
    string poolName = "Enemy";

    int currentHealth;

    [Header("Feedback Configuration")]
    [SerializeField] Material damagedMat; //Material feedback de daño
    Material baseMat; //Material base del enemigo
    MeshRenderer[] enemyRends; //Array para almacenar todos los MeshRenderers del enemigo

    [Header("Player UI")]
    [SerializeField] Image healthBar;

    [Header("Ammo Drop Settings")]
    [SerializeField] GameObject dropPrefab; // Prefab de munición
    [SerializeField, Range(0f, 1f)] float percentChance = 0.3f; // Probabilidad de soltar munición (0.3 = 30%)

    [Header("VFX Settings")]
    [SerializeField] string vfxPoolName = "EnemyDeathVFX";
    #endregion

    #region Events
    public event Action OnDeath; //informa de la muerte del enemigo
    public event Action<Vector3> OnHit; //informa de que le han impactado
    #endregion

    #region Referencias
    EnemyStateMachine fsm;
    #endregion

    // Flag para gestionar si el enemigo puede recibir daño
    bool isDamageable = true;
    bool isLowHealthSoundPlaying;

    private void Awake()
    {
        enemyRends = GetComponentsInChildren<MeshRenderer>();

        health = maxHealth;
        currentHealth = maxHealth;

        if (!isPlayer)
        {
            baseMat = enemyRends[0].material; //usamos el primer meshRenderer como base
            fsm = GetComponent<EnemyStateMachine>();
        }
        else
        {
            //Inicializar barra de vida del jugador
            if (healthBar != null)
                healthBar.fillAmount = 1f;
        }

        //Cambio de color en la barra de vida
        if (isPlayer && healthBar != null)
        {
            currentHealth = maxHealth;
            fullHealthColor = healthBar.color;
            healthBar.fillAmount = 1f;
        }
    }

    //Restaura la salud total del enemigo 
    public void ResetHealth()
    {
        if (!isPlayer)
        {
            //Restauramos todos los materiales a su estado original
            foreach(MeshRenderer rend in enemyRends)
            {
                rend.material = baseMat;
            }
        }
        currentHealth = maxHealth;
    }

    //Aplica daño al enemigo y gestiona el feedback visual.
    public void TakeDamage(int damage)
    {
        if (!isDamageable) return; // Si el enemigo no puede recibir daño, no hacemos nada

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (isPlayer && healthBar != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            healthBar.fillAmount = healthPercent;

            // transición azul (fullHealthColor) -> amarillo (lowHealthColor)
            float t = 1f - healthPercent; // ahora t = 0 cuando vida llena, t = 1 cuando vida baja
            healthBar.color = Color.Lerp(fullHealthColor, lowHealthColor, t);
        }
        if(currentHealth <= maxHealth * 0.2f && isPlayer && !isLowHealthSoundPlaying)
        {
            //Reproducir el SFX de baja salud
            AudioManager.Instance.PlayLoop("LowHealth");
            isLowHealthSoundPlaying = true;
        }
        else if(currentHealth > maxHealth * 0.2f && isPlayer && isLowHealthSoundPlaying)
        {
            //Detener audio
            AudioManager.Instance.StopLoop("LowHealth");
            isLowHealthSoundPlaying = false;
        }

        if (!isPlayer)
        {
            // Lanzamos el evento OnHit siempre que recibimos daño
            OnHit?.Invoke(transform.position);

            //Solo lanza el material de daño si NO está en Chase ni Alert
            if (fsm != null && fsm.currentState != EnemyState.Chase && fsm.currentState != EnemyState.Alert)
            {
                // Aplicamos el material de daño a todos los MeshRenderers
                foreach (MeshRenderer rend in enemyRends)
                {
                    rend.material = damagedMat;
                }
                Invoke(nameof(ResetDamageMat), 0.1f);
            }
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
        foreach(MeshRenderer rend in enemyRends)
        {
            rend.material = baseMat;
        }
    }

    //Gestiona la muerte del enemigo y ejecuta el evento correspondiente
    void Die()
    {
        currentHealth = 0;
        if (!isPlayer)
        {
            OnDeath?.Invoke();

            // Spawnear VFX desde la pool
            if (PoolManager.Instance != null && vfxPoolName != "")
            {
                GameObject vfx = PoolManager.Instance.Spawn(vfxPoolName, transform.position, Quaternion.identity);
                // opcional: si el VFX tiene AutoDespawn, se desactivará solo
            }

            PoolManager.Instance.Despawn(poolName, gameObject);

            // Probabilidad de drop
            if (dropPrefab != null && UnityEngine.Random.value <= percentChance)
            {
                Instantiate(dropPrefab, transform.position, Quaternion.identity);
            }
        }
        else
        {
            //Jugador muere
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    //Método llamado automáticamente al activar el objeto
    private void OnEnable()
    {
        ResetHealth(); //Restauramos la salud y materiales al reaparecer
        isDamageable = true; // Aseguramos que el enemigo pueda recibir daño al reactivarse

        Collider collider = GetComponent<Collider>(); // Obtener el collider
        if (collider != null)
        {
            collider.enabled = true; // Asegurarnos de que el collider esté habilitado
        }

        if (fsm != null)
        {
            fsm.ResetState(); // Si tienes un estado FSM que reiniciar, hazlo aquí
        }
    }

    //Método para desactivar el enemigo
    public void DeactivateEnemy()
    {
        isDamageable = false; // No permitirá más daño cuando está desactivado
        gameObject.SetActive(false); // Desactivamos el enemigo
    }

    //Método para reactivar el enemigo
    public void ReactivateEnemy()
    {
        gameObject.SetActive(true); // Reactivamos el enemigo
        ResetHealth(); // Restauramos la salud
        isDamageable = true; // Permitimos que el enemigo reciba daño nuevamente
    }
}
