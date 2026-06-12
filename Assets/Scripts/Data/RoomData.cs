using UnityEngine;

/// <summary>
/// Level 2'deki bir odanın veri tanımı.
/// Unity Editor'de Assets > Create > Earthquake Essentials > Room Data ile oluşturulur.
/// Her oda için bir ScriptableObject oluşturun.
/// </summary>
[CreateAssetMenu(fileName = "NewRoom", menuName = "Earthquake Essentials/Room Data")]
public class RoomData : ScriptableObject
{
    [Header("Oda Bilgileri")]
    [Tooltip("Odanın görünen adı (ör. 'Salon')")]
    public string roomName;

    [Header("Zamanlama")]
    [Tooltip("Oda süresi (saniye)")]
    public float timeLimit = 30f;

    [Tooltip("Başlangıç geri sayım süresi (saniye)")]
    public int countdownDuration = 3;
}
