using UnityEngine;
using UnityEngine.AI;

public class CameraLinkedAgent : MonoBehaviour
{
    private const float AgentUpdateCameraMovementThreshold = 1f;
    private const float RaycastDistance = 3f;
    private const float NavMeshCheckRadius = 5f;
    public Transform arCamera;
    private NavMeshAgent _agent;
    private Vector3 _lastCameraPosition;
    public bool IsOffNavMesh { get; private set; }
    public Vector3 ClosestNavMeshPoint { get; private set; }

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        ForcedUpdate(arCamera.position);
    }

    private void Update()
    {
        var cameraPosition = arCamera.position;
        if (Vector3.Distance(cameraPosition, _lastCameraPosition) < AgentUpdateCameraMovementThreshold)
            return;

        ForcedUpdate(cameraPosition);
    }

    private void ForcedUpdate(Vector3 cameraPosition)
    {
        _lastCameraPosition = cameraPosition;
        if (Physics.Raycast(cameraPosition, Vector3.down, out var hit, RaycastDistance))
            cameraPosition.y = hit.point.y;

        if (NavMesh.SamplePosition(cameraPosition, out var navHit, NavMeshCheckRadius, NavMesh.AllAreas))
        {
            IsOffNavMesh = false;
            ClosestNavMeshPoint = navHit.position;
        }
        else
        {
            IsOffNavMesh = true;
            ClosestNavMeshPoint = FindClosestNavMeshPoint(cameraPosition);
        }

        _agent.Warp(ClosestNavMeshPoint);
    }

    private Vector3 FindClosestNavMeshPoint(Vector3 position)
    {
        return NavMesh.SamplePosition(position, out var hit, Mathf.Infinity, NavMesh.AllAreas)
            ? hit.position
            : position;
    }
}