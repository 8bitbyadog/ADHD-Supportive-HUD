using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// UI component for displaying and interacting with a task
/// </summary>
public class TaskItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image priorityIndicator;
    [SerializeField] private Image progressBar;
    [SerializeField] private Button completeButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Toggle completedToggle;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject completionEffect;
    [SerializeField] private float progressAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve progressCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Priority Colors")]
    [SerializeField] private Color lowPriorityColor = new Color(0.5f, 0.8f, 0.5f);
    [SerializeField] private Color mediumPriorityColor = new Color(0.5f, 0.5f, 0.8f);
    [SerializeField] private Color highPriorityColor = new Color(0.8f, 0.8f, 0.5f);
    [SerializeField] private Color urgentPriorityColor = new Color(0.8f, 0.5f, 0.5f);
    
    [Header("Interaction")]
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private float grabScale = 1.1f;
    [SerializeField] private float grabDuration = 0.2f;
    
    // Events
    public event Action<TaskItemUI> OnCompleted;
    public event Action<TaskItemUI> OnDeleted;
    
    private TaskManager.Task _task;
    private TaskManager _taskManager;
    private float _targetProgress = 0f;
    private float _displayedProgress = 0f;
    private bool _isAnimatingProgress = false;
    private float _progressAnimationTime = 0f;
    private Vector3 _originalScale;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private bool _isGrabbed;
    
    private void Awake()
    {
        _originalScale = transform.localScale;
        _originalPosition = transform.localPosition;
        _originalRotation = transform.localRotation;
        
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }
    
    private void Start()
    {
        if (_taskManager == null)
        {
            _taskManager = FindObjectOfType<TaskManager>();
        }
        
        if (completeButton != null)
        {
            completeButton.onClick.AddListener(CompleteTask);
        }
        
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(DeleteTask);
        }
        
        if (completedToggle != null)
            completedToggle.onValueChanged.AddListener(OnCompletedToggleChanged);
    }
    
    private void Update()
    {
        if (_isAnimatingProgress)
        {
            _progressAnimationTime += Time.deltaTime;
            float t = Mathf.Clamp01(_progressAnimationTime / progressAnimationDuration);
            float curveT = progressCurve.Evaluate(t);
            
            _displayedProgress = Mathf.Lerp(_displayedProgress, _targetProgress, curveT);
            
            if (progressBar != null)
                progressBar.fillAmount = _displayedProgress;
                
            if (Mathf.Approximately(t, 1f))
                _isAnimatingProgress = false;
        }
    }
    
    /// <summary>
    /// Initialize the task item with data
    /// </summary>
    public void Initialize(TaskManager.Task task, TaskManager taskManager)
    {
        _task = task;
        _taskManager = taskManager;
        
        UpdateUI();
    }
    
    /// <summary>
    /// Update the UI with current task data
    /// </summary>
    public void UpdateUI()
    {
        if (_task == null)
            return;
            
        if (titleText != null)
            titleText.text = _task.title;
            
        if (descriptionText != null)
        {
            descriptionText.text = _task.description;
            descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(_task.description));
        }
        
        if (priorityIndicator != null)
            priorityIndicator.color = GetPriorityColor(_task.priority);
            
        SetProgress(_task.progress);
        
        if (completedToggle != null)
            completedToggle.isOn = _task.isCompleted;
            
        if (completeButton != null)
            completeButton.gameObject.SetActive(!_task.isCompleted);
            
        if (completionEffect != null)
            completionEffect.SetActive(_task.isCompleted);
    }
    
    /// <summary>
    /// Set the progress with optional animation
    /// </summary>
    public void SetProgress(float progress, bool animate = true)
    {
        _targetProgress = Mathf.Clamp01(progress);
        
        if (animate)
        {
            _isAnimatingProgress = true;
            _progressAnimationTime = 0f;
        }
        else
        {
            _displayedProgress = _targetProgress;
            if (progressBar != null)
                progressBar.fillAmount = _displayedProgress;
        }
    }
    
    /// <summary>
    /// Complete the task
    /// </summary>
    public void CompleteTask()
    {
        if (_task == null || _taskManager == null || _task.isCompleted)
            return;
        
        _taskManager.CompleteTask(_task.id);
        
        if (completionEffect != null)
            completionEffect.SetActive(true);
            
        SetProgress(1f, true);
        
        UpdateUI();
        
        OnCompleted?.Invoke(this);
    }
    
    /// <summary>
    /// Delete the task
    /// </summary>
    public void DeleteTask()
    {
        if (_task == null || _taskManager == null)
            return;
        
        _taskManager.RemoveTask(_task.id);
        
        OnDeleted?.Invoke(this);
    }
    
    /// <summary>
    /// Called when the completed toggle is changed
    /// </summary>
    private void OnCompletedToggleChanged(bool isCompleted)
    {
        if (_task == null || _taskManager == null)
            return;
            
        if (isCompleted != _task.isCompleted)
        {
            if (isCompleted)
                _taskManager.CompleteTask(_task.id);
            else
            {
                // We need to manually update the task since TaskManager doesn't have an "uncomplete" method
                _taskManager.UpdateTask(_task.id, progress: 0f);
                _task.isCompleted = false;
                _task.completionTime = null;
                
                // Update the UI
                UpdateUI();
            }
        }
    }
    
    /// <summary>
    /// Get the color for a priority level
    /// </summary>
    private Color GetPriorityColor(TaskManager.TaskPriority priority)
    {
        switch (priority)
        {
            case TaskManager.TaskPriority.Low:
                return lowPriorityColor;
            case TaskManager.TaskPriority.Medium:
                return mediumPriorityColor;
            case TaskManager.TaskPriority.High:
                return highPriorityColor;
            case TaskManager.TaskPriority.Urgent:
                return urgentPriorityColor;
            default:
                return mediumPriorityColor;
        }
    }
    
    /// <summary>
    /// Called when the task is grabbed
    /// </summary>
    private void OnGrabbed(SelectEnterEventArgs args)
    {
        _isGrabbed = true;
        
        // Scale up slightly when grabbed
        LeanTween.scale(gameObject, _originalScale * grabScale, grabDuration)
            .setEase(LeanTweenType.easeOutBack);
    }
    
    /// <summary>
    /// Called when the task is released
    /// </summary>
    private void OnReleased(SelectExitEventArgs args)
    {
        _isGrabbed = false;
        
        // Scale back to original size
        LeanTween.scale(gameObject, _originalScale, grabDuration)
            .setEase(LeanTweenType.easeInOutQuad);
    }
    
    /// <summary>
    /// Resets the task to its original position
    /// </summary>
    public void ResetPosition()
    {
        if (_isGrabbed)
            return;
        
        LeanTween.move(gameObject, _originalPosition, 0.5f)
            .setEase(LeanTweenType.easeOutBack);
        
        LeanTween.rotate(gameObject, _originalRotation.eulerAngles, 0.5f)
            .setEase(LeanTweenType.easeOutBack);
    }
} 