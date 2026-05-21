using System.Collections;
using UnityEngine;
using UnityEngine.Video;

// Attacher ce script sur un GameObject vide dans la scène Game.
// - videoPlayer   : le VideoPlayer qui joue PoemeFemme.mp4
// - videoDisplay  : le GameObject contenant le RawImage (écran vidéo)
// - textCanvas    : le Canvas texte à afficher après la vidéo
public class PoemVideoController : MonoBehaviour
{
    [Header("Vidéo")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject  videoDisplay;

    [Header("Canvas texte (affiché après la vidéo)")]
    [SerializeField] private GameObject textCanvas;

    [Header("Délai avant lecture (secondes)")]
    [SerializeField] private float delayBeforePlay = 5f;

    void Start()
    {
        // État initial : vidéo et texte cachés
        if (videoDisplay != null) videoDisplay.SetActive(false);
        if (textCanvas   != null) textCanvas.SetActive(false);

        videoPlayer.loopPointReached += OnVideoFinished;

        StartCoroutine(PlayAfterDelay());
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private IEnumerator PlayAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforePlay);

        if (videoDisplay != null) videoDisplay.SetActive(true);
        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (videoDisplay != null) videoDisplay.SetActive(false);
        if (textCanvas   != null) textCanvas.SetActive(true);
    }
}
