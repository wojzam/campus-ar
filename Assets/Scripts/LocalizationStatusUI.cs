using UnityEngine;

public class LocalizationStatusUI : MonoBehaviour
{
    [SerializeField] private GameObject indoorStatusIndicator;
    [SerializeField] private GameObject outdoorStatusIndicator;
    [SerializeField] private GameObject activeText;
    [SerializeField] private GameObject inactiveText;

    private void Start()
    {
        UpdateIndoorStatus(false);
        UpdateOutdoorStatus(false);
    }

    private void OnEnable()
    {
        if (LocalizationEventManager.Instance != null)
        {
            LocalizationEventManager.Instance.OnIndoorLocalizationStatusChanged += UpdateIndoorStatus;
            LocalizationEventManager.Instance.OnOutdoorLocalizationStatusChanged += UpdateOutdoorStatus;
        }
    }

    private void OnDisable()
    {
        if (LocalizationEventManager.Instance != null)
        {
            LocalizationEventManager.Instance.OnIndoorLocalizationStatusChanged -= UpdateIndoorStatus;
            LocalizationEventManager.Instance.OnOutdoorLocalizationStatusChanged -= UpdateOutdoorStatus;
        }
    }

    private void UpdateIndoorStatus(bool isLocalized)
    {
        indoorStatusIndicator.SetActive(isLocalized);
        UpdateActiveText();
    }

    private void UpdateOutdoorStatus(bool isLocalized)
    {
        outdoorStatusIndicator.SetActive(isLocalized);
        UpdateActiveText();
    }

    private void UpdateActiveText()
    {
        var active = indoorStatusIndicator.activeSelf || outdoorStatusIndicator.activeSelf;
        activeText.SetActive(active);
        inactiveText.SetActive(!active);
    }
}