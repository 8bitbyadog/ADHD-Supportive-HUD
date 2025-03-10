using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
    
    [Header("Quest Testing")]
    public Button validateBuildButton;
    public Button runPerformanceTestButton;
    public Toggle passthroughTestToggle;
    public Toggle handTrackingTestToggle;
    public TextMeshProUGUI buildStatusText;
    
    private DevelopmentSettings devSettings;
    private QuestBuildValidator questValidator;
    private float updateInterval = 0.5f;
    private float timeSinceLastUpdate = 0f;
    
    private void Start()
    {
        devSettings = DevelopmentSettings.Instance;
        questValidator = QuestBuildValidator.Instance;
        
        if (devSettings == null)
        {
            Debug.LogError("DevelopmentSettings not found in scene!");
            return;
        }
        
        if (questValidator == null)
        {
            Debug.LogError("QuestBuildValidator not found in scene!");
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
        
        // Quest Testing UI setup
        if (validateBuildButton != null)
        {
            validateBuildButton.onClick.AddListener(() => {
                questValidator.ValidateBuildSettings();
                UpdateBuildStatus();
            });
        }
        
        if (runPerformanceTestButton != null)
        {
            runPerformanceTestButton.onClick.AddListener(() => {
                questValidator.RunPerformanceTest();
            });
        }
        
        if (passthroughTestToggle != null)
        {
            passthroughTestToggle.onValueChanged.AddListener((value) => {
                // Toggle passthrough test mode
                if (value)
                {
                    StartPassthroughTest();
                }
                else
                {
                    StopPassthroughTest();
                }
            });
        }
        
        if (handTrackingTestToggle != null)
        {
            handTrackingTestToggle.onValueChanged.AddListener((value) => {
                // Toggle hand tracking test mode
                if (value)
                {
                    StartHandTrackingTest();
                }
                else
                {
                    StopHandTrackingTest();
                }
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
        
        UpdateBuildStatus();
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
    
    private void UpdateBuildStatus()
    {
        if (buildStatusText != null)
        {
            // Check for common Quest build issues
            var issues = new List<string>();
            
            #if UNITY_EDITOR
            // Check XR Plugin Management
            if (!PackageManager.PackageExists("com.unity.xr.management"))
            {
                issues.Add("Missing XR Plugin Management");
            }
            
            // Check Oculus XR Plugin
            if (!PackageManager.PackageExists("com.unity.xr.oculus"))
            {
                issues.Add("Missing Oculus XR Plugin");
            }
            
            // Check Meta XR SDK
            if (!PackageManager.PackageExists("com.meta.xr.sdk"))
            {
                issues.Add("Missing Meta XR SDK");
            }
            
            // Check Android settings
            var androidSettings = PlayerSettings.GetPlatformSettings<AndroidSettings>("Android");
            if (androidSettings != null)
            {
                if (androidSettings.targetArchitectures != AndroidArchitecture.ARM64)
                {
                    issues.Add("Incorrect target architecture");
                }
                
                if (androidSettings.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24)
                {
                    issues.Add("SDK version too low");
                }
            }
            #endif
            
            if (issues.Count > 0)
            {
                buildStatusText.text = "Build Status: Issues Found\n" + string.Join("\n", issues);
                buildStatusText.color = Color.red;
            }
            else
            {
                buildStatusText.text = "Build Status: Ready for Quest 3";
                buildStatusText.color = Color.green;
            }
        }
    }
    
    private void StartPassthroughTest()
    {
        // Test passthrough functionality
        Debug.Log("Starting Passthrough Test...");
        // Add passthrough test logic here
    }
    
    private void StopPassthroughTest()
    {
        Debug.Log("Stopping Passthrough Test...");
        // Clean up passthrough test
    }
    
    private void StartHandTrackingTest()
    {
        // Test hand tracking functionality
        Debug.Log("Starting Hand Tracking Test...");
        // Add hand tracking test logic here
    }
    
    private void StopHandTrackingTest()
    {
        Debug.Log("Stopping Hand Tracking Test...");
        // Clean up hand tracking test
    }
    
    public void ToggleDebugMenu()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
        }
    }
} 