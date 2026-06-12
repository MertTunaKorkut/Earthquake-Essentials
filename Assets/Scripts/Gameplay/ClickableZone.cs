using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;

/// <summary>
/// Level 2'deki tıklanabilir güvenli/tehlikeli bölge component'ı.
/// Her bölge için bir GameObject'e eklenir.
/// EventSystem uyumlu: IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler.
/// Gereksinim: Sahnede EventSystem + Kamerada Physics2DRaycaster olmalı.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ClickableZone : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Zone Verisi")]
    [Tooltip("Bu bölgenin adı")]
    public string zoneName;

    [Tooltip("Bu bölge güvenli mi?")]
    public bool isSafe;

    [TextArea(1, 3)]
    [Tooltip("Geri bildirim metni")]
    public string feedbackText;

    [Header("Karakter Pozisyonu")]
    [Tooltip("Karakter bu bölgeye geldiğinde duracağı pozisyon (child empty GameObject)")]
    public Transform characterAnchor;

    [Header("Tehlike Animasyonu (Sadece Tehlikeli Zone)")]
    [Tooltip("Bu bölgenin tehlike objesi sprite'ı (avize, buzdolabı, alev vb.) — ayrı child GameObject")]
    public SpriteRenderer hazardSpriteRenderer;

    [Tooltip("Tehlike animasyon tipi")]
    public HazardAnimationType hazardAnimationType = HazardAnimationType.FallFromAbove;

    [Tooltip("Objenin düşme/devrilme süresi (saniye)")]
    public float hazardAnimDuration = 0.4f;

    [Tooltip("FallFromAbove: Objenin ne kadar yukarıdan düşeceği (Y offset)")]
    public float hazardFallHeight = 5f;

    [Tooltip("Topple: Devrilme açısı (derece)")]
    public float hazardToppleAngle = 75f;

    [Tooltip("Topple: Devrilme yönü (1 = sağa, -1 = sola)")]
    public float hazardToppleDirection = 1f;

    [Header("Görsel Ayarlar")]
    [Tooltip("Hover efektinde scale çarpanı")]
    public float hoverScaleMultiplier = 1.08f;

    [Tooltip("Hover efektinde alpha artışı")]
    public float hoverAlphaBoost = 0.3f;

    [Tooltip("Yeşil flash rengi")]
    public Color flashColor = new Color(0.2f, 0.9f, 0.2f, 0.6f);

    [Tooltip("Yeşil flash süresi (saniye)")]
    public float flashDuration = 0.6f;

    [Tooltip("Yeşil flash tekrar sayısı")]
    public int flashCount = 3;

    // Olaylar
    /// <summary>
    /// Zone tıklandığında tetiklenir. Level2Controller dinler.
    /// </summary>
    public event Action<ClickableZone> OnZoneClicked;

    // Dahili
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Color originalColor;
    private bool isInteractable = false;
    private bool isDisabled = false;
    private bool isHovering = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        originalColor = spriteRenderer.color;

        // characterAnchor yoksa kendi pozisyonunu kullan
        if (characterAnchor == null)
            characterAnchor = transform;

        // Tehlike sprite'ını başlangıçta gizle
        if (hazardSpriteRenderer != null)
            hazardSpriteRenderer.gameObject.SetActive(false);
    }

    /// <summary>
    /// Zone tıklandığında çağrılır.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable || isDisabled) return;

        Debug.Log($"[ClickableZone] '{zoneName}' tıklandı! (Safe: {isSafe})");
        OnZoneClicked?.Invoke(this);
    }

    /// <summary>
    /// Mouse zone üzerine geldiğinde — hover efekti.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable || isDisabled) return;

        isHovering = true;

        // Scale up
        transform.localScale = originalScale * hoverScaleMultiplier;

        // Alpha artışı
        Color c = spriteRenderer.color;
        c.a = Mathf.Clamp01(c.a + hoverAlphaBoost);
        spriteRenderer.color = c;
    }

    /// <summary>
    /// Mouse zone'dan çıktığında — hover efektini kaldır.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable || isDisabled) return;

        isHovering = false;

        // Scale geri
        transform.localScale = originalScale;

        // Rengi geri
        spriteRenderer.color = originalColor;
    }

    /// <summary>
    /// Zone'un tıklanabilirliğini açar/kapatır.
    /// Countdown sırasında kapalı, gameplay sırasında açık olur.
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;

        // Eğer hover halindeyken kapatılırsa, hover efektini temizle
        if (!interactable && isHovering)
        {
            isHovering = false;
            transform.localScale = originalScale;
            spriteRenderer.color = originalColor;
        }
    }

    /// <summary>
    /// Zone'u kalıcı olarak devre dışı bırakır (yanlış seçim sonrası).
    /// Görsel olarak yarı saydam + küçük yapılır.
    /// </summary>
    public void Disable()
    {
        isDisabled = true;
        isInteractable = false;

        // Hover efektini temizle
        if (isHovering)
        {
            isHovering = false;
            transform.localScale = originalScale;
        }

        // Yarı saydam yap
        Color c = originalColor;
        c.a = 0.3f;
        spriteRenderer.color = c;

        Debug.Log($"[ClickableZone] '{zoneName}' devre dışı bırakıldı.");
    }

    /// <summary>
    /// Doğru seçimde yeşil yanıp sönme efekti.
    /// </summary>
    public void FlashGreen(Action onComplete = null)
    {
        StartCoroutine(FlashGreenRoutine(onComplete));
    }

    private IEnumerator FlashGreenRoutine(Action onComplete)
    {
        float halfFlash = flashDuration / (flashCount * 2f);

        for (int i = 0; i < flashCount; i++)
        {
            // Yeşile geç
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(halfFlash);

            // Orijinale dön
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(halfFlash);
        }

        onComplete?.Invoke();
    }

    /// <summary>
    /// Karakterin duracağı pozisyonu döndürür.
    /// </summary>
    public Vector3 GetCharacterPosition()
    {
        return characterAnchor.position;
    }

    /// <summary>
    /// Zone'un hâlâ aktif (tıklanabilir durumda) olup olmadığını döndürür.
    /// </summary>
    public bool IsActive => !isDisabled;
}
