using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Unity.XR.Oculus;

/// <summary>
/// Manages the XR session lifecycle for Meta Quest devices
/// Handles initialization, state transitions, and recovery
/// </summary>
public class XRSessionManager : MonoBehaviour
{
    [Header("Session Settings")]
    [SerializeField] private float initializationDelay = 0.5f;
    [SerializeField] private int maxRetryAttempts = 3;
    [SerializeField] private float retryDelay = 2.0f;
    
    [Header("Performance Settings")]
    [SerializeField] private bool useHighRefreshRate = true;
    [SerializeField] private bool useDynamicFoveation = true;
    
    [Header("Debug")]
    [SerializeField] private bool verboseLogging = false;
    
    private bool _isInitialized = false;
    private int _currentRetryAttempt = 0;
    
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    
    private void Start()
    {
        StartCoroutine(InitializeXR());
    }
    
    private IEnumerator InitializeXR()
    {
        LogMessage("Starting XR initialization sequence");
        
        // Wait a short delay before initialization to ensure system is ready
        yield return new WaitForSeconds(initializationDelay);
        
        // Initialize XR Plugin Management
        if (XRGeneralSettings.Instance == null)
        {
            LogError("XR General Settings instance is null. Make sure XR Plugin Management is installed.");
            yield break;
        }
        
        var xrManager = XRGeneralSettings.Instance.Manager;
        if (xrManager == null)
        {
            LogError("XR Manager instance is null. Make sure XR Plugin Management is configured correctly.");
            yield break;
        }
        
        // Start the XR subsystems
        if (!xrManager.isInitializationComplete)
        {
            LogMessage("Initializing XR subsystems");
            xrManager.InitializeLoaderSync();
            
            if (!xrManager.isInitializationComplete)
            {
                LogError("XR initialization failed. Retrying...");
                yield return StartCoroutine(RetryInitialization());
                yield break;
            }
        }
        
        // Start the XR subsystems
        if (xrManager.activeLoader != null)
        {
            LogMessage("Starting XR subsystems");
            xrManager.StartSubsystems();
            
            // Configure performance settings
            ConfigurePerformanceSettings();
            
            _isInitialized = true;
            LogMessage("XR initialization complete");
        }
        else
        {
            LogError("XR Loader is null. Retrying...");
            yield return StartCoroutine(RetryInitialization());
        }
    }
    
    private IEnumerator RetryInitialization()
    {
        _currentRetryAttempt++;
        
        if (_currentRetryAttempt <= maxRetryAttempts)
        {
            LogMessage($"Retry attempt {_currentRetryAttempt}/{maxRetryAttempts}");
            yield return new WaitForSeconds(retryDelay);
            yield return StartCoroutine(InitializeXR());
        }
        else
        {
            LogError($"Failed to initialize XR after {maxRetryAttempts} attempts");
        }
    }
    
    private void ConfigurePerformanceSettings()
    {
        if (useHighRefreshRate)
        {
            // Try to set 90Hz refresh rate if supported
            if (XRDevice.refreshRate < 90)
            {
                LogMessage("Setting high refresh rate (90Hz)");
                OculusSettings settings = GetOculusSettings();
                if (settings != null)
                {
                    settings.TargetRefreshRate = OculusSettings.SupportedRefreshRates.RefreshRate90Hz;
                }
            }
        }
        
        if (useDynamicFoveation)
        {
            // Enable dynamic foveation for performance
            LogMessage("Enabling dynamic foveation");
            OculusSettings settings = GetOculusSettings();
            if (settings != null)
            {
                settings.UseDynamicFoveation = true;
            }
        }
    }
    
    private OculusSettings GetOculusSettings()
    {
        OculusSettings settings = null;
        
        // Try to get the Oculus settings
        UnityEngine.Object[] assets = Resources.FindObjectsOfTypeAll(typeof(OculusSettings));
        if (assets.Length > 0)
        {
            settings = assets[0] as OculusSettings;
        }
        
        return settings;
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            LogMessage("Application paused, handling XR session pause");
            // Handle session pause gracefully
            var xrManager = XRGeneralSettings.Instance?.Manager;
            if (xrManager != null && xrManager.activeLoader != null)
            {
                xrManager.StopSubsystems();
            }
        }
        else
        {
            LogMessage("Application resumed, handling XR session resume");
            // Handle session resume
            var xrManager = XRGeneralSettings.Instance?.Manager;
            if (xrManager != null && xrManager.activeLoader != null)
            {
                xrManager.StartSubsystems();
            }
        }
    }
    
    private void OnDestroy()
    {
        LogMessage("Cleaning up XR session");
        var xrManager = XRGeneralSettings.Instance?.Manager;
        if (xrManager != null && xrManager.activeLoader != null)
        {
            xrManager.StopSubsystems();
            xrManager.DeinitializeLoader();
        }
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