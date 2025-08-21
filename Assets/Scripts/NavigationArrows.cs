using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NavigationArrows : MonoBehaviour
{
    private const float FadeIncrement = 0.2f;
    private const float LineHeightOffset = -0.2f;
    private const float LineWidth = 0.2f;
    private const float ArrowSpacing = 0.7f;
    private const int MaxArrows = 30;
    private const float LineLengthLimit = 20f;

    public GameObject arrowPrefab;
    public Material arrowMaterial;
    public Material lineMaterial;

    private readonly List<GameObject> _arrowInstances = new();
    private Material[] _arrowMaterials;
    private LineRenderer _lineRenderer;

    public void Initialize()
    {
        InitializeArrowMaterials();
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.startWidth = LineWidth;
        _lineRenderer.endWidth = LineWidth;
        _lineRenderer.material = lineMaterial;
    }

    private void InitializeArrowMaterials()
    {
        _arrowMaterials = new Material[MaxArrows];
        for (var i = 0; i < MaxArrows; i++)
        {
            var alpha = Mathf.Clamp01((i + 1) * FadeIncrement);
            var mat = new Material(arrowMaterial);
            var color = mat.color;
            color.a = alpha;
            mat.color = color;
            _arrowMaterials[i] = mat;
        }
    }

    public void ClearObjects()
    {
        foreach (var arrow in _arrowInstances.Where(arrow => arrow))
            Destroy(arrow);
        _arrowInstances.Clear();
        if (_lineRenderer)
            _lineRenderer.positionCount = 0;
    }

    public void DrawArrows(Vector3[] pathCorners)
    {
        if (pathCorners == null || pathCorners.Length < 2)
            return;

        var accumulatedDistance = 0f;
        var nextArrowDistance = 0f;
        var arrowCount = 0;

        for (var i = 0; i < pathCorners.Length - 1 && arrowCount < MaxArrows; i++)
        {
            var start = pathCorners[i];
            var end = pathCorners[i + 1];
            var segmentDistance = Vector3.Distance(start, end);

            while (nextArrowDistance <= accumulatedDistance + segmentDistance && arrowCount < MaxArrows)
            {
                var t = (nextArrowDistance - accumulatedDistance) / segmentDistance;
                var arrowPosition = Vector3.Lerp(start, end, t);
                var arrowRotation = Quaternion.LookRotation(end - start);

                var arrowObj = Instantiate(arrowPrefab, arrowPosition, arrowRotation, transform);
                _arrowInstances.Add(arrowObj);

                if (arrowCount < _arrowMaterials.Length)
                {
                    var rend = arrowObj.GetComponentInChildren<Renderer>();
                    if (rend)
                        rend.material = _arrowMaterials[arrowCount];
                }

                arrowCount++;
                nextArrowDistance += ArrowSpacing;
            }

            accumulatedDistance += segmentDistance;
        }
    }

    public void DrawLine(Vector3[] pathCorners)
    {
        if (!_lineRenderer || pathCorners == null || pathCorners.Length < 2)
            return;

        var linePoints = new List<Vector3>();
        var accumulatedDistance = 0f;

        for (var i = 0; i < pathCorners.Length - 1; i++)
        {
            var start = pathCorners[i];
            var end = pathCorners[i + 1];
            var segmentDistance = Vector3.Distance(start, end);

            if (accumulatedDistance + segmentDistance > LineLengthLimit)
            {
                var remainingDistance = LineLengthLimit - accumulatedDistance;
                var t = remainingDistance / segmentDistance;
                var limitedEnd = Vector3.Lerp(start, end, t);

                limitedEnd.y += LineHeightOffset;
                linePoints.Add(limitedEnd);
                break;
            }

            start.y += LineHeightOffset;
            linePoints.Add(start);
            accumulatedDistance += segmentDistance;
        }

        _lineRenderer.positionCount = linePoints.Count;
        _lineRenderer.SetPositions(linePoints.ToArray());
    }
}