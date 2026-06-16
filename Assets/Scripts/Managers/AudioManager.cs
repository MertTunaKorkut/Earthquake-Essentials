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
    [SerializeField] private AudioSource environmentSource;

    [Header("Ses Ayarları")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float environmentVolume = 0.5f;

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

    /// <summary>
    /// Çevresel (loop eden veya yarıda kesilebilen) sesleri oynatır.
    /// </summary>
    public void PlayEnvironment(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        environmentSource.clip = clip;
        environmentSource.loop = loop;
        environmentSource.volume = sfxVolume; // İstersen buna özel bir volume de yapabilirsin
        environmentSource.Play();
    }

    /// <summary>
    /// Çevresel sesi (deprem, ateş vb.) anında durdurur.
    /// </summary>
    public void StopEnvironment()
    {
        environmentSource.Stop();
    }
}
