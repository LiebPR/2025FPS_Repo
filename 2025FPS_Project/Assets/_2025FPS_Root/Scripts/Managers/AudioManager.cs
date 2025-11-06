using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Referencia de los Sonidos")]
    public AudioTable table;

    [Header("Sources Principales")]
    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioSource uiSource;
    [SerializeField] AudioSource sfxSource; //sonidos que siguen al jugador

    [Header("Objetos que sigue el SFX del jugador")]
    [SerializeField] Transform playerTransform;

    [Header("Pool de AudioSource 3D")]
    [SerializeField] int poolSize = 10;
    private Queue<AudioSource> pool;

    #region Inicialización
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
        CrearPool3D();
    }

    void CrearPool3D()
    {
        pool = new Queue<AudioSource>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = new GameObject("Audio3D_Source");
            go.transform.parent = transform;
            AudioSource src = go.AddComponent<AudioSource>();
            src.spatialBlend = 1f;
            src.loop = false;
            go.SetActive(false);
            pool.Enqueue(src);
        }
    }
    #endregion

    public void Play(string id)
    {
        AudioEntry entry = table.Get(id);
        if (entry == null) return;

        switch (entry.group)
        {
            case AudioGroup.BGM: PlayBGM(entry); break;
            case AudioGroup.UI: PlayUI(entry); break;
            case AudioGroup.SFX: PlaySFX(entry); break;
        }
    }
    public void PlayLoop(string id)
    {
        AudioEntry entry = table.Get(id);
        if (entry == null) return;

        sfxSource.clip = entry.clip;
        sfxSource.volume = entry.defaultVolume; // usa defaultVolume que ya existe
        sfxSource.loop = true;
        sfxSource.Play();
    }

    public void StopLoop()
    {
        sfxSource.loop = false;
        sfxSource.Stop();
    }

    public void Play3D(string id, Vector3 position)
    {
        AudioEntry entry = table.Get(id);
        if (entry == null) return;

        if (entry.group != AudioGroup.SFX3D)
        {
            Debug.LogWarning($"'{id}' no está marcado como SFX3D.");
            return;
        }

        AudioSource src = Get3DSource();
        src.clip = entry.clip;
        src.volume = entry.defaultVolume;
        src.transform.position = position;
        src.gameObject.SetActive(true);
        src.Play();
        StartCoroutine(Release3DSource(src));
    }

    private Dictionary<string, AudioSource> activeLoops = new Dictionary<string, AudioSource>();
    public void Play3DLoop(string id, Transform followTarget)
    {
        // Si ya está sonando, no duplicamos
        if (activeLoops.ContainsKey(id))
            return;

        AudioEntry entry = table.Get(id);
        if (entry == null) return;

        // No afecta otros SFX3D: solo se activa cuando lo llamas explícitamente
        AudioSource src = Get3DSource();
        src.clip = entry.clip;
        src.volume = entry.defaultVolume;
        src.loop = true;
        src.transform.position = followTarget.position;
        src.gameObject.SetActive(true);
        src.Play();

        activeLoops.Add(id, src);
        StartCoroutine(FollowTarget(src, followTarget));
    }

    public void Stop3DLoop(string id)
    {
        if (!activeLoops.ContainsKey(id)) return;

        AudioSource src = activeLoops[id];
        src.Stop();
        src.loop = false;
        src.gameObject.SetActive(false);
        pool.Enqueue(src);
        activeLoops.Remove(id);
    }

    System.Collections.IEnumerator FollowTarget(AudioSource src, Transform target)
    {
        while (src != null && src.isPlaying && target != null)
        {
            src.transform.position = target.position;
            yield return null;
        }
    }

    public void StopAllSFXExceptBGM()
    {
        // 1. Detener UI y SFX base
        uiSource.Stop();
        sfxSource.Stop();

        // 2. Detener todos los loops 3D activos
        foreach (var src in activeLoops.Values)
        {
            if (src != null)
            {
                src.Stop();
                src.loop = false;
                src.gameObject.SetActive(false);
                pool.Enqueue(src);
            }
        }
        activeLoops.Clear();

        // 3. Detener cualquier Audio3D que esté sonando del pool
        foreach (Transform child in transform)
        {
            AudioSource src = child.GetComponent<AudioSource>();
            if (src != null && src.isPlaying)
            {
                src.Stop();
                src.gameObject.SetActive(false);
                pool.Enqueue(src);
            }
        }
    }
    #region Internos
    void PlayBGM(AudioEntry entry)
    {
        bgmSource.clip = entry.clip;
        bgmSource.volume = entry.defaultVolume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    void PlayUI(AudioEntry entry)
    {
        uiSource.PlayOneShot(entry.clip, entry.defaultVolume);
    }

    void PlaySFX(AudioEntry entry)
    {
        sfxSource.transform.position = playerTransform.position;
        sfxSource.PlayOneShot(entry.clip, entry.defaultVolume);
    }

    AudioSource Get3DSource()
    {
        if (pool.Count == 0) CrearPool3D();
        return pool.Dequeue();
    }

    System.Collections.IEnumerator Release3DSource(AudioSource src)
    {
        yield return new WaitWhile(() => src.isPlaying);
        src.gameObject.SetActive(false);
        pool.Enqueue(src);
    }
    #endregion
}
