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

    // Dahili
    private float timeRemaining;
    private bool timerActive = false;
    private bool roomCompleted = false;
    private bool processingClick = false;

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

        // Arka plan müziğini başlat
        if (bgmClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic(bgmClip);

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
    /// Timer bittiğinde çağrılır — hasar animasyonu ve sonraki oda.
    /// </summary>
    private void HandleTimerExpired()
    {
        if (roomCompleted) return;

        Debug.Log("[Level2Controller] Süre doldu! Hasar animasyonu başlatılıyor...");

        // Zone'ları kapat
        SetAllZonesInteractable(false);

        // Kamera sallamayı durdur
        if (cameraShake != null)
            cameraShake.StopShake();

        // Karakter ortada — zone olmadan hasar animasyonu
        if (characterAnimator != null)
        {
            characterAnimator.PlayHazardDamage(null, () =>
            {
                // -50 puan
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.AddIncorrect();

                // Geri bildirim göster
                if (feedbackUI != null)
                    feedbackUI.ShowWrong("Süre doldu!");

                // Yanlış SFX
                PlaySFX(incorrectSFX);

                // Oda tamamla
                CompleteRoom();
            });
        }
        else
        {
            // CharacterAnimator yoksa direkt puan ver ve geç
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddIncorrect();

            CompleteRoom();
        }
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

        // Kamera sallamayı durdur
        if (cameraShake != null)
            cameraShake.StopShake();

        // Karakter seçilen bölgeye hareket et
        if (characterAnimator != null)
        {
            characterAnimator.MoveToPosition(zone.GetCharacterPosition(), () =>
            {
                // Korunma pozisyonuna geç
                characterAnimator.ShowProtect();

                // +100 puan
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.AddCorrect();

                // Doğru SFX
                PlaySFX(correctSFX);

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

        // Karakter seçilen bölgeye hareket et
        if (characterAnimator != null)
        {
            characterAnimator.MoveToPosition(zone.GetCharacterPosition(), () =>
            {
                // Zone-spesifik tehlike animasyonu
                characterAnimator.PlayHazardDamage(zone, () =>
                {
                    // -50 puan
                    if (ScoreManager.Instance != null)
                        ScoreManager.Instance.AddIncorrect();

                    // Yanlış SFX
                    PlaySFX(incorrectSFX);

                    // Geri bildirim
                    if (feedbackUI != null)
                        feedbackUI.ShowWrong(zone.feedbackText);

                    // Bu zone'u devre dışı bırak
                    zone.Disable();

                    // Karakter idle'a dön
                    characterAnimator.ShowIdle();

                    // Karakteri odanın ortasına geri getir
                    characterAnimator.ReturnToCenter(() =>
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
}
