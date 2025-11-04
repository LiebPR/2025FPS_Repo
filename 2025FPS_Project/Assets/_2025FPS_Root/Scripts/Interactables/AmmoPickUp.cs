using UnityEngine;

public class AmmoPickUp : MonoBehaviour, IInteractable
{
    [SerializeField] int ammoAmount = 10;              // Cantidad de balas que da
    [Header("Highlight")]
    [SerializeField] Material highlightMaterial;       // Material cuando está apuntado

    private Renderer objRenderer;
    private Material originalMaterial;

    GunSystem playerGun;

    private void Awake()
    {
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
            originalMaterial = objRenderer.material;
    }

    public void SetPlayerGun(GunSystem gun)
    {
        playerGun = gun;
    }

    public void SetAmmoAmount(int amount)
    {
        ammoAmount = amount;
    }

    #region Interacción
    public void OnPress()
    {
        if (playerGun == null)
        {
            Debug.LogWarning("No se ha asignado GunSystem al pick-up.");
            return;
        }

        if (playerGun.BulletsLeft >= playerGun.AmmoSize)
        {
            Debug.Log("Munición al máximo. No puedes recoger más.");
        }
        else
        {
            int before = playerGun.BulletsLeft;
            playerGun.PickUpAmmo(ammoAmount);
            int after = playerGun.BulletsLeft;
            Debug.Log($"Recogido {after - before} balas. Munición actual: {after}/{playerGun.AmmoSize}");
            Destroy(gameObject); // Se recoge y desaparece
        }
    }

    public void OnRelease()
    {
        // No se necesita lógica aquí
    }
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
}
