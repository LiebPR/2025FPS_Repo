using UnityEngine;


public enum AudioGroup
{
    BGM,
    UI,
    SFX,
    SFX3D
}
[System.Serializable]
public class AudioEntry
{
    public string id;
    public AudioClip clip;
    public AudioGroup group;
    public float defaultVolume = 1f;
}
