using UnityEngine;

public class LevelInitializer : MonoBehaviour
{
    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }
}
