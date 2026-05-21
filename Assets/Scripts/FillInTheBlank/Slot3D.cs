using UnityEngine;
using TMPro; 
using System.Collections;

public class Slot3D : MonoBehaviour
{
    public string motAttendu; 
    public TMP_Text texteDeLaPhrase; // Le "___"

    [Header("Sons")]
    public AudioSource audioSource;
    public AudioClip sonVictoire;
    public AudioClip sonErreur;

    private HoverOutlineHighlight highlight;

    void Start()
    {
        highlight = GetComponent<HoverOutlineHighlight>();

        if (highlight == null)
            highlight = gameObject.AddComponent<HoverOutlineHighlight>();

        highlight.Configure(new Color(0.2f, 0.95f, 1f, 1f), 1.03f);
        highlight.SetHighlighted(false);
    }

    public void SetHighlighted(bool highlighted)
    {
        if (highlight != null)
            highlight.SetHighlighted(highlighted);
    }

    public void VerifierReponse(DragObjet3D objetGlisse)
    {
        if (objetGlisse.wordValue == motAttendu) Reussite(objetGlisse);
        else Echec(objetGlisse);
    }

    void Reussite(DragObjet3D objetGlisse)
    {
        if (audioSource && sonVictoire) audioSource.PlayOneShot(sonVictoire);
        
        texteDeLaPhrase.text = motAttendu; // Affiche le mot !
        texteDeLaPhrase.color = Color.green; // Optionnel : met le texte en vert
        
        Destroy(objetGlisse.gameObject); // Détruit l'objet 3D
    }

    void Echec(DragObjet3D objetGlisse)
    {
        if (audioSource && sonErreur) audioSource.PlayOneShot(sonErreur);
        
        objetGlisse.RetourAuSol(); // Renvoie l'objet à sa place
        StartCoroutine(ClignoterTexteErreur());
    }

    IEnumerator ClignoterTexteErreur()
    {
        Color couleurOriginale = texteDeLaPhrase.color;
        texteDeLaPhrase.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        texteDeLaPhrase.color = couleurOriginale;
    }
}