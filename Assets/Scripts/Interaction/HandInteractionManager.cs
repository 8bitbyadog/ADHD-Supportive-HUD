using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;

public class HandInteractionManager : MonoBehaviour
{
    [Header("Hand References")]
    [SerializeField] private HandRef rightHandRef;
    [SerializeField] private HandRef leftHandRef;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 0.5f;
    [SerializeField] private LayerMask uiLayer;

    [Header("References")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private PomodoroManager pomodoroManager;
    [SerializeField] private HealthTracker healthTracker;

    private RaycastHit hit;
    private bool isInteracting;
    private GameObject lastHitObject;

    private void Start()
    {
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
    }

    private void Update()
    {
        CheckHandInteraction();
    }

    private void CheckHandInteraction()
    {
        // Check right hand interaction
        if (rightHandRef != null && rightHandRef.GetComponent<HandPinchInteractor>().IsSelectActive)
        {
            HandleHandInteraction(rightHandRef);
        }

        // Check left hand interaction
        if (leftHandRef != null && leftHandRef.GetComponent<HandPinchInteractor>().IsSelectActive)
        {
            HandleHandInteraction(leftHandRef);
        }
    }

    private void HandleHandInteraction(HandRef handRef)
    {
        // Get hand position and forward direction
        Vector3 handPosition = handRef.transform.position;
        Vector3 handForward = handRef.transform.forward;

        // Cast ray from hand
        if (Physics.Raycast(handPosition, handForward, out hit, interactionDistance, uiLayer))
        {
            GameObject hitObject = hit.collider.gameObject;
            
            // Handle new interaction
            if (hitObject != lastHitObject)
            {
                HandleUIInteraction(hitObject);
                lastHitObject = hitObject;
            }
        }
        else
        {
            lastHitObject = null;
        }
    }

    private void HandleUIInteraction(GameObject uiObject)
    {
        // Get the UI component
        var uiComponent = uiObject.GetComponent<UIInteractionComponent>();
        if (uiComponent != null)
        {
            switch (uiComponent.interactionType)
            {
                case UIInteractionType.Task:
                    taskManager.OnTaskTapped(uiComponent.interactionIndex);
                    break;
                case UIInteractionType.Pomodoro:
                    pomodoroManager.ToggleTimer();
                    break;
                case UIInteractionType.Water:
                    healthTracker.RecordWaterIntake(1);
                    break;
                case UIInteractionType.Break:
                    healthTracker.RecordBreak();
                    break;
            }
        }
    }
}

// Enum for different types of UI interactions
public enum UIInteractionType
{
    Task,
    Pomodoro,
    Water,
    Break
}

// Component to attach to UI elements for interaction
public class UIInteractionComponent : MonoBehaviour
{
    public UIInteractionType interactionType;
    public int interactionIndex;
} 