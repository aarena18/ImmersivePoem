using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class PoemVideoController : MonoBehaviour
{
    [Header("Vidéo")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject  videoDisplay; // Quad dans le jardin

    [Header("Canvas texte affiché APRÈS la vidéo (laisser vide si pas besoin)")]
    [SerializeField] private GameObject textCanvas;

    [Header("Délai avant lecture (secondes)")]
    [SerializeField] private float delayBeforePlay = 5f;

    private RenderTexture rt;

    void Start()
    {
        if (textCanvas   != null) textCanvas.SetActive(false);
        if (videoDisplay != null) videoDisplay.SetActive(false);

        SetupVideo();
        videoPlayer.loopPointReached += OnVideoFinished;
        StartCoroutine(PlayAfterDelay());
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
        if (rt != null) { rt.Release(); Destroy(rt); }
    }

    private void SetupVideo()
    {
        if (videoDisplay == null) return;

        Renderer rend = videoDisplay.GetComponentInChildren<Renderer>(true);
        if (rend == null) { Debug.LogWarning("[Video] Pas de Renderer sur VideoDisplay"); return; }

        // Crée la RenderTexture
        rt = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
        rt.name = "VideoRT";
        rt.Create();

        // Branche le VideoPlayer dessus
        videoPlayer.renderMode    = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = rt;

        // Crée un material Unlit URP et lui assigne la RT
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.name = "VideoMat";
        // URP Unlit : la texture principale est _BaseMap
        mat.SetTexture("_BaseMap", rt);
        mat.mainTexture = rt; // fallback

        rend.material = mat;

        Debug.Log("[Video] Setup OK — RenderTexture branchée sur " + rend.gameObject.name);
    }

    private IEnumerator PlayAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforePlay);
        if (videoDisplay != null) videoDisplay.SetActive(true);
        videoPlayer.Play();
        Debug.Log("[Video] Lecture lancée");
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (videoDisplay != null) videoDisplay.SetActive(false);
        if (textCanvas   != null) textCanvas.SetActive(true);
    }

    [ContextMenu("TEST : afficher vidéo maintenant")]
    private void TestShowNow()
    {
        StopAllCoroutines();
        if (videoDisplay != null) videoDisplay.SetActive(true);
        videoPlayer.Play();
    }

    [ContextMenu("TEST : skip vidéo → afficher texte")]
    private void SkipVideo() => OnVideoFinished(videoPlayer);
}
