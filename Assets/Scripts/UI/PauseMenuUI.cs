using System.Collections.Generic;
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

    // Suivi de l'état du bouton Menu VR pour détecter un appui unique
    private bool menuButtonPressedLastFrame = false;

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
        // PC : touche Échap (utile pour tester en éditeur)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
            return;
        }

        // VR Quest 3 : bouton Menu du contrôleur gauche
        // On utilise les noms complets pour éviter les conflits de namespace
        var leftControllers = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftControllers);

        if (leftControllers.Count > 0)
        {
            bool menuPressed = false;
            leftControllers[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out menuPressed);

            // On déclenche seulement au moment où le bouton est pressé (front montant)
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
