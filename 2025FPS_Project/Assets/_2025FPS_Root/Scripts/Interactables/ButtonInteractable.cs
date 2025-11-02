using UnityEngine;

public class ButtonInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] DoorSystem linkedDoor;       // Puerta controlada
    [SerializeField] string pressDownTrigger = "PressDown"; // Animación hundirse
    [SerializeField] string pressUpTrigger = "PressUp";     // Animación volver

    [Header("Highlight")]
    [SerializeField] Material highlightMaterial;
    Material originalMaterial;
    Renderer objRenderer;

    Animator animButton;

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
        // Animación toggle como feedback
        if (animButton != null)
        {
            if (!string.IsNullOrEmpty(pressDownTrigger))
                animButton.SetTrigger(pressDownTrigger);
        }

        // Cambiar el estado de la puerta
        linkedDoor?.ToggleDoor();
    }

    // No necesitamos OnRelease para toggle
    public void OnRelease() 
    {
        if (animButton != null && !string.IsNullOrEmpty(pressUpTrigger))
            animButton.SetTrigger(pressUpTrigger);
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