using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class GunSystem : MonoBehaviour
{
    #region General Variables
    [Header("General References")]
    [SerializeField] Camera fpsCam; //Ref si disparamos desde el centro de la cam
    [SerializeField] Transform shootPoint; //Ref si queremos disparar desde la punta del cañon
    [SerializeField] LayerMask impactLayer; //Layer con la que el Raycast interactúa
    RaycastHit hit; //Almacén de la información de los objetos con los que impactan los disparos
    

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
    [SerializeField] float recoilAmount = 2f; //Grados que la camara subirá
    [SerializeField] float recoilSpeed = 10f; //Velocidad de subida del recoil
    [SerializeField] float recoilRecovery = 5f; //Velocidad a la que vuelve a la posición original

    [Header("Bullet Management")]
    [SerializeField] int ammoSize = 30; //Cantidad max de balas por cargador
    [SerializeField] int bulletsPerTap = 1; //Cantidad de balas que se disparan por disparo
    int bulletsLeft; //Cantidad de balas dentro del cargador actual

    [Header("Feedback References")]
    //[SerializeField] GameObject impactEffect; //Referencia al VFX de impacto de bala

    Vector3 lastHitPoint; // ultima posición

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
    }

    void Update()
    {
        if (canShoot && shooting && !reloading && bulletsLeft > 0)
        {
            //Inicializar la corrutina de disparo
            StartCoroutine(ShootRoutine());
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

            if (hit.collider.TryGetComponent(out Health health))
            {
                //COMUNICACIÖN ENTRE: OBJETO QUE DISPARA + RAYO + OBJETO QUE RECIBE
                health.TakeDamage(damage);
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
        if(recoilCoroutine != null)
            StopCoroutine(recoilCoroutine);
        recoilCoroutine = StartCoroutine(RecoilRoutine());
    }

    IEnumerator RecoilRoutine()
    {
        Vector3 originalRotation = fpsCam.transform.localEulerAngles;
        float targetX = originalRotation.x - recoilAmount;

        // Ajuste para evitar overflow
        if (targetX < 0) targetX += 360f;

        float velocity = 0f;

        // Subida suave del recoil
        while (Mathf.Abs(Mathf.DeltaAngle(fpsCam.transform.localEulerAngles.x, targetX)) > 0.01f)
        {
            float x = Mathf.SmoothDampAngle(fpsCam.transform.localEulerAngles.x, targetX, ref velocity, 1f / recoilSpeed);
            fpsCam.transform.localEulerAngles = new Vector3(x, fpsCam.transform.localEulerAngles.y, fpsCam.transform.localEulerAngles.z);
            yield return null;
        }

        // Recuperación suave a la posición original
        velocity = 0f;
        while (Mathf.Abs(Mathf.DeltaAngle(fpsCam.transform.localEulerAngles.x, originalRotation.x)) > 0.01f)
        {
            float x = Mathf.SmoothDampAngle(fpsCam.transform.localEulerAngles.x, originalRotation.x, ref velocity, 1f / recoilRecovery);
            fpsCam.transform.localEulerAngles = new Vector3(x, fpsCam.transform.localEulerAngles.y, fpsCam.transform.localEulerAngles.z);
            yield return null;
        }
    }
    #endregion

    #region Reload
    void Reload()
    {
        if (bulletsLeft < ammoSize && !reloading)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    IEnumerator ReloadRoutine()
    {
        reloading = true;
        //Se llama a la animación de recarga
        yield return new WaitForSeconds(reloadTime);
        bulletsLeft = ammoSize;
        reloading = false;
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
