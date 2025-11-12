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
    RaycastHit hit;

    [Header("Ammo UI")]
    [SerializeField] Image ammoBar;

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

    private void OnEnable()
    {
        InputManager.OnShootEvent += HandleShoot;
    }

    private void OnDisable()
    {
        InputManager.OnShootEvent -= HandleShoot;
    }

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

        if (shooting && bulletsLeft > 0 && recoilCoroutine == null)
        {
            recoilCoroutine = StartCoroutine(ShootRoutine());
        }
    }

    private void LateUpdate()
    {
        // Recoil cámara
        camCurrentRecoil = Vector3.Lerp(camCurrentRecoil, camTargetRecoil, Time.deltaTime * recoilSpeed);
        fpsCam.transform.localEulerAngles = camOriginalRotation + camCurrentRecoil;
        camTargetRecoil = Vector3.Lerp(camTargetRecoil, Vector3.zero, Time.deltaTime * recoilRecovery);

        // Recoil arma
        if (weaponMesh != null && !isShakeActive)
        {
            weaponCurrentRecoil = Vector3.Lerp(weaponCurrentRecoil, weaponTargetRecoil, Time.deltaTime * weaponRecoilSpeed);
            weaponMesh.localPosition = weaponOriginalPosition + weaponCurrentRecoil;
            weaponTargetRecoil = Vector3.Lerp(weaponTargetRecoil, Vector3.zero, Time.deltaTime * weaponRecoilSpeed);

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            weaponTargetEuler.y = weaponOriginalEuler.y + mouseDelta.x * 0.1f;
            weaponTargetEuler.x = weaponOriginalEuler.x - mouseDelta.y * 0.1f;
            weaponCurrentEuler = Vector3.Lerp(weaponCurrentEuler, weaponTargetEuler, Time.deltaTime * weaponDamping);
            weaponMesh.localEulerAngles = weaponCurrentEuler;
        }

        // Crosshair
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
        AudioManager.Instance.Play("PlayerShoot");

        float currentSpread = GetCurrentSpread();
        float spreadX = Random.Range(-currentSpread, currentSpread);
        float spreadY = Random.Range(-currentSpread, currentSpread);

        Vector3 spreadDir = fpsCam.transform.forward;
        spreadDir += fpsCam.transform.right * Mathf.Tan(spreadX * Mathf.Deg2Rad);
        spreadDir += fpsCam.transform.up * Mathf.Tan(spreadY * Mathf.Deg2Rad);
        spreadDir.Normalize();

        Debug.DrawRay(fpsCam.transform.position, spreadDir * range, Color.red, 1f);

        // --- Muzzle Flash ---
        if (shootPoint != null && PoolManager.Instance.HasPool(muzzleFlash))
        {
            Vector3 spawnPos = shootPoint.position + fpsCam.transform.forward * 0.1f;
            Quaternion spawnRot = Quaternion.LookRotation(fpsCam.transform.forward);

            GameObject muzzle = PoolManager.Instance.Spawn(muzzleFlash, spawnPos, spawnRot);
            if (muzzle != null)
            {
                if (muzzle.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.Play();
                }
                StartCoroutine(DespawnAfterParticles(muzzle, muzzle.GetComponent<ParticleSystem>()));
            }
        }

        // --- Raycast hit ---
        if (Physics.Raycast(fpsCam.transform.position, spreadDir, out hit, range, impactLayer))
        {
            Health health = hit.collider.GetComponent<Health>() ?? hit.collider.GetComponentInParent<Health>();
            if (health != null)
            {
                int appliedDamage = hit.collider.gameObject.layer == LayerMask.NameToLayer("HeadShoot") ? 100 : damage;
                health.TakeDamage(appliedDamage);
            }

            if (PoolManager.Instance.HasPool(hitEffect))
            {
                GameObject impact = PoolManager.Instance.Spawn(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                if (impact.TryGetComponent<ParticleSystem>(out ParticleSystem ps)) ps.Play();
            }

            lastHitPoint = hit.point;
        }

        ApplyRecoil();
        AnimateCrosshair();
    }

    IEnumerator DespawnAfterParticles(GameObject obj, ParticleSystem ps)
    {
        if (ps != null)
            yield return new WaitUntil(() => !ps.isPlaying);

        PoolManager.Instance.Despawn(muzzleFlash, obj);
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

        Quaternion originalRotation = weaponMesh.localRotation;
        float elapsed = 0f;

        while (elapsed < weaponShakeDuration)
        {
            float shakeAmount = Mathf.Sin(elapsed * Mathf.PI * 2f / weaponShakeDuration) * weaponShakeIntensity;
            weaponMesh.localRotation = Quaternion.Euler(originalRotation.eulerAngles.x, originalRotation.eulerAngles.y + shakeAmount, originalRotation.eulerAngles.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        weaponMesh.localRotation = originalRotation;
        isWeaponShaking = false;
        isShakeActive = false;
    }
    #endregion

    #region Ammo & Crosshair
    void UpdateAmmoUI()
    {
        if (ammoBar)
            ammoBar.fillAmount = (float)bulletsLeft / ammoSize;
    }

    public int PickUpAmmo(int units)
    {
        int prev = bulletsLeft;
        bulletsLeft = Mathf.Min(bulletsLeft + units, ammoSize);
        UpdateAmmoUI();
        return bulletsLeft - prev;
    }

    void AnimateCrosshair()
    {
        if (crosshair == null) return;
        crosshair.localScale = crosshairOriginalScale * crosshairScaleAmount;
        crosshair.localRotation = Quaternion.Euler(crosshairOriginalRotation.eulerAngles.x,
                                                  crosshairOriginalRotation.eulerAngles.y,
                                                  crosshairOriginalRotation.eulerAngles.z + crosshairRotateAmount);
    }
    #endregion

    #region Inputs
    void HandleShoot()
    {
        if (OxygenPickUp.IsConsumingOxygen) return;

        if (bulletsLeft <= 0)
        {
            if (!isWeaponShaking)
            {
                AudioManager.Instance.Play("ShootError");
                StartCoroutine(ShakeWeapon());
            }
            shooting = false;
            return;
        }

        shooting = true;
    }
    #endregion
}
