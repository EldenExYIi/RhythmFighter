using UnityEngine;
using UnityEngine.UI;
using TMPro;  // 如果没有 TMPro，改成 using UnityEngine.UI; 并把 TextMeshProUGUI 换成 Text

public class SettingsPanelUI : MonoBehaviour
{
    public Slider speedSlider;
    public Slider timeOffsetSlider;
    public Slider volumeSlider;
    public TextMeshProUGUI speedValueText;
    public TextMeshProUGUI timeOffsetValueText;
    public TextMeshProUGUI volumeValueText;
    public Button closeButton;

    public GameSettings settings;

    private RhythmManager rhythmManager;

    void Start()
    {
        rhythmManager = FindObjectOfType<RhythmManager>();
        if (rhythmManager == null)
            Debug.LogError("没有找到 RhythmManager");

        settings.Load();

        speedSlider.minValue = 0.5f;
        speedSlider.maxValue = 3.0f;
        timeOffsetSlider.minValue = -200f;
        timeOffsetSlider.maxValue = 200f;
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;

        speedSlider.value = settings.noteSpeed;
        timeOffsetSlider.value = settings.spawnOffsetY;
        volumeSlider.value = settings.masterVolume;

        UpdateValueTexts();

        speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        timeOffsetSlider.onValueChanged.AddListener(OnTimeOffsetChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        closeButton.onClick.AddListener(ClosePanel);

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameObject.activeSelf)
                ClosePanel();
            else
                OpenPanel();
        }
    }

    void OpenPanel()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        if (rhythmManager != null)
        {
            rhythmManager.PauseMusic();
        }
    }

    void ClosePanel()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        if(rhythmManager!= null)
        {
            rhythmManager.ResumeMusic();
        }
        settings.Save();
    }

    void OnSpeedChanged(float value)
    {
        settings.noteSpeed = value;
        UpdateValueTexts();
        if (rhythmManager != null)
            rhythmManager.SetUserSpeedMultiplier(value);
    }

    void OnTimeOffsetChanged(float value)
    {
        int intVal = Mathf.RoundToInt(value);
        settings.spawnOffsetY = intVal;
        UpdateValueTexts();
        if (rhythmManager != null)
            rhythmManager.SetTimeOffset(intVal);
    }

    void OnVolumeChanged(float value)
    {
        settings.masterVolume = value;
        UpdateValueTexts();
        AudioListener.volume = value;
    }

    void UpdateValueTexts()
    {
        speedValueText.text = $"{settings.noteSpeed:F2}";
        timeOffsetValueText.text = $"{settings.spawnOffsetY} ms";
        volumeValueText.text = $"{settings.masterVolume:F2}";
    }
}