using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;

public class SceneManager : MonoBehaviour
{
    [Header("XR References")]
    [SerializeField] private Camera xrCamera;
    [SerializeField] private HandRef rightHandRef;
    [SerializeField] private HandRef leftHandRef;

    [Header("UI References")]
    [SerializeField] private HUDManager hudManager;
    [SerializeField] private ArmbandUI armbandUI;
    [SerializeField] private HandInteractionManager handInteractionManager;

    [Header("Manager References")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private PomodoroManager pomodoroManager;
    [SerializeField] private HealthTracker healthTracker;

    private void Awake()
    {
        // Find references if not set
        if (xrCamera == null)
        {
            xrCamera = Camera.main;
        }

        if (rightHandRef == null || leftHandRef == null)
        {
            var handRefs = FindObjectsOfType<HandRef>();
            foreach (var handRef in handRefs)
            {
                if (handRef.Handedness == Handedness.Right)
                {
                    rightHandRef = handRef;
                }
                else if (handRef.Handedness == Handedness.Left)
                {
                    leftHandRef = handRef;
                }
            }
        }

        // Find managers if not set
        if (hudManager == null)
        {
            hudManager = FindObjectOfType<HUDManager>();
        }
        if (armbandUI == null)
        {
            armbandUI = FindObjectOfType<ArmbandUI>();
        }
        if (handInteractionManager == null)
        {
            handInteractionManager = FindObjectOfType<HandInteractionManager>();
        }
        if (taskManager == null)
        {
            taskManager = FindObjectOfType<TaskManager>();
        }
        if (pomodoroManager == null)
        {
            pomodoroManager = FindObjectOfType<PomodoroManager>();
        }
        if (healthTracker == null)
        {
            healthTracker = FindObjectOfType<HealthTracker>();
        }

        // Validate references
        ValidateReferences();
    }

    private void Start()
    {
        // Initialize components
        InitializeComponents();
    }

    private void ValidateReferences()
    {
        if (xrCamera == null)
        {
            Debug.LogError("XR Camera reference is missing!");
        }
        if (rightHandRef == null)
        {
            Debug.LogError("Right hand reference is missing!");
        }
        if (leftHandRef == null)
        {
            Debug.LogError("Left hand reference is missing!");
        }
        if (hudManager == null)
        {
            Debug.LogError("HUDManager reference is missing!");
        }
        if (armbandUI == null)
        {
            Debug.LogError("ArmbandUI reference is missing!");
        }
        if (handInteractionManager == null)
        {
            Debug.LogError("HandInteractionManager reference is missing!");
        }
        if (taskManager == null)
        {
            Debug.LogError("TaskManager reference is missing!");
        }
        if (pomodoroManager == null)
        {
            Debug.LogError("PomodoroManager reference is missing!");
        }
        if (healthTracker == null)
        {
            Debug.LogError("HealthTracker reference is missing!");
        }
    }

    private void InitializeComponents()
    {
        // Set up hand references
        if (handInteractionManager != null)
        {
            // Set hand references in HandInteractionManager
            var handInteractionFields = typeof(HandInteractionManager).GetFields();
            foreach (var field in handInteractionFields)
            {
                if (field.Name == "rightHandRef")
                {
                    field.SetValue(handInteractionManager, rightHandRef);
                }
                else if (field.Name == "leftHandRef")
                {
                    field.SetValue(handInteractionManager, leftHandRef);
                }
            }
        }

        // Set up armband UI
        if (armbandUI != null)
        {
            // Set hand reference in ArmbandUI
            var armbandFields = typeof(ArmbandUI).GetFields();
            foreach (var field in armbandFields)
            {
                if (field.Name == "leftHandRef")
                {
                    field.SetValue(armbandUI, leftHandRef);
                }
            }
        }

        // Add sample tasks for testing
        if (taskManager != null)
        {
            taskManager.AddSampleTasks();
        }

        // Set up short durations for testing
        if (pomodoroManager != null)
        {
            pomodoroManager.SetShortDurationForTesting();
        }

        if (healthTracker != null)
        {
            healthTracker.SetShortIntervalsForTesting();
        }
    }

    // Debug methods
    public void TestAllComponents()
    {
        if (taskManager != null)
        {
            taskManager.AddSampleTasks();
        }

        if (pomodoroManager != null)
        {
            pomodoroManager.SetShortDurationForTesting();
            pomodoroManager.StartTimer();
        }

        if (healthTracker != null)
        {
            healthTracker.SetShortIntervalsForTesting();
            healthTracker.SimulateWaterIntake();
        }
    }
} 