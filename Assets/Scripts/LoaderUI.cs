using System;
using TMPro;
using UnityEngine;

public class LoaderUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI loaderText;

    private float _timer;
    private const float Step = 0.3f;
    private const int StepsCount = 3;
    private const string StepChar = ".";
    
    public static LoaderUI Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    private void Start() {
        Hide();
    }

    private void Update() {
        _timer += Time.deltaTime;
        if (_timer < Step) {
            return;
        }

        _timer = 0f;
        if (loaderText.text.Length >= StepsCount) {
            loaderText.text = StepChar;
        }
        else {
            loaderText.text += StepChar;
        }
    }
}
