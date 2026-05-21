using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HeavyObjectDrop : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 originalScale;
    private Coroutine squashCoroutine;

    [Header("Paramètres de Chute")]
    public bool dropOnStart = false;

    [Header("Paramètres Physiques du Rebond")]
    [Tooltip("L'énergie conservée à chaque rebond (0 = aucun, 1 = rebond infini). Ex: 0.35 pour un objet lourd.")]
    [Range(0f, 1f)]
    public float bounciness = 0.35f;

    [Tooltip("La vitesse d'impact minimale pour provoquer un rebond. Permet à l'objet de s'arrêter à la fin.")]
    public float minImpactVelocity = 1.5f;

    [Header("Effet Visuel dynamique (Squash & Stretch)")]
    [Tooltip("Taille de l'objet lors de l'impact le plus violent (Ex: 0.6 = s'écrase à 60% de sa taille).")]
    public float maxSquashFactor = 0.6f;
    [Tooltip("Vitesse à laquelle l'objet reprend sa forme initiale.")]
    public float recoverySpeed = 10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;

        rb.useGravity = dropOnStart;
    }

    public void DropObject()
    {
        rb.useGravity = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // On vérifie qu'on tape bien sur une surface pointant vers le haut (le sol)
        if (collision.contacts[0].normal.y > 0.5f)
        {
            // La magnitude de la "relativeVelocity" nous donne la force exacte de l'impact.
            // Une chute de 10m aura une vitesse beaucoup plus grande qu'une chute de 1m.
            float impactSpeed = collision.relativeVelocity.magnitude;

            // On vérifie si l'impact est assez fort pour rebondir (sinon l'objet se pose simplement)
            if (impactSpeed > minImpactVelocity)
            {
                // 1. Calcul du rebond : Vitesse d'impact multipliée par l'élasticité de l'objet
                float bounceVelocity = impactSpeed * bounciness;

                // On applique cette nouvelle vitesse vers le haut
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, bounceVelocity, rb.linearVelocity.z);

                // 2. Calcul de l'écrasement dynamique
                // Plus l'objet tombe vite (jusqu'à une vitesse de 15), plus il s'écrase fort.
                float normalizedImpact = Mathf.Clamp01(impactSpeed / 15f); 
                float dynamicSquashAmount = Mathf.Lerp(1f, maxSquashFactor, normalizedImpact);

                // On arrête l'animation précédente si l'objet rebondit très vite
                if (squashCoroutine != null) StopCoroutine(squashCoroutine);
                
                // On lance l'animation visuelle avec l'écrasement calculé
                squashCoroutine = StartCoroutine(SquashAndStretch(dynamicSquashAmount));
            }
        }
    }

    private IEnumerator SquashAndStretch(float currentSquashFactor)
    {
        // On calcule la forme aplatie en fonction de la violence du choc
        Vector3 squashedScale = new Vector3(
            originalScale.x * (1f + (1f - currentSquashFactor)), 
            originalScale.y * currentSquashFactor, 
            originalScale.z * (1f + (1f - currentSquashFactor))
        );

        float t = 0f;

        // Phase 1 : Écrasement immédiat
        while (t < 1f)
        {
            t += Time.deltaTime * (recoverySpeed * 4f); // Très rapide
            transform.localScale = Vector3.Lerp(originalScale, squashedScale, t);
            yield return null;
        }

        t = 0f;

        // Phase 2 : Retour à la normale (ressort)
        while (t < 1f)
        {
            t += Time.deltaTime * recoverySpeed;
            transform.localScale = Vector3.Lerp(squashedScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}