using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Level 1 HUD UI elemanlarını sahneye oluşturan Editor aracı.
/// Menu: Earthquake Essentials > Setup Level1 HUD
/// </summary>
public class Level1HUDSetup : EditorWindow
{
    [MenuItem("Earthquake Essentials/Setup Level1 HUD")]
    public static void SetupHUD()
    {
        // ============================================
        // 1) CANVAS OLUŞTUR
        // ============================================
        GameObject canvasGO = new GameObject("HUD_Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ============================================
        // 2) FONT ASSET YÜKLE (Oswald-Bold)
        // ============================================
        // Unity 6'da TMP, .ttf dosyalarını doğrudan kullanabilir
        // Ancak SDF font asset yoksa, LiberationSans SDF ile devam edelim
        // ve kullanıcı isterse Font Asset Creator ile değiştirebilir.
        TMP_FontAsset fontAsset = null;

        // Projedeki tüm TMP font asset'lerini ara (önce Fonts, sonra tüm Assets)
        string[] searchPaths = new[] { "Assets/Fonts", "Assets" };
        foreach (string searchPath in searchPaths)
        {
            string[] fontGuids = AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { searchPath });
            if (fontGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(fontGuids[0]);
                fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (fontAsset != null) break;
            }
        }

        // Hâlâ yoksa Resources içinden herhangi bir TMP font asset bul
        if (fontAsset == null)
        {
            TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            if (allFonts.Length > 0)
                fontAsset = allFonts[0];
        }

        if (fontAsset == null)
        {
            Debug.LogWarning("[HUDSetup] TMP Font Asset bulunamadı! Varsayılan font kullanılacak. Font Asset Creator ile bir SDF font oluşturun.");
        }

        // ============================================
        // 3) SOL ÜST: PUAN SAYACI
        // ============================================
        // Arka plan paneli
        GameObject scorePanelGO = CreatePanel(canvasGO.transform, "ScorePanel",
            new Vector2(0, 1), new Vector2(0, 1), // sol üst anchor
            new Vector2(20, -20), new Vector2(260, 70),
            new Color(0.15f, 0.15f, 0.2f, 0.75f));

        // Yıldız ikonu (emoji ile simüle ediyoruz — ileride sprite eklenebilir)
        GameObject scoreIconGO = CreateTextElement(scorePanelGO.transform, "ScoreIcon",
            "", fontAsset, 28,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(15, 0), new Vector2(40, 40),
            new Color(1f, 0.84f, 0f, 1f), TextAlignmentOptions.Center);

        // Puan metni
        GameObject scoreTextGO = CreateTextElement(scorePanelGO.transform, "ScoreText",
            "Puan: 0", fontAsset, 28,
            new Vector2(0, 0), new Vector2(1, 1),
            new Vector2(50, 0), new Vector2(-10, 0),
            Color.white, TextAlignmentOptions.Left);
        // scoreText'in stretch olması için
        RectTransform scoreTextRT = scoreTextGO.GetComponent<RectTransform>();
        scoreTextRT.anchorMin = new Vector2(0, 0);
        scoreTextRT.anchorMax = new Vector2(1, 1);
        scoreTextRT.offsetMin = new Vector2(50, 5);
        scoreTextRT.offsetMax = new Vector2(-10, -5);

        // ============================================
        // 4) SAĞ ÜST: İLERLEME GÖSTER (0/5)
        // ============================================
        GameObject progressPanelGO = CreatePanel(canvasGO.transform, "ProgressPanel",
            new Vector2(1, 1), new Vector2(1, 1), // sağ üst anchor
            new Vector2(-20, -20), new Vector2(220, 70),
            new Color(0.1f, 0.35f, 0.15f, 0.75f));
        // Pivot sağ üst
        RectTransform progressPanelRT = progressPanelGO.GetComponent<RectTransform>();
        progressPanelRT.pivot = new Vector2(1, 1);

        // Çanta ikonu
        GameObject progressIconGO = CreateTextElement(progressPanelGO.transform, "ProgressIcon",
            "", fontAsset, 28,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(15, 0), new Vector2(40, 40),
            Color.white, TextAlignmentOptions.Center);

        // İlerleme metni
        GameObject progressTextGO = CreateTextElement(progressPanelGO.transform, "ProgressText",
            "0 / 5", fontAsset, 30,
            new Vector2(0, 0), new Vector2(1, 1),
            new Vector2(50, 5), new Vector2(-10, -5),
            Color.white, TextAlignmentOptions.Center);
        RectTransform progressTextRT = progressTextGO.GetComponent<RectTransform>();
        progressTextRT.anchorMin = new Vector2(0, 0);
        progressTextRT.anchorMax = new Vector2(1, 1);
        progressTextRT.offsetMin = new Vector2(50, 5);
        progressTextRT.offsetMax = new Vector2(-10, -5);

        // ============================================
        // 5) MERKEZ ALT: GERİ BİLDİRİM PANELLERİ
        // ============================================
        // Doğru panel (yeşil)
        GameObject correctPanelGO = CreatePanel(canvasGO.transform, "CorrectFeedbackPanel",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), // alt merkez
            new Vector2(0, 120), new Vector2(400, 60),
            new Color(0.15f, 0.7f, 0.25f, 0.85f));
        correctPanelGO.SetActive(false);

        GameObject correctTextGO = CreateTextElement(correctPanelGO.transform, "CorrectText",
            "✓ Doğru!", fontAsset, 26,
            new Vector2(0, 0), new Vector2(1, 1),
            new Vector2(10, 5), new Vector2(-10, -5),
            Color.white, TextAlignmentOptions.Center);
        RectTransform correctTextRT = correctTextGO.GetComponent<RectTransform>();
        correctTextRT.anchorMin = Vector2.zero;
        correctTextRT.anchorMax = Vector2.one;
        correctTextRT.offsetMin = new Vector2(10, 5);
        correctTextRT.offsetMax = new Vector2(-10, -5);

        // Yanlış panel (kırmızı)
        GameObject wrongPanelGO = CreatePanel(canvasGO.transform, "WrongFeedbackPanel",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 120), new Vector2(400, 60),
            new Color(0.8f, 0.15f, 0.15f, 0.85f));
        wrongPanelGO.SetActive(false);

        GameObject wrongTextGO = CreateTextElement(wrongPanelGO.transform, "WrongText",
            "✗ Yanlış!", fontAsset, 26,
            new Vector2(0, 0), new Vector2(1, 1),
            new Vector2(10, 5), new Vector2(-10, -5),
            Color.white, TextAlignmentOptions.Center);
        RectTransform wrongTextRT = wrongTextGO.GetComponent<RectTransform>();
        wrongTextRT.anchorMin = Vector2.zero;
        wrongTextRT.anchorMax = Vector2.one;
        wrongTextRT.offsetMin = new Vector2(10, 5);
        wrongTextRT.offsetMax = new Vector2(-10, -5);

        // Level tamamlandı paneli
        GameObject completePanelGO = CreatePanel(canvasGO.transform, "LevelCompletePanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 0), new Vector2(500, 120),
            new Color(0.2f, 0.5f, 0.8f, 0.9f));
        completePanelGO.SetActive(false);

        GameObject completeTextGO = CreateTextElement(completePanelGO.transform, "CompleteText",
            "Tebrikler! Level Tamamlandı!", fontAsset, 30,
            new Vector2(0, 0), new Vector2(1, 1),
            new Vector2(10, 5), new Vector2(-10, -5),
            Color.white, TextAlignmentOptions.Center);
        RectTransform completeTextRT = completeTextGO.GetComponent<RectTransform>();
        completeTextRT.anchorMin = Vector2.zero;
        completeTextRT.anchorMax = Vector2.one;
        completeTextRT.offsetMin = new Vector2(10, 5);
        completeTextRT.offsetMax = new Vector2(-10, -5);

        // ============================================
        // 6) HUDController EKLE VE BAĞLA
        // ============================================
        HUDController hud = canvasGO.AddComponent<HUDController>();
        hud.scoreText = scoreTextGO.GetComponent<TextMeshProUGUI>();
        hud.progressText = progressTextGO.GetComponent<TextMeshProUGUI>();

        // ============================================
        // 7) FeedbackUI EKLE VE BAĞLA
        // ============================================
        FeedbackUI feedback = canvasGO.AddComponent<FeedbackUI>();
        feedback.correctPanel = correctPanelGO;
        feedback.wrongPanel = wrongPanelGO;
        feedback.levelCompletePanel = completePanelGO;
        feedback.correctItemText = correctTextGO.GetComponent<TextMeshProUGUI>();
        feedback.wrongItemText = wrongTextGO.GetComponent<TextMeshProUGUI>();

        // ============================================
        // 8) Level1Controller'a REFERANSLARI BAĞLA
        // ============================================
        Level1Controller controller = Object.FindAnyObjectByType<Level1Controller>();
        if (controller != null)
        {
            controller.hudController = hud;
            controller.feedbackUI = feedback;
            EditorUtility.SetDirty(controller);
            Debug.Log("[HUDSetup] Level1Controller referansları bağlandı!");
        }
        else
        {
            Debug.LogWarning("[HUDSetup] Level1Controller bulunamadı! Referansları manuel bağlayın.");
        }

        // ============================================
        // 9) PANEL KÖŞELERİNİ YUVARLAKLAŞTIR (rounded corner efekti)
        // ============================================
        // Empty sprite'ı arka plan olarak kullanmaya çalış
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Empty512x512.png");
        if (bgSprite != null)
        {
            SetPanelSprite(scorePanelGO, bgSprite);
            SetPanelSprite(progressPanelGO, bgSprite);
            SetPanelSprite(correctPanelGO, bgSprite);
            SetPanelSprite(wrongPanelGO, bgSprite);
            SetPanelSprite(completePanelGO, bgSprite);
        }

        // Sahneyi dirty olarak işaretle
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Selection.activeGameObject = canvasGO;
        Debug.Log("[HUDSetup] ✅ Level 1 HUD başarıyla oluşturuldu!");

        EditorUtility.DisplayDialog("HUD Kurulumu Tamamlandı",
            "Level 1 HUD başarıyla oluşturuldu!\n\n" +
            "• Sol üst: Puan sayacı\n" +
            "• Sağ üst: İlerleme göstergesi (0/5)\n" +
            "• Alt merkez: Doğru/Yanlış geri bildirimleri\n" +
            "• Merkez: Level tamamlandı paneli\n\n" +
            "Font değiştirmek için:\n" +
            "Window > TextMeshPro > Font Asset Creator\n" +
            "ile Oswald-Bold.ttf'den SDF font oluşturun.",
            "Tamam");
    }

    /// <summary>
    /// Yarı saydam arka planlı panel oluşturur.
    /// </summary>
    private static GameObject CreatePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 sizeDelta,
        Color bgColor)
    {
        GameObject panelGO = new GameObject(name);
        panelGO.transform.SetParent(parent, false);

        RectTransform rt = panelGO.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin; // Sol üst için (0,1), sağ üst için (1,1) gibi
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Image img = panelGO.AddComponent<Image>();
        img.color = bgColor;
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 1f;

        // Outline ekle (isteğe bağlı, şık bir çerçeve)
        Outline outline = panelGO.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        outline.effectDistance = new Vector2(1, -1);

        return panelGO;
    }

    /// <summary>
    /// TextMeshPro UI text elemanı oluşturur.
    /// </summary>
    private static GameObject CreateTextElement(Transform parent, string name,
        string text, TMP_FontAsset font, float fontSize,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax,
        Color color, TextAlignmentOptions alignment)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);

        RectTransform rt = textGO.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = (offsetMin + offsetMax) / 2f;
        rt.sizeDelta = offsetMax;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableAutoSizing = false;
        tmp.overflowMode = TextOverflowModes.Overflow;

        if (font != null)
            tmp.font = font;

        // Gölge efekti
        //tmp.fontMaterial.EnableKeyword("UNDERLAY_ON");

        return textGO;
    }

    /// <summary>
    /// Panel'e sprite ata (yuvarlak köşe efekti için).
    /// </summary>
    private static void SetPanelSprite(GameObject panel, Sprite sprite)
    {
        Image img = panel.GetComponent<Image>();
        if (img != null && sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
        }
    }
}
