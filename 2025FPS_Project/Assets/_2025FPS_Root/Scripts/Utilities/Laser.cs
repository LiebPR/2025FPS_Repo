using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer; // LineRenderer para dibujar el rayo
    [SerializeField] private float laserDuration = 1f; // Duración del láser en segundos
    private Material laserMaterial; // Material del láser

    private Vector3 startPosition;
    private Vector3 endPosition;

    private void Awake()
    {
        // Asegurarse de que el LineRenderer esté configurado
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
    }

    // Método para inicializar el láser
    public void Initialize(Vector3 start, Vector3 end, Material material)
    {
        startPosition = start;
        endPosition = end;
        laserMaterial = material;

        // Configuración inicial del LineRenderer
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPosition);
            lineRenderer.material = laserMaterial;

            // Configurar grosor del láser
            lineRenderer.startWidth = 0.1f;  // Grosor al inicio
            lineRenderer.endWidth = 0.1f;    // Grosor al final
        }

        // Iniciar la destrucción del láser después de un tiempo
        Destroy(gameObject, laserDuration);
    }

    private void Update()
    {
        // Asegurarse de que el LineRenderer esté siempre actualizado
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPosition);
        }
    }

    // Método para manejar el impacto del láser, por ejemplo, daño a enemigos o destruir el objeto
    private void OnLaserHit()
    {
        // Solo devolver a la pool si el objeto no ha sido destruido
        if (gameObject != null)
        {
            PoolManager.Instance.Despawn("LaserPool", gameObject);
        }
    }
}
