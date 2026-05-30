using UnityEngine;

/// <summary>
/// Level 1'deki eşyaların veri tanımı.
/// Unity Editor'de Assets > Create > Earthquake Essentials > Item Data ile oluşturulur.
/// Her eşya için bir ScriptableObject oluşturun.
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Earthquake Essentials/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Eşya Bilgileri")]
    [Tooltip("Eşyanın görünen adı")]
    public string itemName;

    [Tooltip("Eşyanın sprite görüntüsü")]
    public Sprite itemSprite;

    [Header("Gameplay")]
    [Tooltip("Bu eşya acil durum çantasına konulmalı mı?")]
    public bool isEssential;

    [Tooltip("Eşya türü")]
    public ItemType itemType;

    [TextArea(2, 4)]
    [Tooltip("Eşya hakkında kısa açıklama (opsiyonel, UI'da gösterilebilir)")]
    public string description;

    /// <summary>
    /// Bu eşyanın puan değerini döndürür.
    /// Essential ise +100, değilse -50.
    /// </summary>
    public int GetPointValue()
    {
        return isEssential ? 100 : -50;
    }
}

/// <summary>
/// Eşya türlerini tanımlayan enum.
/// </summary>
public enum ItemType
{
    Essential,      // Gerekli eşya (su, ilk yardım vb.)
    NonEssential,   // Gereksiz eşya (oyuncak, parfüm vb.)
    Hazardous       // Tehlikeli eşya (çakmak vb.)
}
