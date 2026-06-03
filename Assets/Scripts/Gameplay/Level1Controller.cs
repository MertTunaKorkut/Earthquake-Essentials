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

    [Header("Level Ayarları")]
    [Tooltip("Bu level'da kaç doğru eşya toplanmalı")]
    public int requiredItems = 5;

    [Tooltip("Level tamamlandıktan sonra geçiş bekleme süresi (saniye)")]
    public float completionDelay = 1.5f;

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

        if (feedbackUI != null)
            feedbackUI.ShowLevelComplete();

        StartCoroutine(CompleteLevel());
    }

    private IEnumerator CompleteLevel()
    {
        yield return new WaitForSeconds(completionDelay);

        // GameManager üzerinden Level 2'ye geç
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevel1Complete();
        }
        else
        {
            Debug.LogWarning("[Level1Controller] GameManager bulunamadı, sahne geçişi yapılamıyor.");
        }
    }
}
