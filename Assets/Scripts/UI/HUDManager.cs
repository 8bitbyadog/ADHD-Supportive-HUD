using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("Theme System")]
    [SerializeField] private List<HUDTheme> availableThemes;
    [SerializeField] private ParticleSystem themeParticles;
    [SerializeField] private float themeTransitionDuration = 1f;
    [SerializeField] private AnimationCurve themeTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Task System")]
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private Transform taskContainer;
    [SerializeField] private float taskAnimationDuration = 0.5f;
    [SerializeField] private ParticleSystem taskCompleteEffect;
    
    [Header("Pomodoro System")]
    [SerializeField] private GameObject pomodoroDisplay;
    [SerializeField] private List<string> breakActivities;
    [SerializeField] private AudioClip[] ambientSounds;
    [SerializeField] private float streakUpdateInterval = 24f; // Hours
    
    [Header("Health System")]
    [SerializeField] private GameObject healthDisplay;
    [SerializeField] private GameObject breathingGuide;
    [SerializeField] private GameObject postureReminder;
    [SerializeField] private GameObject moodTracker;
    
    [Header("Gamification")]
    [SerializeField] private GameObject achievementPanel;
    [SerializeField] private GameObject statsDisplay;
    [SerializeField] private List<Achievement> achievements;
    [SerializeField] private float xpPerTask = 100f;
    
    [Header("Notification System")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private float notificationDuration = 5f;
    
    [Header("Accessibility")]
    [SerializeField] private float[] textSizeOptions = { 12f, 14f, 16f, 18f };
    [SerializeField] private bool highContrastMode = false;
    [SerializeField] private bool reducedMotion = false;
    
    [Header("Social Features")]
    [SerializeField] private GameObject socialPanel;
    [SerializeField] private GameObject friendActivityFeed;
    [SerializeField] private GameObject communityChallenges;

    private HUDTheme currentTheme;
    private float themeTransitionProgress = 0f;
    private bool isThemeTransitioning = false;
    private List<TaskItem> activeTasks = new List<TaskItem>();
    private Dictionary<string, Achievement> unlockedAchievements = new Dictionary<string, Achievement>();
    private float currentXP = 0f;
    private int currentLevel = 1;
    private int productivityStreak = 0;
    private float lastStreakUpdate = 0f;
    private Queue<Notification> notificationQueue = new Queue<Notification>();
    private AudioSource ambientAudioSource;
    private int currentTextSizeIndex = 1;
    
    private void Start()
    {
        InitializeSystems();
        LoadUserPreferences();
        StartAmbientAudio();
    }
    
    private void InitializeSystems()
    {
        // Initialize theme system
        if (availableThemes.Count > 0)
        {
            currentTheme = availableThemes[0];
            ApplyTheme(currentTheme);
        }
        
        // Initialize task system
        InitializeTaskSystem();
        
        // Initialize achievement system
        InitializeAchievements();
        
        // Initialize notification system
        InitializeNotifications();
        
        // Initialize social features
        InitializeSocialFeatures();
    }
    
    private void LoadUserPreferences()
    {
        // Load saved preferences
        currentTextSizeIndex = PlayerPrefs.GetInt("TextSize", 1);
        highContrastMode = PlayerPrefs.GetInt("HighContrast", 0) == 1;
        reducedMotion = PlayerPrefs.GetInt("ReducedMotion", 0) == 1;
        
        // Apply preferences
        UpdateTextSize();
        UpdateContrastMode();
        UpdateMotionSettings();
    }
    
    private void StartAmbientAudio()
    {
        ambientAudioSource = gameObject.AddComponent<AudioSource>();
        ambientAudioSource.loop = true;
        ambientAudioSource.volume = 0.3f;
        
        if (ambientSounds.Length > 0)
        {
            ambientAudioSource.clip = ambientSounds[0];
            ambientAudioSource.Play();
        }
    }
    
    private void Update()
    {
        UpdateThemeTransition();
        UpdateStreak();
        ProcessNotifications();
        UpdateSocialFeed();
    }
    
    private void UpdateThemeTransition()
    {
        if (isThemeTransitioning)
        {
            themeTransitionProgress += Time.deltaTime / themeTransitionDuration;
            float t = themeTransitionCurve.Evaluate(themeTransitionProgress);
            
            // Interpolate theme properties
            if (themeTransitionProgress >= 1f)
            {
                isThemeTransitioning = false;
                themeTransitionProgress = 0f;
            }
        }
    }
    
    private void UpdateStreak()
    {
        if (Time.time - lastStreakUpdate >= streakUpdateInterval)
        {
            CheckAndUpdateStreak();
            lastStreakUpdate = Time.time;
        }
    }
    
    private void CheckAndUpdateStreak()
    {
        // Check if user has completed tasks
        bool hasCompletedTasks = false;
        foreach (var task in activeTasks)
        {
            if (task.IsCompleted)
            {
                hasCompletedTasks = true;
                break;
            }
        }
        
        if (hasCompletedTasks)
        {
            productivityStreak++;
            ShowAchievement("Streak" + productivityStreak);
        }
        else
        {
            productivityStreak = 0;
        }
    }
    
    private void ProcessNotifications()
    {
        if (notificationQueue.Count > 0)
        {
            var notification = notificationQueue.Peek();
            if (Time.time >= notification.displayTime + notificationDuration)
            {
                notificationQueue.Dequeue();
                RemoveNotification(notification);
            }
        }
    }
    
    private void UpdateSocialFeed()
    {
        // Update friend activity and community challenges
        if (socialPanel.activeSelf)
        {
            UpdateFriendActivity();
            UpdateCommunityChallenges();
        }
    }
    
    // Theme System Methods
    public void ChangeTheme(HUDTheme newTheme)
    {
        if (currentTheme != newTheme)
        {
            currentTheme = newTheme;
            isThemeTransitioning = true;
            themeTransitionProgress = 0f;
            ApplyTheme(newTheme);
        }
    }
    
    private void ApplyTheme(HUDTheme theme)
    {
        // Apply theme colors and effects
        if (themeParticles != null)
        {
            var main = themeParticles.main;
            main.startColor = theme.primaryColor;
        }
        
        // Update UI elements with theme colors
        UpdateUITheme(theme);
    }
    
    private void UpdateUITheme(HUDTheme theme)
    {
        // Update all UI elements with new theme colors
        // This would be implemented based on your UI structure
    }
    
    // Task System Methods
    private void InitializeTaskSystem()
    {
        // Initialize task container and drag-drop functionality
    }
    
    public void AddTask(string title, TaskPriority priority = TaskPriority.Normal)
    {
        GameObject taskObj = Instantiate(taskItemPrefab, taskContainer);
        TaskItem task = taskObj.GetComponent<TaskItem>();
        task.Initialize(title, priority);
        activeTasks.Add(task);
        
        // Animate task appearance
        AnimateTaskAppearance(task);
    }
    
    private void AnimateTaskAppearance(TaskItem task)
    {
        // Animate task appearance with spring physics
        StartCoroutine(AnimateTaskCoroutine(task));
    }
    
    private System.Collections.IEnumerator AnimateTaskCoroutine(TaskItem task)
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;
        
        while (elapsed < taskAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / taskAnimationDuration;
            task.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        task.transform.localScale = targetScale;
    }
    
    // Achievement System Methods
    private void InitializeAchievements()
    {
        foreach (var achievement in achievements)
        {
            unlockedAchievements[achievement.id] = null;
        }
    }
    
    public void ShowAchievement(string achievementId)
    {
        if (achievements.Exists(a => a.id == achievementId))
        {
            var achievement = achievements.Find(a => a.id == achievementId);
            if (unlockedAchievements[achievementId] == null)
            {
                unlockedAchievements[achievementId] = achievement;
                DisplayAchievement(achievement);
                AddXP(achievement.xpReward);
            }
        }
    }
    
    private void DisplayAchievement(Achievement achievement)
    {
        // Show achievement popup with animation
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(true);
            // Update achievement UI
        }
    }
    
    // XP and Leveling System
    private void AddXP(float amount)
    {
        currentXP += amount;
        CheckLevelUp();
    }
    
    private void CheckLevelUp()
    {
        float xpNeeded = currentLevel * 1000f;
        if (currentXP >= xpNeeded)
        {
            LevelUp();
        }
    }
    
    private void LevelUp()
    {
        currentLevel++;
        currentXP -= currentLevel * 1000f;
        ShowAchievement("Level" + currentLevel);
    }
    
    // Notification System Methods
    private void InitializeNotifications()
    {
        // Initialize notification panel and queue
    }
    
    public void ShowNotification(string message, NotificationPriority priority = NotificationPriority.Normal)
    {
        var notification = new Notification
        {
            message = message,
            priority = priority,
            displayTime = Time.time
        };
        
        notificationQueue.Enqueue(notification);
        DisplayNotification(notification);
    }
    
    private void DisplayNotification(Notification notification)
    {
        GameObject notificationObj = Instantiate(notificationPrefab, notificationPanel.transform);
        // Set up notification UI
    }
    
    private void RemoveNotification(Notification notification)
    {
        // Remove notification from UI
    }
    
    // Accessibility Methods
    public void UpdateTextSize()
    {
        float newSize = textSizeOptions[currentTextSizeIndex];
        // Update all text components with new size
    }
    
    public void ToggleHighContrastMode()
    {
        highContrastMode = !highContrastMode;
        PlayerPrefs.SetInt("HighContrast", highContrastMode ? 1 : 0);
        UpdateContrastMode();
    }
    
    private void UpdateContrastMode()
    {
        // Update UI contrast based on mode
    }
    
    public void ToggleReducedMotion()
    {
        reducedMotion = !reducedMotion;
        PlayerPrefs.SetInt("ReducedMotion", reducedMotion ? 1 : 0);
        UpdateMotionSettings();
    }
    
    private void UpdateMotionSettings()
    {
        // Update animation speeds and effects based on motion settings
    }
    
    // Social Features Methods
    private void InitializeSocialFeatures()
    {
        // Initialize social panel and features
    }
    
    private void UpdateFriendActivity()
    {
        // Update friend activity feed
    }
    
    private void UpdateCommunityChallenges()
    {
        // Update community challenges display
    }
}

// Supporting Classes
[System.Serializable]
public class HUDTheme
{
    public string name;
    public Color primaryColor;
    public Color secondaryColor;
    public Color accentColor;
    public Color backgroundColor;
    public Color textColor;
    public ParticleSystem.MinMaxGradient particleColor;
}

[System.Serializable]
public class Achievement
{
    public string id;
    public string title;
    public string description;
    public Sprite icon;
    public float xpReward;
}

public enum TaskPriority
{
    Low,
    Normal,
    High,
    Urgent
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Critical
}

public struct Notification
{
    public string message;
    public NotificationPriority priority;
    public float displayTime;
} 