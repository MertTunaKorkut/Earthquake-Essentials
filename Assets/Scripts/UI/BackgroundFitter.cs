using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFitter : MonoBehaviour
{
    [Tooltip("Deprem sarsıntısında kenarlarda siyah boşluk çıkmaması için taşma payı (Örn: 1.1 = %10 daha büyük)")]
    public float shakeMargin = 1.1f;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr.sprite == null) return;

        // Kameranın boyutları
        float cameraHeight = Camera.main.orthographicSize * 2f;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        // Resmin boyutları
        float spriteHeight = sr.sprite.bounds.size.y;
        float spriteWidth = sr.sprite.bounds.size.x;

        // Oranlar
        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;

        // Ekranı kesinlikle kaplaması için en büyük oranı seç ve sarsıntı payı (margin) ile çarp
        float maxScale = Mathf.Max(scaleX, scaleY) * shakeMargin;

        transform.localScale = new Vector3(maxScale, maxScale, 1f);
    }
}