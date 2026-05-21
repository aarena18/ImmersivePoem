using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class DragObjet3D : MonoBehaviour
{
    public string wordValue;

    [Tooltip("Cocher pour débloquer l'objet après le poème")]
    public bool peutEtreSaisi = false;

    [HideInInspector] public Vector3 positionAvantClic;

    private Rigidbody rb;
    private Collider monCollider;
    private HoverOutlineHighlight highlight;
    private Slot3D slotSurvole;

    private XRGrabInteractable grabInteractable;
    private bool estSaisi = false;
    private Transform sourceDuRay;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        monCollider = GetComponent<Collider>();

        highlight = GetComponent<HoverOutlineHighlight>();
        if (highlight == null)
            highlight = gameObject.AddComponent<HoverOutlineHighlight>();

        highlight.Configure(new Color(1f, 0.85f, 0.2f, 1f), 1.05f);
        highlight.SetHighlighted(false);

        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = false;
        grabInteractable.movementType = XRBaseInteractable.MovementType.Kinematic;

        grabInteractable.hoverEntered.AddListener(SurvolVR_Debut);
        grabInteractable.hoverExited.AddListener(SurvolVR_Fin);
        grabInteractable.selectEntered.AddListener(SaisieVR_Debut);
        grabInteractable.selectExited.AddListener(SaisieVR_Fin);
    }

    void Update()
    {
        // Active / désactive la saisie
        grabInteractable.interactionLayers =
            peutEtreSaisi ? InteractionLayerMask.GetMask("Default") : 0;

        grabInteractable.trackRotation = false;

        if (estSaisi)
        {
            MettreAJourZoneSurvoleeVR();
        }
    }

    // =========================
    // EVENTS VR
    // =========================

    private void SurvolVR_Debut(HoverEnterEventArgs args)
    {
        if (peutEtreSaisi && highlight != null)
            highlight.SetHighlighted(true);
    }

    private void SurvolVR_Fin(HoverExitEventArgs args)
    {
        if (highlight != null)
            highlight.SetHighlighted(false);
    }

private void SaisieVR_Debut(SelectEnterEventArgs args)
{
        if (!peutEtreSaisi) return;

        positionAvantClic = transform.position;
        estSaisi = true;
        sourceDuRay = args.interactorObject != null ? args.interactorObject.transform : null;

        if (monCollider != null)
            monCollider.enabled = false;

        if (rb != null)
            rb.isKinematic = true;
}

    private void SaisieVR_Fin(SelectExitEventArgs args)
    {
        if (!peutEtreSaisi) return;

        estSaisi = false;

        if (highlight != null)
            highlight.SetHighlighted(false);

        VerifierDepotVR();

        SetZoneSurvolee(null);
        sourceDuRay = null;

        if (monCollider != null)
            monCollider.enabled = true;

        if (rb != null)
            rb.isKinematic = false;
    }

    // =========================
    // VR DROP LOGIC (RAYCAST)
    // =========================

    private void MettreAJourZoneSurvoleeVR()
    {
        if (sourceDuRay == null)
        {
            SetZoneSurvolee(null);
            return;
        }

        SetZoneSurvolee(TrouverZoneSousRayon());
    }

    private void SetZoneSurvolee(Slot3D nouvelleZone)
    {
        if (slotSurvole == nouvelleZone) return;

        if (slotSurvole != null)
            slotSurvole.SetHighlighted(false);

        slotSurvole = nouvelleZone;

        if (slotSurvole != null)
            slotSurvole.SetHighlighted(true);
    }

    private Slot3D TrouverZoneDrop(Collider collider)
    {
        if (collider == null) return null;

        if (collider.CompareTag("ZoneDrop"))
            return collider.GetComponent<Slot3D>();

        return collider.GetComponentInParent<Slot3D>();
    }

    private Slot3D TrouverZoneSousRayon()
    {
        if (sourceDuRay == null) return null;

        Ray ray = new Ray(sourceDuRay.position, sourceDuRay.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, 50f);
        Slot3D meilleureZone = null;
        float meilleureDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if (hit.collider == null) continue;
            if (hit.collider == monCollider) continue;
            if (!hit.collider.CompareTag("ZoneDrop")) continue;
            if (hit.distance >= meilleureDistance) continue;

            Slot3D zone = TrouverZoneDrop(hit.collider);

            if (zone != null)
            {
                meilleureZone = zone;
                meilleureDistance = hit.distance;
            }
        }

        return meilleureZone;
    }

    private void VerifierDepotVR()
    {
        if (slotSurvole != null)
        {
            slotSurvole.VerifierReponse(this);
            return;
        }

        if (sourceDuRay != null)
        {
            Slot3D zone = TrouverZoneSousRayon();

            if (zone != null)
            {
                zone.VerifierReponse(this);
                return;
            }
        }

        RetourAuSol();
    }

    public void RetourAuSol()
    {
        transform.position = positionAvantClic;

        if (rb != null)
            rb.isKinematic = false;
    }

    void OnDestroy()
    {
        if (grabInteractable == null) return;

        grabInteractable.hoverEntered.RemoveListener(SurvolVR_Debut);
        grabInteractable.hoverExited.RemoveListener(SurvolVR_Fin);
        grabInteractable.selectEntered.RemoveListener(SaisieVR_Debut);
        grabInteractable.selectExited.RemoveListener(SaisieVR_Fin);
    }
}