using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioTable", menuName = "Scriptable Objects/AudioTable")]
public class AudioTable : ScriptableObject
{
    public List<AudioEntry> audios;

    public AudioEntry Get(string id)
    {
        return audios.Find(a => a.id == id);
    }
}
