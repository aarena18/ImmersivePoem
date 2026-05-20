using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuActions : MonoBehaviour
{
    public void CommencerPartie()
    {
        SceneManager.LoadScene("scene");
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
