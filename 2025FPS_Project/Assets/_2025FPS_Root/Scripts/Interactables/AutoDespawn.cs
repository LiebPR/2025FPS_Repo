using UnityEngine;

public class AutoDespawn : MonoBehaviour
{
    [SerializeField] float lifeTime = 1f;

    public void SetLifeTime(float time) => lifeTime = time;

    private void OnEnable() => Invoke(nameof(Despawn), lifeTime);
    private void OnDisable() => CancelInvoke();

    void Despawn() => gameObject.SetActive(false);
}
