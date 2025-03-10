using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class TaskItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image priorityIndicator;
    [SerializeField] private Image progressBar;
    [SerializeField] private Button completeButton;
    [SerializeField] private ParticleSystem completionParticles;
    
    [Header("Visual Settings")]
    [SerializeField] private Color[] priorityColors;
    [SerializeField] private float progressAnimationSpeed = 2f;
    [SerializeField] private AnimationCurve progressCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio")]
    [SerializeField] private AudioClip completeSound;
    [SerializeField] private AudioClip dragSound;
    
    private TaskPriority priority;
    private bool isCompleted;
    private float currentProgress;
    private float targetProgress;
    private Vector3 dragOffset;
    private Transform originalParent;
    private Vector3 originalPosition;
    private AudioSource audioSource;
    private bool isDragging;
    
    public bool IsCompleted => isCompleted;
    public event Action<TaskItem> OnTaskCompleted;
    
    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        SetupUI();
    }
    
    private void SetupUI()
    {
        if (completeButton != null)
        {
            completeButton.onClick.AddListener(CompleteTask);
        }
    }
    
    public void Initialize(string title, TaskPriority taskPriority)
    {
        titleText.text = title;
        priority = taskPriority;
        isCompleted = false;
        currentProgress = 0f;
        targetProgress = 0f;
        
        // Set priority color
        if (priorityIndicator != null && priorityColors.Length > (int)priority)
        {
            priorityIndicator.color = priorityColors[(int)priority];
        }
    }
    
    private void Update()
    {
        // Animate progress bar
        if (Mathf.Abs(currentProgress - targetProgress) > 0.01f)
        {
            currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * progressAnimationSpeed);
            UpdateProgressBar();
        }
    }
    
    private void UpdateProgressBar()
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = progressCurve.Evaluate(currentProgress);
        }
    }
    
    public void CompleteTask()
    {
        if (!isCompleted)
        {
            isCompleted = true;
            targetProgress = 1f;
            
            // Play effects
            if (completionParticles != null)
            {
                completionParticles.Play();
            }
            
            if (completeSound != null)
            {
                audioSource.PlayOneShot(completeSound);
            }
            
            // Notify listeners
            OnTaskCompleted?.Invoke(this);
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isCompleted) return;
        
        isDragging = true;
        originalParent = transform.parent;
        originalPosition = transform.position;
        dragOffset = transform.position - Input.mousePosition;
        
        // Play sound
        if (dragSound != null)
        {
            audioSource.PlayOneShot(dragSound);
        }
        
        // Visual feedback
        transform.SetAsLastSibling();
        GetComponent<CanvasGroup>().alpha = 0.8f;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        transform.position = Input.mousePosition + dragOffset;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        isDragging = false;
        GetComponent<CanvasGroup>().alpha = 1f;
        
        // Check if dropped on a valid target
        var results = new RaycastResult[4];
        var raycaster = GetComponent<GraphicRaycaster>();
        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        
        raycaster.Raycast(pointerData, results);
        
        bool validDrop = false;
        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("TaskDropZone"))
            {
                transform.SetParent(result.gameObject.transform);
                validDrop = true;
                break;
            }
        }
        
        if (!validDrop)
        {
            // Return to original position
            transform.SetParent(originalParent);
            transform.position = originalPosition;
        }
    }
    
    public void SetPriority(TaskPriority newPriority)
    {
        priority = newPriority;
        if (priorityIndicator != null && priorityColors.Length > (int)priority)
        {
            priorityIndicator.color = priorityColors[(int)priority];
        }
    }
} 