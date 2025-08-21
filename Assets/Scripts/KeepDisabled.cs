using UnityEngine;

public class KeepDisabled : MonoBehaviour
{
    private void Update()
    {
        gameObject.SetActive(false);
    }
}