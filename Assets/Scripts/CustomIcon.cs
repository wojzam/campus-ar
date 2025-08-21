using UnityEngine;

[ExecuteInEditMode]
public class CustomIcon : MonoBehaviour
{
    public string iconName;

    private void OnDrawGizmos()
    {
        if (!string.IsNullOrEmpty(iconName)) Gizmos.DrawIcon(transform.position, iconName, true);
    }
}