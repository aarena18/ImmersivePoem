using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuActions : MonoBehaviour
{
    public void CommencerPartie()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.StartGame();
        else
            SceneManager.LoadScene("PoemScene");
    }

    public void Parametres()
    {
        // TODO: ouvrir le panneau paramètres
    }

    public void Quitter()
    {
        Application.Quit();
    }
}
