using UnityEngine;

public class AmmoPickUp : MonoBehaviour, IInteractable
{
    #region General Variables
    [SerializeField] int ammoUnits = 1;       // Unidades de munición que da este pickup (editable)
    [Header("Highlight")]
    [SerializeField] Material highlightMaterial;
    #endregion

    #region References
    Renderer objRenderer;
    Material originalMaterial;
    GunSystem playerGun;
    Rigidbody rb;
    #endregion

    private void Awake()
    {
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
            originalMaterial = objRenderer.material;

        rb = GetComponent<Rigidbody>();
    }

    public void SetPlayerGun(GunSystem gun)
    {
        playerGun = gun;
    }
    //Permite asignar la cantidad desde código (usado por Health al instanciar)
    public void SetAmmoAmount(int amount)
    {
        ammoUnits = amount;
    }

    #region Interacción
    public void OnPress()
    {
        if (playerGun == null) return;

        int given = playerGun.PickUpAmmo(ammoUnits);

        if (given > 0)
        {
            AudioManager.Instance.Play("AmmoPickUpTrue");
            PoolManager.Instance.Despawn("Ammo", gameObject); //solo desaparece si realmente dio algo
        }
        else
        {
            AudioManager.Instance.Play("AmmoPickUpFalse");
        }
    }

    public void OnRelease() { }
    #endregion

    #region Highlight
    public void OnHighlight()
    {
        if (objRenderer != null && highlightMaterial != null)
            objRenderer.material = highlightMaterial;
    }

    public void OnRemoveHighlight()
    {
        if (objRenderer != null && originalMaterial != null)
            objRenderer.material = originalMaterial;
    }
    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            rb.constraints = RigidbodyConstraints.FreezePosition| RigidbodyConstraints.FreezeRotation;
        }
    }
}
