using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    #region General Variables
    [SerializeField] Camera cameraSource;           // Cámara desde la que se lanza el rayo
    [SerializeField] float interactRange = 2f;     // Alcance del rayo
    [SerializeField] LayerMask interactMask = ~0;  // Capas válidas
    [SerializeField] bool debugRay = true;         // Mostrar rayo para depuración
    #endregion

    IInteractable currentTarget;
    IInteractable highlightedTarget;

    #region Unity Events
    private void Start()
    {
        if (cameraSource == null)
            cameraSource = Camera.main;

        // Suscribirse a eventos de InputManager
        InputManager.OnInteractHoldStart += HandleInteractHoldStart;
        InputManager.OnInteractHoldEnd += HandleInteractHoldEnd;
    }

    private void OnDestroy()
    {
        InputManager.OnInteractHoldStart -= HandleInteractHoldStart;
        InputManager.OnInteractHoldEnd -= HandleInteractHoldEnd;
    }

    private void Update()
    {
        HandleHighlight();
    }
    #endregion

    #region Interact Logic
    private void HandleInteractHoldStart()
    {
        Ray ray = cameraSource.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask, QueryTriggerInteraction.Collide))
        {
            currentTarget = hit.collider.GetComponent<IInteractable>() ?? hit.collider.GetComponentInParent<IInteractable>();
            currentTarget?.OnPress();
        }

        if (debugRay)
            Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.green, 0.5f);
    }

    private void HandleInteractHoldEnd()
    {
        currentTarget?.OnRelease();
        currentTarget = null;
    }
    #endregion

    #region Highlight Logic
    private void HandleHighlight()
    {
        Ray ray = cameraSource.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask, QueryTriggerInteraction.Collide))
        {
            IInteractable target = hit.collider.GetComponent<IInteractable>() ?? hit.collider.GetComponentInParent<IInteractable>();

            if (target != highlightedTarget)
            {
                highlightedTarget?.OnRemoveHighlight(); // Remover highlight del anterior
                highlightedTarget = target;
                highlightedTarget?.OnHighlight();       // Aplicar highlight al nuevo
            }
        }
        else
        {
            highlightedTarget?.OnRemoveHighlight();
            highlightedTarget = null;
        }

        if (debugRay)
            Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.yellow);
    }
    #endregion
}
