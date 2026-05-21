using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

// Attacher ce script sur le GameObject du lego batman.
// Nécessite un Collider sur cet objet.
// Fonctionne avec la souris (éditeur) et les contrôleurs Quest (VR).
public class LegoBatmanInteraction : MonoBehaviour
{
    [Tooltip("Si coché, un second clic revient à la voix féminine.")]
    [SerializeField] private bool toggleMode = true;

    [Tooltip("Glisse ici ta caméra Player depuis la Hierarchy.")]
    [SerializeField] private Camera playerCamera;

    private bool rightTriggerWasPressed = false;
    private bool leftTriggerWasPressed  = false;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        // --- Éditeur / PC : détection par position écran (compatible caméra VR) ---
        if (Input.GetMouseButtonDown(0) && playerCamera != null)
        {
            Vector3 screenPos = playerCamera.WorldToScreenPoint(transform.position);
            if (screenPos.z > 0 && Vector2.Distance(Input.mousePosition, new Vector2(screenPos.x, screenPos.y)) < 150f)
                SwitchVoice();
        }

        // --- VR : détection du trigger contrôleur + raycast ---
        CheckControllerTrigger(XRNode.RightHand, ref rightTriggerWasPressed);
        CheckControllerTrigger(XRNode.LeftHand,  ref leftTriggerWasPressed);
    }

    private void CheckControllerTrigger(XRNode node, ref bool wasPressed)
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(node, devices);
        if (devices.Count == 0) return;

        InputDevice device = devices[0];
        bool triggerPressed = false;
        device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed);

        // Détection front montant (appui, pas maintien)
        if (triggerPressed && !wasPressed)
        {
            Vector3 position;
            Quaternion rotation;
            if (device.TryGetFeatureValue(CommonUsages.devicePosition, out position) &&
                device.TryGetFeatureValue(CommonUsages.deviceRotation, out rotation))
            {
                Ray ray = new Ray(position, rotation * Vector3.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 20f) && hit.collider.gameObject == gameObject)
                    SwitchVoice();
            }
        }

        wasPressed = triggerPressed;
    }

    private void SwitchVoice()
    {
        if (MenuSoundManager.Instance == null)
        {
            Debug.LogWarning("[Batman] MenuSoundManager introuvable dans la scène !");
            return;
        }

        if (toggleMode)
        {
            if (MenuSoundManager.Instance.CurrentVoice == MenuSoundManager.VoiceType.Batman)
                MenuSoundManager.Instance.SetFemmeVoice();
            else
                MenuSoundManager.Instance.SetBatmanVoice();
        }
        else
        {
            MenuSoundManager.Instance.SetBatmanVoice();
        }
    }
}
