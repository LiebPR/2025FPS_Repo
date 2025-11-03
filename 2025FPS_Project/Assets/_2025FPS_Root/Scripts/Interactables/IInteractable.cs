using UnityEngine;

/// <summary>
/// IInteractable: Interfaz para objetos que pueden ser activador por el sistema de interacción.
/// Implementar interact para definir el efecto concreto.
/// </summary>
public interface IInteractable
{
    void OnPress();
    void OnRelease();

    void OnHighlight();      // Cuando está apuntado
    void OnRemoveHighlight(); // Cuando deja de apuntar
}
