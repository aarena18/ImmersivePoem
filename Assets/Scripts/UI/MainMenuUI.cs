using UnityEngine;
using UnityEngine.UI;

// Attacher ce script au GameObject racine du panneau Menu Principal.
// Dans l'Inspector, glisser-déposer les boutons dans les champs correspondants.
public class MainMenuUI : MonoBehaviour
{
    [Header("Boutons du menu")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        // On lie chaque bouton à la méthode correspondante du GameStateManager.
        // AddListener = "quand on clique sur ce bouton, appelle cette fonction"
        playButton.onClick.AddListener(OnPlayClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        // S'abonner aux changements d'état pour afficher/cacher le panneau
        GameStateManager.Instance.OnStateChanged += HandleStateChange;

        // Afficher le menu au démarrage
        gameObject.SetActive(GameStateManager.Instance.CurrentState == GameStateManager.GameState.MainMenu);
    }

    void OnDestroy()
    {
        // Toujours se désabonner pour éviter des erreurs si l'objet est détruit
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnStateChanged -= HandleStateChange;
    }

    private void HandleStateChange(GameStateManager.GameState newState)
    {
        // Le panneau n'est visible que si on est dans l'état MainMenu
        gameObject.SetActive(newState == GameStateManager.GameState.MainMenu);
    }

    private void OnPlayClicked()
    {
        GameStateManager.Instance.StartGame();
    }

    private void OnQuitClicked()
    {
        GameStateManager.Instance.QuitGame();
    }
}
