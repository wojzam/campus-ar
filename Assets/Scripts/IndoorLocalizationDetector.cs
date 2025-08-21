using System.Linq;
using UnityEngine;
using Vuforia;

public class IndoorLocalizationDetector : MonoBehaviour
{
    private AreaTargetBehaviour[] _areaTargets;
    private bool _isLocalized;

    private void Start()
    {
        _areaTargets = FindObjectsOfType<AreaTargetBehaviour>();
    }

    private void Update()
    {
        var currentLocalizationStatus = CheckIfAnyAreaTargetIsTracked();
        if (currentLocalizationStatus == _isLocalized) return;

        _isLocalized = currentLocalizationStatus;
        LocalizationEventManager.Instance.TriggerIndoorLocalizationStatusChanged(_isLocalized);
    }

    private bool CheckIfAnyAreaTargetIsTracked()
    {
        return _areaTargets.Select(areaTarget => areaTarget.TargetStatus.Status).Any(status =>
            status is Status.TRACKED or Status.EXTENDED_TRACKED or Status.LIMITED);
    }
}