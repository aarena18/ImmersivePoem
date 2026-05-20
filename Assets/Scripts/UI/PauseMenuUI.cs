using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// Ce script doit être sur un GameObject TOUJOURS ACTIF (ex: un parent du Canvas pause).
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
        resumeButton.onClick.AddListener(OnResumeClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        GameStateManager.Instance.OnStateChanged += HandleStateChange;

        // Cacher le panel visuel au départ, mais CE script reste actif
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= HandleStateChange;
    }

    // Update() tourne toujours car ce GameObject reste actif
    void Update()
    {
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
        // On affiche/cache le panel, pas le GameObject de ce script
        if (pausePanel != null)
            pausePanel.SetActive(newState == GameStateManager.GameState.Paused);
    }

    private void OnResumeClicked()  { GameStateManager.Instance.ResumeGame(); }
    private void OnMainMenuClicked(){ GameStateManager.Instance.ReturnToMainMenu(); }
    private void OnQuitClicked()    { GameStateManager.Instance.QuitGame(); }
}
