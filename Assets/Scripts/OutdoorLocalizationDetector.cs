using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class OutdoorLocalizationDetector : MonoBehaviour
{
    [SerializeField] private AREarthManager arEarthManager;
    private bool _isLocalized;

    private void Update()
    {
        if (ARSession.state != ARSessionState.SessionTracking || Input.location.status != LocationServiceStatus.Running)
        {
            UpdateLocalizationStatus(false);
            return;
        }

        UpdateLocalizationStatus(arEarthManager.EarthTrackingState == TrackingState.Tracking);
    }

    private void UpdateLocalizationStatus(bool localized)
    {
        if (_isLocalized != localized)
        {
            _isLocalized = localized;
            LocalizationEventManager.Instance.TriggerOutdoorLocalizationStatusChanged(_isLocalized);
        }
    }
}