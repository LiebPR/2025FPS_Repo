using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GunSystem : MonoBehaviour
{
    #region General Variables
    [Header("Particle Gun")]
    [SerializeField] string muzzleFlash = "MuzzleFlash";
    [SerializeField] string hitEffect = "HitEffect";

    [Header("General References")]
    [SerializeField] Camera fpsCam;
    [SerializeField] Transform shootPoint;
    [SerializeField] LayerMask impactLayer;
    [SerializeField] PoolManager pool;
    RaycastHit hit;

    [Header("Ammo UI")]
    [SerializeField] Image ammoBar; //image tipo fill

    [Header("Weapon Parameters")]
    [SerializeField] int damage = 10;
    [SerializeField] float range = 100f;
    [SerializeField] float baseSpreadAngle = 1f;
    [SerializeField] float moveSpreadMultiplier = 1.5f;
    [SerializeField] float sprintSpreadMultiplier = 2f;
    [SerializeField] float jumpSpreadMultiplier = 2.5f;
    [SerializeField] float shootingCooldown = 0.2f;
    [SerializeField] bool allowButtonHold = false;

    [Header("Recoil Settings")]
    [SerializeField] float recoilAmount = 2f;
    [SerializeField] float recoilSpeed = 10f;
    [SerializeField] float recoilRecovery = 5f;

    [Header("Weapon Shake Effect")]
    [SerializeField] float weaponShakeDuration = 0.5f;
    [SerializeField] float weaponShakeIntensity = 0.2f;
    Vector3 originalWeaponPosition;
    Quaternion originalWeaponRotation;
    private bool isShakeActive = false;
    bool isWeaponShaking = false;

    [Header("Bullet Management")]
    [SerializeField] int ammoSize = 30;
    int bulletsLeft;

    [Header("Weapon Recoil References")]
    [SerializeField] Transform weaponMesh;
    [SerializeField] float weaponRecoilBack = 0.1f;
    [SerializeField] float weaponRecoilSpeed = 10f;
    Vector3 camCurrentRecoil;
    Vector3 camTargetRecoil;
    Vector3 camOriginalRotation;
    Vector3 weaponCurrentRecoil;
    Vector3 weaponTargetRecoil;
    Vector3 weaponOriginalPosition;

    [Header("Damping Mesh")]
    [SerializeField] float weaponDamping = 10f;
    Vector3 weaponOriginalEuler;
    Vector3 weaponTargetEuler;
    Vector3 weaponCurrentEuler;

    [Header("Crosshair")]
    [SerializeField] RectTransform crosshair;
    [SerializeField] float crosshairScaleAmount = 1.2f;
    [SerializeField] float crosshairRotateAmount = 10f;
    [SerializeField] float crosshairReturnSpeed = 8f;

    Vector3 lastHitPoint;
    Vector3 crosshairOriginalScale;
    Quaternion crosshairOriginalRotation;

    bool shooting;
    bool canShoot;

    Coroutine recoilCoroutine;
    #endregion

    #region Getters
    public bool IsShooting => shooting;
    public Vector3 LastHitPoint => lastHitPoint;
    public int BulletsLeft => bulletsLeft;
    public int AmmoSize => ammoSize;
    #endregion

    #region References
    FPSController playerController;
    #endregion

    private void Awake()
    {
        playerController = GetComponent<FPSController>();
        bulletsLeft = ammoSize;
        canShoot = true;
    }

    #region Inputs
    private void OnEnable()
    {
        InputManager.OnShootEvent += HandleShoot;
    }

    private void OnDisable()
    {
        InputManager.OnShootEvent -= HandleShoot;
    }
    #endregion

    void Start()
    {
        if (weaponMesh != null)
        {
            weaponOriginalPosition = weaponMesh.localPosition;
            weaponOriginalEuler = weaponMesh.localEulerAngles;
            weaponCurrentEuler = weaponOriginalEuler;
        }
        camOriginalRotation = fpsCam.transform.localEulerAngles;

        if (crosshair != null)
        {
            crosshairOriginalScale = crosshair.localScale;
            crosshairOriginalRotation = crosshair.localRotation;
        }

        UpdateAmmoUI();
    }

    void Update()
    {
        if (!canShoot) return;

        if (shooting)
        {
            // Solo intentamos disparar si hay balas
            if (bulletsLeft > 0)
            {
                // Si hay munición y podemos disparar
                if (recoilCoroutine == null)
                {
                    recoilCoroutine = StartCoroutine(ShootRoutine()); // Asignar el Coroutine
                }
            }
        }
    }

    private void LateUpdate()
    {
        // CAMARA RECOIL
        camCurrentRecoil = Vector3.Lerp(camCurrentRecoil, camTargetRecoil, Time.deltaTime * recoilSpeed);
        fpsCam.transform.localEulerAngles = camOriginalRotation + camCurrentRecoil;
        camTargetRecoil = Vector3.Lerp(camTargetRecoil, Vector3.zero, Time.deltaTime * recoilRecovery);

        // ARMA RECOIL
        if (weaponMesh != null)
        {
            if (isShakeActive) return;
            weaponCurrentRecoil = Vector3.Lerp(weaponCurrentRecoil, weaponTargetRecoil, Time.deltaTime * weaponRecoilSpeed);
            weaponMesh.localPosition = weaponOriginalPosition + weaponCurrentRecoil;
            weaponTargetRecoil = Vector3.Lerp(weaponTargetRecoil, Vector3.zero, Time.deltaTime * weaponRecoilSpeed);
        }

        // DAMPING ARMA (solo si no está recargando)
        if (weaponMesh != null)
        {
            if (isShakeActive) return;
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            weaponTargetEuler.y = weaponOriginalEuler.y + mouseDelta.x * 0.1f;
            weaponTargetEuler.x = weaponOriginalEuler.x - mouseDelta.y * 0.1f;
            weaponCurrentEuler = Vector3.Lerp(weaponCurrentEuler, weaponTargetEuler, Time.deltaTime * weaponDamping);
            weaponMesh.localEulerAngles = weaponCurrentEuler;
        }

        //Return Crosshair
        if (crosshair != null)
        {
            crosshair.localScale = Vector3.Lerp(crosshair.localScale, crosshairOriginalScale, Time.deltaTime * crosshairReturnSpeed);
            crosshair.localRotation = Quaternion.Lerp(crosshair.localRotation, crosshairOriginalRotation, Time.deltaTime * crosshairReturnSpeed);
        }
    }

    #region Shoot
    IEnumerator ShootRoutine()
    {
        canShoot = false;
        if (!allowButtonHold) shooting = false;

        Shoot();
        bulletsLeft--;

        UpdateAmmoUI();

        yield return new WaitForSeconds(shootingCooldown);
        canShoot = true;

        if (bulletsLeft <= 0) shooting = false;

        recoilCoroutine = null;
    }

    void Shoot()
    {
        //SFX (SHOOT):
        AudioManager.Instance.Play("PlayerShoot");

        float currentSpread = GetCurrentSpread();
        float spreadX = Random.Range(-currentSpread, currentSpread);
        float spreadY = Random.Range(-currentSpread, currentSpread);

        Vector3 spreadDir = fpsCam.transform.forward;
        spreadDir += fpsCam.transform.right * Mathf.Tan(spreadX * Mathf.Deg2Rad);
        spreadDir += fpsCam.transform.up * Mathf.Tan(spreadY * Mathf.Deg2Rad);
        spreadDir.Normalize();

        Debug.DrawRay(fpsCam.transform.position, spreadDir * range, Color.red, 1f);

        // Muzzle flash
        if (shootPoint != null && pool.HasPool(muzzleFlash))
        {
            Vector3 forwardDir = fpsCam.transform.forward;
            Vector3 spawnPos = shootPoint.position + forwardDir * 0.1f;
            Quaternion spawnRot = Quaternion.LookRotation(forwardDir);

            GameObject muzzle = pool.Spawn(muzzleFlash, spawnPos, spawnRot);
            if (muzzle != null && muzzle.activeInHierarchy)
            {
                if (muzzle.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
                {
                    if (ps != null && !ps.isPlaying)  // Verifica si el sistema de partículas no está activo
                    {
                        ps.Play();  // Reproduce las partículas si no está ya en ejecución
                    }
                }
                else
                {
                    Debug.LogWarning("ParticleSystem no encontrado en el GameObject del destello.");
                }

                // Asegúrate de resetear el sistema de partículas antes de liberar el objeto
                ResetMuzzleFlash(muzzle);  // Detenemos y limpiamos el ParticleSystem
                pool.Despawn(muzzle.name, muzzle);  // Liberamos el objeto al pool
            }
            else
            {
                Debug.LogWarning("El GameObject de destello es nulo o no está activo en la jerarquía.");
            }
        }

        // Raycast hit
        if (Physics.Raycast(fpsCam.transform.position, spreadDir, out hit, range, impactLayer))
        {
            Health health = hit.collider.GetComponent<Health>() ?? hit.collider.GetComponentInParent<Health>();
            if (health != null)
            {
                int appliedDamage = hit.collider.gameObject.layer == LayerMask.NameToLayer("HeadShoot") ? 100 : damage;
                health.TakeDamage(appliedDamage);
            }

            // Impact VFX
            if (pool.HasPool(hitEffect))
            {
                GameObject impact = pool.Spawn(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                if (impact.TryGetComponent<ParticleSystem>(out ParticleSystem ps)) ps.Play();
            }

            lastHitPoint = hit.point;
        }

        ApplyRecoil();
        AnimationCrosshair();
    }
    #endregion

    #region Spread
    float GetCurrentSpread()
    {
        float spread = baseSpreadAngle;
        if (playerController == null) return spread;

        if (!playerController.IsGrounded) spread *= jumpSpreadMultiplier;
        if (playerController.IsSprinting) spread *= sprintSpreadMultiplier;
        else if (playerController.HasMovementInput()) spread *= moveSpreadMultiplier;
        if (playerController.IsCrouching) spread *= 0.75f;

        return spread;
    }
    #endregion

    #region Recoil
    public void ApplyRecoil()
    {
        if (isShakeActive) return;

        camTargetRecoil.x -= recoilAmount;
        if (weaponMesh != null) weaponTargetRecoil = new Vector3(0f, 0f, -weaponRecoilBack);
    }
    #endregion

    #region Weapon Shake
    IEnumerator ShakeWeapon()
    {
        isWeaponShaking = true;
        isShakeActive = true;

        originalWeaponRotation = weaponMesh.localRotation;  // Guarda la rotación original del arma (sin tocar la posición)

        float elapsedTime = 0f;

        while (elapsedTime < weaponShakeDuration)
        {
            // Calcular un movimiento de rotación en el eje Y basado en el tiempo
            float shakeAmount = Mathf.Sin(elapsedTime * Mathf.PI * 2f / weaponShakeDuration) * weaponShakeIntensity;

            // Mantener la posición original, solo se modifica la rotación en Y
            weaponMesh.localRotation = Quaternion.Euler(originalWeaponRotation.eulerAngles.x, originalWeaponRotation.eulerAngles.y + shakeAmount, originalWeaponRotation.eulerAngles.z);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Restaurar la rotación original del arma después del shake
        weaponMesh.localRotation = originalWeaponRotation;

        isWeaponShaking = false;
        isShakeActive = false;
    }
    #endregion

    #region Ammo Size
    void UpdateAmmoUI()
    {
        if (ammoBar)
            ammoBar.fillAmount = (float)bulletsLeft / ammoSize;
    }


    public int PickUpAmmo(int units)
    {
        int previousBullets = bulletsLeft;

        bulletsLeft = Mathf.Min(bulletsLeft + units, ammoSize);

        UpdateAmmoUI();

        return bulletsLeft - previousBullets; // devuelve lo que realmente entró
    }

    #endregion

    #region Animation Crosshair
    void AnimationCrosshair()
    {
        if (crosshair == null) return;

        crosshair.localScale = crosshairOriginalScale * crosshairScaleAmount;

        crosshair.localRotation = Quaternion.Euler(crosshairOriginalRotation.eulerAngles.x, crosshairOriginalRotation.eulerAngles.y, crosshairOriginalRotation.eulerAngles.z + crosshairRotateAmount);
    }
    #endregion

    #region Inputs
    void HandleShoot()
    {
        if (OxygenPickUp.IsConsumingOxygen) return; // Bloqueamos disparo si esta consumiendo oxigeno.

        // Si no hay munición, simplemente ignoramos el disparo y no hacemos nada.
        if (bulletsLeft <= 0)
        {
            // Iniciar Shake en el arma
            if (!isWeaponShaking)
            {
                AudioManager.Instance.Play("ShootError");
                StartCoroutine(ShakeWeapon());
            }
            shooting = false;
            return;
        }

        // Si hay munición
        shooting = true;
    }
    #endregion

    public void ResetMuzzleFlash(GameObject muzzle)
    {
        if (muzzle.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // Detener las partículas y limpiar cualquier residual.
        }
        else
        {
            Debug.LogWarning("No se encontró el sistema de partículas en el objeto muzzle.");
        }
    }
}