using UnityEngine;

public class AssignOcclusionMaterial : MonoBehaviour
{
    public Material occlusionMaterial;

    private void Start()
    {
        var meshRenderers = GetComponentsInChildren<MeshRenderer>();

        foreach (var meshRenderer in meshRenderers) meshRenderer.material = occlusionMaterial;
    }
}