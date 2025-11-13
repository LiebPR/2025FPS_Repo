using UnityEngine;

public class MachinePaswordAudio : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.Instance.Play3DLoop("MachinePassword", transform);
    }

    
}
