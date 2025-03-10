using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AccessibilityManager : MonoBehaviour
{
    private static AccessibilityManager instance;
    public static AccessibilityManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AccessibilityManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AccessibilityManager");
                    instance = go.AddComponent<AccessibilityManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("Text Settings")]
    [SerializeField] private float defaultTextSize = 24f;
    [SerializeField] private float textSizeMultiplier = 1.0f;
    [SerializeField] private float contrastRatio = 1.0f;
    [SerializeField] private bool highContrastMode = false;

    [Header("Motion Settings")]
    [SerializeField] private bool reduceMotion = false;
    [SerializeField] private float motionScale = 1.0f;
    [SerializeField] private bool disableAnimations = false;

    [Header("Audio Settings")]
    [SerializeField] private bool enableVoiceFeedback = false;
    [SerializeField] private float voiceVolume = 1.0f;
    [SerializeField] private float speechRate = 1.0f;

    [Header("Color Settings")]
    [SerializeField] private Color highContrastTextColor = Color.white;
    [SerializeField] private Color highContrastBackgroundColor = Color.black;
    [SerializeField] private Color highContrastAccentColor = Color.yellow;

    private Dictionary<TextMeshProUGUI, float> originalTextSizes = new Dictionary<TextMeshProUGUI, float>();
    private Dictionary<Image, Color> originalColors = new Dictionary<Image, Color>();
    private bool isInitialized = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        LoadAccessibilitySettings();
    }

    private void Start()
    {
        InitializeAccessibility();
    }

    private void LoadAccessibilitySettings()
    {
        var settings = AppSettingsManager.Instance.GetSettings();
        textSizeMultiplier = settings.textSize;
        contrastRatio = settings.contrastRatio;
        highContrastMode = settings.highContrastMode;
        reduceMotion = settings.reduceMotion;
        enableVoiceFeedback = settings.enableVoiceFeedback;
    }

    private void InitializeAccessibility()
    {
        if (isInitialized) return;

        // Store original text sizes
        var textComponents = FindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in textComponents)
        {
            originalTextSizes[text] = text.fontSize;
            ApplyTextAccessibility(text);
        }

        // Store original colors
        var imageComponents = FindObjectsOfType<Image>();
        foreach (var image in imageComponents)
        {
            originalColors[image] = image.color;
            ApplyColorAccessibility(image);
        }

        // Apply motion settings
        ApplyMotionSettings();

        isInitialized = true;
    }

    public void UpdateTextSize(float multiplier)
    {
        textSizeMultiplier = multiplier;
        foreach (var text in originalTextSizes.Keys)
        {
            ApplyTextAccessibility(text);
        }
    }

    public void UpdateContrastRatio(float ratio)
    {
        contrastRatio = ratio;
        foreach (var image in originalColors.Keys)
        {
            ApplyColorAccessibility(image);
        }
    }

    public void ToggleHighContrastMode(bool enabled)
    {
        highContrastMode = enabled;
        foreach (var image in originalColors.Keys)
        {
            ApplyColorAccessibility(image);
        }
    }

    public void UpdateMotionSettings(bool reduce, float scale, bool disable)
    {
        reduceMotion = reduce;
        motionScale = scale;
        disableAnimations = disable;
        ApplyMotionSettings();
    }

    public void UpdateVoiceSettings(bool enable, float volume, float rate)
    {
        enableVoiceFeedback = enable;
        voiceVolume = volume;
        speechRate = rate;
        // Implement voice feedback system
    }

    private void ApplyTextAccessibility(TextMeshProUGUI text)
    {
        if (text == null) return;

        float newSize = originalTextSizes[text] * textSizeMultiplier;
        text.fontSize = newSize;

        if (highContrastMode)
        {
            text.color = highContrastTextColor;
        }
    }

    private void ApplyColorAccessibility(Image image)
    {
        if (image == null) return;

        if (highContrastMode)
        {
            // Apply high contrast colors based on image type
            if (image.gameObject.GetComponent<TextMeshProUGUI>() != null)
            {
                image.color = highContrastBackgroundColor;
            }
            else if (image.type == Image.Type.Filled)
            {
                image.color = highContrastAccentColor;
            }
            else
            {
                image.color = highContrastBackgroundColor;
            }
        }
        else
        {
            // Apply contrast ratio to original color
            Color originalColor = originalColors[image];
            image.color = AdjustContrast(originalColor, contrastRatio);
        }
    }

    private void ApplyMotionSettings()
    {
        // Find all animators
        var animators = FindObjectsOfType<Animator>();
        foreach (var animator in animators)
        {
            if (disableAnimations)
            {
                animator.enabled = false;
            }
            else if (reduceMotion)
            {
                animator.speed = motionScale;
            }
        }

        // Find all UI animations
        var uiAnimations = FindObjectsOfType<UIAnimation>();
        foreach (var animation in uiAnimations)
        {
            if (disableAnimations)
            {
                animation.enabled = false;
            }
            else if (reduceMotion)
            {
                animation.duration *= motionScale;
            }
        }
    }

    private Color AdjustContrast(Color color, float ratio)
    {
        // Simple contrast adjustment
        float luminance = (0.299f * color.r + 0.587f * color.g + 0.114f * color.b);
        float factor = (ratio + 1) / (ratio - 1);
        float newLuminance = (luminance - 0.5f) * factor + 0.5f;
        float delta = newLuminance - luminance;

        return new Color(
            Mathf.Clamp01(color.r + delta),
            Mathf.Clamp01(color.g + delta),
            Mathf.Clamp01(color.b + delta),
            color.a
        );
    }

    public void ProvideVoiceFeedback(string message)
    {
        if (!enableVoiceFeedback) return;

        // TODO: Implement text-to-speech system
        Debug.Log($"Voice feedback: {message}");
    }

    public void ProvideHapticFeedback(float intensity = 1.0f)
    {
        // TODO: Implement haptic feedback system
        Debug.Log($"Haptic feedback: {intensity}");
    }

    // Debug methods
    public void TestAccessibilityFeatures()
    {
        // Test text size
        UpdateTextSize(1.5f);

        // Test contrast
        UpdateContrastRatio(2.0f);

        // Test high contrast mode
        ToggleHighContrastMode(true);

        // Test motion settings
        UpdateMotionSettings(true, 0.5f, false);

        // Test voice feedback
        UpdateVoiceSettings(true, 1.0f, 1.0f);
        ProvideVoiceFeedback("Testing accessibility features");

        Debug.Log("Accessibility features tested");
    }

    public void ResetAccessibilitySettings()
    {
        // Reset text size
        UpdateTextSize(1.0f);

        // Reset contrast
        UpdateContrastRatio(1.0f);

        // Reset high contrast mode
        ToggleHighContrastMode(false);

        // Reset motion settings
        UpdateMotionSettings(false, 1.0f, false);

        // Reset voice settings
        UpdateVoiceSettings(false, 1.0f, 1.0f);

        Debug.Log("Accessibility settings reset");
    }
} 