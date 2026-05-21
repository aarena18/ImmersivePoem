using UnityEngine;

// Singleton placé sur le Canvas du MainMenu (ou un objet persistant).
// Gère la banque de sons active pour les boutons du menu.
public class MenuSoundManager : MonoBehaviour
{
    public static MenuSoundManager Instance { get; private set; }

    public enum VoiceType { Femme, Batman }
    public VoiceType CurrentVoice { get; private set; } = VoiceType.Femme;

    [Header("Voix Femme (ordre : Commencer, Paramètres, Quitter)")]
    [SerializeField] private AudioClip[] femmeClips;

    [Header("Voix Batman (ordre : Commencer, Paramètres, Quitter)")]
    [SerializeField] private AudioClip[] batmanClips;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Retourne le clip correspondant au bouton (index) selon la voix active
    public AudioClip GetClipForButton(int buttonIndex)
    {
        AudioClip[] clips = (CurrentVoice == VoiceType.Femme) ? femmeClips : batmanClips;
        if (clips == null || buttonIndex < 0 || buttonIndex >= clips.Length) return null;
        return clips[buttonIndex];
    }

    public void SetVoice(VoiceType voice) => CurrentVoice = voice;
    public void SetBatmanVoice()          => CurrentVoice = VoiceType.Batman;
    public void SetFemmeVoice()           => CurrentVoice = VoiceType.Femme;
}
