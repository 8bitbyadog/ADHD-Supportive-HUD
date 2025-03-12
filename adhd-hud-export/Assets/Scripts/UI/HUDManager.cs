using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Oculus.Interaction;

/// <summary>
/// Unified HUD Manager for the ADHD Focus Assistant
/// Handles all HUD-related functionality including UI elements, layout, and feature coordination
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("HUD Settings")]
    [SerializeField] private float hudOpacity = 0.8f;
    [SerializeField] private Vector3 hudOffset = new Vector3(0.2f, -0.1f, 0.5f);
    [SerializeField] private float hudScale = 1.0f;
    [SerializeField] private bool followHead = true;
    [SerializeField] private float followDistance = 1.0f;
    [SerializeField] private float followHeight = -0.2f;
    [SerializeField] private float followSpeed = 2.0f;

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
    
    [Header("References")]
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private CanvasGroup hudCanvasGroup;
    [SerializeField] private Camera xrCamera;
    
    private TaskManager _taskManager;
    private ThemeManager _themeManager;
    private PomodoroManager _pomodoroManager;
    private AchievementSystem _achievementSystem;
    private List<Notification> _notificationQueue = new List<Notification>();
    private bool _isInitialized = false;
    private Transform _headTransform;
    
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
        InitializeReferences();
        SetupHUDCanvas();
        InitializeManagers();
    }
    
    private void Start()
    {
        InitializeHUD();
        if (followHead)
        {
            StartCoroutine(FollowHeadRoutine());
        }
    }
    
    private void InitializeReferences()
    {
        if (xrCamera == null)
        {
            xrCamera = Camera.main;
        }
        
        _headTransform = xrCamera.transform;
        
        // Find required dependencies
        _taskManager = FindObjectOfType<TaskManager>();
        _themeManager = FindObjectOfType<ThemeManager>();
        _pomodoroManager = FindObjectOfType<PomodoroManager>();
        _achievementSystem = FindObjectOfType<AchievementSystem>();
    }
    
    private void SetupHUDCanvas()
    {
        if (hudCanvas == null)
        {
            CreateHUDCanvas();
        }
        
        if (hudCanvasGroup == null)
        {
            hudCanvasGroup = hudCanvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        SetupHUD();
    }
    
    private void CreateHUDCanvas()
    {
        GameObject hudObject = new GameObject("HUDCanvas");
        hudCanvas = hudObject.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.WorldSpace;
        hudCanvas.worldCamera = xrCamera;

        CanvasScaler scaler = hudObject.AddComponent<CanvasScaler>();
        scaler.scaleFactor = hudScale;
        scaler.dynamicPixelsPerUnit = 100;

        hudObject.AddComponent<GraphicRaycaster>();
        hudObject.transform.SetParent(xrCamera.transform, false);
    }
    
    private void SetupHUD()
    {
        hudCanvas.transform.localPosition = hudOffset;
        hudCanvas.transform.localRotation = Quaternion.identity;
        hudCanvasGroup.alpha = hudOpacity;
        hudCanvas.sortingOrder = 1;
        hudCanvas.overrideSorting = true;
    }
    
    private void InitializeManagers()
    {
        if (_taskManager == null)
        {
            Debug.LogWarning("TaskManager not found. Creating one.");
            GameObject taskManagerObj = new GameObject("TaskManager");
            taskManagerObj.transform.SetParent(transform);
            _taskManager = taskManagerObj.AddComponent<TaskManager>();
        }
    }
    
    private void InitializeHUD()
    {
        if (_isInitialized) return;
        
        // Set up task panel
        if (addTaskButton != null && taskInputField != null)
        {
            addTaskButton.onClick.AddListener(AddTask);
        }
        
        // Set up Pomodoro timer
        if (startPomodoroButton != null && _pomodoroManager != null)
        {
            startPomodoroButton.onClick.AddListener(_pomodoroManager.StartTimer);
        }
        
        // Set up health tracking
        if (breathingGuideButton != null)
        {
            breathingGuideButton.onClick.AddListener(ShowBreathingGuide);
        }
        
        // Apply default theme
        if (_themeManager != null && defaultTheme != null)
        {
            _themeManager.ApplyTheme(defaultTheme);
        }
        
        _isInitialized = true;
    }
    
    private IEnumerator FollowHeadRoutine()
    {
        while (followHead && _headTransform != null)
        {
            Vector3 headForward = _headTransform.forward;
            headForward.y = 0;
            headForward.Normalize();
            
            Vector3 targetPosition = _headTransform.position + headForward * followDistance;
            targetPosition.y = _headTransform.position.y + followHeight;
            
            Quaternion targetRotation = Quaternion.LookRotation(transform.position - _headTransform.position);
            
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * followSpeed
            );
            
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * followSpeed
            );
            
            yield return null;
        }
    }
    
    public void AddTask()
    {
        if (taskInputField == null || string.IsNullOrEmpty(taskInputField.text) || taskItemPrefab == null || taskContainer == null)
            return;
        
        if (_taskManager != null)
        {
            _taskManager.AddTask(taskInputField.text);
            taskInputField.text = "";
        }
    }
    
    public void ShowBreathingGuide()
    {
        // Implementation for breathing guide
    }
    
    public void UpdateHUDPosition(Vector3 newOffset)
    {
        hudOffset = newOffset;
        hudCanvas.transform.localPosition = hudOffset;
    }
    
    public void UpdateHUDOpacity(float newOpacity)
    {
        hudOpacity = Mathf.Clamp01(newOpacity);
        hudCanvasGroup.alpha = hudOpacity;
    }
    
    public void UpdateHUDScale(float newScale)
    {
        hudScale = newScale;
        CanvasScaler scaler = hudCanvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.scaleFactor = hudScale;
        }
    }
    
    public void SetHighContrastMode(bool enabled)
    {
        if (_themeManager != null)
        {
            _themeManager.SetHighContrastMode(enabled);
        }
    }
    
    public void SetReduceMotion(bool enabled)
    {
        // Implementation for motion reduction
    }
    
    public void UpdateTaskDisplay(string[] tasks)
    {
        // Implementation for task display updates
    }
    
    public void UpdatePomodoroDisplay(float timeRemaining, bool isWorkMode)
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(timeRemaining);
        }
    }
    
    public void UpdateHealthDisplay(int waterIntake)
    {
        // Implementation for health display updates
    }
    
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return $"{minutes:00}:{seconds:00}";
    }
}
