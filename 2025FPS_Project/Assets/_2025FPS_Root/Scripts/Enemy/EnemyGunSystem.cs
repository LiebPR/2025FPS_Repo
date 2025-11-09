using System.Collections;
using UnityEngine;

/// <summary>
/// EnemyGunSystem: Gestiona el disparo del enemigo de manera independiente al FSM.
/// Solo dispara si el enemigo está en estado Chase y tiene visión directa del jugador. 
/// </summary>
public class EnemyGunSystem : MonoBehaviour
{
    [SerializeField] Transform shootPoint;
    [SerializeField] string chargeEffect = "ChargeEffect"; // Efecto de partículas al disparar
    [SerializeField] PoolManager pool; // Pool de partículas

    #region New Variables
    [SerializeField] Transform childToShrink; // Hijo al que se le cambiará la escala (puedes asignar el hijo desde el editor)
    [SerializeField] float waitTimeBeforeShoot = 1f; // Tiempo de espera antes de disparar
    [SerializeField] float shrinkDuration = 0.5f; // Tiempo en el que el hijo se encoge
    [SerializeField] float shrinkFactor = 0.8f; // Factor de reducción de tamaño (80% del tamaño original)
    [SerializeField] float restoreSpeed = 5f; // Velocidad para restaurar el tamaño original
    private Vector3 originalScale; // Almacena el tamaño original del hijo
    #endregion

    #region State Variables
    bool canShoot = true;
    RaycastHit hit;
    Coroutine shootingRoutine;
    #endregion

    #region References
    [SerializeField] Enemy enemyData;
    EnemyStateMachine fsm;
    VisionSystem vision;
    LevitationMeshEffect levitationEffect;
    EnemyMovement enemyMovement;
    #endregion

    private void Awake()
    {
        fsm = GetComponent<EnemyStateMachine>();
        vision = GetComponent<VisionSystem>();
        enemyMovement = GetComponent<EnemyMovement>();
        pool = PoolManager.Instance; // Referencia al PoolManager de forma automática
        levitationEffect = GetComponentInChildren<LevitationMeshEffect>();
        

        // Almacena el tamaño original del hijo
        if (childToShrink != null)
        {
            originalScale = childToShrink.localScale;
        }
    }

    private void OnEnable()
    {
        canShoot = true;
        if (fsm == null) fsm = GetComponent<EnemyStateMachine>();
        if (vision == null) vision = GetComponent<VisionSystem>();
    }

    private void OnDisable()
    {
        if (shootingRoutine != null)
        {
            StopCoroutine(shootingRoutine); // Detiene cualquier coroutine activa de disparo
        }
    }

    private void Update()
    {
        if (CanAttackTarget())
        {
            TryShoot();
        }
    }

    #region Shooting Logic
    void TryShoot()
    {
        if (!canShoot) return;
        shootingRoutine = StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        canShoot = false;

        // Espera un tiempo antes de empezar a encoger
        yield return new WaitForSeconds(waitTimeBeforeShoot);
        if (levitationEffect != null)
        {
            levitationEffect.StopLevitating();  // Detener la levitación
        }

        // Detener movimiento antes de disparar
        enemyMovement.StopMovement();

        // Encoge el hijo suavemente
        if (childToShrink != null)
        {
            //VFX
            if (!string.IsNullOrEmpty(chargeEffect) && pool.HasPool(chargeEffect))
            {
                GameObject chargeVFX = pool.Spawn(chargeEffect, shootPoint.position, shootPoint.rotation);
                if (chargeVFX.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
                {
                    ps.Play();
                }
            }
            yield return StartCoroutine(SmoothShrink(childToShrink, shrinkFactor, shrinkDuration));
        }

        // Disparo
        Shoot();

        

        // Restaura la escala del hijo rápidamente después de disparar
        if (childToShrink != null)
        {
            yield return StartCoroutine(SmoothRestore(childToShrink, restoreSpeed));
        }

        levitationEffect.StartLevitating();
        // Reactivar movimiento después de disparar
        enemyMovement.ResumeMovement();

        yield return new WaitForSeconds(enemyData.shootingCooldown);
        canShoot = true;
    }

    void Shoot()
    {
        if (shootPoint == null) return;

        Vector3 direction = shootPoint.forward;

        if (Physics.Raycast(shootPoint.position, direction, out hit, enemyData.range, enemyData.impactLayer))
        {
            Debug.DrawRay(shootPoint.position, direction * enemyData.range, Color.cyan, 1f);

            // Daño al jugador
            if (hit.collider.TryGetComponent(out Health health))
            {
                health.TakeDamage(enemyData.damage);
            }
        }
    }
    #endregion

    #region Conditions 
    bool CanAttackTarget()
    {
        if (vision == null || fsm == null) return false;
        if (!vision.CanSeeTarget) return false;
        if (fsm.currentState != EnemyState.Chase) return false;
        if (!IsTargetInAttackArea()) return false;

        return true;
    }

    bool IsTargetInAttackArea()
    {
        if (vision.Target == null) return false;

        float distance = Vector3.Distance(transform.position, vision.Target.position);
        return distance <= enemyData.attackRange;
    }
    #endregion

    #region Smooth Scaling Logic
    /// <summary>
    /// Realiza un encogimiento suave en el hijo especificado.
    /// </summary>
    /// <param name="child">Transform del hijo a encoger</param>
    /// <param name="shrinkFactor">Factor de reducción de la escala</param>
    /// <param name="duration">Duración de la animación de encogimiento</param>
    IEnumerator SmoothShrink(Transform child, float shrinkFactor, float duration)
    {
        // Realiza el encogimiento suave
        Vector3 originalScale = child.localScale;
        Vector3 targetScale = originalScale * shrinkFactor;

        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            child.localScale = Vector3.Lerp(originalScale, targetScale, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Asegura que la escala final sea exactamente la deseada
        child.localScale = targetScale;
    }

    /// <summary>
    /// Restaura suavemente la escala del hijo a su tamaño original.
    /// </summary>
    /// <param name="child">Transform del hijo a restaurar</param>
    /// <param name="speed">Velocidad de restauración de la escala</param>
    IEnumerator SmoothRestore(Transform child, float speed)
    {
        // Utiliza el tamaño original guardado previamente
        while (child.localScale != originalScale)
        {
            child.localScale = Vector3.MoveTowards(child.localScale, originalScale, speed * Time.deltaTime);
            yield return null;
        }

        // Asegura que la escala final sea exactamente la original
        child.localScale = originalScale;
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
    }
    #endregion
}
