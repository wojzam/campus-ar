using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public GameObject targetPanel;
    public TextMeshProUGUI targetText;
    public TextMeshProUGUI targetDistanceText;
    private readonly List<Transform> _availableTargets = new();
    private Transform _activeTarget;
    private float _activeTargetDistance;

    private void Start()
    {
        LoadChildTargets();
        UpdateTargetVisibility();
    }

    private void OnEnable()
    {
        Target.OnTargetReached += HandleTargetReached;
    }

    private void OnDisable()
    {
        Target.OnTargetReached -= HandleTargetReached;
    }

    public void SetActiveTargetDistance(float distance)
    {
        _activeTargetDistance = distance;
        UpdateTargetText();
    }

    private void HandleTargetReached(Target target)
    {
        ClearActiveTarget();
    }

    private void LoadChildTargets()
    {
        _availableTargets.Clear();
        foreach (Transform child in transform)
            if (child.GetComponent<TargetGroup>() != null || child.GetComponent<Target>() != null)
                _availableTargets.Add(child);
    }

    public List<Transform> GetAvailableTargets()
    {
        return new List<Transform>(_availableTargets);
    }

    public void SetActiveTarget(Transform target)
    {
        if (_availableTargets.Contains(target))
        {
            _activeTarget = target;
            UpdateTargetVisibility();
            UpdateTargetText();
        }
    }

    public void ClearActiveTarget()
    {
        _activeTarget = null;
        UpdateTargetVisibility();
        UpdateTargetText();
    }

    private void UpdateTargetText()
    {
        if (!_activeTarget)
        {
            targetPanel.SetActive(false);
        }
        else
        {
            targetPanel.SetActive(true);
            if (_activeTarget.GetComponent<TargetGroup>() != null)
                targetText.text = "Cel: " + _activeTarget.name;
            else
                targetText.text = "Cel: " + _activeTarget.name;
            targetDistanceText.text = $"Odległość: {Mathf.RoundToInt(_activeTargetDistance)} m";
        }
    }

    public Transform GetActiveTarget()
    {
        return _activeTarget;
    }

    private void UpdateTargetVisibility()
    {
        foreach (var target in _availableTargets)
        {
            var shouldBeVisible = target == _activeTarget ||
                                  (_activeTarget != null && _activeTarget.GetComponent<TargetGroup>() != null &&
                                   _activeTarget.IsChildOf(target));
            target.gameObject.SetActive(shouldBeVisible);
        }
    }
}