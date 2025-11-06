using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GunSystem : MonoBehaviour
{
    #region General Variables
    [Header("General References")]
    [SerializeField] Camera fpsCam;
    [SerializeField] Transform shootPoint;
    [SerializeField] LayerMask impactLayer;
    RaycastHit hit;

    [Header("Ammo UI")]
    [SerializeField] Image ammoBar; //image tipo fill
    [SerializeField] int maxAmmoPercent = 100; //max percentaje
    float currentAmmoPercent; //porcentaje actual
    float percentPerBullet; 

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

    [Header("Bullet Management")]
    [SerializeField] int ammoSize = 30;
    [SerializeField] int bulletsPerTap = 1;
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

    Vector3 lastHitPoint;

    bool shooting;
    bool canShoot;
    bool reloading;
    bool emptyShakeDone;

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
        currentAmmoPercent = maxAmmoPercent;
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

        //Porcentaje de munición
        percentPerBullet = (float)maxAmmoPercent / ammoSize;
        UpdateAmmoUI();
    }

    void Update()
    {
        if (!canShoot || reloading) return;

        if (shooting && bulletsLeft > 0 && recoilCoroutine == null)
        {
            StartCoroutine(ShootRoutine());
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
            weaponCurrentRecoil = Vector3.Lerp(weaponCurrentRecoil, weaponTargetRecoil, Time.deltaTime * weaponRecoilSpeed);
            weaponMesh.localPosition = weaponOriginalPosition + weaponCurrentRecoil;
            weaponTargetRecoil = Vector3.Lerp(weaponTargetRecoil, Vector3.zero, Time.deltaTime * weaponRecoilSpeed);
        }

        // DAMPING ARMA (solo si no está recargando)
        if (weaponMesh != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            weaponTargetEuler.y = weaponOriginalEuler.y + mouseDelta.x * 0.1f;
            weaponTargetEuler.x = weaponOriginalEuler.x - mouseDelta.y * 0.1f;
            weaponCurrentEuler = Vector3.Lerp(weaponCurrentEuler, weaponTargetEuler, Time.deltaTime * weaponDamping);
            weaponMesh.localEulerAngles = weaponCurrentEuler;
        }
    }

    #region Shoot
    IEnumerator ShootRoutine()
    {
        canShoot = false;
        if (!allowButtonHold) shooting = false;
        for (int i = 0; i < bulletsPerTap; i++)
        {
            if (bulletsLeft <= 0) break;
            Shoot();
            bulletsLeft--;
            currentAmmoPercent -= percentPerBullet;
            if (currentAmmoPercent < 0) currentAmmoPercent = 0;
            UpdateAmmoUI();
        }

        
        yield return new WaitForSeconds(shootingCooldown);
        canShoot = true;
        if (bulletsLeft <= 0) shooting = false;
        recoilCoroutine = null;
    }

    void Shoot()
    {

        //SFX (SHOOT):
        AudioManager.Instance.Play("Shoot");

        Vector3 direction = fpsCam.transform.forward;
        float currentSpread = GetCurrentSpread();
        float spreadX = Random.Range(-currentSpread, currentSpread);
        float spreadY = Random.Range(-currentSpread, currentSpread);

        Vector3 spreadDir = fpsCam.transform.forward;
        spreadDir += fpsCam.transform.right * Mathf.Tan(spreadX * Mathf.Deg2Rad);
        spreadDir += fpsCam.transform.up * Mathf.Tan(spreadY * Mathf.Deg2Rad);
        spreadDir.Normalize();
        
        Debug.DrawRay(fpsCam.transform.position, spreadDir * range, Color.red, 1f);

        if (Physics.Raycast(fpsCam.transform.position, spreadDir, out hit, range, impactLayer))
        {
            Health health = hit.collider.GetComponent<Health>() ?? hit.collider.GetComponentInParent<Health>();
            if (health != null)
            {
                int appliedDamage = hit.collider.gameObject.layer == LayerMask.NameToLayer("HeadShoot") ? 100 : damage;
                health.TakeDamage(appliedDamage);
            }
        }
        
        ApplyRecoil();
        
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
        camTargetRecoil.x -= recoilAmount;
        if (weaponMesh != null) weaponTargetRecoil = new Vector3(0f, 0f, -weaponRecoilBack);
    }
    #endregion

    #region Shake
    IEnumerator EmptyShakeRoutine()
    {
        if (weaponMesh == null) yield break;

        Quaternion originalRot = weaponMesh.localRotation;

        float duration = 0.4f; // duración total de la animación
        float elapsed = 0f;

        // Animación tipo NO: de lado a lado en Z suavemente
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Oscilación en Z: usa Sin para suavidad
            float zRotation = Mathf.Sin(t * Mathf.PI * 2f) * 5f; // 5 grados a cada lado
            weaponMesh.localRotation = originalRot * Quaternion.Euler(0f, 0f, zRotation);

            yield return null;
        }

        // Reset exacto
        weaponMesh.localRotation = originalRot;
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

    #region Inputs
    void HandleShoot()
    {
        if (OxygenPickUp.IsConsumingOxygen) return; //bloqueamos disparo si esta consumiendo oxigeno.

        // Si no hay balas, lanzar la animación de sacudida
        if (bulletsLeft <= 0)
        {
            shooting = false;
            StartCoroutine(EmptyShakeRoutine());
            return;
        }
        shooting = true;
    } 
    #endregion
}
