using UnityEngine;
using System.Collections;

/// <summary>
/// Aplica un efecto de levitación vertical sobre el objeto.
/// </summary>
public class Levitating : MonoBehaviour
{
    [SerializeField] float levitationSpeed = 1.5f;   // Velocidad de levitación
    [SerializeField] float levitationAmount = 10f;   // Distancia máxima de levitación

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.position;
        StartCoroutine(Levitate());
    }

    private IEnumerator Levitate()
    {
        while (true)
        {
            float newY = originalPosition.y + Mathf.Sin(Time.time * levitationSpeed) * levitationAmount;
            transform.position = new Vector3(originalPosition.x, newY, originalPosition.z);
            yield return null;
        }
    }
}
