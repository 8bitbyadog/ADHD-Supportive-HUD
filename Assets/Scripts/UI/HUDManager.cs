using UnityEngine;
using System.Collections;
using TMPro;
using Meta.XR;

public class HUDManager : MonoBehaviour
{
    [Header("HUD Settings")]
    [SerializeField] private bool enableHUDOnStart = true;
    [SerializeField] private float hudOpacity = 0.8f;
    [SerializeField] private float hudScale = 1.0f;
    [SerializeField] private float defaultDistance = 2.0f; // Distance from user in meters
    [SerializeField] private bool followUserGaze = true;
    
    [Header("Task Display")]
    [SerializeField] private GameObject taskListPrefab;
    [SerializeField] private Transform taskListParent;
    [SerializeField] private float taskUpdateInterval = 0.5f;
    
    [Header("Pomodoro Timer")]
    [SerializeField] private GameObject pomodoroTimerPrefab;
    [SerializeField] private Transform pomodoroTimerParent;
    [SerializeField] private float defaultWorkDuration = 25f;
    [SerializeField] private float defaultBreakDuration = 5f;
    
    [Header("Health Tracking")]
    [SerializeField] private GameObject healthDisplayPrefab;
    [SerializeField] private Transform healthDisplayParent;
    [SerializeField] private float healthUpdateInterval = 1f;
    
    [Header("Gamification")]
    [SerializeField] private GameObject achievementPrefab;
    [SerializeField] private Transform achievementParent;
    [SerializeField] private float achievementDisplayDuration = 3f;
    
    [Header("Accessibility")]
    [SerializeField] private bool highContrastMode = false;
    [SerializeField] private bool reducedMotion = false;
    [SerializeField] private float textSize = 1f;
    
    private TaskManager taskManager;
    private bool isHUDActive = false;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Coroutine followCoroutine;
    
    private void Start()
    {
        taskManager = FindObjectOfType<TaskManager>();
        
        if (enableHUDOnStart)
        {
            EnableHUD(true);
        }
        
        if (followUserGaze)
        {
            StartFollowingUser();
        }
    }
    
    public void EnableHUD(bool enable)
    {
        if (isHUDActive == enable)
            return;
        
        isHUDActive = enable;
        SetHUDVisibility(enable);
        
        if (enable && followUserGaze)
        {
            StartFollowingUser();
        }
        else if (!enable && followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }
    }
    
    private void SetHUDVisibility(bool visible)
    {
        if (taskListParent != null)
            taskListParent.gameObject.SetActive(visible);
        
        if (pomodoroTimerParent != null)
            pomodoroTimerParent.gameObject.SetActive(visible);
        
        if (healthDisplayParent != null)
            healthDisplayParent.gameObject.SetActive(visible);
        
        if (achievementParent != null)
            achievementParent.gameObject.SetActive(visible);
    }
    
    private void StartFollowingUser()
    {
        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
        }
        
        followCoroutine = StartCoroutine(FollowUserRoutine());
    }
    
    private IEnumerator FollowUserRoutine()
    {
        while (isHUDActive)
        {
            UpdateHUDPosition();
            yield return new WaitForSeconds(0.1f); // Update every 100ms
        }
    }
    
    private void UpdateHUDPosition()
    {
        try
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (Meta.XR.EyeTracking.IsSupported && Meta.XR.EyeTracking.IsEnabled)
            {
                // Use eye tracking if available
                var eyeData = Meta.XR.EyeTracking.GetEyeData();
                if (eyeData.IsValid)
                {
                    targetPosition = Camera.main.transform.position + eyeData.GazeDirection * defaultDistance;
                }
                else
                {
                    // Fallback to head position
                    targetPosition = Camera.main.transform.position + Camera.main.transform.forward * defaultDistance;
                }
            }
            else
            {
                // Use head position
                targetPosition = Camera.main.transform.position + Camera.main.transform.forward * defaultDistance;
            }
            #else
            // Editor fallback
            targetPosition = Camera.main.transform.position + Camera.main.transform.forward * defaultDistance;
            #endif
            
            // Smooth movement
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
            
            // Always face the user
            targetRotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[HUDManager] Error updating HUD position: {e.Message}");
        }
    }
    
    public void SetHUDOpacity(float opacity)
    {
        hudOpacity = Mathf.Clamp01(opacity);
        CanvasGroup[] canvasGroups = GetComponentsInChildren<CanvasGroup>();
        foreach (var group in canvasGroups)
        {
            group.alpha = hudOpacity;
        }
    }
    
    public void SetHUDScale(float scale)
    {
        hudScale = Mathf.Clamp(scale, 0.1f, 2f);
        transform.localScale = Vector3.one * hudScale;
    }
    
    public void UpdateAccessibilitySettings(bool highContrast, bool reduceMotion, float fontSize)
    {
        highContrastMode = highContrast;
        reducedMotion = reduceMotion;
        textSize = fontSize;
        ApplyAccessibilitySettings();
    }
    
    private void ApplyAccessibilitySettings()
    {
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            text.fontSize *= textSize;
            if (highContrastMode)
            {
                text.color = Color.white;
                text.fontStyle = FontStyles.Bold;
            }
        }
        
        if (reducedMotion)
        {
            Animator[] animators = GetComponentsInChildren<Animator>();
            foreach (var animator in animators)
            {
                animator.speed = 0.5f;
            }
        }
    }
    
    private void OnDestroy()
    {
        if (isHUDActive)
        {
            EnableHUD(false);
        }
        
        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
        }
    }
}
