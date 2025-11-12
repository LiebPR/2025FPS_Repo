using System.Collections;
using UnityEngine;

public class EnemyGunSystem : MonoBehaviour
{
    [SerializeField] Transform shootPoint;
    [SerializeField] string chargeEffect = "ChargeEffect";
    [SerializeField] private Material laserMaterial;
    [SerializeField] Transform childToShrink;
    [SerializeField] float waitTimeBeforeShoot = 1f;
    [SerializeField] float shrinkDuration = 0.5f;
    [SerializeField] float shrinkFactor = 0.8f;
    [SerializeField] float restoreSpeed = 5f;

    Vector3 originalScale;
    Coroutine shootingRoutine;
    bool canShoot = true;

    [SerializeField] Enemy enemyData;
    EnemyStateMachine fsm;
    VisionSystem vision;
    LevitationMeshEffect levitationEffect;
    EnemyMovement enemyMovement;
    Health healthComponent;

    GameObject activeChargeVFX;

    private void Awake()
    {
        fsm = GetComponent<EnemyStateMachine>();
        vision = GetComponent<VisionSystem>();
        enemyMovement = GetComponent<EnemyMovement>();
        levitationEffect = GetComponentInChildren<LevitationMeshEffect>();
        healthComponent = GetComponent<Health>();

        if (childToShrink != null)
            originalScale = childToShrink.localScale;
    }

    private void OnEnable()
    {
        canShoot = true;
        if (healthComponent != null)
            healthComponent.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (shootingRoutine != null)
            StopCoroutine(shootingRoutine);

        if (healthComponent != null)
            healthComponent.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        if (CanAttackTarget())
        {
            TryShoot();
        }
    }

    void TryShoot()
    {
        if (!canShoot) return;
        shootingRoutine = StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        canShoot = false;

        // Espera antes de disparar
        yield return new WaitForSeconds(waitTimeBeforeShoot);

        if (!IsAlive()) { canShoot = true; yield break; }

        levitationEffect?.StopLevitating();
        enemyMovement.StopMovement();
        AudioManager.Instance.Play("EnemyAttack");

        if (childToShrink != null)
        {
            // Spawn Charge VFX
            if (!string.IsNullOrEmpty(chargeEffect) && PoolManager.Instance.HasPool(chargeEffect))
            {
                activeChargeVFX = PoolManager.Instance.Spawn(chargeEffect, shootPoint.position, shootPoint.rotation);
                if (activeChargeVFX.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
                    ps.Play();
            }

            yield return StartCoroutine(SmoothShrink(childToShrink, shrinkFactor, shrinkDuration));
            if (!IsAlive()) { yield return SmoothRestoreImmediate(); yield break; }
        }

        Shoot();
        if (!IsAlive()) { yield return SmoothRestoreImmediate(); yield break; }

        if (childToShrink != null)
            yield return StartCoroutine(SmoothRestore(childToShrink, restoreSpeed));

        levitationEffect?.StartLevitating();
        yield return new WaitForSeconds(enemyData.shootingCooldown);
        canShoot = true;
    }

    void Shoot()
    {
        if (!IsAlive() || shootPoint == null) return;

        Vector3 direction = shootPoint.forward;
        enemyMovement.ResumeMovement();

        if (Physics.Raycast(shootPoint.position, direction, out RaycastHit hit, enemyData.range, enemyData.impactLayer))
        {
            GameObject laser = PoolManager.Instance.Spawn("LaserPool", shootPoint.position, shootPoint.rotation);
            if (laser.TryGetComponent<Laser>(out Laser laserScript))
                laserScript.Initialize(shootPoint.position, hit.point, laserMaterial);

            if (hit.collider.TryGetComponent(out Health health))
                health.TakeDamage(enemyData.damage);
        }
    }

    bool CanAttackTarget()
    {
        if (!vision || !fsm || !IsAlive()) return false;
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

    IEnumerator SmoothShrink(Transform child, float factor, float duration)
    {
        Vector3 start = child.localScale;
        Vector3 target = start * factor;
        float t = 0f;

        while (t < duration)
        {
            if (!IsAlive()) yield break;
            t += Time.deltaTime;
            float smooth = Mathf.SmoothStep(0f, 1f, t / duration);
            child.localScale = Vector3.Lerp(start, target, smooth);
            yield return null;
        }

        child.localScale = target;
    }

    IEnumerator SmoothRestore(Transform child, float speed)
    {
        while (child.localScale != originalScale)
        {
            child.localScale = Vector3.MoveTowards(child.localScale, originalScale, speed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator SmoothRestoreImmediate()
    {
        if (childToShrink != null)
        {
            childToShrink.localScale = originalScale;
            if (activeChargeVFX != null)
            {
                PoolManager.Instance.Despawn(chargeEffect, activeChargeVFX);
                activeChargeVFX = null;
            }
        }

        levitationEffect?.StartLevitating();
        enemyMovement.ResumeMovement();
        canShoot = true;
        yield break;
    }

    void HandleDeath()
    {
        canShoot = false;

        if (shootingRoutine != null)
        {
            StopCoroutine(shootingRoutine);
            shootingRoutine = null;
        }
        AudioManager.Instance.Stop("EnemyAttack");
        if (childToShrink != null)
            childToShrink.localScale = originalScale;

        if (activeChargeVFX != null)
        {
            PoolManager.Instance.Despawn(chargeEffect, activeChargeVFX);
            activeChargeVFX = null;
        }

        levitationEffect?.StopLevitating();
        enemyMovement.StopMovement();
    }

    bool IsAlive() => healthComponent != null && healthComponent.IsAlive; // Aquí puedes reemplazar por un flag real de Health
}
