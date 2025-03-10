using UnityEngine;

[System.Serializable]
public class HUDTheme
{
    [Header("Theme Info")]
    public string name;
    public string description;
    public Sprite previewImage;
    
    [Header("Colors")]
    public Color primaryColor = new Color(0.2f, 0.8f, 1f);
    public Color secondaryColor = new Color(0.2f, 0.6f, 0.8f);
    public Color accentColor = new Color(1f, 0.4f, 0.2f);
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.15f);
    public Color textColor = Color.white;
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f);
    
    [Header("Lighting")]
    public Color ambientLight = new Color(0.2f, 0.2f, 0.3f);
    public float lightIntensity = 1f;
    public Color rimLightColor = new Color(0.2f, 0.8f, 1f);
    
    [Header("Particles")]
    public ParticleSystem.MinMaxGradient particleColor;
    public float particleIntensity = 1f;
    public float particleSize = 1f;
    
    [Header("Effects")]
    public float bloomIntensity = 1f;
    public float vignetteIntensity = 0.3f;
    public float chromaticAberration = 0.2f;
    
    [Header("Animation")]
    public float uiAnimationSpeed = 1f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio")]
    public float musicVolume = 0.5f;
    public float sfxVolume = 0.7f;
    public float reverbAmount = 0.3f;
    
    public HUDTheme()
    {
        // Set default particle gradient
        particleColor = new ParticleSystem.MinMaxGradient(primaryColor, accentColor);
    }
    
    public HUDTheme(string themeName)
    {
        name = themeName;
        particleColor = new ParticleSystem.MinMaxGradient(primaryColor, accentColor);
    }
    
    // Preset themes
    public static HUDTheme Cyberpunk()
    {
        return new HUDTheme("Cyberpunk")
        {
            primaryColor = new Color(0f, 1f, 0.8f),
            secondaryColor = new Color(1f, 0f, 0.5f),
            accentColor = new Color(1f, 0.8f, 0f),
            backgroundColor = new Color(0.05f, 0.05f, 0.1f),
            textColor = new Color(0f, 1f, 0.8f),
            ambientLight = new Color(0.1f, 0.2f, 0.3f),
            bloomIntensity = 1.5f,
            chromaticAberration = 0.4f
        };
    }
    
    public static HUDTheme Nature()
    {
        return new HUDTheme("Nature")
        {
            primaryColor = new Color(0.2f, 0.8f, 0.4f),
            secondaryColor = new Color(0.8f, 0.9f, 0.3f),
            accentColor = new Color(1f, 0.6f, 0.2f),
            backgroundColor = new Color(0.1f, 0.15f, 0.1f),
            textColor = new Color(0.9f, 1f, 0.9f),
            ambientLight = new Color(0.6f, 0.8f, 0.5f),
            bloomIntensity = 0.8f,
            vignetteIntensity = 0.2f
        };
    }
    
    public static HUDTheme Ocean()
    {
        return new HUDTheme("Ocean")
        {
            primaryColor = new Color(0f, 0.6f, 0.8f),
            secondaryColor = new Color(0f, 0.4f, 0.6f),
            accentColor = new Color(0f, 0.8f, 0.6f),
            backgroundColor = new Color(0.05f, 0.1f, 0.2f),
            textColor = new Color(0.8f, 0.9f, 1f),
            ambientLight = new Color(0.2f, 0.4f, 0.6f),
            bloomIntensity = 1.2f,
            reverbAmount = 0.5f
        };
    }
    
    public static HUDTheme Sunset()
    {
        return new HUDTheme("Sunset")
        {
            primaryColor = new Color(1f, 0.4f, 0.2f),
            secondaryColor = new Color(0.8f, 0.2f, 0.4f),
            accentColor = new Color(1f, 0.8f, 0.2f),
            backgroundColor = new Color(0.2f, 0.1f, 0.15f),
            textColor = new Color(1f, 0.95f, 0.9f),
            ambientLight = new Color(0.8f, 0.4f, 0.3f),
            bloomIntensity = 1.3f,
            vignetteIntensity = 0.4f
        };
    }
    
    public static HUDTheme Minimal()
    {
        return new HUDTheme("Minimal")
        {
            primaryColor = new Color(0.9f, 0.9f, 0.9f),
            secondaryColor = new Color(0.7f, 0.7f, 0.7f),
            accentColor = new Color(0.3f, 0.3f, 0.3f),
            backgroundColor = new Color(0.1f, 0.1f, 0.1f),
            textColor = Color.white,
            ambientLight = new Color(0.5f, 0.5f, 0.5f),
            bloomIntensity = 0.5f,
            particleIntensity = 0.5f
        };
    }
} 