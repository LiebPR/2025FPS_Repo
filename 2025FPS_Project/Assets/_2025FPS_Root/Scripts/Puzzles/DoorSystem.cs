using UnityEngine;

/// <summary>
/// DoorSystem: Controla la puerta elevadora mediante movimiento vertical.
/// </summary>
public class DoorSystem : MonoBehaviour
{
    #region General Variables
    [SerializeField] bool isOpen = false; // Estado actual de la puerta
    [SerializeField] float openHeight = 3f; // Altura a la que se abre la puerta
    [SerializeField] float moveSpeed = 2f; // Velocidad de movimiento
    #endregion

    #region Private Variables
    Vector3 closedPosition;
    Vector3 openPosition;
    #endregion

    private void Awake()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * openHeight;
    }

    private void Update()
    {
        // Determina la posición objetivo según el estado
        Vector3 targetPosition = isOpen ? openPosition : closedPosition;
        
            // Mueve la puerta suavemente hacia la posición objetivo
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    #region Public Methods
    public void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;

        AudioManager.Instance.Play("PuertaAbrir");
    }

    public void CloseDoor()
    {
        if (!isOpen) return;
        isOpen = false;

        AudioManager.Instance.Play("PuertaCerrar");
    }

    public void ToggleDoor()
    {
        if(isOpen)
            CloseDoor();
        else 
            OpenDoor();
    }
    #endregion
}
