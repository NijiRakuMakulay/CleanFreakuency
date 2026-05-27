using UnityEngine;

public class ObjectHighlight : MonoBehaviour
{
    public Material glowMaterial;

    private Material originalMaterial;

    private Renderer objectRenderer;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
        }
    }

    public void Highlight()
    {
        if (
            objectRenderer != null &&
            objectRenderer.material != glowMaterial
        )
        {
            objectRenderer.material = glowMaterial;
        }
    }

    public void RemoveHighlight()
    {
        if (
            objectRenderer != null &&
            objectRenderer.material != originalMaterial
        )
        {
            objectRenderer.material = originalMaterial;
        }
    }
}