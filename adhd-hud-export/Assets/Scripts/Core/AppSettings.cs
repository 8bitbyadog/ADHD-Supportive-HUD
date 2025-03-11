using UnityEngine;
using System;
using System.IO;

[Serializable]
public class AppSettings
{
    [Header("HUD Settings")]
    public float hudOpacity = 0.8f;
    public Vector3 hudOffset = new Vector3(0.2f, -0.1f, 0.5f);
    public float hudScale = 1.0f;
    public Color hudTextColor = Color.white;
    public Color hudBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    [Header("Task Settings")]
    public int maxDisplayedTasks = 3;
    public bool showTaskPriority = true;
    public bool showTaskDueDate = true;
    public float taskReminderInterval = 30f * 60f; // 30 minutes

    [Header("Pomodoro Settings")]
    public float workDuration = 25f * 60f; // 25 minutes
    public float breakDuration = 5f * 60f; // 5 minutes
    public float longBreakDuration = 15f * 60f; // 15 minutes
    public int sessionsUntilLongBreak = 4;
    public bool autoStartBreaks = true;
    public bool playSoundNotifications = true;

    [Header("Health Settings")]
    public int dailyWaterGoal = 8;
    public float waterReminderInterval = 60f * 60f; // 1 hour
    public float breakReminderInterval = 30f * 60f; // 30 minutes
    public bool enableBreakReminders = true;
    public bool enableWaterReminders = true;

    [Header("Accessibility Settings")]
    public float textSize = 1.0f;
    public float contrastRatio = 1.0f;
    public bool highContrastMode = false;
    public bool reduceMotion = false;
    public bool enableVoiceFeedback = false;
}

public class AppSettingsManager : MonoBehaviour
{
    private static AppSettingsManager instance;
    public static AppSettingsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AppSettingsManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AppSettingsManager");
                    instance = go.AddComponent<AppSettingsManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [SerializeField] private AppSettings currentSettings = new AppSettings();
    private string settingsPath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        settingsPath = Path.Combine(Application.persistentDataPath, "appsettings.json");
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(settingsPath))
            {
                string json = File.ReadAllText(settingsPath);
                currentSettings = JsonUtility.FromJson<AppSettings>(json);
                Debug.Log("Settings loaded successfully");
            }
            else
            {
                Debug.Log("No settings file found, using defaults");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading settings: {e.Message}");
        }
    }

    private void SaveSettings()
    {
        try
        {
            string json = JsonUtility.ToJson(currentSettings, true);
            File.WriteAllText(settingsPath, json);
            Debug.Log("Settings saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving settings: {e.Message}");
        }
    }

    public AppSettings GetSettings()
    {
        return currentSettings;
    }

    public void UpdateSettings(AppSettings newSettings)
    {
        currentSettings = newSettings;
        SaveSettings();
        ApplySettings();
    }

    private void ApplySettings()
    {
        // Apply HUD settings
        var hudManager = FindObjectOfType<HUDManager>();
        if (hudManager != null)
        {
            hudManager.UpdateHUDPosition(currentSettings.hudOffset);
            hudManager.UpdateHUDOpacity(currentSettings.hudOpacity);
            hudManager.UpdateHUDScale(currentSettings.hudScale);
        }

        // Apply task settings
        var taskManager = FindObjectOfType<TaskManager>();
        if (taskManager != null)
        {
            // Update task manager settings
        }

        // Apply Pomodoro settings
        var pomodoroManager = FindObjectOfType<PomodoroManager>();
        if (pomodoroManager != null)
        {
            // Update pomodoro manager settings
        }

        // Apply health settings
        var healthTracker = FindObjectOfType<HealthTracker>();
        if (healthTracker != null)
        {
            // Update health tracker settings
        }

        // Apply accessibility settings
        ApplyAccessibilitySettings();
    }

    private void ApplyAccessibilitySettings()
    {
        // Apply text size
        var textComponents = FindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in textComponents)
        {
            text.fontSize *= currentSettings.textSize;
        }

        // Apply contrast
        if (currentSettings.highContrastMode)
        {
            // Implement high contrast mode
        }

        // Apply motion reduction
        if (currentSettings.reduceMotion)
        {
            // Implement motion reduction
        }
    }

    // Debug methods
    public void ResetToDefaults()
    {
        currentSettings = new AppSettings();
        SaveSettings();
        ApplySettings();
    }

    public void TestSettings()
    {
        Debug.Log($"HUD Opacity: {currentSettings.hudOpacity}");
        Debug.Log($"HUD Offset: {currentSettings.hudOffset}");
        Debug.Log($"HUD Scale: {currentSettings.hudScale}");
        Debug.Log($"Max Displayed Tasks: {currentSettings.maxDisplayedTasks}");
        Debug.Log($"Work Duration: {currentSettings.workDuration}");
        Debug.Log($"Daily Water Goal: {currentSettings.dailyWaterGoal}");
    }
} 