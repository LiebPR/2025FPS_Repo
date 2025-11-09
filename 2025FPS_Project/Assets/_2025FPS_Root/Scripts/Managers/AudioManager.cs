using System.Collections;
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

    // Reproducir sonido basado en el ID.
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

    // Reproducir música de fondo (BGM)
    void PlayBGM(AudioEntry entry)
    {
        bgmSource.clip = entry.clip;
        bgmSource.volume = entry.defaultVolume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // Reproducir UI (sonidos de interfaz)
    void PlayUI(AudioEntry entry)
    {
        uiSource.PlayOneShot(entry.clip, entry.defaultVolume);
    }

    // Reproducir SFX
    void PlaySFX(AudioEntry entry)
    {
        sfxSource.transform.position = playerTransform.position;
        sfxSource.PlayOneShot(entry.clip, entry.defaultVolume);
    }

    // Reproducir bucle de SFX (sonido ambiental o música)
    public void PlayLoop(string id)
    {
        AudioEntry entry = table.Get(id);
        if (entry == null) return;

        sfxSource.clip = entry.clip;
        sfxSource.volume = entry.defaultVolume; // usa defaultVolume que ya existe
        sfxSource.loop = true;
        sfxSource.Play();
    }

    // Detener el loop de la música de fondo o cualquier otro loop
    public void StopLoop()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
            bgmSource.loop = false;
        }

        if (sfxSource.isPlaying && sfxSource.loop)
        {
            sfxSource.Stop();
            sfxSource.loop = false;
        }
    }

    // Detener la música de fondo (BGM) con un fade-out
    public void FadeOutBGM(float fadeDuration)
    {
        StartCoroutine(FadeOutBGMCoroutine(fadeDuration));
    }

    private IEnumerator FadeOutBGMCoroutine(float fadeDuration)
    {
        float startVolume = bgmSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = startVolume; // Restablecer el volumen a su valor original.
    }

    // Reproducir bucle de SFX 3D (sonido espacial)
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

    // Reproducir bucle de SFX 3D que sigue un objeto
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

    // Detener el bucle de SFX 3D
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

    // Controlar la posición de un SFX 3D mientras sigue un objeto
    private IEnumerator FollowTarget(AudioSource src, Transform target)
    {
        while (src != null && src.isPlaying && target != null)
        {
            src.transform.position = target.position;
            yield return null;
        }
    }

    // Detener todos los SFX excepto la música de fondo
    public void StopAllSFXExceptBGM()
    {
        uiSource.Stop();
        sfxSource.Stop();

        // Detener todos los loops 3D activos
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

        // Detener cualquier Audio3D que esté sonando del pool
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

    // Obtener un AudioSource del pool 3D
    AudioSource Get3DSource()
    {
        if (pool.Count == 0) CrearPool3D();
        return pool.Dequeue();
    }

    // Liberar un AudioSource 3D después de que deje de sonar
    private IEnumerator Release3DSource(AudioSource src)
    {
        yield return new WaitWhile(() => src.isPlaying);
        src.gameObject.SetActive(false);
        pool.Enqueue(src);
    }
}
