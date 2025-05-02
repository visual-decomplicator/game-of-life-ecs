using System;
using System.Globalization;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class InitInputUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField gridSizeInputX;
    [SerializeField] private TMP_InputField gridSizeInputY;
    [SerializeField] private TMP_InputField entitiesCountInput;
    [SerializeField] private TMP_InputField stepDelayInput;
    [SerializeField] private Button startButton;
    
    public EventHandler<InitSettings> OnStartButtonClick;
    public class InitSettings : EventArgs {
        public int2 GridSize;
        public int EntitiesCount;
        public float StepDelay;
    }    
    public static InitInputUI Instance { get; private set; }

    private const string GridSizeXPref = "GridSizeXPref";
    private const string GridSizeYPref = "GridSizeYPref";
    private const string EntitiesCountPref = "EntitiesCountPref";
    private const string StepDelayPref = "StepDelayPref";
    
    private void Awake()
    {
        Instance = this;

        gridSizeInputX.text = PlayerPrefs.GetInt(GridSizeXPref, 50).ToString();
        gridSizeInputY.text = PlayerPrefs.GetInt(GridSizeYPref, 50).ToString();
        entitiesCountInput.text = PlayerPrefs.GetInt(EntitiesCountPref, 1000).ToString();
        stepDelayInput.text = string.Format("{0:N1}", PlayerPrefs.GetFloat(StepDelayPref, 0.2f));  
        
        startButton.onClick.AddListener(() => {
            int2 gridSize = new int2(
                Int32.Parse(gridSizeInputX.text),
                Int32.Parse(gridSizeInputY.text)
            );
            int entitiesCount = Int32.Parse(entitiesCountInput.text);
            float stepDelay = float.Parse(stepDelayInput.text, CultureInfo.InvariantCulture.NumberFormat);
            OnStartButtonClick?.Invoke(this, new InitSettings() {
                GridSize = gridSize,
                EntitiesCount = entitiesCount,
                StepDelay = stepDelay
            });
            
            PlayerPrefs.SetInt(GridSizeXPref, gridSize.x);
            PlayerPrefs.SetInt(GridSizeYPref, gridSize.y);
            PlayerPrefs.SetInt(EntitiesCountPref, entitiesCount);
            PlayerPrefs.SetFloat(StepDelayPref, stepDelay);
            
            LoaderUI.Instance.Show();
            gameObject.SetActive(false);
        });
    }
    
}
