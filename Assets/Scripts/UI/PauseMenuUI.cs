using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// Attacher ce script au GameObject racine du panneau Pause.
// Dans l'Inspector, glisser-déposer les boutons dans les champs correspondants.
public class PauseMenuUI : MonoBehaviour
{
    [Header("Boutons du menu pause")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        resumeButton.onClick.AddListener(OnResumeClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        GameStateManager.Instance.OnStateChanged += HandleStateChange;

        // Le menu pause est caché au départ
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= HandleStateChange;
    }

    void Update()
    {
        // Appuyer sur Échap bascule pause / reprise
        // Keyboard.current est la nouvelle API Input System d'Unity
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.Playing)
                GameStateManager.Instance.PauseGame();
            else if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.Paused)
                GameStateManager.Instance.ResumeGame();
        }
    }

    private void HandleStateChange(GameStateManager.GameState newState)
    {
        gameObject.SetActive(newState == GameStateManager.GameState.Paused);
    }

    private void OnResumeClicked()
    {
        GameStateManager.Instance.ResumeGame();
    }

    private void OnMainMenuClicked()
    {
        GameStateManager.Instance.ReturnToMainMenu();
    }

    private void OnQuitClicked()
    {
        GameStateManager.Instance.QuitGame();
    }
}
