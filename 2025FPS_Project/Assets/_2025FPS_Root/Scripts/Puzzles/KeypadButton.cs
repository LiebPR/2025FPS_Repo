using UnityEngine;

/// <summary>
/// KeypadButton: Botón físico del teclado numérico.
/// Se activa mediante el sistema de interacción.
/// </summary>
public class KeypadButton : MonoBehaviour, IInteractable
{
    #region General Variables
    [Header("Configuration")]
    [SerializeField] int number; //numero asignado a este botón (0-9)
    [SerializeField] KeypadPanel panel;

    [Header("Highlight")]
    [SerializeField] Material highlightMaterial; // Material al apuntar
    Material originalMaterial;

    string pressDownTrigger = "PressDown";
    string pressUpTrigger = "PressUp";
    #endregion

    #region References
    Animator animButton;
    Renderer objRenderer;
    #endregion

    void Awake()
    {
        animButton = GetComponent<Animator>();
        objRenderer = GetComponent<Renderer>();

        if (objRenderer != null)
            originalMaterial = objRenderer.material;
    }

    //Al presionar el boton
    public void OnPress()
    {
        // Enviar número al panel
        panel.ReceiveInput(number);

        // Activar animación de presión
        if (animButton != null && !string.IsNullOrEmpty(pressDownTrigger))
            animButton.SetTrigger(pressDownTrigger);
        AudioManager.Instance.Play("ButtonBeep");
    }

    //Al soltar el boton
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
