using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Build Settings'teki sahne sırasını düzenleyen editor scripti.
/// Menüden: Tools > Earthquake Essentials > Fix Build Settings
/// </summary>
public class BuildSettingsFixer : Editor
{
    [MenuItem("Tools/Earthquake Essentials/Fix Build Settings")]
    public static void FixBuildSettings()
    {
        // Doğru sahne sırası
        string[] scenePaths = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Level1_EmergencyBag.unity",
            "Assets/Scenes/Level2_Room1_LivingRoom.unity",
            "Assets/Scenes/Level2_Room2_Kitchen.unity",
            "Assets/Scenes/Level2_Room3_Bedroom.unity",
            "Assets/Scenes/GameOver.unity"
        };

        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

        foreach (string path in scenePaths)
        {
            buildScenes.Add(new EditorBuildSettingsScene(path, true));
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();

        Debug.Log("[BuildSettingsFixer] Build Settings düzenlendi! Sahne sırası:");
        for (int i = 0; i < scenePaths.Length; i++)
        {
            Debug.Log($"  [{i}] {scenePaths[i]}");
        }
    }
}
