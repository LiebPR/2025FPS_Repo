using UnityEngine;

public class SpawnZoneTrigger : MonoBehaviour
{
    public EnemySpawner spawner;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FPSController>() != null)
            spawner.SetPlayerInside(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<FPSController>() != null)
            spawner.SetPlayerInside(false);
    }
}
