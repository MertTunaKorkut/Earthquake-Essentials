using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Level sonu ekranı — arka plan blur + orta kutuda toplam puan ve sonraki seviye butonu.
/// Canvas altında olmalı. Başlangıçta kapalı, Level1Controller tarafından açılır.
/// 
/// Sahne Kurulumu:
/// 1. Canvas altına boş bir GameObject ekle ("LevelCompleteScreen") ve bu scripti bağla
/// 2. Altına bir Image ekle (blurOverlay — tüm ekranı kaplar, yarı saydam siyah)
/// 3. Altına bir Panel ekle (contentPanel — ortalanmış kutu)
/// 4. Panel içine TextMeshPro (scoreText) ve Button (nextLevelButton) ekle
/// </summary>
public class LevelCompleteUI : MonoBehaviour
{
    [Header("Panel Referansları")]
    [Tooltip("Tüm level complete ekranının root objesi")]
    public GameObject rootPanel;

    [Tooltip("Arka plan karartma/blur overlay Image")]
    public Image blurOverlay;

    [Tooltip("Ortadaki içerik kutusu paneli")]
    public GameObject contentPanel;

    [Header("UI Elemanları")]
    [Tooltip("Toplam puanı gösteren metin")]
    public TextMeshProUGUI scoreText;

    [Tooltip("'Sonraki Seviye' butonu")]
    public Button nextLevelButton;

    [Header("Animasyon Ayarları")]
    [Tooltip("Ekranın açılma süresi (saniye)")]
    public float fadeInDuration = 0.5f;

    // Dahili
    private CanvasGroup overlayCanvasGroup;
    private CanvasGroup contentCanvasGroup;

    private void Awake()
    {
        // Başlangıçta gizle
        if (rootPanel != null)
            rootPanel.SetActive(false);

        // Buton olayını bağla
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
    }

    /// <summary>
    /// Level tamamlandığında bu metot çağrılır — ekranı açar ve puanı gösterir.
    /// </summary>
    public void Show()
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        // Puanı ScoreManager'dan al
        int totalScore = 0;
        if (ScoreManager.Instance != null)
            totalScore = ScoreManager.Instance.GetScore();

        // Puan metnini güncelle
        if (scoreText != null)
            scoreText.text = totalScore.ToString();

        // Açılış animasyonu başlat
        StartCoroutine(FadeInRoutine());
    }

    /// <summary>
    /// Ekranı gizler.
    /// </summary>
    public void Hide()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    /// <summary>
    /// "Sonraki Seviye" butonuna basıldığında çağrılır.
    /// </summary>
    private void OnNextLevelClicked()
    {
        // Butonun birden fazla kez tıklanmasını engelle
        if (nextLevelButton != null)
            nextLevelButton.interactable = false;

        // GameManager üzerinden sonraki seviyeye geç
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevel1Complete();
        }
        else
        {
            Debug.LogWarning("[LevelCompleteUI] GameManager bulunamadı!");
        }
    }

    /// <summary>
    /// Overlay ve content panelinin fade-in animasyonu.
    /// CanvasGroup yoksa anlık açar.
    /// </summary>
    private System.Collections.IEnumerator FadeInRoutine()
    {
        // CanvasGroup bileşenlerini ara veya oluştur
        overlayCanvasGroup = GetOrAddCanvasGroup(blurOverlay?.gameObject);
        contentCanvasGroup = GetOrAddCanvasGroup(contentPanel);

        if (overlayCanvasGroup != null)
            overlayCanvasGroup.alpha = 0f;
        if (contentCanvasGroup != null)
        {
            contentCanvasGroup.alpha = 0f;
            // İçerik kutusunu biraz aşağıdan gelsin (scale animasyonu)
            if (contentPanel != null)
                contentPanel.transform.localScale = Vector3.one * 0.8f;
        }

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);

            // Ease out cubic: daha doğal hissettiren eğri
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            if (overlayCanvasGroup != null)
                overlayCanvasGroup.alpha = eased;

            if (contentCanvasGroup != null)
            {
                contentCanvasGroup.alpha = eased;
                if (contentPanel != null)
                    contentPanel.transform.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, eased);
            }

            yield return null;
        }

        // Son değerleri garanti et
        if (overlayCanvasGroup != null) overlayCanvasGroup.alpha = 1f;
        if (contentCanvasGroup != null) contentCanvasGroup.alpha = 1f;
        if (contentPanel != null) contentPanel.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Verilen GameObject'e CanvasGroup ekler veya mevcutu döndürür.
    /// </summary>
    private CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        if (go == null) return null;

        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = go.AddComponent<CanvasGroup>();

        return cg;
    }

    private void OnDestroy()
    {
        if (nextLevelButton != null)
            nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
    }
}
