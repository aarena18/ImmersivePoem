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

    private bool rightTriggerWasPressed = false;
    private bool leftTriggerWasPressed  = false;

    // --- Éditeur / PC : clic souris ---
    private void OnMouseDown() => SwitchVoice();

    // --- VR : détection du trigger contrôleur + raycast ---
    void Update()
    {
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
        if (MenuSoundManager.Instance == null) return;

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
