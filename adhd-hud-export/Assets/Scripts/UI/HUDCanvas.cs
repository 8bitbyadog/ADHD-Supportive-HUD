using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HUDCanvas : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform taskContainer;
    [SerializeField] private RectTransform pomodoroContainer;
    [SerializeField] private RectTransform healthContainer;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private GameObject pomodoroDisplayPrefab;
    [SerializeField] private GameObject healthDisplayPrefab;

    [Header("Visual Settings")]
    [SerializeField] private Color workModeColor = new Color(0.2f, 0.6f, 1f); // Blue
    [SerializeField] private Color breakModeColor = new Color(0.2f, 0.8f, 0.2f); // Green
    [SerializeField] private float elementSpacing = 10f;
    [SerializeField] private float containerPadding = 20f;

    private List<GameObject> taskItems = new List<GameObject>();
    private GameObject pomodoroDisplay;
    private GameObject healthDisplay;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        // Create containers if they don't exist
        if (taskContainer == null)
        {
            taskContainer = CreateContainer("TaskContainer");
        }
        if (pomodoroContainer == null)
        {
            pomodoroContainer = CreateContainer("PomodoroContainer");
        }
        if (healthContainer == null)
        {
            healthContainer = CreateContainer("HealthContainer");
        }

        // Create UI elements
        CreatePomodoroDisplay();
        CreateHealthDisplay();
    }

    private RectTransform CreateContainer(string name)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(transform, false);
        
        RectTransform rectTransform = container.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        VerticalLayoutGroup layoutGroup = container.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = elementSpacing;
        layoutGroup.padding = new RectOffset(
            (int)containerPadding,
            (int)containerPadding,
            (int)containerPadding,
            (int)containerPadding
        );

        return rectTransform;
    }

    private void CreatePomodoroDisplay()
    {
        if (pomodoroDisplayPrefab != null)
        {
            pomodoroDisplay = Instantiate(pomodoroDisplayPrefab, pomodoroContainer);
            var uiComponent = pomodoroDisplay.AddComponent<UIInteractionComponent>();
            uiComponent.interactionType = UIInteractionType.Pomodoro;
        }
    }

    private void CreateHealthDisplay()
    {
        if (healthDisplayPrefab != null)
        {
            healthDisplay = Instantiate(healthDisplayPrefab, healthContainer);
            var uiComponent = healthDisplay.AddComponent<UIInteractionComponent>();
            uiComponent.interactionType = UIInteractionType.Water;
        }
    }

    public void UpdateTaskDisplay(string[] tasks)
    {
        // Clear existing task items
        foreach (var item in taskItems)
        {
            Destroy(item);
        }
        taskItems.Clear();

        // Create new task items
        if (taskItemPrefab != null)
        {
            foreach (var task in tasks)
            {
                GameObject taskItem = Instantiate(taskItemPrefab, taskContainer);
                var textComponent = taskItem.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = task;
                }

                var uiComponent = taskItem.AddComponent<UIInteractionComponent>();
                uiComponent.interactionType = UIInteractionType.Task;
                uiComponent.interactionIndex = taskItems.Count;

                taskItems.Add(taskItem);
            }
        }
    }

    public void UpdatePomodoroDisplay(float timeRemaining, bool isWorkMode)
    {
        if (pomodoroDisplay != null)
        {
            var textComponent = pomodoroDisplay.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                textComponent.text = $"{minutes:00}:{seconds:00}";
            }

            var imageComponent = pomodoroDisplay.GetComponentInChildren<Image>();
            if (imageComponent != null)
            {
                imageComponent.color = isWorkMode ? workModeColor : breakModeColor;
            }
        }
    }

    public void UpdateHealthDisplay(int waterIntake)
    {
        if (healthDisplay != null)
        {
            var textComponent = healthDisplay.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"Water: {waterIntake}/8 cups";
            }

            var imageComponent = healthDisplay.GetComponentInChildren<Image>();
            if (imageComponent != null)
            {
                imageComponent.fillAmount = waterIntake / 8f;
            }
        }
    }

    // Debug methods
    public void TestUIUpdate()
    {
        UpdateTaskDisplay(new string[] { "Test Task 1", "Test Task 2", "Test Task 3" });
        UpdatePomodoroDisplay(120f, true);
        UpdateHealthDisplay(4);
    }
} 