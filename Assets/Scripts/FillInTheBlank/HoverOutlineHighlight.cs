using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class HoverOutlineHighlight : MonoBehaviour
{
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private float outlineScale = 1.05f;

    private readonly List<GameObject> outlineObjects = new List<GameObject>();
    private Material outlineMaterial;
    private bool isInitialized;
    private bool isHighlighted;

    private void Awake()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        if (outlineMaterial != null)
        {
            Destroy(outlineMaterial);
            outlineMaterial = null;
        }
    }

    public void Configure(Color color, float scale)
    {
        highlightColor = color;
        outlineScale = scale;

        if (isInitialized)
        {
            RebuildOutline();
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        Initialize();

        if (isHighlighted == highlighted) return;

        isHighlighted = highlighted;

        for (int i = 0; i < outlineObjects.Count; i++)
        {
            if (outlineObjects[i] != null)
            {
                outlineObjects[i].SetActive(highlighted);
            }
        }
    }

    private void Initialize()
    {
        if (isInitialized) return;

        isInitialized = true;
        RebuildOutline();
    }

    private void RebuildOutline()
    {
        ClearOutlineObjects();
        CreateOutlineForRenderers();
        SetHighlighted(isHighlighted);
    }

    private void ClearOutlineObjects()
    {
        for (int i = 0; i < outlineObjects.Count; i++)
        {
            if (outlineObjects[i] != null)
            {
                Destroy(outlineObjects[i]);
            }
        }

        outlineObjects.Clear();
    }

    private void CreateOutlineForRenderers()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            if (renderer == null) continue;
            if (renderer.gameObject.name.StartsWith("__Outline")) continue;

            if (renderer is MeshRenderer meshRenderer)
            {
                CreateMeshOutline(meshRenderer);
            }
            else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                CreateSkinnedOutline(skinnedMeshRenderer);
            }
        }
    }

    private void CreateMeshOutline(MeshRenderer sourceRenderer)
    {
        MeshFilter meshFilter = sourceRenderer.GetComponent<MeshFilter>();

        if (meshFilter == null || meshFilter.sharedMesh == null) return;

        GameObject outlineObject = new GameObject("__Outline_" + sourceRenderer.gameObject.name);
        outlineObject.transform.SetParent(sourceRenderer.transform, false);
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localRotation = Quaternion.identity;
        outlineObject.transform.localScale = Vector3.one * outlineScale;

        MeshFilter outlineFilter = outlineObject.AddComponent<MeshFilter>();
        outlineFilter.sharedMesh = meshFilter.sharedMesh;

        MeshRenderer outlineRenderer = outlineObject.AddComponent<MeshRenderer>();
        outlineRenderer.sharedMaterial = GetOutlineMaterial();
        outlineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        outlineRenderer.receiveShadows = false;

        outlineObject.SetActive(false);
        outlineObjects.Add(outlineObject);
    }

    private void CreateSkinnedOutline(SkinnedMeshRenderer sourceRenderer)
    {
        if (sourceRenderer.sharedMesh == null) return;

        GameObject outlineObject = new GameObject("__Outline_" + sourceRenderer.gameObject.name);
        outlineObject.transform.SetParent(sourceRenderer.transform, false);
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localRotation = Quaternion.identity;
        outlineObject.transform.localScale = Vector3.one * outlineScale;

        SkinnedMeshRenderer outlineRenderer = outlineObject.AddComponent<SkinnedMeshRenderer>();
        outlineRenderer.sharedMesh = sourceRenderer.sharedMesh;
        outlineRenderer.bones = sourceRenderer.bones;
        outlineRenderer.rootBone = sourceRenderer.rootBone;
        outlineRenderer.sharedMaterial = GetOutlineMaterial();
        outlineRenderer.updateWhenOffscreen = sourceRenderer.updateWhenOffscreen;
        outlineRenderer.quality = sourceRenderer.quality;
        outlineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        outlineRenderer.receiveShadows = false;

        outlineObject.SetActive(false);
        outlineObjects.Add(outlineObject);
    }

    private Material GetOutlineMaterial()
    {
        if (outlineMaterial != null) return outlineMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Standard");

        outlineMaterial = new Material(shader);
        ApplyColor(outlineMaterial, highlightColor);
        outlineMaterial.name = name + "_OutlineMaterial";

        return outlineMaterial;
    }

    private static void ApplyColor(Material material, Color color)
    {
        if (material == null) return;

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", color);

        material.color = color;
    }
}