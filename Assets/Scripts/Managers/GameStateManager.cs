using UnityEngine;
using UnityEngine.SceneManagement;

// Singleton : une seule instance existe dans toute la partie.
// Les autres scripts l'appellent via GameStateManager.Instance
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused }
    public GameState CurrentState { get; private set; }

    // Abonnez-vous à cet événement pour réagir aux changements d'état.
    // Exemple : PauseMenuUI.cs écoute OnStateChanged pour afficher / cacher le panneau.
    public event System.Action<GameState> OnStateChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // survit aux changements de scène
    }

    void Start()
    {
        SetState(GameState.MainMenu);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;

        // Time.timeScale contrôle la vitesse du jeu.
        // 0 = pause totale, 1 = vitesse normale.
        Time.timeScale = (newState == GameState.Paused) ? 0f : 1f;

        OnStateChanged?.Invoke(newState); // notifie tous les abonnés
    }

    // --- Méthodes publiques appelées par les boutons UI ---

    public void StartGame()
    {
        SetState(GameState.Playing);
        SceneManager.LoadScene("Game");
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
            SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
            SetState(GameState.Playing);
    }

    public void ReturnToMainMenu()
    {
        SetState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
