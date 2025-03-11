using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simplified HUD Manager that doesn't require external dependencies
/// Use this for initial setup until all packages are installed
/// </summary>
public class SimpleHUDManager : MonoBehaviour
{
    [Header("Task Management")]
    [SerializeField] private Transform taskContainer;
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private TMP_InputField taskInputField;
    [SerializeField] private Button addTaskButton;
    
    [Header("HUD Positioning")]
    [SerializeField] private Transform headTransform;
    [SerializeField] private float followDistance = 1.0f;
    [SerializeField] private float followHeight = -0.2f;
    [SerializeField] private float followSpeed = 2.0f;
    [SerializeField] private bool followHead = true;
    
    private List<TaskItem> activeTasks = new List<TaskItem>();
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    
    private void Start()
    {
        // Find camera if headTransform is not set
        if (headTransform == null)
        {
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                headTransform = mainCamera.transform;
            }
        }
        
        // Set up task button
        if (addTaskButton != null)
        {
            addTaskButton.onClick.AddListener(AddTask);
        }
        
        // Start following head if enabled
        if (followHead && headTransform != null)
        {
            StartCoroutine(FollowHeadRoutine());
        }
    }
    
    private IEnumerator FollowHeadRoutine()
    {
        while (followHead && headTransform != null)
        {
            // Calculate position in front of the head
            Vector3 headForward = headTransform.forward;
            headForward.y = 0; // Keep UI level with ground
            headForward.Normalize();
            
            targetPosition = headTransform.position + headForward * followDistance;
            targetPosition.y = headTransform.position.y + followHeight;
            
            // Calculate rotation to face the head
            targetRotation = Quaternion.LookRotation(targetPosition - headTransform.position);
            
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
    
    public void AddTask()
    {
        if (taskInputField == null || string.IsNullOrEmpty(taskInputField.text) || taskItemPrefab == null || taskContainer == null)
            return;
        
        // Create task
        GameObject taskObj = Instantiate(taskItemPrefab, taskContainer);
        TaskItem taskItem = taskObj.GetComponent<TaskItem>();
        
        if (taskItem != null)
        {
            // Get random priority for testing
            TaskPriority priority = (TaskPriority)Random.Range(0, 4);
            
            // Initialize task
            taskItem.Initialize(taskInputField.text, priority);
            
            // Subscribe to completion event
            taskItem.OnTaskCompleted += OnTaskCompleted;
            
            // Add to active tasks
            activeTasks.Add(taskItem);
            
            // Clear input field
            taskInputField.text = "";
            taskInputField.ActivateInputField();
        }
    }
    
    private void OnTaskCompleted(TaskItem task)
    {
        Debug.Log($"Task completed: {task.name}");
        
        // Remove from active tasks
        if (activeTasks.Contains(task))
        {
            activeTasks.Remove(task);
        }
    }
    
    // Sample method to demonstrate functionality
    public void AddSampleTasks()
    {
        string[] sampleTasks = new string[]
        {
            "Create UI layout",
            "Implement passthrough",
            "Test on device",
            "Optimize performance"
        };
        
        foreach (var taskText in sampleTasks)
        {
            if (taskInputField != null)
            {
                taskInputField.text = taskText;
                AddTask();
            }
        }
    }
} 