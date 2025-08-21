using System.Collections;
using System.Linq;
using UnityEngine;

public class NavigationUpdate : MonoBehaviour
{
    private const float UpdatedDelay = 0.5f;
    private const float CameraMovementThreshold = 1f;

    public CameraLinkedAgent cameraLinkedAgent;
    public NavigationPathfinder navigationPathfinder;
    public TargetManager targetManager;
    public NavigationArrows navigationArrows;

    private Material[] _arrowMaterials;
    private bool _arrowsUpdated;
    private Vector3 _lastAgentPosition = Vector3.zero;
    private Vector3 _lastCameraPosition = Vector3.zero;
    private Vector3[] _lastPathCorners;
    private LineRenderer _lineRenderer;
    private Vector3[] _pathCorners;
    private Transform _target;

    private void Start()
    {
        navigationArrows.Initialize();
        StartCoroutine(UpdateRoutine());
    }

    private IEnumerator UpdateRoutine()
    {
        while (true)
        {
            if (!_arrowsUpdated)
            {
                UpdateArrows();
                _arrowsUpdated = true;
            }

            yield return new WaitForSeconds(UpdatedDelay);
            _arrowsUpdated = false;
        }
    }

    private void UpdateArrows()
    {
        if (!targetManager.GetActiveTarget())
        {
            navigationArrows.ClearObjects();
            return;
        }

        if (_lastAgentPosition == cameraLinkedAgent.ClosestNavMeshPoint &&
            Vector3.Distance(_lastCameraPosition, cameraLinkedAgent.arCamera.position) < CameraMovementThreshold &&
            _target == targetManager.GetActiveTarget())
            return;

        _lastCameraPosition = cameraLinkedAgent.arCamera.position;
        _lastAgentPosition = cameraLinkedAgent.ClosestNavMeshPoint;
        _target = targetManager.GetActiveTarget();

        var newPathCorners = navigationPathfinder.GetPathCorners();
        if (_lastPathCorners != null && newPathCorners.SequenceEqual(_lastPathCorners))
            return;

        _pathCorners = newPathCorners;
        _lastPathCorners = (Vector3[])_pathCorners.Clone();
        navigationArrows.ClearObjects();
        navigationArrows.DrawArrows(_pathCorners);
        navigationArrows.DrawLine(_pathCorners);
    }
}