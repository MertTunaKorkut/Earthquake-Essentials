using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Level sonu ekranını (blurlu arka plan + puan kutusu + sonraki seviye butonu)
/// sahneye oluşturan Editor aracı.
/// Menu: Earthquake Essentials > Setup Level Complete UI
/// </summary>
public class LevelCompleteUISetup : EditorWindow
{
    [MenuItem("Earthquake Essentials/Setup Level Complete UI")]
    public static void SetupLevelCompleteUI()
    {
        // ============================================
        // 1) FONT ASSET YÜKLE
        // ============================================
        TMP_FontAsset fontAsset = FindFontAsset();

        // ============================================
        // 2) CANVAS OLUŞTUR (HUD'ın üstünde, sortingOrder = 20)
        // ============================================
        GameObject canvasGO = new GameObject("LevelComplete_Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20; // HUD (10) üzerinde

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ============================================
        // 3) ROOT PANEL (LevelCompleteScreen)
        //    — tüm ekranı kaplar, başlangıçta kapalı
        // ============================================
        GameObject rootGO = new GameObject("LevelCompleteScreen");
        rootGO.transform.SetParent(canvasGO.transform, false);
        RectTransform rootRT = rootGO.AddComponent<RectTransform>();
        StretchFull(rootRT);

        // ============================================
        // 4) BLUR OVERLAY — yarı saydam koyu arka plan
        //    (gerçek blur shader olmadan, koyu overlay ile blur hissi)
        // ============================================
        GameObject overlayGO = new GameObject("BlurOverlay");
        overlayGO.transform.SetParent(rootGO.transform, false);
        RectTransform overlayRT = overlayGO.AddComponent<RectTransform>();
        StretchFull(overlayRT);

        Image overlayImg = overlayGO.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.7f); // %70 opak siyah
        overlayImg.raycastTarget = true; // arkadaki tıklamaları engeller

        // ============================================
        // 5) CONTENT PANEL — ortadaki kutu
        // ============================================
        GameObject contentGO = new GameObject("ContentPanel");
        contentGO.transform.SetParent(rootGO.transform, false);
        RectTransform contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.5f, 0.5f);
        contentRT.anchorMax = new Vector2(0.5f, 0.5f);
        contentRT.pivot = new Vector2(0.5f, 0.5f);
        contentRT.sizeDelta = new Vector2(520, 320);
        contentRT.anchoredPosition = Vector2.zero;

        Image contentBg = contentGO.AddComponent<Image>();
        contentBg.color = new Color(0.12f, 0.14f, 0.22f, 0.95f); // koyu mavi-gri

        // Panele outline ekle
        Outline contentOutline = contentGO.AddComponent<Outline>();
        contentOutline.effectColor = new Color(0.4f, 0.6f, 1f, 0.4f); // hafif mavi parlama
        contentOutline.effectDistance = new Vector2(2, -2);

        // Yuvarlak köşe için sprite arayalım
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Empty512x512.png");
        if (bgSprite != null)
        {
            contentBg.sprite = bgSprite;
            contentBg.type = Image.Type.Sliced;
        }

        // ============================================
        // 6) BAŞLIK — "Bölüm Tamamlandı!"
        // ============================================
        GameObject titleGO = CreateText(contentGO.transform, "TitleText",
            "Bölüm Tamamlandı!", fontAsset, 36,
            new Color(0.5f, 0.85f, 1f, 1f), // açık mavi
            TextAlignmentOptions.Center);
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1);
        titleRT.anchoredPosition = new Vector2(0, -25);
        titleRT.sizeDelta = new Vector2(0, 50);

        // ============================================
        // 7) AYIRICI ÇİZGİ
        // ============================================
        GameObject separatorGO = new GameObject("Separator");
        separatorGO.transform.SetParent(contentGO.transform, false);
        RectTransform sepRT = separatorGO.AddComponent<RectTransform>();
        sepRT.anchorMin = new Vector2(0.1f, 1);
        sepRT.anchorMax = new Vector2(0.9f, 1);
        sepRT.pivot = new Vector2(0.5f, 1);
        sepRT.anchoredPosition = new Vector2(0, -85);
        sepRT.sizeDelta = new Vector2(0, 2);

        Image sepImg = separatorGO.AddComponent<Image>();
        sepImg.color = new Color(0.4f, 0.6f, 1f, 0.3f);

        // ============================================
        // 8) PUAN ETİKETİ — "Toplam Puan"
        // ============================================
        GameObject scoreLabelGO = CreateText(contentGO.transform, "ScoreLabel",
            "Toplam Puan", fontAsset, 22,
            new Color(0.7f, 0.75f, 0.85f, 1f), // soluk beyaz-mavi
            TextAlignmentOptions.Center);
        RectTransform scoreLabelRT = scoreLabelGO.GetComponent<RectTransform>();
        scoreLabelRT.anchorMin = new Vector2(0, 1);
        scoreLabelRT.anchorMax = new Vector2(1, 1);
        scoreLabelRT.pivot = new Vector2(0.5f, 1);
        scoreLabelRT.anchoredPosition = new Vector2(0, -100);
        scoreLabelRT.sizeDelta = new Vector2(0, 35);

        // ============================================
        // 9) PUAN DEĞERİ — büyük sayı
        // ============================================
        GameObject scoreValueGO = CreateText(contentGO.transform, "ScoreValue",
            "0", fontAsset, 56,
            new Color(1f, 0.9f, 0.3f, 1f), // altın sarısı
            TextAlignmentOptions.Center);
        RectTransform scoreValueRT = scoreValueGO.GetComponent<RectTransform>();
        scoreValueRT.anchorMin = new Vector2(0, 1);
        scoreValueRT.anchorMax = new Vector2(1, 1);
        scoreValueRT.pivot = new Vector2(0.5f, 1);
        scoreValueRT.anchoredPosition = new Vector2(0, -135);
        scoreValueRT.sizeDelta = new Vector2(0, 70);

        // ============================================
        // 10) SONRAKİ SEVİYE BUTONU
        // ============================================
        // Buton container
        GameObject buttonGO = new GameObject("NextLevelButton");
        buttonGO.transform.SetParent(contentGO.transform, false);
        RectTransform buttonRT = buttonGO.AddComponent<RectTransform>();
        buttonRT.anchorMin = new Vector2(0.5f, 0);
        buttonRT.anchorMax = new Vector2(0.5f, 0);
        buttonRT.pivot = new Vector2(0.5f, 0);
        buttonRT.anchoredPosition = new Vector2(0, 30);
        buttonRT.sizeDelta = new Vector2(280, 60);

        Image buttonImg = buttonGO.AddComponent<Image>();
        buttonImg.color = new Color(0.2f, 0.65f, 0.35f, 1f); // yeşil buton
        if (bgSprite != null)
        {
            buttonImg.sprite = bgSprite;
            buttonImg.type = Image.Type.Sliced;
        }

        Button button = buttonGO.AddComponent<Button>();
        // Buton hover/click renk geçişleri
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.65f, 0.35f, 1f);
        colors.highlightedColor = new Color(0.25f, 0.75f, 0.4f, 1f);
        colors.pressedColor = new Color(0.15f, 0.5f, 0.25f, 1f);
        colors.selectedColor = new Color(0.2f, 0.65f, 0.35f, 1f);
        colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        button.colors = colors;

        // Buton metni
        GameObject buttonTextGO = CreateText(buttonGO.transform, "ButtonText",
            "Sonraki Seviye ▶", fontAsset, 26,
            Color.white,
            TextAlignmentOptions.Center);
        RectTransform buttonTextRT = buttonTextGO.GetComponent<RectTransform>();
        StretchFull(buttonTextRT);

        // ============================================
        // 11) LevelCompleteUI COMPONENT EKLE VE BAĞLA
        // ============================================
        LevelCompleteUI lcUI = canvasGO.AddComponent<LevelCompleteUI>();
        lcUI.rootPanel = rootGO;
        lcUI.blurOverlay = overlayImg;
        lcUI.contentPanel = contentGO;
        lcUI.scoreText = scoreValueGO.GetComponent<TextMeshProUGUI>();
        lcUI.nextLevelButton = button;

        // ============================================
        // 12) Level1Controller'a REFERANSI BAĞLA
        // ============================================
        Level1Controller controller = Object.FindAnyObjectByType<Level1Controller>();
        if (controller != null)
        {
            controller.levelCompleteUI = lcUI;
            EditorUtility.SetDirty(controller);
            Debug.Log("[LevelCompleteUISetup] Level1Controller'a LevelCompleteUI referansı bağlandı!");
        }
        else
        {
            Debug.LogWarning("[LevelCompleteUISetup] Level1Controller bulunamadı! Referansı manuel bağlayın.");
        }

        // ============================================
        // 13) BAŞLANGIÇTA KAPAT (rootPanel.SetActive(false))
        // ============================================
        rootGO.SetActive(false);

        // Sahneyi dirty olarak işaretle
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Selection.activeGameObject = canvasGO;
        Debug.Log("[LevelCompleteUISetup] ✅ Level Complete UI başarıyla oluşturuldu!");

        EditorUtility.DisplayDialog("Level Complete UI Kurulumu",
            "Level sonu ekranı başarıyla oluşturuldu!\n\n" +
            "• Koyu overlay (blur efekti)\n" +
            "• Ortada panel: Başlık + Toplam Puan + Sonraki Seviye butonu\n\n" +
            "Level1Controller'a otomatik bağlandı.\n" +
            "Level bittiğinde bu ekran açılır, butona basınca Level 2'ye geçer.",
            "Tamam");
    }

    // ============================================
    // YARDIMCI METOTLAR
    // ============================================

    /// <summary>
    /// TMP Font Asset bulur — projeden veya Resources'tan.
    /// </summary>
    private static TMP_FontAsset FindFontAsset()
    {
        TMP_FontAsset fontAsset = null;

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

        if (fontAsset == null)
        {
            TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            if (allFonts.Length > 0)
                fontAsset = allFonts[0];
        }

        if (fontAsset == null)
            Debug.LogWarning("[LevelCompleteUISetup] TMP Font Asset bulunamadı! Varsayılan font kullanılacak.");

        return fontAsset;
    }

    /// <summary>
    /// TextMeshPro UI text elemanı oluşturur.
    /// </summary>
    private static GameObject CreateText(Transform parent, string name,
        string text, TMP_FontAsset font, float fontSize,
        Color color, TextAlignmentOptions alignment)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent, false);
        textGO.AddComponent<RectTransform>();

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableAutoSizing = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;

        if (font != null)
            tmp.font = font;

        return textGO;
    }

    /// <summary>
    /// RectTransform'u tam stretch (tüm parent'ı kapla) yapar.
    /// </summary>
    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
