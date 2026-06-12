using UnityEngine;

/// <summary>
/// Bir tıklanabilir bölgenin veri tanımı.
/// ScriptableObject değil, ClickableZone component'ı üzerinde inline serialize edilir.
/// </summary>
[System.Serializable]
public class ZoneData
{
    [Tooltip("Bölgenin görünen adı (ör. 'Sağlam Ahşap Sehpa Altı')")]
    public string zoneName;

    [Tooltip("Bu bölge güvenli mi?")]
    public bool isSafe;

    [TextArea(1, 3)]
    [Tooltip("Geri bildirim metni (ör. 'Çök-Kapan-Tutun!' veya 'Cam kırılma tehlikesi!')")]
    public string feedbackText;
}
