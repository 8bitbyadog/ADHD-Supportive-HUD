using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

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
    [SerializeField] private bool enableEyeTracking = false;

    public int callbackOrder => 0;

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

    [MenuItem("Build/Build Quest 3")]
    public static void BuildQuest3()
    {
        var buildManager = FindObjectOfType<BuildManager>();
        if (buildManager == null)
        {
            Debug.LogError("BuildManager not found in scene!");
            return;
        }

        // Get build path
        string fullBuildPath = Path.Combine(Application.dataPath, "..", buildManager.buildPath);
        string apkPath = Path.Combine(fullBuildPath, "ADHDSupportiveHUD.apk");

        // Configure build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = buildManager.developmentBuild ? BuildOptions.Development : BuildOptions.None
        };

        // Build the player
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {summary.totalSize / 1024 / 1024} MB");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed!");
        }
    }

    private static string[] GetEnabledScenes()
    {
        var scenes = new List<string>();
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            if (EditorBuildSettings.scenes[i].enabled)
            {
                scenes.Add(EditorBuildSettings.scenes[i].path);
            }
        }
        return scenes.ToArray();
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