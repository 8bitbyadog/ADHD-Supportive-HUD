using UnityEngine;
using System.Collections;
using Meta.XR;

public class XRSessionManager : MonoBehaviour
{
    [Header("XR Settings")]
    [SerializeField] private bool enableHandTracking = true;
    [SerializeField] private bool enableEyeTracking = true;
    [SerializeField] private bool enablePassthrough = true;
    
    [Header("Performance")]
    [SerializeField] private bool optimizeForQuest3 = true;
    [SerializeField] private int targetFrameRate = 90;
    
    private void Start()
    {
        ConfigureXRSettings();
        if (optimizeForQuest3)
        {
            OptimizeForQuest3();
        }
    }
    
    private void ConfigureXRSettings()
    {
        try
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (Meta.XR.XRSystem.Instance != null)
            {
                // Configure hand tracking
                if (enableHandTracking)
                {
                    Meta.XR.HandTracking.Enable();
                }
                
                // Configure eye tracking
                if (enableEyeTracking)
                {
                    Meta.XR.EyeTracking.Enable();
                }
                
                // Configure passthrough
                Meta.XR.XRSystem.Instance.PassthroughEnabled = enablePassthrough;
            }
            #endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[XRSessionManager] Error configuring XR settings: {e.Message}");
        }
    }
    
    private void OptimizeForQuest3()
    {
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0;
        QualitySettings.maxQueuedFrames = 2;
        Time.fixedDeltaTime = 1f / targetFrameRate;
        Physics.defaultSolverIterations = 8;
        Physics.defaultSolverVelocityIterations = 2;
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            ConfigureXRSettings();
        }
    }
    
    private void OnApplicationQuit()
    {
        try
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (Meta.XR.XRSystem.Instance != null)
            {
                // Disable features
                if (enableHandTracking)
                {
                    Meta.XR.HandTracking.Disable();
                }
                
                if (enableEyeTracking)
                {
                    Meta.XR.EyeTracking.Disable();
                }
                
                Meta.XR.XRSystem.Instance.PassthroughEnabled = false;
            }
            #endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[XRSessionManager] Error cleaning up XR settings: {e.Message}");
        }
    }
}
