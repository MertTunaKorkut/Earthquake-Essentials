using UnityEngine;

/// <summary>
/// Deprem kamera sallama efekti.
/// Perlin noise bazlı smooth sallama ile doğal deprem hissi verir.
/// Ana kameraya eklenmeli. Level2Controller tarafından başlatılır/durdurulur.
/// </summary>
public class CameraShake : MonoBehaviour
{
    [Header("Sallama Ayarları")]
    [Tooltip("Sallama şiddeti (piksel cinsinden offset)")]
    public float magnitude = 0.15f;

    [Tooltip("Sallama frekansı (ne kadar hızlı sallanacak)")]
    public float frequency = 3f;

    // Dahili
    private Vector3 originalPosition;
    private bool isShaking = false;
    private float noiseSeedX;
    private float noiseSeedY;

    private void Awake()
    {
        originalPosition = transform.localPosition;
        // Her seferinde farklı sallama deseni için rastgele seed
        noiseSeedX = Random.Range(0f, 100f);
        noiseSeedY = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (!isShaking) return;

        float time = Time.time * frequency;

        // Perlin noise ile smooth, doğal sallama
        float offsetX = (Mathf.PerlinNoise(noiseSeedX + time, 0f) - 0.5f) * 2f * magnitude;
        float offsetY = (Mathf.PerlinNoise(0f, noiseSeedY + time) - 0.5f) * 2f * magnitude;

        transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
    }

    /// <summary>
    /// Kamera sallamayı başlatır.
    /// </summary>
    public void StartShake()
    {
        if (isShaking) return;

        originalPosition = transform.localPosition;
        isShaking = true;

        // Yeni seed ile her başlatmada farklı desen
        noiseSeedX = Random.Range(0f, 100f);
        noiseSeedY = Random.Range(0f, 100f);
    }

    /// <summary>
    /// Kamera sallamayı durdurur ve pozisyonu eski haline getirir.
    /// </summary>
    public void StopShake()
    {
        isShaking = false;
        transform.localPosition = originalPosition;
    }

    /// <summary>
    /// Sallama şiddetini runtime'da değiştirmek için.
    /// </summary>
    public void SetMagnitude(float newMagnitude)
    {
        magnitude = Mathf.Max(0f, newMagnitude);
    }
}
