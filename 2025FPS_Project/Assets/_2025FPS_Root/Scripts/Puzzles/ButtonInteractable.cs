using UnityEngine;

public class ButtonInteractable : MonoBehaviour, IInteractable
{
    #region General Variables
    [SerializeField] DoorSystem linkedDoor;       // Puerta controlada
    string pressDownTrigger = "PressDown"; // Animación hundirse
    string pressUpTrigger = "PressUp";     // Animación volver

    [Header("Highlight")]
    [SerializeField] Material highlightMaterial;
    Material originalMaterial;
    #endregion

    #region References
    Renderer objRenderer;
    Animator animButton;
    #endregion 

    void Awake()
    {
        animButton = GetComponent<Animator>();
        objRenderer = GetComponent<Renderer>();
        if(objRenderer != null)
        {
            originalMaterial = objRenderer.material;
        }
    }

    // Se llama al presionar el botón
    public void OnPress()
    {
        if (animButton != null)
        {
            animButton.ResetTrigger(pressUpTrigger);  // Asegura que el trigger anterior se resetee
            if (!string.IsNullOrEmpty(pressDownTrigger))
                animButton.SetTrigger(pressDownTrigger);
            linkedDoor?.ToggleDoor();
            AudioManager.Instance.Play("ClickButton");
        }

        
    }

    public void OnRelease()
    {
        if (animButton != null && !string.IsNullOrEmpty(pressUpTrigger))
        {
            animButton.ResetTrigger(pressDownTrigger);  // Asegura que el trigger anterior se resetee
            animButton.SetTrigger(pressUpTrigger);
        }

        AudioManager.Instance.Play("UnClickButton");
    }

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