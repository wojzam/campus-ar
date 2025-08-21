using UnityEngine;

public class Target : MonoBehaviour
{
    public delegate void TargetReachedHandler(Target target);

    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
        var collider = GetComponent<Collider>();
        collider.isTrigger = true;
    }

    private void Update()
    {
        RotateTowardsCamera();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera")) NotifyTargetReached();
    }

    public static event TargetReachedHandler OnTargetReached;

    private void RotateTowardsCamera()
    {
        if (!_mainCamera) return;

        var targetPosition = transform.position;
        var cameraPosition = _mainCamera.transform.position;

        var direction = cameraPosition - targetPosition;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.001f)
        {
            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }

    private void NotifyTargetReached()
    {
        OnTargetReached?.Invoke(this);
    }
}