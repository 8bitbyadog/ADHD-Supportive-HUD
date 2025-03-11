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
    [SerializeField] private int maxTasks = 50;
    [SerializeField] private bool autoSaveTasks = true;
    [SerializeField] private float autoSaveInterval = 60f; // seconds
    
    [Header("Events")]
    public Action<Task> OnTaskAdded;
    public Action<Task> OnTaskUpdated;
    public Action<Task> OnTaskCompleted;
    public Action<Task> OnTaskRemoved;
    public Action<List<Task>> OnTasksLoaded;
    
    private List<Task> _tasks = new List<Task>();
    private float _timeSinceLastSave = 0f;
    
    private void Start()
    {
        LoadTasks();
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
    /// Creates a new task
    /// </summary>
    /// <param name="title">Task title</param>
    /// <param name="description">Task description</param>
    /// <param name="priority">Task priority</param>
    /// <returns>The created task</returns>
    public Task CreateTask(string title, string description = "", TaskPriority priority = TaskPriority.Medium)
    {
        Task task = new Task(title, description, priority);
        _tasks.Add(task);
        
        if (_tasks.Count > maxTasks)
        {
            // Remove oldest completed task if we exceed the limit
            Task oldestCompleted = _tasks.Where(t => t.isCompleted).OrderBy(t => t.completionTime).FirstOrDefault();
            if (oldestCompleted != null)
            {
                _tasks.Remove(oldestCompleted);
            }
        }
        
        OnTaskAdded?.Invoke(task);
        
        if (autoSaveTasks)
        {
            SaveTasks();
        }
        
        return task;
    }
    
    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <param name="title">New title (null to keep current)</param>
    /// <param name="description">New description (null to keep current)</param>
    /// <param name="priority">New priority (null to keep current)</param>
    /// <param name="progress">New progress (-1 to keep current)</param>
    /// <returns>True if the task was updated</returns>
    public bool UpdateTask(string taskId, string title = null, string description = null, TaskPriority? priority = null, float progress = -1)
    {
        Task task = _tasks.FirstOrDefault(t => t.id == taskId);
        if (task == null)
            return false;
        
        bool changed = false;
        
        if (title != null && task.title != title)
        {
            task.title = title;
            changed = true;
        }
        
        if (description != null && task.description != description)
        {
            task.description = description;
            changed = true;
        }
        
        if (priority.HasValue && task.priority != priority.Value)
        {
            task.priority = priority.Value;
            changed = true;
        }
        
        if (progress >= 0 && progress <= 1 && Math.Abs(task.progress - progress) > 0.01f)
        {
            task.progress = progress;
            changed = true;
        }
        
        if (changed)
        {
            OnTaskUpdated?.Invoke(task);
            
            if (autoSaveTasks)
            {
                SaveTasks();
            }
        }
        
        return changed;
    }
    
    /// <summary>
    /// Completes a task
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <returns>True if the task was completed</returns>
    public bool CompleteTask(string taskId)
    {
        Task task = _tasks.FirstOrDefault(t => t.id == taskId);
        if (task == null || task.isCompleted)
            return false;
        
        task.isCompleted = true;
        task.completionTime = DateTime.Now;
        task.progress = 1f;
        
        OnTaskCompleted?.Invoke(task);
        
        if (autoSaveTasks)
        {
            SaveTasks();
        }
        
        return true;
    }
    
    /// <summary>
    /// Removes a task
    /// </summary>
    /// <param name="taskId">Task ID</param>
    /// <returns>True if the task was removed</returns>
    public bool RemoveTask(string taskId)
    {
        Task task = _tasks.FirstOrDefault(t => t.id == taskId);
        if (task == null)
            return false;
        
        _tasks.Remove(task);
        
        OnTaskRemoved?.Invoke(task);
        
        if (autoSaveTasks)
        {
            SaveTasks();
        }
        
        return true;
    }
    
    /// <summary>
    /// Gets all tasks
    /// </summary>
    /// <returns>List of all tasks</returns>
    public List<Task> GetAllTasks()
    {
        return new List<Task>(_tasks);
    }
    
    /// <summary>
    /// Gets active (non-completed) tasks
    /// </summary>
    /// <returns>List of active tasks</returns>
    public List<Task> GetActiveTasks()
    {
        return _tasks.Where(t => !t.isCompleted).ToList();
    }
    
    /// <summary>
    /// Gets completed tasks
    /// </summary>
    /// <returns>List of completed tasks</returns>
    public List<Task> GetCompletedTasks()
    {
        return _tasks.Where(t => t.isCompleted).ToList();
    }
    
    /// <summary>
    /// Gets tasks by priority
    /// </summary>
    /// <param name="priority">Priority to filter by</param>
    /// <returns>List of tasks with the specified priority</returns>
    public List<Task> GetTasksByPriority(TaskPriority priority)
    {
        return _tasks.Where(t => t.priority == priority).ToList();
    }
    
    /// <summary>
    /// Gets tasks by tag
    /// </summary>
    /// <param name="tag">Tag to filter by</param>
    /// <returns>List of tasks with the specified tag</returns>
    public List<Task> GetTasksByTag(string tag)
    {
        return _tasks.Where(t => t.tags.Contains(tag)).ToList();
    }
    
    /// <summary>
    /// Saves tasks to PlayerPrefs
    /// </summary>
    public void SaveTasks()
    {
        try
        {
            string json = JsonUtility.ToJson(new TaskList { tasks = _tasks });
            PlayerPrefs.SetString("ADHDFocusAssistant_Tasks", json);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"[TaskManager] Error saving tasks: {e.Message}");
        }
    }
    
    /// <summary>
    /// Loads tasks from PlayerPrefs
    /// </summary>
    public void LoadTasks()
    {
        try
        {
            string json = PlayerPrefs.GetString("ADHDFocusAssistant_Tasks", "");
            if (!string.IsNullOrEmpty(json))
            {
                TaskList taskList = JsonUtility.FromJson<TaskList>(json);
                if (taskList != null && taskList.tasks != null)
                {
                    _tasks = taskList.tasks;
                    OnTasksLoaded?.Invoke(_tasks);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[TaskManager] Error loading tasks: {e.Message}");
        }
    }
    
    /// <summary>
    /// Clears all tasks
    /// </summary>
    public void ClearAllTasks()
    {
        _tasks.Clear();
        PlayerPrefs.DeleteKey("ADHDFocusAssistant_Tasks");
        PlayerPrefs.Save();
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && autoSaveTasks)
        {
            SaveTasks();
        }
    }
    
    private void OnApplicationQuit()
    {
        if (autoSaveTasks)
        {
            SaveTasks();
        }
    }
    
    [Serializable]
    private class TaskList
    {
        public List<Task> tasks;
    }
} 