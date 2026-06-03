using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Acil durum çantasının drop zone mantığı.
/// Eşyalar çantanın üzerine bırakıldığında doğru/yanlış kontrolü yapar.
/// Event-driven yaklaşım: DraggableItem'ın OnDropped event'ini dinler.
/// Trigger ile zone içindeki eşyaları takip eder.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class BackpackDropZone : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Kabul edilmesi gereken toplam doğru eşya sayısı")]
    public int requiredCorrectItems = 5;

    [Header("Durum")]
    [SerializeField] private int acceptedItemCount = 0;

    /// <summary>
    /// Tüm doğru eşyalar toplandığında tetiklenir.
    /// </summary>
    public event Action OnAllItemsCollected;

    /// <summary>
    /// Bir eşya kabul edildiğinde tetiklenir. Parametre: kabul edilen eşyanın verisi.
    /// </summary>
    public event Action<ItemData> OnItemAccepted;

    /// <summary>
    /// Bir eşya reddedildiğinde tetiklenir. Parametre: reddedilen eşyanın verisi.
    /// </summary>
    public event Action<ItemData> OnItemRejected;

    /// <summary>
    /// Şu anda zone içinde bulunan DraggableItem'ların listesi.
    /// Trigger giriş/çıkış ile güncellenir.
    /// </summary>
    private HashSet<DraggableItem> itemsInZone = new HashSet<DraggableItem>();

    private void Awake()
    {
        // Collider'ın trigger olduğundan emin ol
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    /// <summary>
    /// Bir eşya zone'a girdiğinde: kaydet ve OnDropped event'ine abone ol.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        DraggableItem item = other.GetComponent<DraggableItem>();
        if (item == null) return;
        if (item.itemData == null) return;

        // Zone listesine ekle
        if (itemsInZone.Add(item))
        {
            // Bırakma event'ine abone ol
            item.OnDropped += HandleItemDropped;
            Debug.Log($"[Backpack] '{item.itemData.itemName}' zone'a girdi.");
        }
    }

    /// <summary>
    /// Bir eşya zone'dan çıktığında: kayıttan sil ve event aboneliğini kaldır.
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        DraggableItem item = other.GetComponent<DraggableItem>();
        if (item == null) return;

        if (itemsInZone.Remove(item))
        {
            // Event aboneliğini temizle
            item.OnDropped -= HandleItemDropped;
            Debug.Log($"[Backpack] '{item.itemData?.itemName}' zone'dan çıktı.");
        }
    }

    /// <summary>
    /// DraggableItem bırakıldığında çağrılır (event callback).
    /// Eşya hâlâ zone içindeyse işle.
    /// </summary>
    private void HandleItemDropped(DraggableItem item)
    {
        // Eşya zone içinde mi kontrol et
        if (!itemsInZone.Contains(item)) return;

        // Aboneliği temizle (bir kez işlendi)
        item.OnDropped -= HandleItemDropped;
        itemsInZone.Remove(item);

        ProcessItem(item);
    }

    /// <summary>
    /// Eşyayı kontrol edip kabul veya reddet.
    /// </summary>
    private void ProcessItem(DraggableItem item)
    {
        if (item.itemData.isEssential)
        {
            // Doğru eşya — kabul et
            acceptedItemCount++;
            ScoreManager.Instance.AddCorrect();
            item.Accept();
            OnItemAccepted?.Invoke(item.itemData);

            Debug.Log($"[Backpack] '{item.itemData.itemName}' kabul edildi! ({acceptedItemCount}/{requiredCorrectItems})");

            // Tüm eşyalar toplandı mı?
            if (acceptedItemCount >= requiredCorrectItems)
            {
                Debug.Log("[Backpack] Tüm eşyalar toplandı! Level tamamlandı!");
                OnAllItemsCollected?.Invoke();
            }
        }
        else
        {
            // Yanlış eşya — reddet
            ScoreManager.Instance.AddIncorrect();
            item.Reject();
            OnItemRejected?.Invoke(item.itemData);

            Debug.Log($"[Backpack] '{item.itemData.itemName}' reddedildi!");
        }
    }

    /// <summary>
    /// Kabul edilen eşya sayısını döndürür.
    /// </summary>
    public int GetAcceptedCount() => acceptedItemCount;

    /// <summary>
    /// Drop zone'u sıfırla (yeniden başlatma için).
    /// </summary>
    public void ResetDropZone()
    {
        acceptedItemCount = 0;

        // Tüm event aboneliklerini temizle
        foreach (var item in itemsInZone)
        {
            if (item != null)
                item.OnDropped -= HandleItemDropped;
        }
        itemsInZone.Clear();
    }

    private void OnDestroy()
    {
        // Sahne kapanırken tüm abonelikleri temizle (memory leak önlemi)
        foreach (var item in itemsInZone)
        {
            if (item != null)
                item.OnDropped -= HandleItemDropped;
        }
        itemsInZone.Clear();
    }
}
