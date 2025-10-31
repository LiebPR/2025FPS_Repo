using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunSystem : MonoBehaviour
{
    #region General Variables
    [Header("General References")]
    [SerializeField] Camera fpsCam; //ref si disparamos desde el centro de la cam
    [SerializeField] Transform shootPoint; //ref si queremos disparar desde la punta del cañon
    [SerializeField] LayerMask impactLayer; //layer con la que el Raycast interactúa
    RaycastHit hit; //almacén de la información de los objetos con los que impactan los disparos

    [Header("Weapon Parameters")]
    [SerializeField] int damage = 10; //daño del arma
    [SerializeField] float range = 100f; //distancia de disparo
    [SerializeField] float baseSpreadAngle = 1f; //dispersión base en grados
    [SerializeField] float moveSpreadMultiplier = 1.5f; //aumento de dispersión al moverse
    [SerializeField] float sprintSpreadMultiplier = 2f; //aumento de dispersión al moverse
    [SerializeField] float jumpSpreadMultiplier = 2.5f; //aumento de dispersión al saltar
    [SerializeField] float shootingCooldown = 0.2f; //tiempo entre disparos
    [SerializeField] float reloadTime = 1.5f; //tiempo entre disparos
    [SerializeField] bool allowButtonHold = false; //si se dispara click a click o por mantener

    [Header("Recoil Settings")]
    [SerializeField] float recoilAmount = 2f; //grados que la camara subirá
    [SerializeField] float recoilSpeed = 10f; //velocidad de subida del recoil
    [SerializeField] float recoilRecovery = 5f; //velocidad a la que vuelve a la posición original

    [Header("Bullet Management")]
    [SerializeField] int ammoSize = 30; //cantidad max de balas por cargador
    [SerializeField] int bulletsPerTap = 1; //cantidad de balas que se disparan por disparo
    int bulletsLeft; //cantidad de balas dentro del cargador actual

    [Header("Weapon Recoil References")]
    [SerializeField] Transform weaponMesh; //referencia al mesh del arma
    [SerializeField] float weaponRecoilBack = 0.1f; //distancia que retrocede el arma
    [SerializeField] float weaponRecoilSpeed = 10f; //velocidad de retroceso del arma
    Vector3 camCurrentRecoil;
    Vector3 camTargetRecoil; 
    Vector3 camOriginalRotation; //rotación original de la cámara
    Vector3 weaponCurrentRecoil; 
    Vector3 weaponTargetRecoil; 
    Vector3 weaponOriginalPosition; //posición original del arma

    [Header("Damping Mesh")]
    [SerializeField] float weaponDamping = 10f;
    Vector3 weaponOriginalEuler; //Rotación base del arma
    Vector3 weaponTargetEuler; //rotación objetivo (según imput horizontal)
    Vector3 weaponCurrentEuler; //rotación actual interpolada

    [Header("Feedback References")]
    //[SerializeField] GameObject impactEffect; //Referencia al VFX de impacto de bala

    Vector3 lastHitPoint;

    //Bools de estado
    bool shooting; //Indica que estamos disparando
    bool canShoot; //Indica que en este momento del juego se puede disparar
    bool reloading; //Indica si estamos en proceso de recarga

    Coroutine recoilCoroutine;
    #endregion

    #region Getters
    public bool IsShooting => shooting;
    public Vector3 LastHitPoint => lastHitPoint;
    #endregion

    #region References
    FPSController playerController;
    #endregion

    private void Awake()
    {
        playerController = GetComponent<FPSController>();
        bulletsLeft = ammoSize; //Al inicio de la partida, tenemos cargador lleno
        canShoot = true;
    }

    #region Input Event Suscription
    private void OnEnable()
    {
        InputManager.OnShootEvent += HandleShoot;
        InputManager.OnReloadEvent += HandleReload;
    }

    private void OnDisable()
    {
        InputManager.OnShootEvent -= HandleShoot;
        InputManager.OnReloadEvent -= HandleReload;
    }
    #endregion

    void Start()
    {
        //impactEffect.SetActive(false); //Apaga el efecto de impacto al iniciar el juego
        if(weaponMesh != null)
        {
            //Recoil:
            weaponOriginalPosition = weaponMesh.localPosition;

            //Damping
            weaponOriginalEuler = weaponMesh.localEulerAngles;
            weaponCurrentEuler = weaponOriginalEuler;
        }

        camOriginalRotation = fpsCam.transform.localEulerAngles;
    }

    void Update()
    {
        if (!canShoot || reloading) return;
        if (shooting && bulletsLeft > 0 && recoilCoroutine == null)
        {
            //Inicializar la corrutina de disparo
            StartCoroutine(ShootRoutine());
        }
    }

    private void LateUpdate()
    {
        //RECOIL:
        //CAMARA: 
        //Suavizado de movimiento de la cámara hacia el target recoil
        camCurrentRecoil = Vector3.Lerp(camCurrentRecoil, camTargetRecoil, Time.deltaTime * recoilSpeed);
        fpsCam.transform.localEulerAngles = camOriginalRotation + camCurrentRecoil;

        //Hacemos que el recoil se disipe con el tiempo
        camTargetRecoil = Vector3.Lerp(camTargetRecoil, Vector3.zero, Time.deltaTime * recoilRecovery);

        //ARMA:
        if (weaponMesh != null)
        {
            //Movimiento suave del arma hacia atrás y de vuelta.
            weaponCurrentRecoil = Vector3.Lerp(weaponCurrentRecoil, weaponTargetRecoil, Time.deltaTime * weaponRecoilSpeed);
            weaponMesh.localPosition = weaponOriginalPosition + weaponCurrentRecoil;

            //Volver al punto original suavemente
            weaponTargetRecoil = Vector3.Lerp(weaponTargetRecoil, Vector3.zero, Time.deltaTime * weaponRecoilSpeed);
        }

        //DAMPING: 
        //ARMA: 
        if (weaponMesh != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue(); // nuevo Input System

            // Definir rotación objetivo según movimiento del mouse
            weaponTargetEuler.y = weaponOriginalEuler.y + mouseDelta.x * 0.1f; // rotación horizontal
            weaponTargetEuler.x = weaponOriginalEuler.x - mouseDelta.y * 0.1f; // rotación vertical (invertida para FPS típico)

            // Interpolación suave hacia el objetivo
            weaponCurrentEuler = Vector3.Lerp(weaponCurrentEuler, weaponTargetEuler, Time.deltaTime * weaponDamping);

            // Aplicar rotación
            weaponMesh.localEulerAngles = weaponCurrentEuler;
        }
    }

    #region Shoot
    IEnumerator ShootRoutine()
    {
        canShoot = false; //Previene la acumulación por frame de disparos
        if (!allowButtonHold) shooting = false; //Configuración del disparo por tap
        for (int i = 0; i < bulletsPerTap; i++)
        {
            if (bulletsLeft <= 0) break; //Segunda prevención de errores

            Shoot();
            bulletsLeft--;
        }

        yield return new WaitForSeconds(shootingCooldown);
        canShoot = true; //Resetea la posibilidad de disparar

        if(bulletsLeft <= 0)
            shooting = false; //seguridad add al quedarse sin balas.
        recoilCoroutine = null;
    }

    void Shoot()
    {
        //ESTE ES EL MÉTODO MÁS IMPORTANTE
        //AQUÍ SE DEFINE EL DISPARO POR RAYCAST

        //Almacenar la dirección del disparo
        Vector3 direction = fpsCam.transform.forward; //dirección base(frontal  de la cámara)
        
        //Calcular el spread actual según el estado del jugador
        float currentSpread = GetCurrentSpread();

        //Aplicar dispersión angular realista
        float spreadX = Random.Range(-currentSpread, currentSpread);
        float spreadY = Random.Range(-currentSpread, currentSpread);

        Vector3 spreadDirection = fpsCam.transform.forward;
        spreadDirection += fpsCam.transform.right * Mathf.Tan(spreadX * Mathf.Deg2Rad);
        spreadDirection += fpsCam.transform.up * Mathf.Tan(spreadY * Mathf.Deg2Rad);
        spreadDirection.Normalize();

        Debug.DrawRay(fpsCam.transform.position, spreadDirection * range, Color.red, 1f);

        //DECLARACIÓN DEL RAYCAST
        //Physics.Raycast(Origen del rayo, dirección, almacén de info de impacto, longitud del rayo, layer a la que impacta (opcional)
        if (Physics.Raycast(fpsCam.transform.position, spreadDirection, out hit, range, impactLayer))
        {
            //AQUI PUEDO CODEAR TODOS LOS EFECTOS QUE QUIERO PARA MI INTERACCIÓN
            
            Health health = hit.collider.GetComponent<Health>();
            if (health == null)
                health = hit.collider.GetComponentInParent<Health>();

            if (health != null)
            {
                int appliedDamage = damage;
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("HeadShoot"))
                    appliedDamage = 100;

                health.TakeDamage(appliedDamage);
            }
        }

        ApplyRecoil();
    }
    #endregion

    #region Spread
    float GetCurrentSpread()
    {
        float currentSpread = baseSpreadAngle;

        if (playerController == null)
            return currentSpread;

        // Multiplicadores acumulativos según estado
        if (!playerController.IsGrounded)
            currentSpread *= jumpSpreadMultiplier;

        if (playerController.IsSprinting)
            currentSpread *= sprintSpreadMultiplier;
        else if (playerController.HasMovementInput())
            currentSpread *= moveSpreadMultiplier;

        if (playerController.IsCrouching)
            currentSpread *= 0.75f; // más precisión al estar agachado

        return currentSpread;
    }
    #endregion

    #region Recoil
    public void ApplyRecoil()
    {
        //Añadimos un poco de kick vertical (hacia arriba) 
        camTargetRecoil.x -= recoilAmount;

        //Recoil acumulativo del arma
        if (weaponMesh != null)
        {
            //Recoil hacía atras
            weaponTargetRecoil = new Vector3(0f, 0f, -weaponRecoilBack);
        }
    }
    #endregion

    #region Reload
    void Reload()
    {
        if (bulletsLeft < ammoSize && !reloading)
        {
            shooting = false; //evita disparos fantasma durante la recarga
            canShoot = false; //bloquea la posibilidad de disparar
            StartCoroutine(ReloadRoutine());
        }
        if(recoilCoroutine != null)
        {
            StopCoroutine(recoilCoroutine);
            recoilCoroutine = null;
        }
    }

    IEnumerator ReloadRoutine()
    {
        reloading = true;
        //Se llama a la animación de recarga
        yield return new WaitForSeconds(reloadTime);
        bulletsLeft = ammoSize;
        reloading = false;
        canShoot = true; //cuando la recarga termina se reavilita el disparo.
    }
    #endregion

    #region Inputs Handlers
    void HandleShoot()
    {
        shooting = true;
    }

    void HandleReload()
    {
        Reload();
    }
    #endregion
}
