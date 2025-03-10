using UnityEngine;
using System.Collections.Generic;

public class DevelopmentSettings : MonoBehaviour
{
    [Header("Development Mode")]
    public bool isDevelopmentMode = true;
    public bool enableDebugLogging = true;
    public bool showDebugUI = true;
    
    [Header("Quick Testing")]
    public bool autoSpawnTestTasks = false;
    public int testTaskCount = 3;
    public bool autoStartPomodoro = false;
    public float pomodoroDuration = 25f;
    
    [Header("Performance Testing")]
    public bool enablePerformanceMetrics = false;
    public float fpsUpdateInterval = 1f;
    
    [Header("Debug Tools")]
    public bool enableHandTrackingDebug = false;
    public bool showHandTrackingPoints = false;
    public bool enableHUDDebugMode = false;
    
    [Header("ADHD-Specific Testing")]
    public bool enableFocusTesting = false;
    public float focusTestDuration = 5f;
    public bool enableOverstimulationTest = false;
    public float overstimulationIntensity = 1f;
    public bool enableTaskOverflowTest = false;
    public int overflowTaskCount = 10;
    
    [Header("UI Testing")]
    public bool enableUIStressTest = false;
    public float uiUpdateFrequency = 0.5f;
    public bool enableColorContrastTest = false;
    public bool enableMotionSensitivityTest = false;
    
    private static DevelopmentSettings instance;
    public static DevelopmentSettings Instance => instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (isDevelopmentMode)
        {
            SetupDevelopmentEnvironment();
        }
    }
    
    private void SetupDevelopmentEnvironment()
    {
        if (enableDebugLogging)
        {
            Debug.unityLogger.logEnabled = true;
        }
        
        if (autoSpawnTestTasks)
        {
            SpawnTestTasks();
        }
        
        if (autoStartPomodoro)
        {
            StartTestPomodoro();
        }
        
        if (enablePerformanceMetrics)
        {
            StartPerformanceMonitoring();
        }
        
        if (enableFocusTesting)
        {
            StartFocusTesting();
        }
        
        if (enableOverstimulationTest)
        {
            StartOverstimulationTest();
        }
        
        if (enableTaskOverflowTest)
        {
            SpawnOverflowTasks();
        }
        
        if (enableUIStressTest)
        {
            StartUIStressTest();
        }
    }
    
    private void SpawnTestTasks()
    {
        var taskManager = FindObjectOfType<TaskManager>();
        if (taskManager != null)
        {
            for (int i = 0; i < testTaskCount; i++)
            {
                taskManager.AddTask($"Test Task {i + 1}", $"This is a test task for development purposes. Priority: {(i % 3) + 1}");
            }
        }
    }
    
    private void StartTestPomodoro()
    {
        var pomodoroManager = FindObjectOfType<PomodoroManager>();
        if (pomodoroManager != null)
        {
            pomodoroManager.StartTimer(pomodoroDuration);
        }
    }
    
    private void StartPerformanceMonitoring()
    {
        StartCoroutine(MonitorPerformance());
    }
    
    private System.Collections.IEnumerator MonitorPerformance()
    {
        while (true)
        {
            float fps = 1.0f / Time.deltaTime;
            Debug.Log($"FPS: {fps:F2}");
            yield return new WaitForSeconds(fpsUpdateInterval);
        }
    }
    
    private void StartFocusTesting()
    {
        StartCoroutine(FocusTest());
    }
    
    private System.Collections.IEnumerator FocusTest()
    {
        while (enableFocusTesting)
        {
            // Simulate focus changes
            float focusLevel = Mathf.PingPong(Time.time, 1f);
            Debug.Log($"Focus Level: {focusLevel:F2}");
            yield return new WaitForSeconds(focusTestDuration);
        }
    }
    
    private void StartOverstimulationTest()
    {
        StartCoroutine(OverstimulationTest());
    }
    
    private System.Collections.IEnumerator OverstimulationTest()
    {
        while (enableOverstimulationTest)
        {
            // Simulate overstimulation effects
            float intensity = Mathf.PingPong(Time.time * overstimulationIntensity, 1f);
            Debug.Log($"Overstimulation Intensity: {intensity:F2}");
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private void SpawnOverflowTasks()
    {
        var taskManager = FindObjectOfType<TaskManager>();
        if (taskManager != null)
        {
            for (int i = 0; i < overflowTaskCount; i++)
            {
                taskManager.AddTask($"Overflow Task {i + 1}", $"Testing task overflow with high priority task {i + 1}");
            }
        }
    }
    
    private void StartUIStressTest()
    {
        StartCoroutine(UIStressTest());
    }
    
    private System.Collections.IEnumerator UIStressTest()
    {
        while (enableUIStressTest)
        {
            // Simulate rapid UI updates
            Debug.Log("UI Stress Test Update");
            yield return new WaitForSeconds(uiUpdateFrequency);
        }
    }
    
    public void ToggleDevelopmentMode()
    {
        isDevelopmentMode = !isDevelopmentMode;
        SetupDevelopmentEnvironment();
    }
    
    public void ToggleDebugUI()
    {
        showDebugUI = !showDebugUI;
        // Update UI visibility
    }
    
    public void ToggleHandTrackingDebug()
    {
        enableHandTrackingDebug = !enableHandTrackingDebug;
        // Update hand tracking debug visualization
    }
    
    public void ToggleHUDDebugMode()
    {
        enableHUDDebugMode = !enableHUDDebugMode;
        // Update HUD debug features
    }
    
    public void ToggleFocusTesting()
    {
        enableFocusTesting = !enableFocusTesting;
        if (enableFocusTesting)
        {
            StartFocusTesting();
        }
    }
    
    public void ToggleOverstimulationTest()
    {
        enableOverstimulationTest = !enableOverstimulationTest;
        if (enableOverstimulationTest)
        {
            StartOverstimulationTest();
        }
    }
    
    public void ToggleUIStressTest()
    {
        enableUIStressTest = !enableUIStressTest;
        if (enableUIStressTest)
        {
            StartUIStressTest();
        }
    }
} 