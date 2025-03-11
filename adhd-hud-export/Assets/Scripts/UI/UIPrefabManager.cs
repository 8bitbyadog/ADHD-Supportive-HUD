using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPrefabManager : MonoBehaviour
{
    [Header("Task Item Prefab")]
    [SerializeField] private GameObject taskItemPrefab;
    [SerializeField] private Color taskTextColor = Color.white;
    [SerializeField] private Color taskBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private float taskItemHeight = 40f;
    [SerializeField] private float taskItemPadding = 10f;

    [Header("Pomodoro Display Prefab")]
    [SerializeField] private GameObject pomodoroDisplayPrefab;
    [SerializeField] private Color pomodoroTextColor = Color.white;
    [SerializeField] private float pomodoroDisplayHeight = 60f;
    [SerializeField] private float progressBarHeight = 10f;

    [Header("Health Display Prefab")]
    [SerializeField] private GameObject healthDisplayPrefab;
    [SerializeField] private Color healthTextColor = Color.white;
    [SerializeField] private Color healthBarColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private float healthDisplayHeight = 40f;
    [SerializeField] private float healthBarHeight = 8f;

    private void Awake()
    {
        CreatePrefabs();
    }

    private void CreatePrefabs()
    {
        CreateTaskItemPrefab();
        CreatePomodoroDisplayPrefab();
        CreateHealthDisplayPrefab();
    }

    private void CreateTaskItemPrefab()
    {
        if (taskItemPrefab == null)
        {
            // Create task item container
            GameObject taskItem = new GameObject("TaskItem");
            RectTransform rectTransform = taskItem.AddComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Height, taskItemHeight);

            // Add background
            Image background = taskItem.AddComponent<Image>();
            background.color = taskBackgroundColor;

            // Add text
            GameObject textObj = new GameObject("TaskText");
            textObj.transform.SetParent(taskItem.transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.color = taskTextColor;
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Left;

            // Set text rect transform
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(taskItemPadding, 0);
            textRect.offsetMax = new Vector2(-taskItemPadding, 0);

            // Add UI interaction component
            taskItem.AddComponent<UIInteractionComponent>();

            taskItemPrefab = taskItem;
        }
    }

    private void CreatePomodoroDisplayPrefab()
    {
        if (pomodoroDisplayPrefab == null)
        {
            // Create pomodoro container
            GameObject pomodoroDisplay = new GameObject("PomodoroDisplay");
            RectTransform rectTransform = pomodoroDisplay.AddComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Height, pomodoroDisplayHeight);

            // Add background
            Image background = pomodoroDisplay.AddComponent<Image>();
            background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // Add text
            GameObject textObj = new GameObject("TimerText");
            textObj.transform.SetParent(pomodoroDisplay.transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.color = pomodoroTextColor;
            text.fontSize = 32;
            text.alignment = TextAlignmentOptions.Center;

            // Set text rect transform
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = new Vector2(0, -progressBarHeight);

            // Add progress bar
            GameObject progressBar = new GameObject("ProgressBar");
            progressBar.transform.SetParent(pomodoroDisplay.transform, false);
            Image progressImage = progressBar.AddComponent<Image>();
            progressImage.type = Image.Type.Filled;
            progressImage.fillMethod = Image.FillMethod.Horizontal;
            progressImage.color = Color.white;

            // Set progress bar rect transform
            RectTransform progressRect = progressBar.GetComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0, 0);
            progressRect.anchorMax = new Vector2(1, 0);
            progressRect.offsetMin = Vector2.zero;
            progressRect.offsetMax = new Vector2(0, progressBarHeight);

            // Add UI interaction component
            pomodoroDisplay.AddComponent<UIInteractionComponent>();

            pomodoroDisplayPrefab = pomodoroDisplay;
        }
    }

    private void CreateHealthDisplayPrefab()
    {
        if (healthDisplayPrefab == null)
        {
            // Create health container
            GameObject healthDisplay = new GameObject("HealthDisplay");
            RectTransform rectTransform = healthDisplay.AddComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Height, healthDisplayHeight);

            // Add background
            Image background = healthDisplay.AddComponent<Image>();
            background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // Add text
            GameObject textObj = new GameObject("HealthText");
            textObj.transform.SetParent(healthDisplay.transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.color = healthTextColor;
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Left;

            // Set text rect transform
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = new Vector2(0.7f, 1);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Add health bar
            GameObject healthBar = new GameObject("HealthBar");
            healthBar.transform.SetParent(healthDisplay.transform, false);
            Image barImage = healthBar.AddComponent<Image>();
            barImage.type = Image.Type.Filled;
            barImage.fillMethod = Image.FillMethod.Horizontal;
            barImage.color = healthBarColor;

            // Set health bar rect transform
            RectTransform barRect = healthBar.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0.7f, 0.5f);
            barRect.anchorMax = new Vector2(1, 0.5f);
            barRect.offsetMin = new Vector2(5, -healthBarHeight / 2);
            barRect.offsetMax = new Vector2(-5, healthBarHeight / 2);

            // Add UI interaction component
            healthDisplay.AddComponent<UIInteractionComponent>();

            healthDisplayPrefab = healthDisplay;
        }
    }

    // Public methods to access prefabs
    public GameObject GetTaskItemPrefab()
    {
        return taskItemPrefab;
    }

    public GameObject GetPomodoroDisplayPrefab()
    {
        return pomodoroDisplayPrefab;
    }

    public GameObject GetHealthDisplayPrefab()
    {
        return healthDisplayPrefab;
    }

    // Debug methods
    public void TestPrefabs()
    {
        if (taskItemPrefab != null)
        {
            var text = taskItemPrefab.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "Test Task";
            }
        }

        if (pomodoroDisplayPrefab != null)
        {
            var text = pomodoroDisplayPrefab.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "25:00";
            }
        }

        if (healthDisplayPrefab != null)
        {
            var text = healthDisplayPrefab.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "Water: 4/8 cups";
            }
        }
    }
} 