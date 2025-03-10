using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class TutorialStep
{
    public string title;
    public string description;
    public Vector3 targetPosition;
    public Vector3 targetScale = Vector3.one;
    public float duration = 5f;
    public bool requiresInteraction = false;
    public string interactionType;
    public GameObject highlightObject;
}

public class TutorialManager : MonoBehaviour
{
    private static TutorialManager instance;
    public static TutorialManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TutorialManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("TutorialManager");
                    instance = go.AddComponent<TutorialManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("Tutorial UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button closeButton;

    [Header("Tutorial Steps")]
    [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();

    [Header("Visual Settings")]
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0f, 0.3f);
    [SerializeField] private float highlightPulseSpeed = 2f;
    [SerializeField] private float highlightPulseScale = 0.1f;

    private int currentStepIndex = -1;
    private bool isTutorialActive = false;
    private GameObject currentHighlight;
    private Material highlightMaterial;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        InitializeTutorialUI();
        CreateHighlightMaterial();
    }

    private void InitializeTutorialUI()
    {
        if (tutorialPanel == null)
        {
            CreateTutorialUI();
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextStep);
        }
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipTutorial);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseTutorial);
        }
    }

    private void CreateTutorialUI()
    {
        // Create tutorial panel
        GameObject panel = new GameObject("TutorialPanel");
        panel.transform.SetParent(transform, false);
        tutorialPanel = panel;

        // Add panel components
        CanvasGroup canvasGroup = panel.AddComponent<CanvasGroup>();
        Image background = panel.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.8f);

        // Set panel rect transform
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Create content container
        GameObject content = new GameObject("Content");
        content.transform.SetParent(panel.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(600, 400);
        contentRect.anchoredPosition = Vector2.zero;

        // Add title text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(content.transform, false);
        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.fontSize = 32;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // Add description text
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(content.transform, false);
        descriptionText = descObj.AddComponent<TextMeshProUGUI>();
        descriptionText.fontSize = 24;
        descriptionText.alignment = TextAlignmentOptions.Center;
        descriptionText.color = Color.white;

        // Add buttons
        CreateButton("NextButton", "Next", NextStep);
        CreateButton("SkipButton", "Skip Tutorial", SkipTutorial);
        CreateButton("CloseButton", "Close", CloseTutorial);

        // Set up layout
        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 20;
        layout.padding = new RectOffset(20, 20, 20, 20);
    }

    private void CreateButton(string name, string text, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(tutorialPanel.transform, false);
        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        // Add button image
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Add button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 24;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;

        // Set button rect transform
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(200, 50);
    }

    private void CreateHighlightMaterial()
    {
        // Create highlight material
        highlightMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        highlightMaterial.color = highlightColor;
        highlightMaterial.renderQueue = 3000; // Transparent
    }

    public void StartTutorial()
    {
        if (tutorialSteps.Count == 0)
        {
            Debug.LogWarning("No tutorial steps defined!");
            return;
        }

        isTutorialActive = true;
        currentStepIndex = -1;
        tutorialPanel.SetActive(true);
        NextStep();
    }

    private void NextStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= tutorialSteps.Count)
        {
            CompleteTutorial();
            return;
        }

        TutorialStep step = tutorialSteps[currentStepIndex];
        UpdateTutorialUI(step);
        UpdateHighlight(step);
    }

    private void UpdateTutorialUI(TutorialStep step)
    {
        if (titleText != null)
        {
            titleText.text = step.title;
        }
        if (descriptionText != null)
        {
            descriptionText.text = step.description;
        }
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(!step.requiresInteraction);
        }
    }

    private void UpdateHighlight(TutorialStep step)
    {
        // Remove previous highlight
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
        }

        if (step.highlightObject != null)
        {
            // Create highlight object
            currentHighlight = new GameObject("TutorialHighlight");
            currentHighlight.transform.SetParent(step.highlightObject.transform, false);

            // Add mesh renderer
            MeshRenderer renderer = currentHighlight.AddComponent<MeshRenderer>();
            renderer.material = highlightMaterial;

            // Add mesh filter
            MeshFilter filter = currentHighlight.AddComponent<MeshFilter>();
            filter.mesh = step.highlightObject.GetComponent<MeshFilter>().sharedMesh;

            // Add highlight animation
            StartCoroutine(PulseHighlight(currentHighlight));
        }
    }

    private System.Collections.IEnumerator PulseHighlight(GameObject highlight)
    {
        Vector3 originalScale = highlight.transform.localScale;
        float time = 0;

        while (highlight != null)
        {
            time += Time.deltaTime * highlightPulseSpeed;
            float scale = 1 + Mathf.Sin(time) * highlightPulseScale;
            highlight.transform.localScale = originalScale * scale;
            yield return null;
        }
    }

    public void HandleInteraction(string interactionType)
    {
        if (!isTutorialActive || currentStepIndex < 0) return;

        TutorialStep currentStep = tutorialSteps[currentStepIndex];
        if (currentStep.requiresInteraction && currentStep.interactionType == interactionType)
        {
            NextStep();
        }
    }

    private void CompleteTutorial()
    {
        isTutorialActive = false;
        tutorialPanel.SetActive(false);
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
        }

        // Save tutorial completion
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();
    }

    private void SkipTutorial()
    {
        CompleteTutorial();
    }

    private void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
    }

    public bool HasCompletedTutorial()
    {
        return PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;
    }

    // Debug methods
    public void TestTutorial()
    {
        // Add test steps
        tutorialSteps.Clear();
        tutorialSteps.Add(new TutorialStep
        {
            title = "Welcome to ADHD Supportive HUD",
            description = "Let's learn how to use your new productivity assistant.",
            duration = 5f
        });
        tutorialSteps.Add(new TutorialStep
        {
            title = "Task Management",
            description = "Your tasks will appear here. Tap to complete them.",
            duration = 5f,
            requiresInteraction = true,
            interactionType = "task_tap"
        });
        tutorialSteps.Add(new TutorialStep
        {
            title = "Pomodoro Timer",
            description = "Use the timer to maintain focus and take regular breaks.",
            duration = 5f
        });

        StartTutorial();
    }

    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey("TutorialCompleted");
        PlayerPrefs.Save();
        Debug.Log("Tutorial progress reset");
    }
} 