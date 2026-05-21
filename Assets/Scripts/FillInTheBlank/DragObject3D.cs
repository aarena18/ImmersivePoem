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
    private bool estSaisi       = false;
    private bool estSaisiSouris = false;
    private Transform sourceDuRay;
    private Camera mainCam;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        monCollider = GetComponent<Collider>();

        highlight = GetComponent<HoverOutlineHighlight>();
        if (highlight == null)
            highlight = gameObject.AddComponent<HoverOutlineHighlight>();

        highlight.Configure(new Color(1f, 0.85f, 0.2f, 1f), 1.05f);
        highlight.SetHighlighted(false);

        mainCam = Camera.main;
        if (mainCam == null) mainCam = FindFirstObjectByType<Camera>();

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

        if (estSaisi) MettreAJourZoneSurvoleeVR();

        // ---- Fallback souris (éditeur / simulateur) ----
        if (!peutEtreSaisi) return;

        if (Input.GetMouseButtonDown(0) && !estSaisi)
            TenterSaisieSouris();

        if (estSaisiSouris)
        {
            if (Input.GetMouseButton(0))
                DeplacerAvecSouris();
            else
                RelacherSouris();
        }

        // Hover souris → highlight
        if (!estSaisiSouris && mainCam != null)
        {
            Ray r = mainCam.ScreenPointToRay(Input.mousePosition);
            bool survole = Physics.Raycast(r, out RaycastHit h, 100f)
                           && (h.transform == transform || h.transform.IsChildOf(transform));
            if (highlight != null) highlight.SetHighlighted(survole);
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

    // =========================
    // FALLBACK SOURIS
    // =========================

    private void TenterSaisieSouris()
    {
        if (mainCam == null) return;
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f)
            && (hit.transform == transform || hit.transform.IsChildOf(transform)))
        {
            positionAvantClic = transform.position;
            estSaisiSouris = true;
            if (rb != null) rb.isKinematic = true;
            if (highlight != null) highlight.SetHighlighted(true);
        }
    }

    private void DeplacerAvecSouris()
    {
        if (mainCam == null) return;
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        Plane plan = new Plane(-mainCam.transform.forward, transform.position);
        if (plan.Raycast(ray, out float dist))
            transform.position = ray.GetPoint(dist);

        SetZoneSurvolee(TrouverSlotSousSouris());
    }

    private void RelacherSouris()
    {
        estSaisiSouris = false;
        if (highlight != null) highlight.SetHighlighted(false);

        if (slotSurvole != null)
            slotSurvole.VerifierReponse(this);
        else
            RetourAuSol();

        SetZoneSurvolee(null);
        if (rb != null) rb.isKinematic = false;
    }

    private Slot3D TrouverSlotSousSouris()
    {
        if (mainCam == null) return null;
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
        Slot3D meilleur = null;
        float dist = float.MaxValue;
        foreach (var hit in hits)
        {
            if (hit.collider == monCollider) continue;
            if (!hit.collider.CompareTag("ZoneDrop")) continue;
            if (hit.distance >= dist) continue;
            Slot3D z = TrouverZoneDrop(hit.collider);
            if (z != null) { meilleur = z; dist = hit.distance; }
        }
        return meilleur;
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
