using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the task panel UI and task items
/// </summary>
public class TaskPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private Transform taskContainer;
    [SerializeField] private TMP_InputField taskTitleInput;
    [SerializeField] private TMP_Dropdown priorityDropdown;
    [SerializeField] private Button addTaskButton;
    [SerializeField] private Button clearCompletedButton;
    [SerializeField] private Toggle showCompletedToggle;
    
    [Header("Layout")]
    [SerializeField] private float taskSpacing = 10f;
    [SerializeField] private float taskAppearDuration = 0.3f;
    [SerializeField] private float taskDisappearDuration = 0.2f;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem addTaskParticles;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip addTaskSound;
    [SerializeField] private AudioClip completeTaskSound;
    [SerializeField] private AudioClip deleteTaskSound;
    
    private Dictionary<string, TaskItemUI> _taskItems = new Dictionary<string, TaskItemUI>();
    private bool _showCompleted = false;
    
    private void Start()
    {
        if (taskManager == null)
        {
            taskManager = FindObjectOfType<TaskManager>();
        }
        
        if (taskManager != null)
        {
            // Subscribe to task events
            taskManager.OnTaskAdded += OnTaskAdded;
            taskManager.OnTaskUpdated += OnTaskUpdated;
            taskManager.OnTaskCompleted += OnTaskCompleted;
            taskManager.OnTaskRemoved += OnTaskRemoved;
            taskManager.OnTasksLoaded += OnTasksLoaded;
        }
        
        if (addTaskButton != null)
        {
            addTaskButton.onClick.AddListener(AddTask);
        }
        
        if (clearCompletedButton != null)
        {
            clearCompletedButton.onClick.AddListener(ClearCompletedTasks);
        }
        
        if (showCompletedToggle != null)
        {
            showCompletedToggle.onValueChanged.AddListener(OnShowCompletedChanged);
            _showCompleted = showCompletedToggle.isOn;
        }
        
        // Initialize priority dropdown
        if (priorityDropdown != null)
        {
            priorityDropdown.ClearOptions();
            List<string> options = new List<string>
            {
                "Low",
                "Medium",
                "High",
                "Urgent"
            };
            priorityDropdown.AddOptions(options);
            priorityDropdown.value = 1; // Default to Medium
        }
        
        // Load existing tasks
        RefreshTaskList();
    }
    
    private void OnDestroy()
    {
        if (taskManager != null)
        {
            // Unsubscribe from task events
            taskManager.OnTaskAdded -= OnTaskAdded;
            taskManager.OnTaskUpdated -= OnTaskUpdated;
            taskManager.OnTaskCompleted -= OnTaskCompleted;
            taskManager.OnTaskRemoved -= OnTaskRemoved;
            taskManager.OnTasksLoaded -= OnTasksLoaded;
        }
    }
    
    /// <summary>
    /// Adds a new task
    /// </summary>
    public void AddTask()
    {
        if (taskManager == null || taskTitleInput == null || string.IsNullOrWhiteSpace(taskTitleInput.text))
            return;
        
        // Get task details
        string title = taskTitleInput.text.Trim();
        TaskManager.TaskPriority priority = TaskManager.TaskPriority.Medium;
        
        if (priorityDropdown != null)
        {
            priority = (TaskManager.TaskPriority)priorityDropdown.value;
        }
        
        // Create task
        taskManager.CreateTask(title, "", priority);
        
        // Clear input
        taskTitleInput.text = "";
        
        // Play effects
        if (addTaskParticles != null)
        {
            addTaskParticles.Play();
        }
        
        if (audioSource != null && addTaskSound != null)
        {
            audioSource.PlayOneShot(addTaskSound);
        }
    }
    
    /// <summary>
    /// Clears all completed tasks
    /// </summary>
    public void ClearCompletedTasks()
    {
        if (taskManager == null)
            return;
        
        List<TaskManager.Task> completedTasks = taskManager.GetCompletedTasks();
        foreach (var task in completedTasks)
        {
            taskManager.RemoveTask(task.id);
        }
    }
    
    /// <summary>
    /// Refreshes the task list UI
    /// </summary>
    public void RefreshTaskList()
    {
        if (taskManager == null)
            return;
        
        // Get tasks
        List<TaskManager.Task> tasks = _showCompleted 
            ? taskManager.GetAllTasks() 
            : taskManager.GetActiveTasks();
        
        // Sort tasks by priority and completion status
        tasks.Sort((a, b) => 
        {
            if (a.isCompleted != b.isCompleted)
                return a.isCompleted ? 1 : -1;
            
            return ((int)b.priority).CompareTo((int)a.priority);
        });
        
        // Create a set of current task IDs
        HashSet<string> currentTaskIds = new HashSet<string>();
        foreach (var task in tasks)
        {
            currentTaskIds.Add(task.id);
        }
        
        // Remove task items that are no longer in the list
        List<string> tasksToRemove = new List<string>();
        foreach (var kvp in _taskItems)
        {
            if (!currentTaskIds.Contains(kvp.Key))
            {
                tasksToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var taskId in tasksToRemove)
        {
            RemoveTaskItem(taskId);
        }
        
        // Add or update task items
        for (int i = 0; i < tasks.Count; i++)
        {
            var task = tasks[i];
            
            if (_taskItems.TryGetValue(task.id, out TaskItemUI taskItem))
            {
                // Update existing task item
                taskItem.UpdateUI();
                
                // Update position
                taskItem.transform.SetSiblingIndex(i);
            }
            else
            {
                // Create new task item
                CreateTaskItem(task, i);
            }
        }
    }
    
    /// <summary>
    /// Creates a new task item
    /// </summary>
    /// <param name="task">Task data</param>
    /// <param name="index">Index in the container</param>
    private void CreateTaskItem(TaskManager.Task task, int index)
    {
        if (taskItemPrefab == null || taskContainer == null)
            return;
        
        // Instantiate task item
        GameObject taskItemObj = Instantiate(taskItemPrefab, taskContainer);
        taskItemObj.name = $"Task_{task.id}";
        
        // Set position
        taskItemObj.transform.SetSiblingIndex(index);
        
        // Get task item component
        TaskItemUI taskItem = taskItemObj.GetComponent<TaskItemUI>();
        if (taskItem != null)
        {
            // Initialize task item
            taskItem.Initialize(task, taskManager);
            
            // Subscribe to events
            taskItem.OnTaskCompleted += OnTaskItemCompleted;
            taskItem.OnTaskDeleted += OnTaskItemDeleted;
            
            // Add to dictionary
            _taskItems[task.id] = taskItem;
            
            // Animate appearance
            taskItemObj.transform.localScale = Vector3.zero;
            LeanTween.scale(taskItemObj, Vector3.one, taskAppearDuration)
                .setEase(LeanTweenType.easeOutBack)
                .setDelay(index * 0.05f);
        }
    }
    
    /// <summary>
    /// Removes a task item
    /// </summary>
    /// <param name="taskId">Task ID</param>
    private void RemoveTaskItem(string taskId)
    {
        if (!_taskItems.TryGetValue(taskId, out TaskItemUI taskItem))
            return;
        
        // Unsubscribe from events
        taskItem.OnTaskCompleted -= OnTaskItemCompleted;
        taskItem.OnTaskDeleted -= OnTaskItemDeleted;
        
        // Animate disappearance
        LeanTween.scale(taskItem.gameObject, Vector3.zero, taskDisappearDuration)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(() => 
            {
                Destroy(taskItem.gameObject);
                _taskItems.Remove(taskId);
            });
    }
    
    /// <summary>
    /// Called when a task is added
    /// </summary>
    /// <param name="task">Added task</param>
    private void OnTaskAdded(TaskManager.Task task)
    {
        RefreshTaskList();
    }
    
    /// <summary>
    /// Called when a task is updated
    /// </summary>
    /// <param name="task">Updated task</param>
    private void OnTaskUpdated(TaskManager.Task task)
    {
        if (_taskItems.TryGetValue(task.id, out TaskItemUI taskItem))
        {
            taskItem.UpdateUI();
        }
        
        RefreshTaskList();
    }
    
    /// <summary>
    /// Called when a task is completed
    /// </summary>
    /// <param name="task">Completed task</param>
    private void OnTaskCompleted(TaskManager.Task task)
    {
        if (_taskItems.TryGetValue(task.id, out TaskItemUI taskItem))
        {
            taskItem.UpdateUI();
        }
        
        if (!_showCompleted)
        {
            RefreshTaskList();
        }
    }
    
    /// <summary>
    /// Called when a task is removed
    /// </summary>
    /// <param name="task">Removed task</param>
    private void OnTaskRemoved(TaskManager.Task task)
    {
        RemoveTaskItem(task.id);
    }
    
    /// <summary>
    /// Called when tasks are loaded
    /// </summary>
    /// <param name="tasks">Loaded tasks</param>
    private void OnTasksLoaded(List<TaskManager.Task> tasks)
    {
        RefreshTaskList();
    }
    
    /// <summary>
    /// Called when a task item is completed
    /// </summary>
    /// <param name="taskItem">Completed task item</param>
    private void OnTaskItemCompleted(TaskItemUI taskItem)
    {
        if (audioSource != null && completeTaskSound != null)
        {
            audioSource.PlayOneShot(completeTaskSound);
        }
    }
    
    /// <summary>
    /// Called when a task item is deleted
    /// </summary>
    /// <param name="taskItem">Deleted task item</param>
    private void OnTaskItemDeleted(TaskItemUI taskItem)
    {
        if (audioSource != null && deleteTaskSound != null)
        {
            audioSource.PlayOneShot(deleteTaskSound);
        }
    }
    
    /// <summary>
    /// Called when the show completed toggle is changed
    /// </summary>
    /// <param name="showCompleted">Whether to show completed tasks</param>
    private void OnShowCompletedChanged(bool showCompleted)
    {
        _showCompleted = showCompleted;
        RefreshTaskList();
    }
} 