using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD Controller — skor, ilerleme göstergesi ve timer.
/// Level 1'de skor + ilerleme, Level 2'de skor + timer gösterir.
/// Canvas altında olmalı.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Skor UI")]
    [Tooltip("Mevcut skoru gösteren metin")]
    public TextMeshProUGUI scoreText;

    [Header("İlerleme UI")]
    [Tooltip("'X / Y eşya' ilerleme metni")]
    public TextMeshProUGUI progressText;

    [Tooltip("İlerleme çubuğu (opsiyonel)")]
    public Slider progressSlider;

    [Header("Timer UI (Level 2)")]
    [Tooltip("Geri sayım göstergesi (Level 2'de kullanılır, Level 1'de null bırakılır)")]
    public TextMeshProUGUI timerText;

    [Tooltip("Timer bu sürenin altına düşünce kırmızı nabız efekti başlar (saniye)")]
    public float timerWarningThreshold = 5f;

    // Dahili
    private int totalRequired = 0;
    private float totalTimerSeconds = 0f;
    private Color timerDefaultColor;
    private Color timerWarningColor = Color.red;
    private bool timerInitialized = false;

    private void Start()
    {
        // ScoreManager olaylarını dinle
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
            UpdateScoreDisplay(ScoreManager.Instance.GetScore());
        }
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
    }

    /// <summary>
    /// Level başında çağrılır — toplam eşya sayısını ayarlar.
    /// </summary>
    public void Initialize(int required)
    {
        totalRequired = required;
        UpdateProgress(0, required);

        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = required;
            progressSlider.value = 0;
        }
    }

    /// <summary>
    /// İlerleme göstergesini günceller.
    /// </summary>
    public void UpdateProgress(int current, int total)
    {
        if (progressText != null)
            progressText.text = $"{current} / {total}";

        if (progressSlider != null)
            progressSlider.value = current;
    }

    // ─────────────────────────── TIMER (Level 2) ───────────────────────────

    /// <summary>
    /// Level 2 başında çağrılır — timer'ı ayarlar.
    /// </summary>
    public void InitializeTimer(float totalSeconds)
    {
        totalTimerSeconds = totalSeconds;
        timerInitialized = true;

        if (timerText != null)
        {
            timerDefaultColor = timerText.color;
            UpdateTimer(totalSeconds);
        }
    }

    /// <summary>
    /// Timer göstergesini günceller. Level2Controller her frame çağırır.
    /// 5 saniyenin altında kırmızı nabız efekti başlar.
    /// </summary>
    public void UpdateTimer(float remainingSeconds)
    {
        if (timerText == null || !timerInitialized) return;

        // Negatife düşmesin
        remainingSeconds = Mathf.Max(0f, remainingSeconds);

        // Saniye formatı: "00:15" veya sadece "15"
        int seconds = Mathf.CeilToInt(remainingSeconds);
        timerText.text = seconds.ToString();

        // Uyarı efekti (5 saniye altı)
        if (remainingSeconds <= timerWarningThreshold && remainingSeconds > 0f)
        {
            // Nabız efekti: sin dalgası ile alpha ve renk değişimi
            float pulse = (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f; // 0-1 arası nabız
            timerText.color = Color.Lerp(timerDefaultColor, timerWarningColor, pulse);

            // Scale nabız
            float scalePulse = 1f + pulse * 0.15f; // 1.0 - 1.15 arası
            timerText.rectTransform.localScale = Vector3.one * scalePulse;
        }
        else
        {
            timerText.color = timerDefaultColor;
            timerText.rectTransform.localScale = Vector3.one;
        }
    }

    // ─────────────────────────── SKOR ───────────────────────────

    private void HandleScoreChanged(int newScore, int delta)
    {
        UpdateScoreDisplay(newScore);
    }

    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Puan: {score}";
    }
}
