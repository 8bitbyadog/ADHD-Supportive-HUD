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
} 