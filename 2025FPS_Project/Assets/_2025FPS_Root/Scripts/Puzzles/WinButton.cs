using UnityEngine;
using System.Collections;

public class WinButton : MonoBehaviour, IInteractable
{
    [Header("Hold Settings")]
    [SerializeField] float holdTime = 5f;

    [Header("Animator")]
    [SerializeField] Animator animButton;
    string pressBool = "PressDown";
    string unPressBool = "PressUp";

    [Header("Highlight")]
    [SerializeField] Material highlightMaterial;
    Material originalMaterial;
    private Renderer objRenderer;

    private Coroutine holdCoroutine;

    void Awake()
    {
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
            originalMaterial = objRenderer.material;

        if (animButton == null)
            animButton = GetComponent<Animator>();
    }

    public void OnPress()
    {
        AudioManager.Instance.Play("ClickButton");

        if (animButton != null)
            animButton.SetBool(pressBool, true);

        holdCoroutine = StartCoroutine(HoldCountdown());
    }

    public void OnRelease()
    {
        AudioManager.Instance.Play("UnClickButton");

        if (animButton != null)
        {
            animButton.SetBool(pressBool, false);
            animButton.SetBool(unPressBool, true);
        }
            

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }

    IEnumerator HoldCountdown()
    {
        float elapsed = 0f;
        while (elapsed < holdTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        GameManager.Instance.GameWin();
        holdCoroutine = null;
    }

    #region Highlight
    public void OnHighlight()
    {
        if (objRenderer != null && highlightMaterial != null)
            objRenderer.material = highlightMaterial;
    }

    public void OnRemoveHighlight()
    {
        if (objRenderer != null)
            objRenderer.material = originalMaterial;
    }
    #endregion
}
