using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class QuestBuildValidator : MonoBehaviour
{
    [Header("Build Settings")]
    public bool validateOnStart = true;
    public bool autoFixIssues = true;
    
    [Header("Required Settings")]
    public bool checkXRPlugin = true;
    public bool checkPassthrough = true;
    public bool checkHandTracking = true;
    public bool checkPerformance = true;
    
    private static QuestBuildValidator instance;
    public static QuestBuildValidator Instance => instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (validateOnStart)
        {
            ValidateBuildSettings();
        }
    }
    
    public void ValidateBuildSettings()
    {
        var issues = new List<string>();
        
        if (checkXRPlugin)
        {
            ValidateXRPlugin(issues);
        }
        
        if (checkPassthrough)
        {
            ValidatePassthrough(issues);
        }
        
        if (checkHandTracking)
        {
            ValidateHandTracking(issues);
        }
        
        if (checkPerformance)
        {
            ValidatePerformance(issues);
        }
        
        if (issues.Count > 0)
        {
            Debug.LogWarning("Quest Build Validation Issues Found:");
            foreach (var issue in issues)
            {
                Debug.LogWarning($"- {issue}");
            }
            
            if (autoFixIssues)
            {
                AutoFixIssues(issues);
            }
        }
        else
        {
            Debug.Log("Quest Build Validation: All checks passed!");
        }
    }
    
    private void ValidateXRPlugin(List<string> issues)
    {
        #if UNITY_EDITOR
        // Check if XR Plugin Management is installed
        if (!PackageManager.PackageExists("com.unity.xr.management"))
        {
            issues.Add("XR Plugin Management package is missing");
        }
        
        // Check if Oculus XR Plugin is installed
        if (!PackageManager.PackageExists("com.unity.xr.oculus"))
        {
            issues.Add("Oculus XR Plugin is missing");
        }
        
        // Check if Meta XR SDK is installed
        if (!PackageManager.PackageExists("com.meta.xr.sdk"))
        {
            issues.Add("Meta XR SDK is missing");
        }
        #endif
    }
    
    private void ValidatePassthrough(List<string> issues)
    {
        #if UNITY_EDITOR
        // Check if Passthrough is enabled in XR Plugin Management
        var xrGeneralSettings = XRGeneralSettings.Instance;
        if (xrGeneralSettings != null)
        {
            var xrManager = xrGeneralSettings.Manager;
            if (xrManager != null)
            {
                var oculusSettings = xrManager.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
                if (oculusSettings != null && !oculusSettings.enablePassthrough)
                {
                    issues.Add("Passthrough is not enabled in Oculus XR Plugin settings");
                }
            }
        }
        #endif
    }
    
    private void ValidateHandTracking(List<string> issues)
    {
        #if UNITY_EDITOR
        // Check if Hand Tracking is enabled in Oculus settings
        var oculusSettings = OculusProjectConfig.GetProjectConfig();
        if (oculusSettings != null && !oculusSettings.handTrackingSupport)
        {
            issues.Add("Hand Tracking is not enabled in Oculus Project Settings");
        }
        #endif
    }
    
    private void ValidatePerformance(List<string> issues)
    {
        #if UNITY_EDITOR
        // Check Android build settings
        var androidSettings = PlayerSettings.GetPlatformSettings<AndroidSettings>("Android");
        if (androidSettings != null)
        {
            if (androidSettings.targetArchitectures != AndroidArchitecture.ARM64)
            {
                issues.Add("Target architecture should be ARM64 for Quest 3");
            }
            
            if (androidSettings.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24)
            {
                issues.Add("Minimum SDK version should be at least 24 for Quest 3");
            }
        }
        
        // Check quality settings
        var qualitySettings = QualitySettings.GetQualityLevel();
        var qualityLevels = QualitySettings.names;
        if (qualityLevels != null && qualityLevels.Length > qualitySettings)
        {
            var currentQuality = qualityLevels[qualitySettings];
            if (currentQuality.ToLower().Contains("high") || currentQuality.ToLower().Contains("ultra"))
            {
                issues.Add("Quality settings might be too high for Quest 3 performance");
            }
        }
        #endif
    }
    
    private void AutoFixIssues(List<string> issues)
    {
        #if UNITY_EDITOR
        foreach (var issue in issues)
        {
            if (issue.Contains("XR Plugin Management"))
            {
                PackageManager.Install("com.unity.xr.management");
            }
            else if (issue.Contains("Oculus XR Plugin"))
            {
                PackageManager.Install("com.unity.xr.oculus");
            }
            else if (issue.Contains("Meta XR SDK"))
            {
                PackageManager.Install("com.meta.xr.sdk");
            }
            else if (issue.Contains("Passthrough"))
            {
                var xrGeneralSettings = XRGeneralSettings.Instance;
                if (xrGeneralSettings != null)
                {
                    var xrManager = xrGeneralSettings.Manager;
                    if (xrManager != null)
                    {
                        var oculusSettings = xrManager.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
                        if (oculusSettings != null)
                        {
                            oculusSettings.enablePassthrough = true;
                        }
                    }
                }
            }
            else if (issue.Contains("Hand Tracking"))
            {
                var oculusSettings = OculusProjectConfig.GetProjectConfig();
                if (oculusSettings != null)
                {
                    oculusSettings.handTrackingSupport = true;
                }
            }
            else if (issue.Contains("Target architecture"))
            {
                var androidSettings = PlayerSettings.GetPlatformSettings<AndroidSettings>("Android");
                if (androidSettings != null)
                {
                    androidSettings.targetArchitectures = AndroidArchitecture.ARM64;
                }
            }
            else if (issue.Contains("Minimum SDK version"))
            {
                var androidSettings = PlayerSettings.GetPlatformSettings<AndroidSettings>("Android");
                if (androidSettings != null)
                {
                    androidSettings.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
                }
            }
        }
        #endif
    }
    
    public void RunPerformanceTest()
    {
        StartCoroutine(PerformanceTest());
    }
    
    private System.Collections.IEnumerator PerformanceTest()
    {
        Debug.Log("Starting Quest Performance Test...");
        
        // Test FPS stability
        float testDuration = 10f;
        float startTime = Time.time;
        List<float> fpsSamples = new List<float>();
        
        while (Time.time - startTime < testDuration)
        {
            float fps = 1.0f / Time.deltaTime;
            fpsSamples.Add(fps);
            yield return null;
        }
        
        // Calculate average FPS
        float avgFps = 0f;
        foreach (var fps in fpsSamples)
        {
            avgFps += fps;
        }
        avgFps /= fpsSamples.Count;
        
        Debug.Log($"Performance Test Results:");
        Debug.Log($"Average FPS: {avgFps:F1}");
        Debug.Log($"Min FPS: {Mathf.Min(fpsSamples.ToArray()):F1}");
        Debug.Log($"Max FPS: {Mathf.Max(fpsSamples.ToArray()):F1}");
        
        if (avgFps < 70f)
        {
            Debug.LogWarning("Performance Warning: Average FPS is below 70, which may cause discomfort");
        }
    }
} 