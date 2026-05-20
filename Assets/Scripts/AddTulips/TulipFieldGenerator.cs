using System.Collections;
using UnityEngine;

public class TulipFieldGenerator : MonoBehaviour
{
    [Header("Paramètres du Poème")]
    [Tooltip("La durée totale du poème en secondes.")]
    public float poemDuration = 60f; 
    
    [Header("Paramètres des Tulipes")]
    [Tooltip("Le modèle 3D de la tulipe (Prefab).")]
    public GameObject tulipPrefab;
    
    [Tooltip("Le nombre total de tulipes qui vont apparaitre.")]
    public int totalTulips = 100;
    
    [Tooltip("Le rayon de la zone où les tulipes vont pousser (autour de cet objet).")]
    public float spawnRadius = 10f;
    
    [Tooltip("Le temps qu'il faut à une seule tulipe pour atteindre sa taille finale.")]
    public float timeToGrow = 2f;

    [Header("Variations Visuelles")]
    public float minScale = 0.8f;
    public float maxScale = 1.2f;

    [Header("Comportement")]
    [Tooltip("Si vrai, le champ commence automatiquement au Start() de la scène.")]
    public bool autoStart = true;

    // Indique si la génération est en cours pour éviter les doubles démarrages
    private bool isGrowing = false;

    [Header("Variantes de Tulipes")]
    [Tooltip("Prefab alternatif pour une tulipe d'une autre couleur (optionnel).")]
    public GameObject tulipPrefabAlt;

    [Tooltip("Probabilité (0-1) d'utiliser le prefab alternatif lors du spawn.")]
    [Range(0f, 1f)]
    public float altPrefabChance = 0.5f;

    // Fonction à appeler pour lancer l'apparition (quand l'audio commence par exemple)
    public void StartGrowingField()
    {
        if (isGrowing) return;


        if (tulipPrefab == null && tulipPrefabAlt == null)
        {
            Debug.LogWarning("TulipFieldGenerator: aucun prefab de tulipe n'est assigné (tulipPrefab et tulipPrefabAlt sont vides).");
            return;
        }

        if (totalTulips <= 0 || poemDuration <= 0f)
        {
            Debug.LogWarning("TulipFieldGenerator: 'totalTulips' et 'poemDuration' doivent être > 0.");
            return;
        }

        isGrowing = true;
        StartCoroutine(SpawnTulipsOverTime());
    }

    private void Start()
    {
        if (autoStart)
            StartGrowingField();
    }

    private IEnumerator SpawnTulipsOverTime()
    {
        // Calcul du délai entre l'apparition de chaque tulipe
        float delayBetweenSpawns = poemDuration / totalTulips;

        for (int i = 0; i < totalTulips; i++)
        {
            SpawnSingleTulip();
            
            // On attend avant de faire apparaitre la suivante
            yield return new WaitForSeconds(delayBetweenSpawns);
        }

        // Marquer la génération comme terminée
        isGrowing = false;
    }

    private void SpawnSingleTulip()
    {
        // 1. Déterminer une position aléatoire dans un cercle
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        // Optionnel mais recommandé : Faire un Raycast vers le bas pour coller la tulipe au sol (si le terrain n'est pas plat)
        RaycastHit hit;
        if (Physics.Raycast(spawnPosition + Vector3.up * 5f, Vector3.down, out hit, 10f))
        {
            spawnPosition.y = hit.point.y;
        }

        // 2. Déterminer une rotation aléatoire (pour que les tulipes ne regardent pas toutes dans la même direction)
        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // Choisir quel prefab utiliser (préfab alternativ si disponible selon la probabilité)
        GameObject prefabToUse = tulipPrefab;
        if (tulipPrefabAlt != null)
        {
            prefabToUse = (Random.value < altPrefabChance) ? tulipPrefabAlt : (tulipPrefab ?? tulipPrefabAlt);
        }

        // 3. Instancier la tulipe
        if (prefabToUse == null)
        {
            Debug.LogWarning("TulipFieldGenerator: aucun prefab valide trouvé au moment du spawn.");
            return;
        }

        GameObject newTulip = Instantiate(prefabToUse, spawnPosition, randomRotation);

        // 4. Déterminer une taille finale aléatoire pour plus de réalisme
        float finalScale = Random.Range(minScale, maxScale);
        Vector3 targetScale = new Vector3(finalScale, finalScale, finalScale);

        // 5. Lancer l'animation de pousse pour cette tulipe spécifique
        StartCoroutine(GrowTulip(newTulip.transform, targetScale));
    }

    private IEnumerator GrowTulip(Transform tulipTransform, Vector3 targetScale)
    {
        float currentTime = 0f;
        
        // La tulipe commence à la taille 0 (invisible)
        tulipTransform.localScale = Vector3.zero;

        while (currentTime < timeToGrow)
        {
            currentTime += Time.deltaTime;
            
            // Lerp permet une transition fluide entre la taille 0 et la taille cible
            float progress = currentTime / timeToGrow;
            tulipTransform.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);
            
            yield return null; // Attendre la frame suivante
        }

        // S'assurer que la taille finale est exacte à la fin
        tulipTransform.localScale = targetScale;
    }

    // --- Utilitaires ---
    // Permet de visualiser la zone d'apparition dans l'éditeur Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}