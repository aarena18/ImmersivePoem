using UnityEngine;

public class DragObjet3D : MonoBehaviour
{
    public string wordValue; 
    
    [Tooltip("Cocher pour débloquer l'objet après le poème")]
    public bool peutEtreSaisi = false; 

    [HideInInspector] public Vector3 positionAvantClic; 
    
    private float zDistanceToCamera;
    private Rigidbody rb; 
    private Collider monCollider; // On stocke le collider de l'objet

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        monCollider = GetComponent<Collider>();
    }

    void OnMouseDown()
    {
        if (peutEtreSaisi == false) return; 

        positionAvantClic = transform.position; 
        if (rb != null) rb.isKinematic = true; 

        zDistanceToCamera = Camera.main.WorldToScreenPoint(transform.position).z;
    }

    void OnMouseDrag()
    {
        if (peutEtreSaisi == false) return; 

        Vector3 positionSourisEcran = new Vector3(Input.mousePosition.x, Input.mousePosition.y, zDistanceToCamera);
        transform.position = Camera.main.ScreenToWorldPoint(positionSourisEcran);
    }

    // ON A SUPPRIMÉ ONTRIGGERENTER ET EXIT ICI !

    void OnMouseUp()
    {
        if (peutEtreSaisi == false) return;

        // 1. On désactive temporairement le collider de cet objet 
        // pour que le "laser" ne tape pas dessus par erreur
        if (monCollider != null) monCollider.enabled = false;

        // 2. On tire un rayon depuis la position de la souris
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 3. Si le rayon touche quelque chose en 3D
        if (Physics.Raycast(ray, out hit))
        {
            // Est-ce que ce qu'il touche est un trou ?
            if (hit.collider.CompareTag("ZoneDrop"))
            {
                Slot3D zoneTouchee = hit.collider.GetComponent<Slot3D>();
                if (zoneTouchee != null)
                {
                    zoneTouchee.VerifierReponse(this); // On vérifie la réponse !
                    if (monCollider != null) monCollider.enabled = true; // On réactive
                    return; // On arrête la fonction ici, c'est un succès (ou une erreur gérée)
                }
            }
        }

        // 4. Si on arrive ici, c'est qu'on a cliqué dans le vide ou à côté
        if (monCollider != null) monCollider.enabled = true; // On réactive
        RetourAuSol(); 
    }

    public void RetourAuSol()
    {
        transform.position = positionAvantClic;
        if (rb != null) rb.isKinematic = false; 
    }
}