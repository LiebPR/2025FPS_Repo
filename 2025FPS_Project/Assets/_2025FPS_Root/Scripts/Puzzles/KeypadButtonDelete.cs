using UnityEngine;

public class KeypadButtonDelete : MonoBehaviour, IInteractable
{
    #region General Variabels
    [SerializeField] KeypadPanel panel;

    [Header("Animación y Highlight")]
    [SerializeField] string pressDownTrigger = "PressDown";
    [SerializeField] string pressUpTrigger = "PressUp";
    [SerializeField] Material highlightMaterial;
    #endregion

    #region References
    Animator animButton;
    Renderer objRenderer;
    Material originalMaterial;
    #endregion

    void Awake()
    {
        animButton = GetComponent<Animator>();
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
            originalMaterial = objRenderer.material;
    }

    public void OnPress()
    {
        panel.DeleteLastDigit();

        if (animButton != null && !string.IsNullOrEmpty(pressDownTrigger))
            animButton.SetTrigger(pressDownTrigger);
    }

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
