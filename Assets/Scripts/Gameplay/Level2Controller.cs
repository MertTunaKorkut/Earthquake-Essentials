using UnityEngine;
using System.Collections;

/// <summary>
/// Level 2'nin tek bir odasını orkestre eden ana controller.
/// Countdown → Timer → Zone tıklama → Karakter hareket → Animasyon → Oda geçişi
/// akışını yönetir. Her Level2 sahnesine bir tane eklenir.
/// </summary>
public class Level2Controller : MonoBehaviour
{
    [Header("Oda Verisi")]
    [Tooltip("Bu odanın ScriptableObject verisi")]
    public RoomData roomData;

    [Header("Zone Referansları")]
    [Tooltip("Sahnedeki tüm tıklanabilir bölgeler")]
    public ClickableZone[] zones;

    [Header("Karakter")]
    [Tooltip("Karakter animasyon controller'ı")]
    public CharacterAnimator characterAnimator;

    [Header("Kamera")]
    [Tooltip("Ana kameradaki CameraShake component'ı")]
    public CameraShake cameraShake;

    [Header("UI Referansları")]
    [Tooltip("HUD controller (timer + skor)")]
    public HUDController hudController;

    [Tooltip("Geri bildirim UI (doğru/yanlış mesajları)")]
    public FeedbackUI feedbackUI;

    [Tooltip("Geri sayım UI (3-2-1)")]
    public CountdownUI countdownUI;

    [Header("Ses Dosyaları")]
    [Tooltip("Arka plan müziği")]
    public AudioClip bgmClip;

    [Tooltip("Doğru seçim ses efekti")]
    public AudioClip correctSFX;

    [Tooltip("Yanlış seçim / hasar ses efekti")]
    public AudioClip incorrectSFX;

    [Header("Geçiş Ayarları")]
    [Tooltip("Oda tamamlandıktan sonra sonraki odaya geçiş bekleme süresi (saniye)")]
    public float roomCompleteDelay = 2f;

    [Header("Kaya (Süre Dolduğunda)")]
    [Tooltip("Süre dolduğunda düşen kaya sprite rendererı")]
    public SpriteRenderer rockSpriteRenderer;

    [Tooltip("Kayanın düşme yüksekliği")]
    public float rockFallHeight = 5f;

    [Tooltip("Kayanın düşme süresi (saniye)")]
    public float rockFallDuration = 0.5f;

    [Tooltip("Kayanın düşerken dönme açısı (derece)")]
    public float rockFallRotation = 180f;

    [Header("Tehlikeli obje ayarları")]
    [Tooltip("Tehlikeli objenin oyun sonunda yok olma süresi (saniye)")]
    public float hazardDestroyDelay = 0.7f;

    [Header("Oda Geneli Sesler")]
    public AudioClip earthquakeRumbleSFX;
    public AudioClip rockCrashSFX;
    public AudioClip timerTickSFX;

    // Dahili
    private float timeRemaining;
    private bool timerActive = false;
    private bool roomCompleted = false;
    private bool processingClick = false;
    private float nextTickTime;

    private void Start()
    {
        // Başlangıç kontrolleri
        if (roomData == null)
        {
            Debug.LogError("[Level2Controller] RoomData atanmamış!");
            return;
        }

        if (countdownUI == null)
        {
            Debug.LogError("[Level2Controller] CountdownUI atanmamış!");
            return;
        }

        // Timer'ı ayarla
        timeRemaining = roomData.timeLimit;

        // Tüm zone'ları devre dışı bırak (countdown sırasında tıklanamaz)
        SetAllZonesInteractable(false);

        // Zone event'lerini dinle
        RegisterZoneEvents();

        // Countdown event'ini dinle
        countdownUI.OnCountdownFinished += HandleCountdownFinished;

        // HUD'ı başlat
        if (hudController != null)
            hudController.InitializeTimer(roomData.timeLimit);

        // Karakteri idle pozisyonuna getir
        if (characterAnimator != null)
            characterAnimator.ShowIdle();

        // Countdown başlat
        countdownUI.StartCountdown(roomData.countdownDuration);

        Debug.Log($"[Level2Controller] {roomData.roomName} odası başladı! Countdown...");
    }

    private void Update()
    {
        if (!timerActive || roomCompleted) return;

        // Timer geri sayımı
        timeRemaining -= Time.deltaTime;

        // HUD güncelle
        if (hudController != null)
            hudController.UpdateTimer(timeRemaining);

        // Timer bitti mi?
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            timerActive = false;
            HandleTimerExpired();
        }
        // Timer 5 saniyenin altındaysa ve 1 saniye geçtiyse
        if (timeRemaining <= 5f && timeRemaining > 0f)
        {
            if (Time.time >= nextTickTime)
            {
                if (timerTickSFX != null) PlaySFX(timerTickSFX);
                nextTickTime = Time.time + 1f; // Bir sonraki tik 1 saniye sonra
            }
        }
    }

    private void OnDestroy()
    {
        // Event dinleyicilerini temizle
        UnregisterZoneEvents();

        if (countdownUI != null)
            countdownUI.OnCountdownFinished -= HandleCountdownFinished;
    }

    // ─────────────────────────────── EVENT HANDLERS ───────────────────────────────

    /// <summary>
    /// Countdown tamamlandığında çağrılır — gameplay başlar.
    /// </summary>
    private void HandleCountdownFinished()
    {
        Debug.Log($"[Level2Controller] Countdown bitti! Timer başlıyor: {roomData.timeLimit} saniye.");

        // Timer başlat
        timerActive = true;

        // Kamera sallama başlat
        if (cameraShake != null)
            cameraShake.StartShake();
        
        //Deprem sesini başlat
        if (earthquakeRumbleSFX != null && AudioManager.Instance != null) 
            AudioManager.Instance.PlayEnvironment(earthquakeRumbleSFX);
            
        // Arka plan müziğini başlat
        if (bgmClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(bgmClip);
            
        // Zone'ları tıklanabilir yap
        SetAllZonesInteractable(true);
    }

    /// <summary>
    /// Bir zone tıklandığında çağrılır (event callback).
    /// </summary>
    private void HandleZoneClicked(ClickableZone zone)
    {
        if (roomCompleted || processingClick) return;

        processingClick = true;

        // Tüm zone'ları geçici olarak kilitle (çift tıklama engeli)
        SetAllZonesInteractable(false);

        if (zone.isSafe)
        {
            HandleSafeZone(zone);
        }
        else
        {
            HandleHazardZone(zone);
        }
    }

    /// <summary>
    /// Timer bittiğinde çağrılır — kaya düşme animasyonu ve sonraki oda.
    /// 30 saniye boyunca doğru seçim yapılmazsa kaya karakterin üstüne düşer.
    /// </summary>
    private void HandleTimerExpired()
    {
        if (roomCompleted) return;

        Debug.Log("[Level2Controller] Süre doldu! Kaya düşme animasyonu başlatılıyor...");

        roomCompleted = true;

        // Zone'ları kapat
        SetAllZonesInteractable(false);

        // Kamera sallamayı durdur
        if (cameraShake != null)
            cameraShake.StopShake();

        // Kaya düşme animasyonu
        if (characterAnimator != null && rockSpriteRenderer != null)
        {
            StartCoroutine(RockFallOnCharacterRoutine());
        }
        else if (characterAnimator != null)
        {
            // rockSpriteRenderer yoksa sadece hasar sprite'ı göster
            characterAnimator.PlayHazardDamage(null, () =>
            {
                ApplyTimerExpiredPenalty();
            });
        }
        else
        {
            // CharacterAnimator yoksa direkt puan ver ve geç
            ApplyTimerExpiredPenalty();
        }
    }

    /// <summary>
    /// Süre dolduğunda kaya düşme animasyonu.
    /// Kaya yukarıdan yavaşça dönerek karakterin üstüne düşer.
    /// </summary>
    private IEnumerator RockFallOnCharacterRoutine()
    {
        // Pozisyonları ayarla
        Vector3 characterPos = characterAnimator.transform.position;
        Vector3 targetPos = characterPos + Vector3.up * 0.5f;
        Vector3 startPos = characterPos + Vector3.up * rockFallHeight;

        rockSpriteRenderer.transform.position = startPos;
        rockSpriteRenderer.transform.rotation = Quaternion.identity;
        rockSpriteRenderer.gameObject.SetActive(true);

        // Sorting order — karakterin önünde
        SpriteRenderer charSR = characterAnimator.GetComponent<SpriteRenderer>();
        if (charSR != null)
            rockSpriteRenderer.sortingOrder = charSR.sortingOrder + 1;

        // Düşüş animasyonu (ease in — yerçekimi hissi) + yavaş dönme
        float elapsed = 0f;
        while (elapsed < rockFallDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rockFallDuration);
            float eased = t * t; // Ease in quad

            // Pozisyon
            rockSpriteRenderer.transform.position = Vector3.Lerp(startPos, targetPos, eased);

            // Yavaşça dönme
            if (rockFallRotation != 0f)
            {
                float currentRotation = Mathf.Lerp(0f, rockFallRotation, t);
                rockSpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
            }

            yield return null;
        }

        rockSpriteRenderer.transform.position = targetPos;
        if (rockFallRotation != 0f)
            rockSpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, rockFallRotation);

        // Hasar sprite'ına geç
        characterAnimator.ShowDamage();

        // Yanlış SFX
        if (incorrectSFX != null) PlaySFX(incorrectSFX);

        if (rockCrashSFX != null) PlaySFX(rockCrashSFX);

        // Kısa bekleme
        yield return new WaitForSeconds(hazardDestroyDelay);

        // Kaya fade out
        yield return StartCoroutine(FadeOutSpriteRoutine(rockSpriteRenderer));

        // Puan ve geri bildirim
        ApplyTimerExpiredPenalty();
    }

    /// <summary>
    /// Süre dolduğunda uygulanan ceza: -50 puan, geri bildirim, oda tamamla.
    /// </summary>
    private void ApplyTimerExpiredPenalty()
    {
        // -50 puan
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddIncorrect();

        // Geri bildirim göster
        if (feedbackUI != null)
            feedbackUI.ShowWrong("Süre doldu!");

        // Oda tamamla
        CompleteRoom();
    }

    /// <summary>
    /// SpriteRenderer'ı fade out yapar.
    /// </summary>
    private IEnumerator FadeOutSpriteRoutine(SpriteRenderer sr)
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

        // Alpha'yı geri yükle
        Color resetColor = startColor;
        resetColor.a = 1f;
        sr.color = resetColor;

        // Rotasyonu sıfırla
        sr.transform.rotation = Quaternion.identity;
    }

    // ─────────────────────────────── ZONE İŞLEMLERİ ───────────────────────────────

    /// <summary>
    /// Güvenli bölge seçildiğinde:
    /// Karakter → Hareket → Korunma pozu → +100 → SFX → Yeşil flash → Oda biter.
    /// </summary>
    private void HandleSafeZone(ClickableZone zone)
    {
        Debug.Log($"[Level2Controller] Güvenli bölge seçildi: '{zone.zoneName}'");

        roomCompleted = true;
        timerActive = false;

        // Kamera sallama yı durdur
        if (cameraShake != null)
            cameraShake.StopShake();
        
        // Doğru SFX
        if (correctSFX != null) PlaySFX(correctSFX);

        // Karakter fade-out → teleport → korunma sprite → fade-in
        if (characterAnimator != null)
        {
            characterAnimator.FadeToPosition(zone.GetCharacterPosition(), () =>
            {
                // Korunma pozisyonuna geç (fade-in ile birlikte görünecek)
                characterAnimator.ShowProtect();

                // +100 puan
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.AddCorrect();

                // Geri bildirim
                if (feedbackUI != null)
                    feedbackUI.ShowCorrect(zone.feedbackText);

                // Yeşil flash
                zone.FlashGreen(() =>
                {
                    // Flash bittikten sonra oda tamamla
                    CompleteRoom();
                });
            });
        }
        else
        {
            // Animator yoksa direkt puan ver ve geç
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddCorrect();

            CompleteRoom();
        }
    }

    /// <summary>
    /// Tehlikeli bölge seçildiğinde:
    /// Karakter → Hareket → Kaya düşme → Hasar → -50 → Zone kapanır → Devam.
    /// </summary>
    private void HandleHazardZone(ClickableZone zone)
    {
        Debug.Log($"[Level2Controller] Tehlikeli bölge seçildi: '{zone.zoneName}'");

        // Yanlış SFX
        if (incorrectSFX != null) PlaySFX(incorrectSFX);

        // Karakter fade ile seçilen bölgeye git
        if (characterAnimator != null)
        {
            characterAnimator.FadeToPosition(zone.GetCharacterPosition(), () =>
            {   
                if(zone.hazardSFX != null)
                    PlaySFX(zone.hazardSFX);
                    
                // Zone-spesifik tehlike animasyonu
                characterAnimator.PlayHazardDamage(zone, () =>
                {
                    // -50 puan
                    if (ScoreManager.Instance != null)
                        ScoreManager.Instance.AddIncorrect();

                    // Geri bildirim
                    if (feedbackUI != null)
                        feedbackUI.ShowWrong(zone.feedbackText);

                    // Bu zone'u devre dışı bırak
                    zone.Disable();

                    // Karakter idle'a dön
                    characterAnimator.ShowIdle();

                    // Karakteri odanın ortasına fade ile geri getir
                    characterAnimator.FadeToCenter(() =>
                    {
                        // Hâlâ aktif zone var mı kontrol et
                        if (HasActiveZones() && !roomCompleted)
                        {
                            // Zone'ları tekrar aç
                            SetAllZonesInteractable(true);
                            processingClick = false;
                        }
                        else
                        {
                            // Tüm zone'lar tükendi — timer bitişi gibi davran
                            Debug.Log("[Level2Controller] Tüm zone'lar tükendi!");
                            HandleTimerExpired();
                        }
                    });
                });
            });
        }
        else
        {
            // Animator yoksa direkt
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddIncorrect();

            zone.Disable();
            processingClick = false;

            if (HasActiveZones())
                SetAllZonesInteractable(true);
            else
                CompleteRoom();
        }
    }

    // ─────────────────────────────── YARDIMCI METOTLAR ───────────────────────────────

    /// <summary>
    /// Odayı tamamlar ve sonraki odaya geçişi başlatır.
    /// </summary>
    private void CompleteRoom()
    {
        roomCompleted = true;
        timerActive = false;

        // Müziği durdur
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.StopEnvironment();

        StartCoroutine(CompleteRoomRoutine());
    }

    private IEnumerator CompleteRoomRoutine()
    {
        yield return new WaitForSeconds(roomCompleteDelay);

        // Sonraki odaya geç
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToNextRoom();
        }
        else
        {
            Debug.LogWarning("[Level2Controller] GameManager bulunamadı, sahne geçişi yapılamıyor.");
        }
    }

    /// <summary>
    /// Tüm zone'ların tıklanabilirliğini toplu olarak ayarlar.
    /// Disabled zone'lar etkilenmez.
    /// </summary>
    private void SetAllZonesInteractable(bool interactable)
    {
        if (zones == null) return;

        foreach (var zone in zones)
        {
            if (zone != null && zone.IsActive)
                zone.SetInteractable(interactable);
        }
    }

    /// <summary>
    /// Hâlâ aktif (devre dışı bırakılmamış) zone olup olmadığını kontrol eder.
    /// </summary>
    private bool HasActiveZones()
    {
        if (zones == null) return false;

        foreach (var zone in zones)
        {
            if (zone != null && zone.IsActive)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Zone event'lerine abone ol.
    /// </summary>
    private void RegisterZoneEvents()
    {
        if (zones == null) return;

        foreach (var zone in zones)
        {
            if (zone != null)
                zone.OnZoneClicked += HandleZoneClicked;
        }
    }

    /// <summary>
    /// Zone event aboneliklerini temizle.
    /// </summary>
    private void UnregisterZoneEvents()
    {
        if (zones == null) return;

        foreach (var zone in zones)
        {
            if (zone != null)
                zone.OnZoneClicked -= HandleZoneClicked;
        }
    }

    /// <summary>
    /// Ses efekti çal (AudioManager üzerinden).
    /// </summary>
    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clip);
    }

    /// <summary>
    /// Tehlike sesini tam animasyonun çarpma anında (belirli bir süre sonra) çalar.
    /// </summary>
    private IEnumerator PlayHazardSFXWithDelay(AudioClip clip, float delay)
    {
        if (clip == null || AudioManager.Instance == null) yield break;
        
        // Objenin havada süzülme süresi kadar bekle
        yield return new WaitForSeconds(delay);
        
        // Tam karakterin kafasına değdiği salisede sesi patlat!
        AudioManager.Instance.PlaySFX(clip);
    }
}
