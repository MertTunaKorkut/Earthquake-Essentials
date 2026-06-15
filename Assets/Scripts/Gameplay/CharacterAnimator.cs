using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Level 2'de karakterin sprite değişimlerini, pozisyon hareketini
/// ve zone-spesifik tehlike animasyonlarını yöneten component.
/// Karakter GameObject'ine eklenmeli.
/// 
/// Desteklenen tehlike animasyonları:
/// - FallFromAbove: Avize, kaya vb. yukarıdan düşer
/// - Topple: Buzdolabı, gardırop vb. öne/yana devrilir
/// - Shatter: Cam pencere vb. yerinde kırılır (scale + shake)
/// - Ignite: Ocak vb. alev sprite'ı büyüyerek belirir
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class CharacterAnimator : MonoBehaviour
{
    [Header("Karakter Sprite'ları")]
    [Tooltip("Ayakta durma sprite'ı")]
    public Sprite idleSprite;

    [Tooltip("Korunma pozisyonu sprite'ı (Çök-Kapan-Tutun)")]
    public Sprite protectSprite;

    [Tooltip("Hasar alma sprite'ı")]
    public Sprite damageSprite;

    [Header("Hareket Ayarları")]
    [Tooltip("Karakterin bölgeye hareket hızı")]
    public float moveSpeed = 8f;

    [Header("Fade Ayarları")]
    [Tooltip("Karakterin kaybolma/belirme süresi (saniye)")]
    public float fadeDuration = 0.3f;

    [Header("Hasar Ayarları")]
    [Tooltip("Hasar pozisyonunda bekleme süresi (saniye)")]
    public float damageHoldDuration = 1.0f;

    // Dahili
    private SpriteRenderer characterSpriteRenderer;
    private Vector3 roomCenterPosition;
    private bool isMoving = false;

    private void Awake()
    {
        characterSpriteRenderer = GetComponent<SpriteRenderer>();
        roomCenterPosition = transform.position;
    }

    // ════════════════════════════ SPRITE DEĞİŞİMLERİ ════════════════════════════

    /// <summary>
    /// Karakteri idle sprite'ına geçirir.
    /// </summary>
    public void ShowIdle()
    {
        if (idleSprite != null)
            characterSpriteRenderer.sprite = idleSprite;

        StartCoroutine(SpriteBounce());
    }

    /// <summary>
    /// Karakteri korunma pozisyonu sprite'ına geçirir.
    /// </summary>
    public void ShowProtect()
    {
        if (protectSprite != null)
            characterSpriteRenderer.sprite = protectSprite;

        StartCoroutine(SpriteBounce());
    }

    /// <summary>
    /// Karakteri hasar sprite'ına geçirir.
    /// </summary>
    public void ShowDamage()
    {
        if (damageSprite != null)
            characterSpriteRenderer.sprite = damageSprite;

        StartCoroutine(SpriteBounce());
    }

    // ════════════════════════════ HAREKET ════════════════════════════

    /// <summary>
    /// Karakteri hedefe doğru smooth hareket ettirir.
    /// Hareket tamamlandığında onComplete callback çağrılır.
    /// </summary>
    public void MoveToPosition(Vector3 target, Action onComplete = null)
    {
        if (isMoving) return;
        StartCoroutine(MoveRoutine(target, onComplete));
    }

    private IEnumerator MoveRoutine(Vector3 target, Action onComplete)
    {
        isMoving = true;
        Vector3 start = transform.position;
        target.z = start.z; // Z pozisyonunu koru

        float distance = Vector3.Distance(start, target);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        // Karakter sağa/sola mı gidiyor? Sprite flip
        if (target.x < start.x)
            characterSpriteRenderer.flipX = true;
        else if (target.x > start.x)
            characterSpriteRenderer.flipX = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease out quad: hızlı başla, yavaş bitir
            float eased = 1f - (1f - t) * (1f - t);
            transform.position = Vector3.Lerp(start, target, eased);

            yield return null;
        }

        transform.position = target;
        isMoving = false;
        onComplete?.Invoke();
    }

    // ════════════════════════════ FADE TELEPORT ════════════════════════════

    /// <summary>
    /// Karakteri fade-out → teleport → fade-in ile hedefe taşır.
    /// Oyuncu bir alana tıkladığında yürümek yerine bu kullanılır.
    /// </summary>
    public void FadeToPosition(Vector3 target, Action onComplete = null)
    {
        if (isMoving) return;
        StartCoroutine(FadeTeleportRoutine(target, onComplete));
    }

    private IEnumerator FadeTeleportRoutine(Vector3 target, Action onComplete)
    {
        isMoving = true;

        // 1. Fade out (idle pozisyonunda kaybol)
        yield return StartCoroutine(FadeCharacter(1f, 0f, fadeDuration));

        // 2. Teleport
        target.z = transform.position.z;
        transform.position = target;

        // 3. Fade in (yeni pozisyonda belir)
        yield return StartCoroutine(FadeCharacter(0f, 1f, fadeDuration));

        isMoving = false;
        onComplete?.Invoke();
    }

    /// <summary>
    /// Karakterin alpha değerini belirli sürede değiştirir (fade-in / fade-out).
    /// </summary>
    private IEnumerator FadeCharacter(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        Color c = characterSpriteRenderer.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(fromAlpha, toAlpha, t);
            characterSpriteRenderer.color = c;
            yield return null;
        }

        c.a = toAlpha;
        characterSpriteRenderer.color = c;
    }

    /// <summary>
    /// Karakteri fade ile odanın ortasına (başlangıç pozisyonuna) geri getirir.
    /// </summary>
    public void FadeToCenter(Action onComplete = null)
    {
        FadeToPosition(roomCenterPosition, onComplete);
    }

    // ════════════════════════════ TEHLİKE ANİMASYONLARI ════════════════════════════

    /// <summary>
    /// Zone-spesifik tehlike animasyonu + hasar combo.
    /// ClickableZone'daki hazard bilgilerini kullanır.
    /// Animasyon tiplerine göre farklı görsel efekt oynatır.
    /// </summary>
    public void PlayHazardDamage(ClickableZone zone, Action onComplete = null)
    {
        StartCoroutine(HazardDamageRoutine(zone, onComplete));
    }

    private IEnumerator HazardDamageRoutine(ClickableZone zone, Action onComplete)
    {
        SpriteRenderer hazardSR = zone != null ? zone.hazardSpriteRenderer : null;

        // Hazard sprite yoksa sadece hasar sprite'ını göster
        if (hazardSR == null)
        {
            ShowDamage();
            yield return new WaitForSeconds(damageHoldDuration);
            onComplete?.Invoke();
            yield break;
        }

        // Animasyon tipine göre dallan
        switch (zone.hazardAnimationType)
        {
            case HazardAnimationType.FallFromAbove:
                yield return StartCoroutine(AnimFallFromAbove(hazardSR, zone));
                break;

            case HazardAnimationType.Topple:
                yield return StartCoroutine(AnimTopple(hazardSR, zone));
                break;

            case HazardAnimationType.Shatter:
                yield return StartCoroutine(AnimShatter(hazardSR, zone));
                break;

            case HazardAnimationType.Ignite:
                yield return StartCoroutine(AnimIgnite(hazardSR, zone));
                break;
        }

        // Tüm animasyon tiplerinde ortak: hasar sprite'ına geç
        ShowDamage();

        // Çarpma sarsılması
        yield return StartCoroutine(ImpactShake());

        // Hasar pozisyonunda bekle
        yield return new WaitForSeconds(damageHoldDuration);

        // Hazard sprite fade out
        yield return StartCoroutine(FadeOutSprite(hazardSR));

        // Callback
        onComplete?.Invoke();
    }

    // ───────────────── FallFromAbove: Avize, kaya düşmesi ─────────────────

    /// <summary>
    /// Obje yukarıdan karakterin üstüne düşer.
    /// Kullanım: Avize, kaya, raf vb.
    /// </summary>
    private IEnumerator AnimFallFromAbove(SpriteRenderer hazardSR, ClickableZone zone)
    {
        // Başlangıç ve bitiş pozisyonları
        Vector3 targetPos = transform.position + Vector3.up * 0.5f;
        Vector3 startPos = transform.position + Vector3.up * zone.hazardFallHeight;

        hazardSR.transform.position = startPos;
        hazardSR.transform.rotation = Quaternion.identity;
        hazardSR.gameObject.SetActive(true);

        // Sorting order — karakterin önünde
        hazardSR.sortingOrder = characterSpriteRenderer.sortingOrder + 1;

        // Düşerken dönme açısı (kaya için yavaşça dönme)
        float rotationAmount = zone.hazardFallRotation;

        // Düşüş animasyonu (ease in — yerçekimi hissi) + yavaş dönme
        float elapsed = 0f;
        while (elapsed < zone.hazardAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / zone.hazardAnimDuration);
            float eased = t * t; // Ease in quad
            hazardSR.transform.position = Vector3.Lerp(startPos, targetPos, eased);

            // Düşerken yavaşça dön (lineer — doğal görünüm)
            if (rotationAmount != 0f)
            {
                float currentRotation = Mathf.Lerp(0f, rotationAmount, t);
                hazardSR.transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
            }

            yield return null;
        }

        hazardSR.transform.position = targetPos;
        if (rotationAmount != 0f)
            hazardSR.transform.rotation = Quaternion.Euler(0f, 0f, rotationAmount);
    }

    // ───────────────── Topple: Buzdolabı/gardırop devrilmesi ─────────────────

    /// <summary>
    /// Obje yerinde pivot etrafında öne/yana doğru devrilir.
    /// Kullanım: Buzdolabı, gardırop, kitaplık vb.
    /// </summary>
    private IEnumerator AnimTopple(SpriteRenderer hazardSR, ClickableZone zone)
    {
        hazardSR.gameObject.SetActive(true);

        // Sorting order — karakterin önünde
        hazardSR.sortingOrder = characterSpriteRenderer.sortingOrder + 1;

        // Başlangıç rotasyonu kaydet
        Quaternion startRotation = hazardSR.transform.rotation;
        float targetAngle = zone.hazardToppleAngle * zone.hazardToppleDirection;
        Quaternion endRotation = startRotation * Quaternion.Euler(0f, 0f, targetAngle);

        // Devrilme animasyonu (ease in — yavaş başla, hızlı bitir)
        float elapsed = 0f;
        while (elapsed < zone.hazardAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / zone.hazardAnimDuration);
            float eased = t * t; // Ease in quad — yerçekimi
            hazardSR.transform.rotation = Quaternion.Slerp(startRotation, endRotation, eased);
            yield return null;
        }

        hazardSR.transform.rotation = endRotation;
    }

    // ───────────────── Shatter: Cam kırılması ─────────────────

    /// <summary>
    /// Obje yerinde sallanır, sonra parçalanır (scale bounce + alpha fade).
    /// Kullanım: Cam pencere, ayna, cam dolap vb.
    /// </summary>
    private IEnumerator AnimShatter(SpriteRenderer hazardSR, ClickableZone zone)
    {
        hazardSR.gameObject.SetActive(true);
        hazardSR.sortingOrder = characterSpriteRenderer.sortingOrder + 1;

        Vector3 originalPos = hazardSR.transform.position;
        Vector3 originalScale = hazardSR.transform.localScale;
        float duration = zone.hazardAnimDuration;

        // Faz 1: Hızlı sallama (sürenin %60'ı)
        float shakeDuration = duration * 0.6f;
        float shakeElapsed = 0f;
        float shakeMag = 0.1f;

        while (shakeElapsed < shakeDuration)
        {
            shakeElapsed += Time.deltaTime;
            float intensity = shakeElapsed / shakeDuration; // Artan yoğunluk
            float offsetX = Mathf.Sin(shakeElapsed * 60f) * shakeMag * intensity;
            hazardSR.transform.position = originalPos + new Vector3(offsetX, 0f, 0f);
            yield return null;
        }

        hazardSR.transform.position = originalPos;

        // Faz 2: Patlama efekti (sürenin %40'ı)
        // Scale büyüme + alpha azalma → kırılma hissi
        float burstDuration = duration * 0.4f;
        float burstElapsed = 0f;
        Color startColor = hazardSR.color;

        while (burstElapsed < burstDuration)
        {
            burstElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(burstElapsed / burstDuration);

            // Scale büyüme (1.0 → 1.3)
            float scaleMul = 1f + t * 0.3f;
            hazardSR.transform.localScale = originalScale * scaleMul;

            // Alpha azalma
            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0.2f, t);
            hazardSR.color = c;

            yield return null;
        }

        // Orijinal değerleri geri yükle (FadeOutSprite halleder son fade'i)
        hazardSR.transform.localScale = originalScale;
        Color resetColor = startColor;
        resetColor.a = 1f;
        hazardSR.color = resetColor;
    }

    // ───────────────── Ignite: Alev/ateş yükselişi ─────────────────

    /// <summary>
    /// Ateş/alev sprite'ı küçükten büyüyerek ve alpha artarak belirir.
    /// Kullanım: Ocak, gaz kaçağı vb.
    /// </summary>
    private IEnumerator AnimIgnite(SpriteRenderer hazardSR, ClickableZone zone)
    {
        hazardSR.gameObject.SetActive(true);
        hazardSR.sortingOrder = characterSpriteRenderer.sortingOrder + 1;

        Vector3 targetScale = hazardSR.transform.localScale;
        Vector3 startScale = targetScale * 0.1f; // Çok küçükten başla
        Color startColor = hazardSR.color;

        hazardSR.transform.localScale = startScale;
        Color c = startColor;
        c.a = 0f;
        hazardSR.color = c;

        float elapsed = 0f;
        while (elapsed < zone.hazardAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / zone.hazardAnimDuration);

            // Scale büyüme (ease out — hızlı büyü, yavaş dur)
            float eased = 1f - (1f - t) * (1f - t);
            hazardSR.transform.localScale = Vector3.Lerp(startScale, targetScale, eased);

            // Alpha artışı
            c = startColor;
            c.a = Mathf.Lerp(0f, 1f, eased);
            hazardSR.color = c;

            // Hafif titreşim (alev hissi)
            float flicker = 1f + Mathf.Sin(elapsed * 25f) * 0.05f;
            hazardSR.transform.localScale = Vector3.Lerp(startScale, targetScale, eased) * flicker;

            yield return null;
        }

        hazardSR.transform.localScale = targetScale;
        hazardSR.color = startColor;
    }

    // ════════════════════════════ ORTAK EFEKTLER ════════════════════════════

    /// <summary>
    /// Çarpma anında küçük bir sarsılma efekti (karakter scale).
    /// </summary>
    private IEnumerator ImpactShake()
    {
        Vector3 originalScale = transform.localScale;
        float shakeDuration = 0.15f;
        float shakeMag = 0.08f;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float offsetX = UnityEngine.Random.Range(-shakeMag, shakeMag);
            float offsetY = UnityEngine.Random.Range(-shakeMag, shakeMag);
            transform.localScale = originalScale + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    /// <summary>
    /// Herhangi bir SpriteRenderer'ı fade out yaparak gizler.
    /// </summary>
    private IEnumerator FadeOutSprite(SpriteRenderer sr)
    {
        if (sr == null) yield break;

        float duration = 0.3f;
        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            sr.color = c;
            yield return null;
        }

        sr.gameObject.SetActive(false);

        // Alpha'yı geri yükle (tekrar kullanım için)
        Color resetColor = startColor;
        resetColor.a = 1f;
        sr.color = resetColor;

        // Rotasyonu sıfırla (Topple sonrası)
        sr.transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Sprite değişimlerinde hafif scale bounce efekti.
    /// </summary>
    private IEnumerator SpriteBounce()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 punchScale = originalScale * 1.1f;
        float duration = 0.15f;

        // Büyü
        float elapsed = 0f;
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.5f);
            transform.localScale = Vector3.Lerp(originalScale, punchScale, t);
            yield return null;
        }

        // Küçül
        elapsed = 0f;
        while (elapsed < duration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.5f);
            transform.localScale = Vector3.Lerp(punchScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    // ════════════════════════════ YARDIMCI ════════════════════════════

    /// <summary>
    /// Karakteri odanın ortasına (başlangıç pozisyonuna) geri getirir.
    /// </summary>
    public void ReturnToCenter(Action onComplete = null)
    {
        MoveToPosition(roomCenterPosition, onComplete);
    }

    /// <summary>
    /// Karakterin hareket halinde olup olmadığını döndürür.
    /// </summary>
    public bool IsMoving => isMoving;
}
