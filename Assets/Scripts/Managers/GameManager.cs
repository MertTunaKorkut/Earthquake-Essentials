using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Oyunun genel akışını yöneten singleton manager.
/// Sahne geçişleri, oyun durumu ve level yönetimi burada yapılır.
/// DontDestroyOnLoad ile sahneler arası yaşar.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Oyun Durumu")]
    public GameState CurrentState = GameState.MainMenu;
    public int CurrentRoom = 0; // Level 2'de hangi odadayız (0 = henüz başlamadı, 1-3 = odalar)

    [Header("Sahne İsimleri")]
    public string mainMenuScene = "MainMenu";
    public string level1Scene = "Level1_EmergencyBag";
    public string level2Room1Scene = "Level2_Room1_LivingRoom";
    public string level2Room2Scene = "Level2_Room2_Kitchen";
    public string level2Room3Scene = "Level2_Room3_Bedroom";
    public string gameOverScene = "GameOver";

    [Header("Geçiş Ayarları")]
    public float transitionDuration = 1f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        transform.SetParent(null);

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Oyunu başlatır — Ana Menüden Level 1'e geçiş.
    /// </summary>
    public void StartGame()
    {
        ScoreManager.Instance.ResetScore();
        CurrentState = GameState.Level1;
        CurrentRoom = 0;
        LoadScene(level1Scene);
    }

    /// <summary>
    /// Level 1 tamamlandığında çağrılır — Level 2 Room 1'e geçiş.
    /// </summary>
    public void OnLevel1Complete()
    {
        CurrentState = GameState.Level2;
        CurrentRoom = 1;
        LoadScene(level2Room1Scene);
    }

    /// <summary>
    /// Level 2'de bir sonraki odaya geçiş yapar.
    /// 3 oda bitince oyun sonu ekranına gider.
    /// </summary>
    public void GoToNextRoom()
    {
        CurrentRoom++;

        switch (CurrentRoom)
        {
            case 2:
                LoadScene(level2Room2Scene);
                break;
            case 3:
                LoadScene(level2Room3Scene);
                break;
            default:
                // 3 oda bitti, oyun sonu
                OnGameComplete();
                break;
        }
    }

    /// <summary>
    /// Oyun tamamlandığında çağrılır.
    /// </summary>
    public void OnGameComplete()
    {
        CurrentState = GameState.GameOver;
        LoadScene(gameOverScene);
    }

    /// <summary>
    /// Ana menüye dön.
    /// </summary>
    public void ReturnToMainMenu()
    {
        CurrentState = GameState.MainMenu;
        CurrentRoom = 0;
        ScoreManager.Instance.ResetScore();
        LoadScene(mainMenuScene);
    }

    /// <summary>
    /// Oyunu tekrar başlat (Game Over ekranından).
    /// </summary>
    public void RestartGame()
    {
        StartGame();
    }

    /// <summary>
    /// Sahne yükleme — fade geçişi ile.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        // TODO: Faz 4'te fade-out animasyonu eklenecek
        yield return new WaitForSeconds(transitionDuration * 0.5f);

        SceneManager.LoadScene(sceneName);

        // TODO: Faz 4'te fade-in animasyonu eklenecek
        yield return new WaitForSeconds(transitionDuration * 0.5f);
    }
}

/// <summary>
/// Oyunun mevcut durumunu temsil eden enum.
/// </summary>
public enum GameState
{
    MainMenu,
    Level1,
    Level2,
    GameOver
}
