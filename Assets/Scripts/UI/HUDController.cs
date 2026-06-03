using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Level 1 HUD Controller — skor ve eşya ilerleme göstergesi.
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

    // Dahili
    private int totalRequired = 0;

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
