using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// A simplified HUD manager for initial setup
/// Works without requiring complex dependencies
/// </summary>
public class SimpleHUDManager : MonoBehaviour
{
    [Header("Task Management")]
    [SerializeField] private Transform taskContainer;
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private Button addTaskButton;
    [SerializeField] private InputField taskInputField;
    
    [Header("HUD Positioning")]
    [SerializeField] private Transform headTransform;
    [SerializeField] private float followDistance = 1.0f;
    [SerializeField] private float followHeight = -0.2f;
    [SerializeField] private float followSpeed = 2.0f;
    [SerializeField] private bool followHead = true;
    
    [Header("UI Panels")]
    [SerializeField] private GameObject taskPanel;
    [SerializeField] private GameObject healthPanel;
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Debug")]
    [SerializeField] private bool verboseLogging = false;
    
    private List<GameObject> activeTasks = new List<GameObject>();
    private SimpleThemeManager _themeManager;
    
    private void Awake()
    {
        _themeManager = FindObjectOfType<SimpleThemeManager>();
        
        // Initialize head transform if not set
        if (headTransform == null)
        {
            headTransform = Camera.main.transform;
        }
    }
    
    private void Start()
    {
        InitializeHUD();
        
        if (followHead && headTransform != null)
        {
            StartCoroutine(FollowHeadRoutine());
        }
    }
    
    private void InitializeHUD()
    {
        // Set up task panel
        if (addTaskButton != null && taskInputField != null)
        {
            addTaskButton.onClick.AddListener(AddTask);
        }
        
        // Initially show only the task panel
        SetActivePanel(taskPanel);
        
        LogMessage("HUD initialized");
    }
    
    /// <summary>
    /// Makes only the specified panel active, hiding all others
    /// </summary>
    public void SetActivePanel(GameObject activePanel)
    {
        if (taskPanel != null)
            taskPanel.SetActive(taskPanel == activePanel);
            
        if (healthPanel != null)
            healthPanel.SetActive(healthPanel == activePanel);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(settingsPanel == activePanel);
    }
    
    /// <summary>
    /// Adds a task to the task list based on the input field
    /// </summary>
    public void AddTask()
    {
        if (taskInputField == null || string.IsNullOrEmpty(taskInputField.text) || taskItemPrefab == null || taskContainer == null)
            return;
        
        // Create task gameobject
        GameObject taskObj = Instantiate(taskItemPrefab, taskContainer);
        activeTasks.Add(taskObj);
        
        // Set task text if the object has a text component
        Text taskText = taskObj.GetComponentInChildren<Text>();
        if (taskText != null)
        {
            taskText.text = taskInputField.text;
        }
        
        // Set up complete button if it exists
        Button completeButton = taskObj.GetComponentInChildren<Button>();
        if (completeButton != null)
        {
            completeButton.onClick.AddListener(() => CompleteTask(taskObj));
        }
        
        // Clear input field
        taskInputField.text = "";
        
        LogMessage($"Added task: {taskText?.text}");
    }
    
    /// <summary>
    /// Toggles visibility of a specific panel
    /// </summary>
    public void TogglePanel(GameObject panel)
    {
        if (panel != null)
        {
            bool isActive = panel.activeSelf;
            SetActivePanel(isActive ? null : panel);
        }
    }
    
    /// <summary>
    /// Completes and removes a task
    /// </summary>
    private void CompleteTask(GameObject taskObj)
    {
        if (taskObj != null)
        {
            // Get task text before destroying
            string taskText = taskObj.GetComponentInChildren<Text>()?.text ?? "task";
            
            // Animate and remove
            StartCoroutine(AnimateTaskCompletion(taskObj));
            
            LogMessage($"Completed task: {taskText}");
        }
    }
    
    private IEnumerator AnimateTaskCompletion(GameObject taskObj)
    {
        if (taskObj == null)
            yield break;
            
        // Simple completion animation
        float duration = 0.5f;
        float elapsed = 0f;
        
        Vector3 originalScale = taskObj.transform.localScale;
        Color originalColor = taskObj.GetComponent<Image>()?.color ?? Color.white;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Scale down and fade out
            taskObj.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            
            Image image = taskObj.GetComponent<Image>();
            if (image != null)
            {
                Color color = image.color;
                color.a = Mathf.Lerp(originalColor.a, 0f, t);
                image.color = color;
            }
            
            yield return null;
        }
        
        // Remove from list and destroy
        activeTasks.Remove(taskObj);
        Destroy(taskObj);
    }
    
    private IEnumerator FollowHeadRoutine()
    {
        while (followHead && headTransform != null)
        {
            // Calculate position in front of the head
            Vector3 headForward = headTransform.forward;
            headForward.y = 0; // Keep UI level with ground
            headForward.Normalize();
            
            Vector3 targetPosition = headTransform.position + headForward * followDistance;
            targetPosition.y = headTransform.position.y + followHeight;
            
            // Calculate rotation to face the head
            Quaternion targetRotation = Quaternion.LookRotation(transform.position - headTransform.position);
            
            // Move the UI
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
    
    public void SetHighContrastMode(bool enabled)
    {
        if (_themeManager != null)
        {
            _themeManager.SetHighContrastMode(enabled);
        }
    }
    
    public void AddSampleTasks()
    {
        string[] sampleTasks = new string[] {
            "Complete Unity tutorial",
            "Test on device",
            "Fix UI scaling issues",
            "Implement passthrough"
        };
        
        foreach (string task in sampleTasks)
        {
            if (taskInputField != null)
            {
                taskInputField.text = task;
                AddTask();
            }
        }
    }
    
    private void LogMessage(string message)
    {
        if (verboseLogging)
        {
            Debug.Log($"[SimpleHUDManager] {message}");
        }
    }
}
