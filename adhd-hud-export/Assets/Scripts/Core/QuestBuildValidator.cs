using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
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
    public bool checkGraphicsAPI = true;
    public bool checkAndroidSettings = true;
    public bool checkQualitySettings = true;
    
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
        
        if (checkGraphicsAPI)
        {
            ValidateGraphicsAPI(issues);
        }
        
        if (checkAndroidSettings)
        {
            ValidateAndroidSettings(issues);
        }
        
        if (checkQualitySettings)
        {
            ValidateQualitySettings(issues);
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
        
        // Check if XR Plugin is enabled for Android
        var generalSettings = XRGeneralSettings.Instance;
        if (generalSettings == null || !generalSettings.Manager.isInitializationComplete)
        {
            issues.Add("XR Plugin is not properly initialized");
        }
    }
    
    private void ValidatePassthrough(List<string> issues)
    {
        var settings = OculusProjectConfig.GetProjectConfig();
        if (settings != null && !settings.enablePassthrough)
        {
            issues.Add("Passthrough is not enabled in Oculus settings");
        }
    }
    
    private void ValidateHandTracking(List<string> issues)
    {
        var settings = OculusProjectConfig.GetProjectConfig();
        if (settings != null && !settings.enableHandTrackingSupport)
        {
            issues.Add("Hand Tracking is not enabled in Oculus settings");
        }
    }
    
    private void ValidatePerformance(List<string> issues)
    {
        // Check target frame rate
        if (Application.targetFrameRate != 90)
        {
            issues.Add("Target frame rate should be set to 90Hz for Quest 3");
        }
        
        // Check VSync
        if (QualitySettings.vSyncCount != 0)
        {
            issues.Add("VSync should be disabled as Quest handles it internally");
        }
        
        // Check physics timestep
        if (Mathf.Approximately(Time.fixedDeltaTime, 1f/90f) == false)
        {
            issues.Add("Physics timestep should be set to 1/90 for Quest 3");
        }
    }
    
    private void ValidateGraphicsAPI(List<string> issues)
    {
        var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
        if (apis.Length == 0 || (apis[0] != GraphicsDeviceType.Vulkan && apis[0] != GraphicsDeviceType.OpenGLES3))
        {
            issues.Add("Graphics API should be set to Vulkan or OpenGL ES 3.1");
        }
    }
    
    private void ValidateAndroidSettings(List<string> issues)
    {
        // Check architecture
        if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
        {
            issues.Add("Target architecture should be ARM64 for Quest 3");
        }
        
        // Check minimum SDK version
        if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24)
        {
            issues.Add("Minimum SDK version should be at least 24 for Quest 3");
        }
        
        // Check target SDK version
        if (PlayerSettings.Android.targetSdkVersion != AndroidSdkVersions.AndroidApiLevelAuto)
        {
            issues.Add("Target SDK version should be set to 'Automatic Highest'");
        }
    }
    
    private void ValidateQualitySettings(List<string> issues)
    {
        // Check shadow settings
        if (QualitySettings.shadows > ShadowQuality.HardOnly)
        {
            issues.Add("Shadow quality should be set to 'Hard Only' or lower for performance");
        }
        
        if (QualitySettings.shadowResolution > ShadowResolution.Medium)
        {
            issues.Add("Shadow resolution should be set to 'Medium' or lower");
        }
        
        if (QualitySettings.shadowDistance > 20f)
        {
            issues.Add("Shadow distance should be limited to 20 meters for performance");
        }
        
        // Check texture settings
        if (QualitySettings.masterTextureLimit > 0)
        {
            issues.Add("Texture quality is reduced, which may impact visual quality");
        }
        
        if (QualitySettings.anisotropicFiltering != AnisotropicFiltering.Enable)
        {
            issues.Add("Anisotropic filtering should be enabled for better texture quality");
        }
    }
    
    private void AutoFixIssues(List<string> issues)
    {
        var buildManager = FindObjectOfType<BuildManager>();
        if (buildManager != null)
        {
            buildManager.ConfigureForQuest3();
            Debug.Log("Applied Quest 3 configuration using BuildManager");
        }
        else
        {
            Debug.LogWarning("BuildManager not found. Some issues may need to be fixed manually.");
        }
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
        
        // Calculate frame time statistics
        float minFrameTime = float.MaxValue;
        float maxFrameTime = float.MinValue;
        float totalFrameTime = 0f;
        
        foreach (var fps in fpsSamples)
        {
            float frameTime = 1000f / fps; // Convert to milliseconds
            minFrameTime = Mathf.Min(minFrameTime, frameTime);
            maxFrameTime = Mathf.Max(maxFrameTime, frameTime);
            totalFrameTime += frameTime;
        }
        
        float avgFrameTime = totalFrameTime / fpsSamples.Count;
        
        Debug.Log($"Performance Test Results:");
        Debug.Log($"Average FPS: {avgFps:F1}");
        Debug.Log($"Min FPS: {Mathf.Min(fpsSamples.ToArray()):F1}");
        Debug.Log($"Max FPS: {Mathf.Max(fpsSamples.ToArray()):F1}");
        Debug.Log($"Frame Time (ms) - Avg: {avgFrameTime:F2}, Min: {minFrameTime:F2}, Max: {maxFrameTime:F2}");
        
        if (avgFps < 70f)
        {
            Debug.LogWarning("Performance Warning: Average FPS is below 70, which may cause discomfort");
        }
        
        if (maxFrameTime > 16.7f) // 60 FPS threshold
        {
            Debug.LogWarning($"Performance Warning: Some frames took longer than 16.7ms to render (max: {maxFrameTime:F2}ms)");
        }
    }
}
#endif 