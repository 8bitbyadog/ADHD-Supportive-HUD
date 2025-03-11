using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the XR session setup, initialization and configuration
/// Handles device performance settings and features like passthrough
/// </summary>
public class XRSessionManager : MonoBehaviour
{
    [Header("Session Settings")]
    [SerializeField] private bool initializeOnStart = true;
    [SerializeField] private int maxRetryAttempts = 3;
    [SerializeField] private float retryDelay = 2.0f;
    
    [Header("Performance Settings")]
    [SerializeField] private bool optimizeForMobileVR = true;
    [SerializeField] private bool useDynamicFoveation = true;
    
    [Header("Debug")]
    [SerializeField] private bool verboseLogging = false;
    
    private bool _isInitialized = false;
    private int _currentRetryAttempt = 0;
    
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    
    private void Start()
    {
        if (initializeOnStart)
        {
            StartCoroutine(InitializeXR());
        }
    }
    
    private IEnumerator InitializeXR()
    {
        LogMessage("Initializing XR session...");
        
        try
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            // Mobile XR initialization
            if (optimizeForMobileVR)
            {
                ConfigurePerformanceSettings();
            }
            
            // Check if Oculus Integration is available
            var ovrManager = FindObjectOfType<OVRManager>();
            if (ovrManager != null)
            {
                // Configure OVRManager settings if available
                ovrManager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
                ovrManager.useRecommendedMSAALevel = true;
                ovrManager.useDynamicFixedFoveatedRendering = useDynamicFoveation;
                
                // Initialize passthrough if device supports it
                if (OVRManager.IsPassthroughSupported())
                {
                    yield return new WaitForSeconds(0.5f); // Brief delay for OVR to initialize
                    InitializePassthrough();
                }
                else
                {
                    LogMessage("Passthrough is not supported on this device");
                }
                
                _isInitialized = true;
                LogMessage("XR session initialized successfully");
            }
            else
            {
                LogError("OVRManager not found in the scene. Some features may not work properly.");
                _isInitialized = false;
                
                if (_currentRetryAttempt < maxRetryAttempts)
                {
                    yield return StartCoroutine(RetryInitialization());
                }
            }
            #else
            // Editor initialization
            LogMessage("Running in editor mode");
            _isInitialized = true;
            #endif
        }
        catch (System.Exception e)
        {
            LogError($"Error initializing XR session: {e.Message}");
            _isInitialized = false;
            
            if (_currentRetryAttempt < maxRetryAttempts)
            {
                yield return StartCoroutine(RetryInitialization());
            }
        }
    }
    
    private IEnumerator RetryInitialization()
    {
        _currentRetryAttempt++;
        LogMessage($"Retrying initialization (Attempt {_currentRetryAttempt}/{maxRetryAttempts})...");
        
        yield return new WaitForSeconds(retryDelay);
        
        yield return StartCoroutine(InitializeXR());
        
        if (!_isInitialized && _currentRetryAttempt >= maxRetryAttempts)
        {
            LogError($"Failed to initialize XR session after {maxRetryAttempts} attempts.");
        }
    }
    
    private void ConfigurePerformanceSettings()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // Set CPU/GPU performance levels (if Oculus Integration is available)
            if (OVRManager.instance != null)
            {
                // Set initial performance levels
                // CPU and GPU levels range from 0 to 3 (higher = more performance, more power)
                OVRManager.cpuLevel = 2;
                OVRManager.gpuLevel = 2;
                
                // Configure fixed foveated rendering
                if (useDynamicFoveation && OVRManager.fixedFoveatedRenderingSupported)
                {
                    OVRManager.useDynamicFixedFoveatedRendering = true;
                    LogMessage("Dynamic fixed foveated rendering enabled");
                }
            }
            
            // General performance optimizations
            Application.targetFrameRate = 72;  // 72Hz for Quest, 90Hz for Quest 2
            QualitySettings.vSyncCount = 0;    // VSync is handled by the XR system
            QualitySettings.maxQueuedFrames = 1;
            
            // Disable unnecessary features
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.antiAliasing = 0;   // MSAA is handled separately
            
            LogMessage("Mobile VR performance settings applied");
        }
        catch (System.Exception e)
        {
            LogError($"Error configuring performance settings: {e.Message}");
        }
        #endif
    }
    
    private void InitializePassthrough()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            var passthroughManager = FindObjectOfType<PassthroughManager>();
            if (passthroughManager == null)
            {
                LogMessage("No PassthroughManager found. Creating one.");
                
                // Create a new GameObject with PassthroughManager if it doesn't exist
                var passthroughObject = new GameObject("PassthroughManager");
                passthroughObject.transform.SetParent(transform);
                passthroughManager = passthroughObject.AddComponent<PassthroughManager>();
            }
            
            LogMessage("Passthrough initialized");
        }
        catch (System.Exception e)
        {
            LogError($"Error initializing passthrough: {e.Message}");
        }
        #endif
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            LogMessage("Application paused");
            
            #if UNITY_ANDROID && !UNITY_EDITOR
            // Conserve power when app is paused
            if (OVRManager.instance != null)
            {
                // Lower CPU/GPU levels when app is in background
                OVRManager.cpuLevel = 0;
                OVRManager.gpuLevel = 0;
            }
            #endif
        }
        else
        {
            LogMessage("Application resumed");
            
            #if UNITY_ANDROID && !UNITY_EDITOR
            // Restore performance when app resumes
            if (!_isInitialized)
            {
                // Re-initialize if needed
                StartCoroutine(InitializeXR());
            }
            else if (optimizeForMobileVR && OVRManager.instance != null)
            {
                // Restore CPU/GPU levels
                OVRManager.cpuLevel = 2;
                OVRManager.gpuLevel = 2;
            }
            #endif
        }
    }
    
    private void OnDestroy()
    {
        LogMessage("XR session manager destroyed");
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // Clean up resources if needed
        }
        catch (System.Exception e)
        {
            LogError($"Error during cleanup: {e.Message}");
        }
        #endif
    }
    
    private void LogMessage(string message)
    {
        if (verboseLogging)
        {
            Debug.Log($"[XRSessionManager] {message}");
        }
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[XRSessionManager] {message}");
    }
}
