using TMPro;
using UnityEngine;

public class CountersUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI hiddenEntitiesText;
    [SerializeField] private TextMeshProUGUI visibleEntitiesText;
    [SerializeField] private TextMeshProUGUI totalEntitiesText;
    [SerializeField] private TextMeshProUGUI fpsText;
    
    private int _hiddenEntitiesCount;
    private int _visibleEntitiesCount;
    
    public static CountersUI Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }

    public void SetHiddenEntitiesCount(int count) {
        _hiddenEntitiesCount = count;
        hiddenEntitiesText.SetText(count.ToString());
        SetTotalEntitiesCount();
    }
    
    public void SetVisibleEntitiesCount(int count) {
        _visibleEntitiesCount = count;
        visibleEntitiesText.SetText(count.ToString());
        SetTotalEntitiesCount();
    }

    private void SetTotalEntitiesCount() {
        int sum = _hiddenEntitiesCount + _visibleEntitiesCount;
        totalEntitiesText.SetText(sum.ToString());
    }
    
    public void SetFps(int fps) {
        fpsText.SetText(fps.ToString());
    }
}
