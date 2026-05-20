using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// Ce script doit être sur un GameObject TOUJOURS ACTIF (PauseMenuController).
// Assigner le Canvas/Panel visuel dans pausePanel — c'est lui qui sera affiché/caché.
public class PauseMenuUI : MonoBehaviour
{
    [Header("Panel visuel du menu pause (à afficher/cacher)")]
    [SerializeField] private GameObject pausePanel;

    [Header("Boutons du menu pause")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    private bool menuButtonPressedLastFrame = false;

    void Start()
    {
        // Cacher le panel dès le départ
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Vérification sécurisée avant de s'abonner
        if (GameStateManager.Instance != null)
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            quitButton.onClick.AddListener(OnQuitClicked);
            GameStateManager.Instance.OnStateChanged += HandleStateChange;

            // Si on arrive dans la scène en état Playing, on s'assure que le panel est caché
            if (GameStateManager.Instance.CurrentState != GameStateManager.GameState.Paused)
                if (pausePanel != null) pausePanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("PauseMenuUI : GameStateManager introuvable. " +
                             "Ajoute-le à la PoemScene ou lance depuis MainMenu.");
        }
    }

    void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= HandleStateChange;
    }

    void Update()
    {
        if (GameStateManager.Instance == null) return;

        // PC : touche Échap
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
            return;
        }

        // VR Quest 3 : bouton Menu du contrôleur gauche
        var leftControllers = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftControllers);

        if (leftControllers.Count > 0)
        {
            bool menuPressed = false;
            leftControllers[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out menuPressed);

            if (menuPressed && !menuButtonPressedLastFrame)
                TogglePause();

            menuButtonPressedLastFrame = menuPressed;
        }
    }

    private void TogglePause()
    {
        if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.Playing)
            GameStateManager.Instance.PauseGame();
        else if (GameStateManager.Instance.CurrentState == GameStateManager.GameState.Paused)
            GameStateManager.Instance.ResumeGame();
    }

    private void HandleStateChange(GameStateManager.GameState newState)
    {
        if (pausePanel != null)
            pausePanel.SetActive(newState == GameStateManager.GameState.Paused);
    }

    private void OnResumeClicked()  { GameStateManager.Instance.ResumeGame(); }
    private void OnMainMenuClicked(){ GameStateManager.Instance.ReturnToMainMenu(); }
    private void OnQuitClicked()    { GameStateManager.Instance.QuitGame(); }
}
