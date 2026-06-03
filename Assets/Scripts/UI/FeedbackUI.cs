using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Doğru/yanlış seçim ve level tamamlanma geri bildirimlerini gösteren UI.
/// Canvas altında olmalı. Panel önce görünmez, tetiklenince animasyonla açılır.
/// </summary>
public class FeedbackUI : MonoBehaviour
{
    [Header("Panel Referansları")]
    [Tooltip("Doğru geri bildirim paneli (yeşil)")]
    public GameObject correctPanel;

    [Tooltip("Yanlış geri bildirim paneli (kırmızı)")]
    public GameObject wrongPanel;

    [Tooltip("Level tamamlandı paneli")]
    public GameObject levelCompletePanel;

    [Header("Metin Referansları")]
    [Tooltip("Doğru eşya ismini gösteren metin")]
    public TextMeshProUGUI correctItemText;

    [Tooltip("Yanlış eşya ismini gösteren metin")]
    public TextMeshProUGUI wrongItemText;

    [Header("Animasyon Ayarları")]
    [Tooltip("Geri bildirimin ekranda kalma süresi (saniye)")]
    public float displayDuration = 1.5f;

    private Coroutine hideCorrectCoroutine;
    private Coroutine hideWrongCoroutine;

    private void Awake()
    {
        // Panelleri başlangıçta gizle
        if (correctPanel != null) correctPanel.SetActive(false);
        if (wrongPanel != null)   wrongPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
    }

    /// <summary>
    /// Doğru seçim geri bildirimini gösterir.
    /// </summary>
    public void ShowCorrect(string itemName = "")
    {
        if (correctPanel == null) return;

        // Önceki coroutine varsa durdur
        if (hideCorrectCoroutine != null)
            StopCoroutine(hideCorrectCoroutine);

        if (correctItemText != null && !string.IsNullOrEmpty(itemName))
            correctItemText.text = $"{itemName}";

        correctPanel.SetActive(true);
        hideCorrectCoroutine = StartCoroutine(HideAfterDelay(correctPanel, displayDuration));
    }

    /// <summary>
    /// Yanlış seçim geri bildirimini gösterir.
    /// </summary>
    public void ShowWrong(string itemName = "")
    {
        if (wrongPanel == null) return;

        if (hideWrongCoroutine != null)
            StopCoroutine(hideWrongCoroutine);

        if (wrongItemText != null && !string.IsNullOrEmpty(itemName))
            wrongItemText.text = $"{itemName}";

        wrongPanel.SetActive(true);
        hideWrongCoroutine = StartCoroutine(HideAfterDelay(wrongPanel, displayDuration));
    }

    /// <summary>
    /// Level tamamlandı ekranını gösterir (kalıcı).
    /// </summary>
    public void ShowLevelComplete()
    {
        // Diğer panelleri kapat
        if (correctPanel != null) correctPanel.SetActive(false);
        if (wrongPanel != null)   wrongPanel.SetActive(false);

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);
    }

    private IEnumerator HideAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panel != null)
            panel.SetActive(false);
    }
}
