using System.Collections;
using System.Collections.Generic;
using System.Linq; // Necesario para .FirstOrDefault() y .ToList()
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Referencia de los Sonidos")]
    public AudioTable table;

    [Header("Sources Principales")]
    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioSource uiSource;
    [SerializeField] AudioSource sfxSource; // Sonidos 2D one-shot generales (no en bucle)

    [Header("Objetos que sigue el SFX del jugador")]
    [SerializeField] Transform playerTransform; // Solo para referencia

    [Header("Pool de AudioSource 3D/Loop")]
    [SerializeField] int poolSize = 10;
    private Queue<AudioSource> pool;

    // Diccionarios para rastrear bucles activos y su AudioSource.
    private Dictionary<string, AudioSource> active3DLoops = new Dictionary<string, AudioSource>();
    private Dictionary<string, AudioSource> active2DLoops = new Dictionary<string, AudioSource>();

    #region Inicialización
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
        CrearPool();
    }

    void CrearPool()
    {
        pool = new Queue<AudioSource>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = new GameObject("Pool_Source");
            go.transform.parent = transform;
            AudioSource src = go.AddComponent<AudioSource>();

            src.spatialBlend = 1f; // Predeterminado a 3D para la mayoría de usos del pool
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
                // Los SFX3D y Loops se llaman directamente con sus métodos específicos
        }
    }

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
        sfxSource.PlayOneShot(entry.clip, entry.defaultVolume);
    }

    // --- GESTIÓN DE LOOPS ---

    // Reproducir bucle de SFX 2D (utiliza el pool)
    public void PlayLoop(string id)
    {
        Play2DLoop(id);
    }

    // Implementación del bucle 2D (utiliza el pool)
    void Play2DLoop(string id)
    {
        if (active2DLoops.ContainsKey(id)) return;

        AudioEntry entry = table.Get(id);
        if (entry == null) return;

        AudioSource src = GetPoolSource();
        src.clip = entry.clip;
        src.volume = entry.defaultVolume;
        src.loop = true;
        src.spatialBlend = 0f; // Aseguramos 2D para este loop
        src.gameObject.SetActive(true);
        src.Play();

        active2DLoops.Add(id, src);
    }

    // Detener cualquier loop (BGM, 2D o 3D) de forma inmediata
    public void StopLoop(string id)
    {
        AudioEntry entry = table.Get(id);
        if (entry == null) return;

        // 1. Detener BGM
        if (entry.group == AudioGroup.BGM && bgmSource.isPlaying && bgmSource.clip == entry.clip)
        {
            bgmSource.Stop();
            bgmSource.loop = false;
            return;
        }

        // 2. Detener loop 2D del pool
        if (active2DLoops.ContainsKey(id))
        {
            AudioSource src = active2DLoops[id];
            StopAndReleaseSource(src, active2DLoops);
            return;
        }

        // 3. Detener loop 3D del pool
        if (active3DLoops.ContainsKey(id))
        {
            AudioSource src = active3DLoops[id];
            StopAndReleaseSource(src, active3DLoops);
            return;
        }
    }

    // Detener BGM con fade-out (solo afecta a bgmSource, para loops del pool usar FadeOutLoop)
    public void FadeOutBGM(float fadeDuration)
    {
        // Verifica si está sonando antes de intentar el fade
        if (!bgmSource.isPlaying) return;

        StartCoroutine(FadeOutCoroutine(bgmSource, fadeDuration, AudioGroup.BGM));
    }

    // *** NUEVO *** Método para hacer fade out a cualquier loop (BGM, 2D, 3D)
    public void FadeOutLoop(string id, float fadeDuration)
    {
        AudioEntry entry = table.Get(id);
        if (entry == null) return;

        AudioSource targetSource = null;
        AudioGroup group = entry.group; // Usamos el grupo de la entrada para la lógica de limpieza.

        if (entry.group == AudioGroup.BGM)
        {
            targetSource = bgmSource;
        }
        else if (active2DLoops.ContainsKey(id))
        {
            targetSource = active2DLoops[id];
        }
        else if (active3DLoops.ContainsKey(id))
        {
            targetSource = active3DLoops[id];
        }

        if (targetSource != null && targetSource.isPlaying)
        {
            StartCoroutine(FadeOutCoroutine(targetSource, fadeDuration, group, id));
        }
    }

    // Corrutina genérica para manejar el desvanecimiento y la limpieza.
    private IEnumerator FadeOutCoroutine(AudioSource src, float fadeDuration, AudioGroup group, string id = null)
    {
        float startVolume = src.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            src.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        src.Stop();

        // Limpieza y reseteo
        if (group == AudioGroup.BGM)
        {
            // Resetear el volumen original de BGM para la próxima vez
            src.volume = startVolume;
        }
        else // Si es SFX o SFX3D (del pool), lo liberamos.
        {
            // Determinar el diccionario correcto y llamar a la liberación
            Dictionary<string, AudioSource> activeDict = (group == AudioGroup.SFX) ? active2DLoops : active3DLoops;

            // Usar la función de liberación para manejar la limpieza del diccionario y el pool.
            if (activeDict.ContainsKey(id))
            {
                StopAndReleaseSource(src, activeDict);
            }
        }
    }


    // Reproducir SFX 3D one-shot
    public void Play3D(string id, Vector3 position)
    {
        AudioEntry entry = table.Get(id);
        if (entry == null) return;

        AudioSource src = GetPoolSource();
        src.clip = entry.clip;
        src.volume = entry.defaultVolume;
        src.loop = false;
        src.spatialBlend = 1f; // Aseguramos 3D 
        src.transform.position = position;
        src.gameObject.SetActive(true);
        src.Play();
        StartCoroutine(ReleasePoolSource(src)); // Devuelve al pool después de terminar
    }

    // Reproducir bucle de SFX 3D que sigue un objeto
    public void Play3DLoop(string id, Transform followTarget)
    {
        if (active3DLoops.ContainsKey(id)) return;

        AudioEntry entry = table.Get(id);
        if (entry == null) return;

        AudioSource src = GetPoolSource();
        src.clip = entry.clip;
        src.volume = entry.defaultVolume;
        src.loop = true;
        src.spatialBlend = 1f; // Aseguramos 3D 
        src.transform.position = followTarget.position;
        src.gameObject.SetActive(true);
        src.Play();

        active3DLoops.Add(id, src);
        StartCoroutine(FollowTarget(id, src, followTarget));
    }

    // Detener el bucle de SFX 3D
    public void Stop3DLoop(string id)
    {
        if (!active3DLoops.ContainsKey(id)) return;
        AudioSource src = active3DLoops[id];
        StopAndReleaseSource(src, active3DLoops);
    }

    // Método auxiliar para detener y liberar una fuente
    private void StopAndReleaseSource(AudioSource src, Dictionary<string, AudioSource> activeDict)
    {
        // Busca el ID para poder removerlo
        string idToRemove = activeDict.FirstOrDefault(x => x.Value == src).Key;
        if (idToRemove != null) activeDict.Remove(idToRemove);

        src.Stop();
        src.loop = false;
        src.spatialBlend = 1f; // Restablecer a 3D por defecto
        src.gameObject.SetActive(false);
        pool.Enqueue(src);
    }

    // Controlar la posición de un SFX 3D mientras sigue un objeto
    private IEnumerator FollowTarget(string id, AudioSource src, Transform target)
    {
        while (active3DLoops.ContainsKey(id) && src != null && src.isPlaying && target != null)
        {
            src.transform.position = target.position;
            yield return null;
        }

        // Si el loop se detiene de forma externa o el target se destruye, lo liberamos
        if (src != null && src.isPlaying)
        {
            StopAndReleaseSource(src, active3DLoops);
        }
    }

    // Detener todos los SFX excepto la música de fondo
    public void StopAllSFXExceptBGM()
    {
        uiSource.Stop();
        sfxSource.Stop();

        ReleaseAllActiveLoops(active2DLoops);
        ReleaseAllActiveLoops(active3DLoops);

        // Detener cualquier Audio3D one-shot que esté sonando del pool
        foreach (Transform child in transform)
        {
            AudioSource src = child.GetComponent<AudioSource>();
            if (src != null && src.isPlaying)
            {
                src.Stop();
                src.loop = false;
                src.gameObject.SetActive(false);
            }
        }
    }

    // Método auxiliar para liberar todos los loops de un diccionario
    private void ReleaseAllActiveLoops(Dictionary<string, AudioSource> activeDict)
    {
        foreach (var src in activeDict.Values.ToList())
        {
            if (src != null)
            {
                src.Stop();
                src.loop = false;
                src.spatialBlend = 1f;
                src.gameObject.SetActive(false);
                pool.Enqueue(src);
            }
        }
        activeDict.Clear();
    }

    // Obtener un AudioSource del pool
    AudioSource GetPoolSource()
    {
        if (pool.Count == 0) CrearPool();
        AudioSource src = pool.Dequeue();

        // Reiniciar valores por defecto
        src.loop = false;
        src.spatialBlend = 1f;

        return src;
    }

    // Liberar un AudioSource del pool después de que deje de sonar (para one-shots 3D)
    private IEnumerator ReleasePoolSource(AudioSource src)
    {
        yield return new WaitWhile(() => src.isPlaying);
        if (src != null)
        {
            src.gameObject.SetActive(false);
            pool.Enqueue(src);
        }
    }
}