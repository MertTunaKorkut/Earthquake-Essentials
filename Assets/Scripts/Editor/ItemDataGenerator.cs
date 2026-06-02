using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Unity Editor başladığında veya Assets > Create Level1 Data menüsü çalıştırıldığında
/// Level 1 ItemData ScriptableObject'lerini otomatik oluşturur.
/// Bu script yalnızca bir kere çalışır ve ardından kendini devre dışı bırakır.
/// </summary>
[InitializeOnLoad]
public class Level1DataBootstrapper
{
    private const string DoneKey = "EQ_Level1Data_Created_v1";

    static Level1DataBootstrapper()
    {
        // Zaten oluşturulduysa çalışma
        if (SessionState.GetBool(DoneKey, false)) return;

        // Derlemeden hemen sonra bir kere çalış
        EditorApplication.delayCall += TryCreateData;
    }

    [MenuItem("Tools/Earthquake Essentials/Create Level 1 Item Data")]
    public static void TryCreateData()
    {
        string soFolder = "Assets/ScriptableObjects/Level1";

        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");

        if (!AssetDatabase.IsValidFolder(soFolder))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Level1");

        int created = 0;

        // ── ESSENTIAL ────────────────────────────────────────────────────
        created += Make(soFolder, "Item_WaterBottle",
            "Su Şişesi", true, ItemType.Essential,
            "Assets/Sprites/Level 1/Essentials/water bottle.png",
            "Acil durum çantasında mutlaka bulunması gereken içme suyu.");

        created += Make(soFolder, "Item_FirstAid",
            "İlk Yardım Çantası", true, ItemType.Essential,
            "Assets/Sprites/Level 1/Essentials/first aid.png",
            "Yaralanmalarda ilk müdahale için şart.");

        created += Make(soFolder, "Item_Flashlight",
            "El Feneri", true, ItemType.Essential,
            "Assets/Sprites/Level 1/Essentials/flashlight.png",
            "Karanlık ortamlarda hayat kurtarır.");

        created += Make(soFolder, "Item_Nuts",
            "Kuruyemiş", true, ItemType.Essential,
            "Assets/Sprites/Level 1/Essentials/nuts.png",
            "Uzun süre tok tutan enerji kaynağı.");

        created += Make(soFolder, "Item_RadioBattery",
            "Radyo ve Pil", true, ItemType.Essential,
            "Assets/Sprites/Level 1/Essentials/radio n battery.png",
            "Acil durum yayınlarını dinlemek için.");

        // ── NON-ESSENTIAL ─────────────────────────────────────────────────
        created += Make(soFolder, "Item_Chocolate",
            "Çikolata", false, ItemType.NonEssential,
            "Assets/Sprites/Level 1/Non-essentials/chocolate.png",
            "Lezzetli ama acil çantasına gereksiz.");

        created += Make(soFolder, "Item_Laptop",
            "Dizüstü Bilgisayar", false, ItemType.NonEssential,
            "Assets/Sprites/Level 1/Non-essentials/laptop.png",
            "Deprem çantasında yeri yok.");

        created += Make(soFolder, "Item_Lighter",
            "Çakmak", false, ItemType.Hazardous,
            "Assets/Sprites/Level 1/Non-essentials/lighter.png",
            "Tehlikeli — gaz kaçağı varsa yangına neden olabilir.");

        created += Make(soFolder, "Item_Perfume",
            "Parfüm", false, ItemType.NonEssential,
            "Assets/Sprites/Level 1/Non-essentials/perfume.png",
            "Gereksiz — acil çantanıza koymayın.");

        created += Make(soFolder, "Item_TeddyBear",
            "Oyuncak Ayı", false, ItemType.NonEssential,
            "Assets/Sprites/Level 1/Non-essentials/teddy bear.png",
            "Sevimli ama acil çantasında gereksiz.");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        SessionState.SetBool(DoneKey, true);

        Debug.Log($"[EQ] Level 1 ItemData oluşturma tamamlandı. Yeni: {created}, Konum: {soFolder}");

        // Project penceresinde klasörü seç
        Object folder = AssetDatabase.LoadAssetAtPath<Object>(soFolder);
        if (folder != null) Selection.activeObject = folder;
    }

    private static int Make(
        string folder, string fileName,
        string itemName, bool isEssential, ItemType itemType,
        string spritePath, string description)
    {
        string assetPath = $"{folder}/{fileName}.asset";
        if (AssetDatabase.LoadAssetAtPath<ItemData>(assetPath) != null) return 0;

        ItemData data = ScriptableObject.CreateInstance<ItemData>();
        data.itemName    = itemName;
        data.isEssential = isEssential;
        data.itemType    = itemType;
        data.description = description;

        // Sprite bul — önce direkt, sonra sub-asset olarak
        data.itemSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (data.itemSprite == null)
        {
            foreach (Object a in AssetDatabase.LoadAllAssetsAtPath(spritePath))
                if (a is Sprite s) { data.itemSprite = s; break; }
        }

        if (data.itemSprite == null)
            Debug.LogWarning($"[EQ] Sprite bulunamadı: {spritePath}");

        AssetDatabase.CreateAsset(data, assetPath);
        Debug.Log($"[EQ] Oluşturuldu: {fileName}");
        return 1;
    }
}
