using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class InitInputUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField gridSizeInputX;
    [SerializeField] private TMP_InputField gridSizeInputY;
    [SerializeField] private TMP_InputField entitiesCountInput;
    [SerializeField] private Button startButton;
    
    public EventHandler<InitSettings> OnStartButtonClick;
    public class InitSettings : EventArgs {
        public int2 GridSize;
        public int EntitiesCount;
    }    
    public static InitInputUI Instance { get; private set; }

    private const string GridSizeXPref = "GridSizeXPref";
    private const string GridSizeYPref = "GridSizeYPref";
    private const string EntitiesCountPref = "EntitiesCountPref";
    
    private void Awake()
    {
        Instance = this;

        gridSizeInputX.text = PlayerPrefs.GetInt(GridSizeXPref, 50).ToString();
        gridSizeInputY.text = PlayerPrefs.GetInt(GridSizeYPref, 50).ToString();
        entitiesCountInput.text = PlayerPrefs.GetInt(EntitiesCountPref, 1000).ToString();
        
        startButton.onClick.AddListener(() => {
            int2 gridSize = new int2(
                Int32.Parse(gridSizeInputX.text),
                Int32.Parse(gridSizeInputY.text)
            );
            int entitiesCount = Int32.Parse(entitiesCountInput.text);
            OnStartButtonClick?.Invoke(this, new InitSettings() {
                GridSize = gridSize,
                EntitiesCount = entitiesCount
            });
            
            PlayerPrefs.SetInt(GridSizeXPref, gridSize.x);
            PlayerPrefs.SetInt(GridSizeYPref, gridSize.y);
            PlayerPrefs.SetInt(EntitiesCountPref, entitiesCount);
            
            gameObject.SetActive(false);
        });
    }
    
}
