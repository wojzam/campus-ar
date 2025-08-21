using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class NavigationPathfinder : MonoBehaviour
{
    private const float MinDistance = 1.5f;
    private const float FeetOffset = 1.2f;
    private const float IntermediatePointSpacing = 1f;
    private const float PathHeightOffset = 0.5f;

    public CameraLinkedAgent cameraLinkedAgent;
    public TargetManager targetManager;

    public Vector3[] GetPathCorners()
    {
        if (!cameraLinkedAgent || !cameraLinkedAgent.arCamera || !targetManager)
            return Array.Empty<Vector3>();

        var userPosition = cameraLinkedAgent.arCamera.position;

        var destination = GetClosestTargetPosition(targetManager.GetActiveTarget());
        var pathCorners = new List<Vector3>();
        var agentPos = cameraLinkedAgent.ClosestNavMeshPoint;

        if (cameraLinkedAgent.IsOffNavMesh)
        {
            var horizontalDirection = (new Vector3(agentPos.x, userPosition.y, agentPos.z) - userPosition).normalized;
            if (horizontalDirection == Vector3.zero) horizontalDirection = Vector3.forward;
            var transitionPoint = userPosition + horizontalDirection * MinDistance;
            transitionPoint.y = userPosition.y - FeetOffset;
            pathCorners.Add(transitionPoint);
            pathCorners.AddRange(GenerateIntermediatePoints(transitionPoint, agentPos));
        }

        var navPath = new NavMeshPath();
        if (NavMesh.CalculatePath(agentPos, destination, NavMesh.AllAreas, navPath) &&
            navPath.status == NavMeshPathStatus.PathComplete)
        {
            var fullPath = new List<Vector3>(navPath.corners);
            if (fullPath.Count > 1)
            {
                var newStartPoint = fullPath[0] + (fullPath[1] - fullPath[0]).normalized * MinDistance;
                pathCorners.Add(newStartPoint);
                pathCorners.AddRange(fullPath.Skip(1));
            }
            else
            {
                pathCorners.AddRange(fullPath);
            }
        }
        else
        {
            pathCorners.Add(destination);
        }

        var finalCorners = ApplyHeightOffset(SmoothPath(pathCorners.ToArray()), PathHeightOffset);

        UpdateTargetDistance(finalCorners);

        return finalCorners.ToArray();
    }

    private void UpdateTargetDistance(List<Vector3> finalCorners)
    {
        var totalLength = 0f;
        for (var i = 0; i < finalCorners.Count - 1; i++)
            totalLength += Vector3.Distance(finalCorners[i], finalCorners[i + 1]);

        targetManager?.SetActiveTargetDistance(totalLength);
    }

    private Vector3 GetClosestTargetPosition(Transform activeTarget)
    {
        if (!activeTarget) return Vector3.zero;
        var targetGroup = activeTarget.GetComponent<TargetGroup>();
        if (!targetGroup) return activeTarget.position;

        var childTargets = activeTarget.GetComponentsInChildren<Target>();
        if (childTargets.Length == 0) return activeTarget.position;

        var closestTarget = childTargets.Select(ct => ct.transform.position)
            .OrderBy(CalculatePathLengthTo).FirstOrDefault();
        return closestTarget;
    }

    private float CalculatePathLengthTo(Vector3 position)
    {
        var path = new NavMeshPath();
        if (NavMesh.CalculatePath(cameraLinkedAgent.ClosestNavMeshPoint, position, NavMesh.AllAreas, path) &&
            path.status == NavMeshPathStatus.PathComplete)
            return path.corners.Zip(path.corners.Skip(1), Vector3.Distance).Sum();
        return float.MaxValue;
    }

    private List<Vector3> SmoothPath(Vector3[] pathCorners)
    {
        var smoothedPath = new List<Vector3>();
        for (var i = 0; i < pathCorners.Length - 1; i++)
        {
            var startPoint = pathCorners[i];
            var endPoint = pathCorners[i + 1];
            smoothedPath.Add(startPoint);

            var distance = Vector3.Distance(startPoint, endPoint);
            var numIntermediatePoints = Mathf.FloorToInt(distance / IntermediatePointSpacing);
            for (var j = 1; j <= numIntermediatePoints; j++)
            {
                var intermediatePoint = Vector3.Lerp(startPoint, endPoint, (float)j / (numIntermediatePoints + 1));
                smoothedPath.Add(NavMesh.SamplePosition(intermediatePoint, out var hit, 1.0f, NavMesh.AllAreas)
                    ? hit.position
                    : intermediatePoint);
            }
        }

        smoothedPath.Add(pathCorners[^1]);
        return smoothedPath;
    }

    private List<Vector3> ApplyHeightOffset(List<Vector3> path, float heightOffset)
    {
        return path.Select(point => new Vector3(point.x, point.y + heightOffset, point.z)).ToList();
    }

    private List<Vector3> GenerateIntermediatePoints(Vector3 start, Vector3 end)
    {
        var points = new List<Vector3>();
        var steps = Mathf.FloorToInt(Vector3.Distance(start, end) / IntermediatePointSpacing);
        for (var i = 1; i <= steps; i++) points.Add(Vector3.Lerp(start, end, (float)i / steps));
        return points;
    }
}