using System.Collections;
using UnityEngine;

/// <summary>
/// EnemyGunSystem: Gestiona el disparo del enemigo de manera independiente al FSM.
/// Solo dispara si el enemigo está en estado Chase y tiene visión directa del jugador. 
/// </summary>
public class EnemyGunSystem : MonoBehaviour
{
    [SerializeField] Transform shootPoint;

    #region State Variables
    bool canShoot = true;
    RaycastHit hit;
    Coroutine shootingRoutine;
    #endregion

    #region References
    [SerializeField] Enemy enemyData;

    EnemyStateMachine fsm;
    VisionSystem vision;
    #endregion

    private void Awake()
    {
        fsm = GetComponent<EnemyStateMachine>();
        vision = GetComponent<VisionSystem>();
    }

    private void OnEnable()
    {
        canShoot = true; // Restablece el valor de canShoot cuando el enemigo se activa

        // Reasignar las referencias si es necesario
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
        Shoot();
        yield return new WaitForSeconds(enemyData.shootingCooldown);
        canShoot = true;
    }

    void Shoot()
    {
        if (shootPoint == null) return;

        // Dispara solo hacia adelante, según la rotación actual del enemigo
        Vector3 direction = shootPoint.forward;

        if (Physics.Raycast(shootPoint.position, direction, out hit, enemyData.range, enemyData.impactLayer))
        {
            Debug.DrawRay(shootPoint.position, direction * enemyData.range, Color.cyan, 1f);

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

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        // Visualización del área de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
    }
    #endregion
}
