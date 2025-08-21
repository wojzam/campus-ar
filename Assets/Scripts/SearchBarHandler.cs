using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class SearchBarHandler : MonoBehaviour
{
    [Header("UI References")] public TMP_InputField searchInput;

    public Transform scrollView;
    public Transform listContent;
    public GameObject targetItemPrefab;

    [Header("Dependencies")] public TargetManager targetManager;

    private readonly List<GameObject> _currentListItems = new();

    private readonly Dictionary<char, char> _polishToAsciiMap = new()
    {
        { 'ó', 'o' }, { 'ą', 'a' }, { 'ę', 'e' }, { 'ś', 's' },
        { 'ł', 'l' }, { 'ż', 'z' }, { 'ź', 'z' }, { 'ć', 'c' },
        { 'ń', 'n' }
    };

    private List<(Transform target, string processedName)> _processedTargets;

    private void Start()
    {
        searchInput.onValueChanged.AddListener(OnSearchValueChanged);
        InitializeProcessedTargets();
        ClearCurrentListItems();
    }

    private void InitializeProcessedTargets()
    {
        _processedTargets = targetManager
            .GetAvailableTargets()
            .Select(target => (
                target,
                processedName: ProcessString(target.name.ToLower())
            ))
            .ToList();
    }

    private void PopulateTargetList(List<Transform> targets)
    {
        ClearCurrentListItems();
        scrollView.gameObject.SetActive(targets.Count > 0);

        foreach (var target in targets)
        {
            var item = Instantiate(targetItemPrefab, listContent);
            var button = item.GetComponent<Button>();
            var text = item.GetComponentInChildren<TMP_Text>();

            text.text = target.name;
            button.onClick.AddListener(() => OnTargetSelected(target));

            _currentListItems.Add(item);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(listContent.GetComponent<RectTransform>());
    }

    private void ClearCurrentListItems()
    {
        foreach (var item in _currentListItems) Destroy(item);
        _currentListItems.Clear();
    }

    private void OnTargetSelected(Transform target)
    {
        targetManager.SetActiveTarget(target);
        ClearCurrentListItems();
        searchInput.text = "";
    }

    private void OnSearchValueChanged(string searchText)
    {
        var processedSearchText = ProcessString(searchText.ToLower());

        if (string.IsNullOrEmpty(processedSearchText))
        {
            ClearCurrentListItems();
            scrollView.gameObject.SetActive(false);
            return;
        }

        var filteredTargets = _processedTargets
            .Where(pt => pt.processedName.Contains(processedSearchText))
            .Select(pt => pt.target)
            .ToList();

        PopulateTargetList(filteredTargets);
    }

    private string ProcessString(string input)
    {
        var normalizedString = input.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var character in normalizedString)
            if (_polishToAsciiMap.TryGetValue(character, out var value))
                stringBuilder.Append(value);
            else if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(character);

        return Regex.Replace(stringBuilder.ToString(), "[^a-z0-9]", "");
    }
}