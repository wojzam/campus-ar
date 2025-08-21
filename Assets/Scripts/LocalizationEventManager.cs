using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class LocalizationEventManager : MonoBehaviour
{
    public GameObject geospatialManager;
    public MonoBehaviour geospatialAnchorCreator;
    private ARAnchorManager _arAnchorManager;
    private bool _indoorActive;
    private bool _outdoorActive;

    public static LocalizationEventManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _arAnchorManager = FindObjectOfType<ARAnchorManager>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public event Action<bool> OnIndoorLocalizationStatusChanged;
    public event Action<bool> OnOutdoorLocalizationStatusChanged;

    public void TriggerIndoorLocalizationStatusChanged(bool isLocalized)
    {
        OnIndoorLocalizationStatusChanged?.Invoke(isLocalized);
        _indoorActive = isLocalized;
        UpdateTrackingMode();
    }

    public void TriggerOutdoorLocalizationStatusChanged(bool isLocalized)
    {
        OnOutdoorLocalizationStatusChanged?.Invoke(isLocalized);
        _outdoorActive = isLocalized;
        UpdateTrackingMode();
    }

    private void UpdateTrackingMode()
    {
        ShowAugmentations(_indoorActive || _outdoorActive);
        geospatialManager.SetActive(!_indoorActive);
        geospatialAnchorCreator.enabled = !_indoorActive;

        if (_indoorActive) RemoveAllGeospatialAnchors();
    }

    private void RemoveAllGeospatialAnchors()
    {
        foreach (var anchor in _arAnchorManager.trackables) Destroy(anchor.gameObject);
    }

    private void ShowAugmentations(bool show)
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var rnd in renderers) rnd.enabled = show;
    }
}