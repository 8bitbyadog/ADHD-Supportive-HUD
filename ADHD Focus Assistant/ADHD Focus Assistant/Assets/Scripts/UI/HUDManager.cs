using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages the ADHD Focus Assistant HUD system
/// Handles UI elements, layout, and feature coordination
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("Theme Settings")]
    [SerializeField] private List<HUDTheme> availableThemes;
    [SerializeField] private HUDTheme defaultTheme;
    [SerializeField] private float themeTransitionDuration = 1.0f;
    
    [Header("Task Management")]
    [SerializeField] private Transform taskContainer;
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private Button addTaskButton;
    [SerializeField] private TMP_InputField taskInputField;
    
    [Header("Pomodoro Timer")]
    [SerializeField] private GameObject pomodoroDisplay;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Button startPomodoroButton;
    [SerializeField] private Button pausePomodoroButton;
    
    [Header("Health Tracking")]
    [SerializeField] private GameObject healthDisplay;
    [SerializeField] private Slider stressLevelSlider;
    [SerializeField] private Button breathingGuideButton;
    
    [Header("Gamification")]
    [SerializeField] private GameObject achievementPanel;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private Slider progressBar;
    
    [Header("Accessibility")]
    [SerializeField] private float defaultTextSize = 18f;
    [SerializeField] private bool highContrastModeDefault = false;
    [SerializeField] private bool reduceMotionDefault = false;
    
    private TaskManager _taskManager;
    private ThemeManager _themeManager;
    private PomodoroManager _pomodoroManager;
    private AchievementSystem _achievementSystem;
    private List<Notification> _notificationQueue = new List<Notification>();
    private bool _isInitialized = false;
    
    private class Notification
    {
        public string title;
        public string message;
        public NotificationPriority priority;
        public float duration;
    }
    
    private enum NotificationPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }
    
    private void Awake()
    {
        // Find required dependencies
        _taskManager = FindObjectOfType<TaskManager>();
        _themeManager = FindObjectOfType<ThemeManager>();
        _pomodoroManager = FindObjectOfType<PomodoroManager>();
        _achievementSystem = FindObjectOfType<AchievementSystem>();
        
        if (_taskManager == null)
        {
            Debug.LogWarning("TaskManager not found. Creating one.");
            GameObject taskManagerObj = new GameObject("TaskManager");
            taskManagerObj.transform.SetParent(transform);
            _taskManager = taskManagerObj.AddComponent<TaskManager>();
        }
    }
    
    private void Start()
    {
        InitializeHUD();
    }
    
    private void InitializeHUD()
    {
        // Load user preferences
        LoadUserPreferences();
        
        // Initialize theme
        if (_themeManager != null && defaultTheme != null)
        {
            _themeManager.ChangeTheme(defaultTheme);
        }
        
        // Set up task management
        if (addTaskButton != null && taskInputField != null)
        {
            addTaskButton.onClick.AddListener(AddTask);
        }
        
        // Set up pomodoro timer
        if (startPomodoroButton != null && _pomodoroManager != null)
        {
            startPomodoroButton.onClick.AddListener(_pomodoroManager.StartSession);
            
            if (pausePomodoroButton != null)
            {
                pausePomodoroButton.onClick.AddListener(_pomodoroManager.PauseSession);
            }
        }
        
        // Set up breathing guide
        if (breathingGuideButton != null)
        {
            breathingGuideButton.onClick.AddListener(StartBreathingExercise);
        }
        
        // Subscribe to events
        SubscribeToEvents();
        
        _isInitialized = true;
    }
    
    private void LoadUserPreferences()
    {
        // Text size preference
        float textSize = PlayerPrefs.GetFloat("TextSize", defaultTextSize);
        SetGlobalTextSize(textSize);
        
        // High contrast mode preference
        bool highContrastMode = PlayerPrefs.GetInt("HighContrastMode", highContrastModeDefault ? 1 : 0) == 1;
        SetHighContrastMode(highContrastMode);
        
        // Reduced motion preference
        bool reduceMotion = PlayerPrefs.GetInt("ReduceMotion", reduceMotionDefault ? 1 : 0) == 1;
        SetReducedMotion(reduceMotion);
    }
    
    private void SetGlobalTextSize(float size)
    {
        // Find all TextMeshPro components and set their size
        TMP_Text[] texts = FindObjectsOfType<TMP_Text>();
        foreach (var text in texts)
        {
            text.fontSize = size;
        }
    }
    
    private void SetHighContrastMode(bool enabled)
    {
        // Apply high contrast mode if theme manager is available
        if (_themeManager != null)
        {
            _themeManager.SetHighContrastMode(enabled);
        }
    }
    
    private void SetReducedMotion(bool enabled)
    {
        // Store the setting for other components to access
        PlayerPrefs.SetInt("ReduceMotion", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    private void SubscribeToEvents()
    {
        if (_taskManager != null)
        {
            _taskManager.OnTaskCompleted += OnTaskCompleted;
        }
        
        if (_pomodoroManager != null)
        {
            _pomodoroManager.OnSessionCompleted += OnPomodoroSessionCompleted;
        }
    }
    
    private void AddTask()
    {
        if (_taskManager != null && taskInputField != null && !string.IsNullOrEmpty(taskInputField.text))
        {
            _taskManager.CreateTask(taskInputField.text);
            taskInputField.text = "";
            
            // Give focus back to input field
            taskInputField.ActivateInputField();
        }
    }
    
    private void OnTaskCompleted(TaskManager.Task task)
    {
        // Award XP for completing a task
        if (_achievementSystem != null)
        {
            int xpAmount = 0;
            
            // XP based on priority
            switch (task.priority)
            {
                case TaskManager.TaskPriority.Low:
                    xpAmount = 5;
                    break;
                case TaskManager.TaskPriority.Medium:
                    xpAmount = 10;
                    break;
                case TaskManager.TaskPriority.High:
                    xpAmount = 15;
                    break;
                case TaskManager.TaskPriority.Urgent:
                    xpAmount = 20;
                    break;
            }
            
            AddExperience(xpAmount);
        }
    }
    
    private void OnPomodoroSessionCompleted()
    {
        // Award XP for completing a pomodoro session
        AddExperience(15);
        
        // Add a notification
        AddNotification("Pomodoro completed!", "Take a short break before continuing.", NotificationPriority.Medium, 5f);
    }
    
    private void StartBreathingExercise()
    {
        // Find and activate the breathing guide
        BreathingGuide breathingGuide = FindObjectOfType<BreathingGuide>();
        if (breathingGuide != null)
        {
            breathingGuide.StartBreathingExercise();
        }
        else
        {
            Debug.LogWarning("BreathingGuide component not found");
        }
    }
    
    private void AddExperience(int amount)
    {
        // Update XP display
        if (xpText != null)
        {
            int currentXP = PlayerPrefs.GetInt("UserXP", 0);
            int newXP = currentXP + amount;
            
            PlayerPrefs.SetInt("UserXP", newXP);
            PlayerPrefs.Save();
            
            xpText.text = $"XP: {newXP}";
            
            // Update progress bar
            if (progressBar != null)
            {
                int xpForNextLevel = 100; // Simple example
                float progress = Mathf.Clamp01((float)newXP % xpForNextLevel / xpForNextLevel);
                StartCoroutine(AnimateProgressBar(progressBar.value, progress));
            }
        }
    }
    
    private IEnumerator AnimateProgressBar(float from, float to)
    {
        float duration = 1.0f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            if (progressBar != null)
            {
                progressBar.value = Mathf.Lerp(from, to, t);
            }
            
            yield return null;
        }
        
        if (progressBar != null)
        {
            progressBar.value = to;
        }
    }
    
    public void AddNotification(string title, string message, NotificationPriority priority, float duration)
    {
        Notification notification = new Notification
        {
            title = title,
            message = message,
            priority = priority,
            duration = duration
        };
        
        _notificationQueue.Add(notification);
        
        if (_notificationQueue.Count == 1)
        {
            StartCoroutine(ProcessNotificationQueue());
        }
    }
    
    private IEnumerator ProcessNotificationQueue()
    {
        while (_notificationQueue.Count > 0)
        {
            Notification notification = _notificationQueue[0];
            
            // Display notification (implementation would depend on your UI)
            Debug.Log($"[Notification] {notification.title}: {notification.message}");
            
            // Wait for the specified duration
            yield return new WaitForSeconds(notification.duration);
            
            // Remove from queue
            _notificationQueue.RemoveAt(0);
        }
    }
}
