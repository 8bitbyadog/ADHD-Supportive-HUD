using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages tasks for the ADHD Focus Assistant
/// Handles task creation, storage, filtering, and prioritization
/// </summary>
public class TaskManager : MonoBehaviour
{
    [Serializable]
    public class Task
    {
        public string id;
        public string title;
        public string description;
        public TaskPriority priority;
        public bool isCompleted;
        public DateTime creationTime;
        public DateTime? completionTime;
        public float progress; // 0-1
        public List<string> tags;
        
        public Task(string title, string description = "", TaskPriority priority = TaskPriority.Medium)
        {
            this.id = Guid.NewGuid().ToString();
            this.title = title;
            this.description = description;
            this.priority = priority;
            this.isCompleted = false;
            this.creationTime = DateTime.Now;
            this.completionTime = null;
            this.progress = 0f;
            this.tags = new List<string>();
        }
    }
    
    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }
    
    [Header("Task Settings")]
    [SerializeField] private bool loadTasksOnStart = true;
    [SerializeField] private bool autoSaveTasks = true;
    [SerializeField] private float autoSaveInterval = 60f; // seconds
    
    [Header("Events")]
    [SerializeField] private bool enableEvents = true;
    public Action<Task> OnTaskUpdated;
    public Action<Task> OnTaskCompleted;
    public Action<Task> OnTaskRemoved;
    public Action<List<Task>> OnTasksLoaded;
    
    private List<Task> _tasks = new List<Task>();
    private float _timeSinceLastSave = 0f;
    
    private void Start()
    {
        if (loadTasksOnStart)
        {
            LoadTasks();
        }
    }
    
    private void Update()
    {
        if (autoSaveTasks)
        {
            _timeSinceLastSave += Time.deltaTime;
            
            if (_timeSinceLastSave >= autoSaveInterval)
            {
                SaveTasks();
                _timeSinceLastSave = 0f;
            }
        }
    }
    
    /// <summary>
    /// Creates a new task and adds it to the task list
    /// </summary>
    /// <param name="title">Task title</param>
    /// <param name="description">Task description (optional)</param>
    /// <param name="priority">Task priority (optional, defaults to Medium)</param>
    /// <returns>The newly created task</returns>
    public Task CreateTask(string title, string description = "", TaskPriority priority = TaskPriority.Medium)
    {
        if (string.IsNullOrEmpty(title))
        {
            Debug.LogWarning("Cannot create task with empty title");
            return null;
        }
        
        var task = new Task(title, description, priority);
        _tasks.Add(task);
        
        if (autoSaveTasks)
        {
            SaveTasks();
        }
        
        if (enableEvents && OnTaskUpdated != null)
        {
            OnTaskUpdated(task);
        }
        
        return task;
    }
    
    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="taskId">ID of the task to update</param>
    /// <param name="title">New title (null to keep unchanged)</param>
    /// <param name="description">New description (null to keep unchanged)</param>
    /// <param name="priority">New priority (null to keep unchanged)</param>
    /// <param name="progress">New progress (-1 to keep unchanged)</param>
    /// <returns>True if the task was updated successfully</returns>
    public bool UpdateTask(string taskId, string title = null, string description = null, TaskPriority? priority = null, float progress = -1)
    {
        var task = _tasks.FirstOrDefault(t => t.id == taskId);
        if (task == null)
        {
            Debug.LogWarning($"Cannot update task: Task with ID {taskId} not found");
            return false;
        }
        
        bool wasUpdated = false;
        
        if (title != null)
        {
            task.title = title;
            wasUpdated = true;
        }
        
        if (description != null)
        {
            task.description = description;
            wasUpdated = true;
        }
        
        if (priority.HasValue)
        {
            task.priority = priority.Value;
            wasUpdated = true;
        }
        
        if (progress >= 0)
        {
            task.progress = Mathf.Clamp01(progress);
            wasUpdated = true;
        }
        
        if (wasUpdated)
        {
            if (autoSaveTasks)
            {
                SaveTasks();
            }
            
            if (enableEvents && OnTaskUpdated != null)
            {
                OnTaskUpdated(task);
            }
        }
        
        return wasUpdated;
    }
    
    /// <summary>
    /// Marks a task as completed
    /// </summary>
    /// <param name="taskId">ID of the task to complete</param>
    /// <returns>True if the task was completed successfully</returns>
    public bool CompleteTask(string taskId)
    {
        var task = _tasks.FirstOrDefault(t => t.id == taskId);
        if (task == null)
        {
            Debug.LogWarning($"Cannot complete task: Task with ID {taskId} not found");
            return false;
        }
        
        if (task.isCompleted)
        {
            return false; // Already completed
        }
        
        task.isCompleted = true;
        task.completionTime = DateTime.Now;
        task.progress = 1f;
        
        if (autoSaveTasks)
        {
            SaveTasks();
        }
        
        if (enableEvents)
        {
            if (OnTaskUpdated != null)
            {
                OnTaskUpdated(task);
            }
            
            if (OnTaskCompleted != null)
            {
                OnTaskCompleted(task);
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Removes a task from the task list
    /// </summary>
    /// <param name="taskId">ID of the task to remove</param>
    /// <returns>True if the task was removed successfully</returns>
    public bool RemoveTask(string taskId)
    {
        var task = _tasks.FirstOrDefault(t => t.id == taskId);
        if (task == null)
        {
            Debug.LogWarning($"Cannot remove task: Task with ID {taskId} not found");
            return false;
        }
        
        _tasks.Remove(task);
        
        if (autoSaveTasks)
        {
            SaveTasks();
        }
        
        if (enableEvents && OnTaskRemoved != null)
        {
            OnTaskRemoved(task);
        }
        
        return true;
    }
    
    /// <summary>
    /// Gets all tasks
    /// </summary>
    /// <returns>A list of all tasks</returns>
    public List<Task> GetAllTasks()
    {
        return new List<Task>(_tasks);
    }
    
    /// <summary>
    /// Gets all active (non-completed) tasks
    /// </summary>
    /// <returns>A list of active tasks</returns>
    public List<Task> GetActiveTasks()
    {
        return _tasks.Where(t => !t.isCompleted).ToList();
    }
    
    /// <summary>
    /// Gets all completed tasks
    /// </summary>
    /// <returns>A list of completed tasks</returns>
    public List<Task> GetCompletedTasks()
    {
        return _tasks.Where(t => t.isCompleted).ToList();
    }
    
    /// <summary>
    /// Gets tasks filtered by priority
    /// </summary>
    /// <param name="priority">The priority to filter by</param>
    /// <returns>A list of tasks with the specified priority</returns>
    public List<Task> GetTasksByPriority(TaskPriority priority)
    {
        return _tasks.Where(t => t.priority == priority).ToList();
    }
    
    /// <summary>
    /// Gets tasks filtered by tag
    /// </summary>
    /// <param name="tag">The tag to filter by</param>
    /// <returns>A list of tasks with the specified tag</returns>
    public List<Task> GetTasksByTag(string tag)
    {
        return _tasks.Where(t => t.tags.Contains(tag)).ToList();
    }
    
    /// <summary>
    /// Saves all tasks to PlayerPrefs
    /// </summary>
    public void SaveTasks()
    {
        try
        {
            var taskList = new TaskList { tasks = _tasks };
            string json = JsonUtility.ToJson(taskList);
            PlayerPrefs.SetString("Tasks", json);
            PlayerPrefs.Save();
            
            Debug.Log($"Saved {_tasks.Count} tasks to PlayerPrefs");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving tasks: {e.Message}");
        }
    }
    
    /// <summary>
    /// Loads all tasks from PlayerPrefs
    /// </summary>
    public void LoadTasks()
    {
        try
        {
            if (PlayerPrefs.HasKey("Tasks"))
            {
                string json = PlayerPrefs.GetString("Tasks");
                var taskList = JsonUtility.FromJson<TaskList>(json);
                
                if (taskList != null && taskList.tasks != null)
                {
                    _tasks = taskList.tasks;
                    Debug.Log($"Loaded {_tasks.Count} tasks from PlayerPrefs");
                    
                    if (enableEvents && OnTasksLoaded != null)
                    {
                        OnTasksLoaded(_tasks);
                    }
                }
            }
            else
            {
                Debug.Log("No saved tasks found");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading tasks: {e.Message}");
        }
    }
    
    /// <summary>
    /// Clears all tasks from memory and PlayerPrefs
    /// </summary>
    public void ClearAllTasks()
    {
        _tasks.Clear();
        PlayerPrefs.DeleteKey("Tasks");
        PlayerPrefs.Save();
        Debug.Log("All tasks cleared");
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && autoSaveTasks)
        {
            // Save when app is paused (backgrounded)
            SaveTasks();
        }
    }
    
    private void OnApplicationQuit()
    {
        if (autoSaveTasks)
        {
            // Save when app is quit
            SaveTasks();
        }
    }
    
    [Serializable]
    private class TaskList
    {
        public List<Task> tasks;
    }
}
