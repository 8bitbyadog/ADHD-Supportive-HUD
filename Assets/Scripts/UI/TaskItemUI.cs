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
    [Header("References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image priorityIndicator;
    [SerializeField] private Image progressBar;
    [SerializeField] private Button completeButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private GameObject completionParticles;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Priority Colors")]
    [SerializeField] private Color lowPriorityColor = new Color(0.5f, 0.8f, 0.5f);
    [SerializeField] private Color mediumPriorityColor = new Color(0.5f, 0.5f, 0.8f);
    [SerializeField] private Color highPriorityColor = new Color(0.8f, 0.8f, 0.5f);
    [SerializeField] private Color urgentPriorityColor = new Color(0.8f, 0.5f, 0.5f);
    
    [Header("Animation")]
    [SerializeField] private float progressAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve progressAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Interaction")]
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private float grabScale = 1.1f;
    [SerializeField] private float grabDuration = 0.2f;
    
    private TaskManager.Task _task;
    private TaskManager _taskManager;
    private float _targetProgress;
    private float _currentProgress;
    private float _progressAnimationTime;
    private bool _isAnimatingProgress;
    private Vector3 _originalScale;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private bool _isGrabbed;
    
    // Events
    public Action<TaskItemUI> OnTaskCompleted;
    public Action<TaskItemUI> OnTaskDeleted;
    
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
    }
    
    private void Update()
    {
        if (_isAnimatingProgress)
        {
            _progressAnimationTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(_progressAnimationTime / progressAnimationDuration);
            float curveValue = progressAnimationCurve.Evaluate(normalizedTime);
            _currentProgress = Mathf.Lerp(_currentProgress, _targetProgress, curveValue);
            
            if (progressBar != null)
            {
                progressBar.fillAmount = _currentProgress;
            }
            
            if (Mathf.Approximately(normalizedTime, 1f))
            {
                _isAnimatingProgress = false;
            }
        }
    }
    
    /// <summary>
    /// Initializes the task item with data
    /// </summary>
    /// <param name="task">Task data</param>
    /// <param name="taskManager">Task manager reference</param>
    public void Initialize(TaskManager.Task task, TaskManager taskManager)
    {
        _task = task;
        _taskManager = taskManager;
        
        UpdateUI();
    }
    
    /// <summary>
    /// Updates the UI based on the current task data
    /// </summary>
    public void UpdateUI()
    {
        if (_task == null)
            return;
        
        if (titleText != null)
        {
            titleText.text = _task.title;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = _task.description;
            descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(_task.description));
        }
        
        if (priorityIndicator != null)
        {
            priorityIndicator.color = GetPriorityColor(_task.priority);
        }
        
        SetProgress(_task.progress);
        
        if (completeButton != null)
        {
            completeButton.gameObject.SetActive(!_task.isCompleted);
        }
    }
    
    /// <summary>
    /// Sets the progress of the task
    /// </summary>
    /// <param name="progress">Progress value (0-1)</param>
    /// <param name="animate">Whether to animate the progress change</param>
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
            _currentProgress = _targetProgress;
            if (progressBar != null)
            {
                progressBar.fillAmount = _currentProgress;
            }
        }
    }
    
    /// <summary>
    /// Completes the task
    /// </summary>
    public void CompleteTask()
    {
        if (_task == null || _task.isCompleted || _taskManager == null)
            return;
        
        _taskManager.CompleteTask(_task.id);
        
        // Play completion effects
        if (completionParticles != null)
        {
            completionParticles.SetActive(true);
        }
        
        if (audioSource != null)
        {
            audioSource.Play();
        }
        
        // Update UI
        SetProgress(1f, true);
        
        if (completeButton != null)
        {
            completeButton.gameObject.SetActive(false);
        }
        
        OnTaskCompleted?.Invoke(this);
    }
    
    /// <summary>
    /// Deletes the task
    /// </summary>
    public void DeleteTask()
    {
        if (_task == null || _taskManager == null)
            return;
        
        _taskManager.RemoveTask(_task.id);
        
        OnTaskDeleted?.Invoke(this);
    }
    
    /// <summary>
    /// Gets the color for a priority level
    /// </summary>
    /// <param name="priority">Priority level</param>
    /// <returns>Color for the priority</returns>
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