using UnityEngine;

public class DoorSystem : MonoBehaviour
{
    #region General Variables
    [SerializeField] bool isOpen = false; // Estado actual de la puerta
    #endregion

    #region References
    Animator doorAnimator;  // Referencia al Animator de la puerta
    #endregion

    private void Awake()
    {
        doorAnimator = GetComponent<Animator>();  // Obtener el Animator
    }

    #region Public Methods
    public void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;
        AudioManager.Instance.Play("PuertaAbrir");
        // Resetear el trigger antes de activar el nuevo
        doorAnimator.SetTrigger("PressUp");

        
    }

    public void CloseDoor()
    {
        if (!isOpen) return;
        isOpen = false;
        AudioManager.Instance.Play("PuertaCerrar");
        // Resetear el trigger antes de activar el nuevo
        doorAnimator.SetTrigger("PressDown");

        
    }

    public void ToggleDoor()
    {
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }
    #endregion
}