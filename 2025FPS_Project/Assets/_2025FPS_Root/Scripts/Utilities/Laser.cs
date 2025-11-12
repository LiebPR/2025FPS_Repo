using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float laserDuration = 1f;
    private Material laserMaterial;

    private Vector3 startPosition;
    private Vector3 endPosition;

    private void Awake()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
    }

    public void Initialize(Vector3 start, Vector3 end, Material material)
    {
        startPosition = start;
        endPosition = end;
        laserMaterial = material;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPosition);
            lineRenderer.material = laserMaterial;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
        }

        // Inicia la devolución a la pool después de la duración
        StartCoroutine(AutoDespawn());
    }

    IEnumerator AutoDespawn()
    {
        yield return new WaitForSeconds(laserDuration);

        // Devolver a la pool, no destruir
        if (PoolManager.Instance != null)
            PoolManager.Instance.Despawn("LaserPool", gameObject);
    }

    private void Update()
    {
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPosition);
        }
    }
}
