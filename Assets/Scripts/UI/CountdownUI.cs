using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// Oda başı geri sayım ekranı — ekranın ortasında "3… 2… 1… BAŞLA!" gösterir.
/// Canvas altında olmalı. Level2Controller tarafından başlatılır.
/// </summary>
public class CountdownUI : MonoBehaviour
{
    [Header("UI Referansları")]
    [Tooltip("Geri sayım metnini gösteren TextMeshPro (ekranın ortasında, büyük font)")]
    public TextMeshProUGUI countdownText;
    public Image countdownBlur;

    [Header("Animasyon Ayarları")]
    [Tooltip("Her sayı arası bekleme süresi (saniye)")]
    public float countdownInterval = 1f;

    [Tooltip("'BAŞLA!' yazısının ekranda kalma süresi (saniye)")]
    public float startTextDuration = 0.6f;

    [Tooltip("Sayı değişiminde maksimum scale çarpanı")]
    public float punchScaleMax = 1.5f;

    [Header("Sesler")]
    public AudioClip beepSFX;
    public AudioClip goSFX;

    /// <summary>
    /// Geri sayım tamamlandığında tetiklenir.
    /// Level2Controller bu event'i dinleyerek timer'ı başlatır.
    /// </summary>
    public event Action OnCountdownFinished;

    private void Awake()
    {
        // Başlangıçta gizle
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Geri sayımı başlatır (3'ten geriye).
    /// </summary>
    public void StartCountdown(int from = 3)
    {
        if (countdownBlur != null)
            countdownBlur.gameObject.SetActive(true);
        StartCoroutine(CountdownRoutine(from));
    }

    private IEnumerator CountdownRoutine(int from)
    {
        if (countdownText == null) yield break;

        countdownText.gameObject.SetActive(true);

        // Sayıları göster: 3, 2, 1
        for (int i = from; i >= 1; i--)
        {
            countdownText.text = i.ToString();

            if (beepSFX != null) AudioManager.Instance.PlaySFX(beepSFX);

            yield return StartCoroutine(PunchScaleText());
            yield return new WaitForSeconds(countdownInterval * 0.5f); // Punch + bekleme
        }

        // "BAŞLA!" göster
        countdownText.text = "BAŞLA!";

        if (goSFX != null) AudioManager.Instance.PlaySFX(goSFX);

        yield return StartCoroutine(PunchScaleText());
        yield return new WaitForSeconds(startTextDuration * 0.5f);

        // Fade out
        yield return StartCoroutine(FadeOutText());

        countdownText.gameObject.SetActive(false);
        if (countdownBlur != null) 
            countdownBlur.gameObject.SetActive(false);

        // Event tetikle
        OnCountdownFinished?.Invoke();
    }

    /// <summary>
    /// Sayıda scale punch animasyonu — büyüyüp küçülme.
    /// </summary>
    private IEnumerator PunchScaleText()
    {
        if (countdownText == null) yield break;

        // DÜZELTME: CanvasGroup'u yazının kendisine DEĞİL, bu scriptin bağlı olduğu ana objeye (Panel'e) ekliyoruz!
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        RectTransform rect = countdownText.rectTransform;
        Vector3 originalScale = Vector3.one;
        Vector3 punchScale = Vector3.one * punchScaleMax;
        float duration = countdownInterval * 0.5f;

        // Hem yazıyı hem de mavi arka planı tamamen görünür yap
        cg.alpha = 1f;

        // Scale başlangıcı (Sadece yazıyı büyütüyoruz, arka plan sabit kalıyor)
        rect.localScale = punchScale;

        // Küçüle küçüle orijinal boyuta gel
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease out elastic hissi veren eğri
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            rect.localScale = Vector3.Lerp(punchScale, originalScale, eased);

            yield return null;
        }

        rect.localScale = originalScale;
    }

    /// <summary>
    /// Metnin hızla kaybolma animasyonu.
    /// </summary>
    private IEnumerator FadeOutText()
    {
        if (countdownText == null) yield break;

        // DÜZELTME: CanvasGroup'u ana objeden (Panel'den) alıyoruz
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = countdownText.rectTransform.localScale;
        Vector3 endScale = startScale * 1.3f; // Sadece yazı büyüyerek kaybolsun

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // TÜM PANELİ (Mavi arka plan + Yazı) aynı anda şeffaflaştır
            cg.alpha = Mathf.Lerp(1f, 0f, t);

            // Sadece YAZIYI büyüt
            countdownText.rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        // Reset (Bir sonraki oyun başladığında kutu yine görünür olsun diye)
        //cg.alpha = 1f;
        countdownText.rectTransform.localScale = Vector3.one;
    }
}
