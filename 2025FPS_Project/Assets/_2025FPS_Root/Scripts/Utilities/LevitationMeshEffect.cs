using UnityEngine;

/// <summary>
/// LevitationEffect: Aplica un efecto de levitación suave al mesh de un enemigo.
/// </summary>
public class LevitationMeshEffect : MonoBehaviour
{
    [SerializeField] float levitationHeight = 0.5f;  // Altura máxima a la que el enemigo se elevará
    [SerializeField] float levitationSpeed = 2f;     // Velocidad de la oscilación
    [SerializeField] float levitationStopSpeed = 2f; // Velocidad de la desaceleración al detener la levitación (suavizado)

    private Vector3 originalLocalPosition;  // Posición local original del mesh
    private float oscillationTime;
    private bool isLevitating = false;
    private float currentHeightOffset;  // Altura actual desde el suelo

    private void Start()
    {
        // Guarda la posición local original del mesh
        originalLocalPosition = transform.localPosition;
        StartLevitating();
    }

    private void Update()
    {
        // Solo levita si está activado
        if (isLevitating)
        {
            // Incrementa el tiempo de oscilación
            oscillationTime += Time.deltaTime * levitationSpeed;

            // La función PingPong oscila entre 0 y levitationHeight
            currentHeightOffset = Mathf.PingPong(oscillationTime, levitationHeight);

            // Aplica la oscilación solo en el eje Y
            transform.localPosition = new Vector3(originalLocalPosition.x, originalLocalPosition.y + currentHeightOffset, originalLocalPosition.z);
        }
        else
        {
            // Si la levitación está desactivada, suaviza la vuelta a la posición original
            float smoothY = Mathf.Lerp(transform.localPosition.y, originalLocalPosition.y, levitationStopSpeed * Time.deltaTime);
            transform.localPosition = new Vector3(originalLocalPosition.x, smoothY, originalLocalPosition.z);
        }
    }

    // Llama a este método para comenzar la levitación
    public void StartLevitating()
    {
        isLevitating = true;
        oscillationTime = 0f; // Reinicia el tiempo de oscilación
    }

    // Llama a este método para detener la levitación y regresar suavemente a la posición original
    public void StopLevitating()
    {
        isLevitating = false;
    }
}
