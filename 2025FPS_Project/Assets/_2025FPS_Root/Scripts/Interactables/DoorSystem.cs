using UnityEngine;

/// <summary>
/// DoorSystem: Controla el estado de la puerta (abrir/cerrar) mediante animación o lógica directa.
/// </summary>
public class DoorSystem : MonoBehaviour
{
    #region General Variables
    [SerializeField] string openBoolName = "Open"; //Nombre del parámetro booleano en el Animator
    [SerializeField] bool isOpen = false; //Estado actual de la puerta
    #endregion

    #region References
    Animator doorAnimator;
    #endregion

    private void Awake()
    {
        doorAnimator = GetComponent<Animator>();
    }

    #region Public Methods
    //Abre la puerta si no está abierta
    public void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;

        if (doorAnimator != null)
            doorAnimator.SetBool(openBoolName, true);
    }
    
    //Cierra la puerta si está abierta
    public void CloseDoor()
    {
        if (!isOpen) return;
        isOpen = false;

        if (doorAnimator != null)
            doorAnimator.SetBool(openBoolName, false);
    }

    //Cambia el estado de la puerta (abre si está cerrado y cierra si esta abierta).
    public void ToggleDoor()
    {
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }
    #endregion 
}
