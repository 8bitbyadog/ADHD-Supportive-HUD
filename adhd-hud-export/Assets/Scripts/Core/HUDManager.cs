using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Oculus.Interaction;

public class HUDManager : MonoBehaviour
{
    [Header("HUD Settings")]
    [SerializeField] private float hudOpacity = 0.8f;
    [SerializeField] private Vector3 hudOffset = new Vector3(0.2f, -0.1f, 0.5f);
    [SerializeField] private float hudScale = 1.0f;

    [Header("References")]
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private CanvasGroup hudCanvasGroup;
    [SerializeField] private Camera xrCamera;

    private void Awake()
    {
        if (xrCamera == null)
        {
            xrCamera = Camera.main;
        }

        if (hudCanvas == null)
        {
            CreateHUDCanvas();
        }

        if (hudCanvasGroup == null)
        {
            hudCanvasGroup = hudCanvas.gameObject.AddComponent<CanvasGroup>();
        }

        SetupHUD();
    }

    private void CreateHUDCanvas()
    {
        // Create HUD Canvas
        GameObject hudObject = new GameObject("HUDCanvas");
        hudCanvas = hudObject.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.WorldSpace;
        hudCanvas.worldCamera = xrCamera;

        // Add Canvas Scaler
        CanvasScaler scaler = hudObject.AddComponent<CanvasScaler>();
        scaler.scaleFactor = hudScale;
        scaler.dynamicPixelsPerUnit = 100;

        // Add Graphic Raycaster
        hudObject.AddComponent<GraphicRaycaster>();

        // Parent to camera
        hudObject.transform.SetParent(xrCamera.transform, false);
    }

    private void SetupHUD()
    {
        // Set position and rotation
        hudCanvas.transform.localPosition = hudOffset;
        hudCanvas.transform.localRotation = Quaternion.identity;

        // Set transparency
        hudCanvasGroup.alpha = hudOpacity;

        // Ensure HUD is always visible
        hudCanvas.sortingOrder = 1;
        hudCanvas.overrideSorting = true;
    }

    public void UpdateHUDPosition(Vector3 newOffset)
    {
        hudOffset = newOffset;
        hudCanvas.transform.localPosition = hudOffset;
    }

    public void UpdateHUDOpacity(float newOpacity)
    {
        hudOpacity = Mathf.Clamp01(newOpacity);
        hudCanvasGroup.alpha = hudOpacity;
    }

    public void UpdateHUDScale(float newScale)
    {
        hudScale = newScale;
        CanvasScaler scaler = hudCanvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.scaleFactor = hudScale;
        }
    }

    // Called by other managers to update HUD content
    public void UpdateTaskDisplay(string[] tasks)
    {
        // Implementation will be added when we create the task UI
    }

    public void UpdatePomodoroDisplay(float timeRemaining, bool isWorkMode)
    {
        // Implementation will be added when we create the Pomodoro UI
    }

    public void UpdateHealthDisplay(int waterIntake)
    {
        // Implementation will be added when we create the health UI
    }
} 