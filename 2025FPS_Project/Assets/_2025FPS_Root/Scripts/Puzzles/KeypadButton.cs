using UnityEngine;

/// <summary>
/// KeypadButton: Botón físico del teclado numérico.
/// Se activa mediante el sistema de interacción.
/// </summary>
public class KeypadButton : MonoBehaviour, IInteractable
{
    #region General Variables
    [Header("Configuration")]
    [SerializeField] int number; // número asignado a este botón (0-9)
    [SerializeField] KeypadPanel panel;

    [Header("Highlight")]
    [SerializeField] Material highlightMaterial; // Material al apuntar
    Material originalMaterial;
    #endregion

    #region References
    Renderer objRenderer;
    #endregion

    void Awake()
    {
        objRenderer = GetComponent<Renderer>();

        if (objRenderer != null)
            originalMaterial = objRenderer.material;
    }

    // Al presionar el botón
    public void OnPress()
    {
        
        panel.ReceiveInput(number);
        AudioManager.Instance.Play("ButtonBeep");
        
    }

    public void OnRelease()
    {
        
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
