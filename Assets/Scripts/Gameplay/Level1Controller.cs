using UnityEngine;
using System.Collections;

/// <summary>
/// Level 1 (Acil Durum Çantası) sahnesini orkestre eden controller.
/// BackpackDropZone olaylarını dinler, HUD'ı günceller ve sahneyi bitirir.
/// </summary>
public class Level1Controller : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Sahnedeki çanta drop zone")]
    public BackpackDropZone backpackDropZone;

    [Tooltip("HUD controller (skor, ilerleme göstergesi)")]
    public HUDController hudController;

    [Tooltip("Geri bildirim UI")]
    public FeedbackUI feedbackUI;

    [Tooltip("Level sonu ekranı (puan + sonraki seviye butonu)")]
    public LevelCompleteUI levelCompleteUI;

    [Header("Level Ayarları")]
    [Tooltip("Bu level'da kaç doğru eşya toplanmalı")]
    public int requiredItems = 5;

    [Tooltip("Level tamamlandıktan sonra geçiş bekleme süresi (saniye) — sadece FeedbackUI gecikmesi için")]
    public float completionDelay = 1.5f;

    [Header("Ses Dosyaları")]
    [Tooltip("Arka plan müziği (loop olarak çalar)")]
    public AudioClip bgmClip;

    [Tooltip("Doğru eşya kabul edildiğinde çalan ses")]
    public AudioClip acceptSFX;

    [Tooltip("Yanlış eşya reddedildiğinde çalan ses")]
    public AudioClip rejectSFX;

    private bool levelCompleted = false;

    private void Start()
    {
        if (backpackDropZone == null)
        {
            Debug.LogError("[Level1Controller] BackpackDropZone referansı atanmamış!");
            return;
        }

        // Drop zone olaylarını dinle
        backpackDropZone.OnItemAccepted  += HandleItemAccepted;
        backpackDropZone.OnItemRejected  += HandleItemRejected;
        backpackDropZone.OnAllItemsCollected += HandleAllItemsCollected;

        // HUD'ı başlat
        if (hudController != null)
            hudController.Initialize(requiredItems);

        // Arka plan müziğini başlat
        if (bgmClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(bgmClip);

        Debug.Log("[Level1Controller] Level 1 başladı!");
    }

    private void OnDestroy()
    {
        // Olay dinleyicilerini temizle (memory leak önlemi)
        if (backpackDropZone != null)
        {
            backpackDropZone.OnItemAccepted      -= HandleItemAccepted;
            backpackDropZone.OnItemRejected      -= HandleItemRejected;
            backpackDropZone.OnAllItemsCollected -= HandleAllItemsCollected;
        }
    }

    /// <summary>
    /// Doğru eşya çantaya konulduğunda çağrılır.
    /// </summary>
    private void HandleItemAccepted(ItemData item)
    {
        int accepted = backpackDropZone.GetAcceptedCount();

        // Kabul ses efekti
        if (acceptSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(acceptSFX);

        if (feedbackUI != null)
            feedbackUI.ShowCorrect(item.itemName);

        if (hudController != null)
            hudController.UpdateProgress(accepted, requiredItems);

        Debug.Log($"[Level1]  '{item.itemName}' kabul edildi. İlerleme: {accepted}/{requiredItems}");
    }

    /// <summary>
    /// Yanlış eşya çantaya konulmak istendiğinde çağrılır.
    /// </summary>
    private void HandleItemRejected(ItemData item)
    {
        // Red ses efekti
        if (rejectSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(rejectSFX);

        if (feedbackUI != null)
            feedbackUI.ShowWrong(item.itemName);

        Debug.Log($"[Level1]  '{item.itemName}' reddedildi.");
    }

    /// <summary>
    /// Tüm gerekli eşyalar toplandığında çağrılır — level'ı tamamlar.
    /// </summary>
    private void HandleAllItemsCollected()
    {
        if (levelCompleted) return;
        levelCompleted = true;

        Debug.Log("[Level1]  Tüm eşyalar toplandı! Level tamamlanıyor...");

        // Level bittiğinde müziği durdur
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();

        // Level sonu ekranını göster (puan + sonraki seviye butonu)
        if (levelCompleteUI != null)
        {
            levelCompleteUI.Show();
        }
        else
        {
            // Fallback: LevelCompleteUI yoksa eski davranış
            Debug.LogWarning("[Level1Controller] LevelCompleteUI atanmamış, doğrudan geçiş yapılıyor.");
            if (feedbackUI != null)
                feedbackUI.ShowLevelComplete();
            StartCoroutine(FallbackCompleteLevel());
        }
    }

    /// <summary>
    /// LevelCompleteUI atanmamışsa kullanılacak fallback geçişi.
    /// </summary>
    private IEnumerator FallbackCompleteLevel()
    {
        yield return new WaitForSeconds(completionDelay);

        if (GameManager.Instance != null)
            GameManager.Instance.OnLevel1Complete();
        else
            Debug.LogWarning("[Level1Controller] GameManager bulunamadı, sahne geçişi yapılamıyor.");
    }
}
