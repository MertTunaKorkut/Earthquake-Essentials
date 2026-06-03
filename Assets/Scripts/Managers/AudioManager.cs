using UnityEngine;

/// <summary>
/// Oyunun ses sistemini yöneten singleton manager.
/// Müzik ve ses efektlerini oynatır/durdurur.
/// DontDestroyOnLoad ile sahneler arası yaşar.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Ses Ayarları")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        /*
        // Child objelerden AudioSource'ları otomatik bul
        if (musicSource == null)
        {
            Transform musicChild = transform.Find("MusicSource");
            if (musicChild != null)
                musicSource = musicChild.GetComponent<AudioSource>();

            if (musicSource == null)
                Debug.LogError("[AudioManager] MusicSource child objesi veya AudioSource bulunamadı!");
        }

        if (sfxSource == null)
        {
            Transform sfxChild = transform.Find("SFXSource");
            if (sfxChild != null)
                sfxSource = sfxChild.GetComponent<AudioSource>();

            if (sfxSource == null)
                Debug.LogError("[AudioManager] SFXSource child objesi veya AudioSource bulunamadı!");
        }
        */
        
    }

    /// <summary>
    /// Arka plan müziği oynatır.
    /// </summary>
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    /// <summary>
    /// Arka plan müziğini durdurur.
    /// </summary>
    public void StopMusic()
    {
        musicSource.Stop();
    }

    /// <summary>
    /// Tek seferlik ses efekti oynatır.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>
    /// Müzik sesini ayarlar.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    /// <summary>
    /// SFX sesini ayarlar.
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
}
