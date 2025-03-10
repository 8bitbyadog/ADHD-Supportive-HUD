using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Task
{
    public string title;
    public bool isCompleted;
    public DateTime createdAt;
    public int priority;

    public Task(string title, int priority)
    {
        this.title = title;
        this.priority = priority;
        this.isCompleted = false;
        this.createdAt = DateTime.Now;
    }
}

public class TaskManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HUDManager hudManager;
    
    [Header("Task Settings")]
    [SerializeField] private int maxDisplayedTasks = 3;
    
    private List<Task> tasks = new List<Task>();
    private List<Task> displayedTasks = new List<Task>();

    private void Start()
    {
        if (hudManager == null)
        {
            hudManager = FindObjectOfType<HUDManager>();
        }
    }

    public void AddTask(string title, int priority)
    {
        Task newTask = new Task(title, priority);
        tasks.Add(newTask);
        UpdateDisplayedTasks();
    }

    public void CompleteTask(int taskIndex)
    {
        if (taskIndex >= 0 && taskIndex < tasks.Count)
        {
            tasks[taskIndex].isCompleted = true;
            UpdateDisplayedTasks();
        }
    }

    public void RemoveTask(int taskIndex)
    {
        if (taskIndex >= 0 && taskIndex < tasks.Count)
        {
            tasks.RemoveAt(taskIndex);
            UpdateDisplayedTasks();
        }
    }

    private void UpdateDisplayedTasks()
    {
        // Sort tasks by priority and completion status
        tasks.Sort((a, b) =>
        {
            if (a.isCompleted != b.isCompleted)
                return a.isCompleted ? 1 : -1;
            return b.priority.CompareTo(a.priority);
        });

        // Get top N incomplete tasks
        displayedTasks.Clear();
        foreach (Task task in tasks)
        {
            if (!task.isCompleted && displayedTasks.Count < maxDisplayedTasks)
            {
                displayedTasks.Add(task);
            }
        }

        // Update HUD
        UpdateHUDDisplay();
    }

    private void UpdateHUDDisplay()
    {
        string[] taskStrings = new string[displayedTasks.Count];
        for (int i = 0; i < displayedTasks.Count; i++)
        {
            taskStrings[i] = $"{i + 1}. {displayedTasks[i].title}";
        }
        hudManager.UpdateTaskDisplay(taskStrings);
    }

    // Hand tracking interaction methods
    public void OnTaskTapped(int taskIndex)
    {
        CompleteTask(taskIndex);
    }

    // Debug methods
    public void AddSampleTasks()
    {
        AddTask("Complete project documentation", 3);
        AddTask("Review code changes", 2);
        AddTask("Test HUD functionality", 1);
        AddTask("Update README", 2);
    }
} 