using UnityEngine;
using System;

/// <summary>
/// Acil durum çantasının drop zone mantığı.
/// Eşyalar çantanın üzerine bırakıldığında doğru/yanlış kontrolü yapar.
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

    private void Awake()
    {
        // Collider'ın trigger olduğundan emin ol
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Sadece bırakılan (sürükleme biten) DraggableItem'ları kontrol et
        DraggableItem item = other.GetComponent<DraggableItem>();
        if (item == null) return;
        if (item.itemData == null) return;

        // Eşya hâlâ sürükleniyorsa henüz işlem yapma
        // OnMouseUp tetiklenince trigger tekrar kontrol edilecek
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // DraggableItem'ın mouse bırakılmasını bekle
        DraggableItem item = other.GetComponent<DraggableItem>();
        if (item == null) return;
        if (item.itemData == null) return;

        // Mouse'un bırakılıp bırakılmadığını kontrol et
        if (Input.GetMouseButtonUp(0))
        {
            ProcessItem(item);
        }
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
    }
}
