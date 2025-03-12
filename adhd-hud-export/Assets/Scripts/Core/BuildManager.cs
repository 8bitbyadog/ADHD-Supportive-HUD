using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour, IPreprocessBuildWithReport
{
    [Header("Build Settings")]
    [SerializeField] private string appName = "ADHD Supportive HUD";
    [SerializeField] private string bundleId = "com.yourcompany.adhdhud";
    [SerializeField] private string buildPath = "Builds/Quest3";
    [SerializeField] private bool developmentBuild = true;

    [Header("XR Settings")]
    [SerializeField] private bool enablePassthrough = true;
    [SerializeField] private bool enableHandTracking = true;
    [SerializeField] private bool enableEyeTracking = true;

    [Header("Performance Settings")]
    [SerializeField] private bool optimizeForQuest3 = true;
    [SerializeField] private bool useVulkanAPI = true;
    [SerializeField] private int targetFrameRate = 90;

    public int callbackOrder => 0;

    private static BuildManager instance;
    public static BuildManager Instance => instance;

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

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.Android)
        {
            ConfigureAndroidBuild();
        }
    }

    private void ConfigureAndroidBuild()
    {
        // Set Android build settings
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;

        // Configure XR settings
        ConfigureXRSettings();

        // Set application name and bundle ID
        PlayerSettings.productName = appName;
        PlayerSettings.applicationIdentifier = bundleId;

        // Configure build path
        string fullBuildPath = Path.Combine(Application.dataPath, "..", buildPath);
        if (!Directory.Exists(fullBuildPath))
        {
            Directory.CreateDirectory(fullBuildPath);
        }

        // Set development build
        EditorUserBuildSettings.development = developmentBuild;
    }

    private void ConfigureXRSettings()
    {
        // Enable Oculus XR
        EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
        PlayerSettings.SetVirtualRealitySDKs(BuildTargetGroup.Android, new[] { "Oculus" });

        // Configure Oculus specific settings
        var oculusSettings = UnityEditor.Editor.CreateInstance<OculusProjectConfig>();
        oculusSettings.targetDeviceTypes = OculusProjectConfig.TargetDeviceType.Quest;
        oculusSettings.targetQuest3 = true;
        oculusSettings.targetQuestPro = false;
        oculusSettings.targetQuest2 = false;
        oculusSettings.targetQuest1 = false;
        oculusSettings.targetGo = false;
        oculusSettings.targetGearVrOrGo = false;

        // Enable Passthrough
        if (enablePassthrough)
        {
            oculusSettings.enablePassthrough = true;
        }

        // Enable Hand Tracking
        if (enableHandTracking)
        {
            oculusSettings.enableHandTrackingSupport = true;
        }

        // Enable Eye Tracking
        if (enableEyeTracking)
        {
            oculusSettings.enableEyeTracking = true;
        }

        // Save Oculus settings
        UnityEditor.EditorUtility.SetDirty(oculusSettings);
    }

    public void ConfigureForQuest3()
    {
        // Set Android as target platform
        EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        
        // Configure Android settings
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        
        // Set graphics API
        if (useVulkanAPI)
        {
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
        }
        else
        {
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
        }
        
        // Configure XR settings
        PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
        PlayerSettings.SetVirtualRealitySDKs(BuildTargetGroup.Android, new[] { "Oculus" });
        
        // Configure Oculus settings
        var oculusSettings = UnityEditor.Editor.CreateInstance<OculusProjectConfig>();
        oculusSettings.targetDeviceTypes = OculusProjectConfig.TargetDeviceType.Quest;
        oculusSettings.targetQuest3 = true;
        oculusSettings.targetQuestPro = false;
        oculusSettings.targetQuest2 = false;
        oculusSettings.targetQuest1 = false;
        oculusSettings.targetGo = false;
        oculusSettings.targetGearVrOrGo = false;
        
        // Enable features
        if (enablePassthrough)
        {
            oculusSettings.enablePassthrough = true;
        }
        
        if (enableHandTracking)
        {
            oculusSettings.enableHandTrackingSupport = true;
        }
        
        if (enableEyeTracking)
        {
            oculusSettings.enableEyeTracking = true;
        }
        
        // Configure performance settings
        if (optimizeForQuest3)
        {
            OptimizeForQuest3();
        }
        
        // Save Oculus settings
        UnityEditor.EditorUtility.SetDirty(oculusSettings);
        AssetDatabase.SaveAssets();
    }
    
    private void OptimizeForQuest3()
    {
        // Set target frame rate
        Application.targetFrameRate = targetFrameRate;
        
        // Configure quality settings
        QualitySettings.vSyncCount = 0; // Disable VSync as Quest handles it
        QualitySettings.maxQueuedFrames = 2; // Reduce frame latency
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable; // Enable anisotropic filtering
        
        // Optimize rendering settings
        QualitySettings.shadows = ShadowQuality.HardOnly;
        QualitySettings.shadowResolution = ShadowResolution.Medium;
        QualitySettings.shadowDistance = 20f;
        QualitySettings.shadowCascades = 2;
        
        // Set texture quality
        QualitySettings.masterTextureLimit = 0; // Full resolution
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        
        // Configure physics
        Time.fixedDeltaTime = 1f / 90f; // Match physics to target frame rate
        Physics.defaultSolverIterations = 8;
        Physics.defaultSolverVelocityIterations = 2;
    }

    [MenuItem("Build/Build Quest 3")]
    public static void BuildQuest3()
    {
        // Ensure proper configuration
        Instance.ConfigureForQuest3();
        
        // Define build path
        string buildPath = EditorUtility.SaveFilePanel(
            "Build Quest 3 APK",
            "",
            "ADHD_Focus_Assistant.apk",
            "apk"
        );
        
        if (string.IsNullOrEmpty(buildPath)) return;
        
        // Get all scenes from build settings
        var scenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                scenes.Add(scene.path);
            }
        }
        
        // Build the APK
        BuildPipeline.BuildPlayer(
            scenes.ToArray(),
            buildPath,
            BuildTarget.Android,
            BuildOptions.Development | BuildOptions.AutoRunPlayer
        );
    }
    
    [MenuItem("Build/Configure Quest 3 Settings")]
    public static void ConfigureQuest3Settings()
    {
        Instance.ConfigureForQuest3();
        EditorUtility.DisplayDialog(
            "Quest 3 Configuration",
            "Quest 3 build settings have been configured successfully.",
            "OK"
        );
    }

    // Debug methods
    public void TestBuildSettings()
    {
        Debug.Log($"App Name: {appName}");
        Debug.Log($"Bundle ID: {bundleId}");
        Debug.Log($"Build Path: {buildPath}");
        Debug.Log($"Development Build: {developmentBuild}");
        Debug.Log($"Enable Passthrough: {enablePassthrough}");
        Debug.Log($"Enable Hand Tracking: {enableHandTracking}");
        Debug.Log($"Enable Eye Tracking: {enableEyeTracking}");
    }
} 