using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuActions : MonoBehaviour
{
    public void CommencerPartie()
    {
        Debug.Log("a");
        if (GameStateManager.Instance != null)
        {
            Debug.Log("b");
            GameStateManager.Instance.StartGame();
        }
        else
            SceneManager.LoadScene("Game");

        Debug.Log("c");
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
