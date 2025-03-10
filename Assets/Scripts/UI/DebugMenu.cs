using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject debugPanel;
    public Toggle developmentModeToggle;
    public Toggle debugUIToggle;
    public Toggle handTrackingDebugToggle;
    public Toggle hudDebugToggle;
    public Toggle performanceMetricsToggle;
    
    [Header("Quick Actions")]
    public Button spawnTestTasksButton;
    public Button startTestPomodoroButton;
    public Button clearAllDataButton;
    
    [Header("Performance Display")]
    public TextMeshProUGUI fpsText;
    public TextMeshProUGUI memoryText;
    
    [Header("ADHD Testing")]
    public Toggle focusTestingToggle;
    public Slider focusTestDurationSlider;
    public Toggle overstimulationTestToggle;
    public Slider overstimulationIntensitySlider;
    public Button spawnOverflowTasksButton;
    public Toggle uiStressTestToggle;
    public Slider uiUpdateFrequencySlider;
    public Toggle colorContrastTestToggle;
    public Toggle motionSensitivityTestToggle;
    
    [Header("Focus Metrics")]
    public TextMeshProUGUI focusLevelText;
    public TextMeshProUGUI overstimulationText;
    
    private DevelopmentSettings devSettings;
    private float updateInterval = 0.5f;
    private float timeSinceLastUpdate = 0f;
    
    private void Start()
    {
        devSettings = DevelopmentSettings.Instance;
        if (devSettings == null)
        {
            Debug.LogError("DevelopmentSettings not found in scene!");
            return;
        }
        
        SetupUI();
        UpdateUIState();
    }
    
    private void Update()
    {
        if (!devSettings.isDevelopmentMode) return;
        
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= updateInterval)
        {
            UpdatePerformanceDisplay();
            UpdateFocusMetrics();
            timeSinceLastUpdate = 0f;
        }
    }
    
    private void SetupUI()
    {
        if (developmentModeToggle != null)
        {
            developmentModeToggle.onValueChanged.AddListener((value) => {
                devSettings.ToggleDevelopmentMode();
                UpdateUIState();
            });
        }
        
        if (debugUIToggle != null)
        {
            debugUIToggle.onValueChanged.AddListener((value) => {
                devSettings.ToggleDebugUI();
                UpdateUIState();
            });
        }
        
        if (handTrackingDebugToggle != null)
        {
            handTrackingDebugToggle.onValueChanged.AddListener((value) => {
                devSettings.ToggleHandTrackingDebug();
                UpdateUIState();
            });
        }
        
        if (hudDebugToggle != null)
        {
            hudDebugToggle.onValueChanged.AddListener((value) => {
                devSettings.ToggleHUDDebugMode();
                UpdateUIState();
            });
        }
        
        if (spawnTestTasksButton != null)
        {
            spawnTestTasksButton.onClick.AddListener(() => {
                var taskManager = FindObjectOfType<TaskManager>();
                if (taskManager != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        taskManager.AddTask($"Quick Test Task {i + 1}", "Quick test task");
                    }
                }
            });
        }
        
        if (startTestPomodoroButton != null)
        {
            startTestPomodoroButton.onClick.AddListener(() => {
                var pomodoroManager = FindObjectOfType<PomodoroManager>();
                if (pomodoroManager != null)
                {
                    pomodoroManager.StartTimer(25f);
                }
            });
        }
        
        if (clearAllDataButton != null)
        {
            clearAllDataButton.onClick.AddListener(() => {
                var dataManager = DataManager.Instance;
                if (dataManager != null)
                {
                    dataManager.ClearAllData();
                    Debug.Log("All data cleared!");
                }
            });
        }
        
        if (focusTestingToggle != null)
        {
            focusTestingToggle.onValueChanged.AddListener((value) => {
                devSettings.ToggleFocusTesting();
                UpdateUIState();
            });
        }
        
        if (focusTestDurationSlider != null)
        {
            focusTestDurationSlider.onValueChanged.AddListener((value) => {
                devSettings.focusTestDuration = value;
            });
        }
        
        if (overstimulationTestToggle != null)
        {
            overstimulationTestToggle.onValueChanged.AddListener((value) => {
                devSettings.ToggleOverstimulationTest();
                UpdateUIState();
            });
        }
        
        if (overstimulationIntensitySlider != null)
        {
            overstimulationIntensitySlider.onValueChanged.AddListener((value) => {
                devSettings.overstimulationIntensity = value;
            });
        }
        
        if (spawnOverflowTasksButton != null)
        {
            spawnOverflowTasksButton.onClick.AddListener(() => {
                devSettings.SpawnOverflowTasks();
            });
        }
        
        if (uiStressTestToggle != null)
        {
            uiStressTestToggle.onValueChanged.AddListener((value) => {
                devSettings.ToggleUIStressTest();
                UpdateUIState();
            });
        }
        
        if (uiUpdateFrequencySlider != null)
        {
            uiUpdateFrequencySlider.onValueChanged.AddListener((value) => {
                devSettings.uiUpdateFrequency = value;
            });
        }
        
        if (colorContrastTestToggle != null)
        {
            colorContrastTestToggle.onValueChanged.AddListener((value) => {
                devSettings.enableColorContrastTest = value;
                UpdateUIState();
            });
        }
        
        if (motionSensitivityTestToggle != null)
        {
            motionSensitivityTestToggle.onValueChanged.AddListener((value) => {
                devSettings.enableMotionSensitivityTest = value;
                UpdateUIState();
            });
        }
    }
    
    private void UpdateUIState()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(devSettings.isDevelopmentMode);
        }
        
        if (developmentModeToggle != null)
        {
            developmentModeToggle.isOn = devSettings.isDevelopmentMode;
        }
        
        if (debugUIToggle != null)
        {
            debugUIToggle.isOn = devSettings.showDebugUI;
        }
        
        if (handTrackingDebugToggle != null)
        {
            handTrackingDebugToggle.isOn = devSettings.enableHandTrackingDebug;
        }
        
        if (hudDebugToggle != null)
        {
            hudDebugToggle.isOn = devSettings.enableHUDDebugMode;
        }
        
        if (performanceMetricsToggle != null)
        {
            performanceMetricsToggle.isOn = devSettings.enablePerformanceMetrics;
        }
        
        if (focusTestingToggle != null)
        {
            focusTestingToggle.isOn = devSettings.enableFocusTesting;
        }
        
        if (focusTestDurationSlider != null)
        {
            focusTestDurationSlider.value = devSettings.focusTestDuration;
        }
        
        if (overstimulationTestToggle != null)
        {
            overstimulationTestToggle.isOn = devSettings.enableOverstimulationTest;
        }
        
        if (overstimulationIntensitySlider != null)
        {
            overstimulationIntensitySlider.value = devSettings.overstimulationIntensity;
        }
        
        if (uiStressTestToggle != null)
        {
            uiStressTestToggle.isOn = devSettings.enableUIStressTest;
        }
        
        if (uiUpdateFrequencySlider != null)
        {
            uiUpdateFrequencySlider.value = devSettings.uiUpdateFrequency;
        }
        
        if (colorContrastTestToggle != null)
        {
            colorContrastTestToggle.isOn = devSettings.enableColorContrastTest;
        }
        
        if (motionSensitivityTestToggle != null)
        {
            motionSensitivityTestToggle.isOn = devSettings.enableMotionSensitivityTest;
        }
    }
    
    private void UpdatePerformanceDisplay()
    {
        if (fpsText != null)
        {
            float fps = 1.0f / Time.deltaTime;
            fpsText.text = $"FPS: {fps:F1}";
        }
        
        if (memoryText != null)
        {
            float memoryMB = SystemInfo.systemMemorySize;
            memoryText.text = $"Memory: {memoryMB:F1} MB";
        }
    }
    
    private void UpdateFocusMetrics()
    {
        if (focusLevelText != null && devSettings.enableFocusTesting)
        {
            float focusLevel = Mathf.PingPong(Time.time, 1f);
            focusLevelText.text = $"Focus Level: {focusLevel:F2}";
        }
        
        if (overstimulationText != null && devSettings.enableOverstimulationTest)
        {
            float intensity = Mathf.PingPong(Time.time * devSettings.overstimulationIntensity, 1f);
            overstimulationText.text = $"Overstimulation: {intensity:F2}";
        }
    }
    
    public void ToggleDebugMenu()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
        }
    }
} 