using UnityEngine;
using System;

/// <summary>
/// Oyunun global puan sistemini yöneten singleton manager.
/// Doğru seçimler +100, yanlış seçimler -50 puan verir.
/// DontDestroyOnLoad ile sahneler arası yaşar.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Puan Ayarları")]
    public int correctPoints = 100;
    public int incorrectPoints = -50;

    [Header("Mevcut Durum")]
    [SerializeField] private int currentScore = 0;
    [SerializeField] private int correctCount = 0;
    [SerializeField] private int incorrectCount = 0;

    /// <summary>
    /// Skor değiştiğinde tetiklenir. UI güncellemeleri için kullanın.
    /// Parametreler: yeni skor, eklenen/çıkarılan puan
    /// </summary>
    public event Action<int, int> OnScoreChanged;

    /// <summary>
    /// Doğru seçim yapıldığında tetiklenir.
    /// </summary>
    public event Action OnCorrectChoice;

    /// <summary>
    /// Yanlış seçim yapıldığında tetiklenir.
    /// </summary>
    public event Action OnIncorrectChoice;

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
    /// Doğru bir seçim yapıldığında çağrılır (+100 puan).
    /// </summary>
    public void AddCorrect()
    {
        correctCount++;
        currentScore += correctPoints;
        OnScoreChanged?.Invoke(currentScore, correctPoints);
        OnCorrectChoice?.Invoke();
        Debug.Log($"[ScoreManager] Doğru! Skor: {currentScore} (+{correctPoints})");
    }

    /// <summary>
    /// Yanlış bir seçim yapıldığında çağrılır (-50 puan).
    /// </summary>
    public void AddIncorrect()
    {
        incorrectCount++;
        currentScore += incorrectPoints; // negatif değer eklenecek
        OnScoreChanged?.Invoke(currentScore, incorrectPoints);
        OnIncorrectChoice?.Invoke();
        Debug.Log($"[ScoreManager] Yanlış! Skor: {currentScore} ({incorrectPoints})");
    }

    /// <summary>
    /// Mevcut skoru döndürür.
    /// </summary>
    public int GetScore() => currentScore;

    /// <summary>
    /// Doğru seçim sayısını döndürür.
    /// </summary>
    public int GetCorrectCount() => correctCount;

    /// <summary>
    /// Yanlış seçim sayısını döndürür.
    /// </summary>
    public int GetIncorrectCount() => incorrectCount;

    /// <summary>
    /// Tüm puanları sıfırlar (yeni oyun başlatırken).
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        correctCount = 0;
        incorrectCount = 0;
        OnScoreChanged?.Invoke(currentScore, 0);
        Debug.Log("[ScoreManager] Skor sıfırlandı.");
    }
}
