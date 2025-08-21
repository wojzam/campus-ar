using UnityEngine;

public class DisableRenderers : MonoBehaviour
{
    private void Start()
    {
        var meshRenderers = GetComponentsInChildren<MeshRenderer>();

        foreach (var meshRenderer in meshRenderers) meshRenderer.gameObject.SetActive(false);
    }
}