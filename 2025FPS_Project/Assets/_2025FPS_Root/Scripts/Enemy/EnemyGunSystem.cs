using System.Collections;
using UnityEngine;

/// <summary>
/// EnemyGunSystem: Gestiona el disparo del enemigo de manera independiente al FSM.
/// Solo dispara si el enemigo está en estado Chase y tiene visión directa del jugador. 
/// </summary>
public class EnemyGunSystem : MonoBehaviour
{
    #region General Variables
    [Header("General References")]
    [SerializeField] Transform shootPoint; //punto de disparo del enemigo
    [SerializeField] LayerMask impactLayer; //capas con las que puede colisionar el raycast

    [Header("Wapon Parameters")]
    [SerializeField] int damage = 10;
    [SerializeField] float range = 10f;
    [SerializeField] float shootingCooldown = 1f;
    [SerializeField] float attackRange = 20f; //distancia máxima a la que puede atacar

    bool canShoot = true;
    RaycastHit hit;
    Coroutine shootingRoutine;
    #endregion

    #region References
    EnemyStateMachine fsm;
    VisionSystem vision;
    #endregion

    private void Awake()
    {
        fsm = GetComponent<EnemyStateMachine>();
        vision = GetComponent<VisionSystem>();
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
        yield return new WaitForSeconds(shootingCooldown);
        canShoot = true;
    }

    void Shoot()
    {
        if (shootPoint == null) return;

        //Dispara solo hacia adelante, según la rotación actual del enemigo
        Vector3 direction = shootPoint.forward;

        if(Physics.Raycast(shootPoint.position, direction, out hit, range, impactLayer))
        {
            Debug.DrawRay(shootPoint.position, direction * range, Color.cyan, 1f);

            if(hit.collider.TryGetComponent(out Health health))
            {
                health.TakeDamage(damage);
            }
        }
    }
    #endregion

    #region Conditions 
    bool CanAttackTarget()
    {
        if(vision == null || fsm == null) return false;
        if(!vision.CanSeeTarget) return false;
        if(fsm.currentState != EnemyState.Chase) return false;
        if(!IsTargetInAttackArea()) return false;

        return true;
    }

    bool IsTargetInAttackArea()
    {
        if(vision.Target == null) return false;

        float distance = Vector3.Distance(transform.position, vision.Target.position);
        return distance <= attackRange;
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        //Visualización del área de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    #endregion
}
