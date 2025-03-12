using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ThemeManager : MonoBehaviour
{
    [Header("Theme Presets")]
    [SerializeField] private List<HUDTheme> availableThemes;
    [SerializeField] private HUDTheme defaultTheme;
    
    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem themeTransitionParticles;
    [SerializeField] private ParticleSystem backgroundParticles;
    [SerializeField] private float particleEmissionRate = 10f;
    [SerializeField] private float particleLifetime = 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip themeChangeSound;
    [SerializeField] private AudioClip ambientMusic;
    [SerializeField] private float musicVolume = 0.3f;
    [SerializeField] private float musicFadeTime = 1f;
    
    private HUDTheme currentTheme;
    private bool isTransitioning;
    private AudioSource audioSource;
    private AudioSource musicSource;
    private List<Image> themeableImages = new List<Image>();
    private List<TextMeshProUGUI> themeableTexts = new List<TextMeshProUGUI>();
    private Material skyboxMaterial;
    
    private void Awake()
    {
        // Set up audio sources
        audioSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        
        // Get skybox material
        skyboxMaterial = RenderSettings.skybox;
        
        // Find all themeable UI elements
        FindThemeableElements();
        
        // Apply default theme
        if (defaultTheme != null)
        {
            ApplyTheme(defaultTheme, false);
        }
        
        // Start ambient music
        if (ambientMusic != null)
        {
            musicSource.clip = ambientMusic;
            musicSource.volume = 0f;
            musicSource.Play();
            StartCoroutine(FadeMusicVolume(musicVolume, musicFadeTime));
        }
    }
    
    private void FindThemeableElements()
    {
        // Find all UI elements that can be themed
        Image[] images = FindObjectsOfType<Image>(true);
        TextMeshProUGUI[] texts = FindObjectsOfType<TextMeshProUGUI>(true);
        
        foreach (var image in images)
        {
            if (image.CompareTag("Themeable"))
            {
                themeableImages.Add(image);
            }
        }
        
        foreach (var text in texts)
        {
            if (text.CompareTag("Themeable"))
            {
                themeableTexts.Add(text);
            }
        }
    }
    
    public void ChangeTheme(string themeName)
    {
        HUDTheme newTheme = availableThemes.Find(t => t.name == themeName);
        if (newTheme != null && !isTransitioning)
        {
            StartCoroutine(TransitionToTheme(newTheme));
        }
    }
    
    private IEnumerator TransitionToTheme(HUDTheme newTheme)
    {
        isTransitioning = true;
        
        // Store initial colors
        Dictionary<Image, Color> initialImageColors = new Dictionary<Image, Color>();
        Dictionary<TextMeshProUGUI, Color> initialTextColors = new Dictionary<TextMeshProUGUI, Color>();
        Color initialSkyboxTint = skyboxMaterial.GetColor("_Tint");
        Color initialAmbientLight = RenderSettings.ambientLight;
        
        foreach (var image in themeableImages)
        {
            initialImageColors[image] = image.color;
        }
        
        foreach (var text in themeableTexts)
        {
            initialTextColors[text] = text.color;
        }
        
        // Play transition effects
        if (themeTransitionParticles != null)
        {
            var main = themeTransitionParticles.main;
            main.startColor = newTheme.particleColor;
            themeTransitionParticles.Play();
        }
        
        if (themeChangeSound != null)
        {
            audioSource.PlayOneShot(themeChangeSound);
        }
        
        // Transition colors
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            float t = transitionCurve.Evaluate(elapsed / transitionDuration);
            
            // Update UI elements
            foreach (var image in themeableImages)
            {
                if (image != null)
                {
                    Color targetColor = GetTargetColor(image, newTheme);
                    image.color = Color.Lerp(initialImageColors[image], targetColor, t);
                }
            }
            
            foreach (var text in themeableTexts)
            {
                if (text != null)
                {
                    text.color = Color.Lerp(initialTextColors[text], newTheme.textColor, t);
                }
            }
            
            // Update environment
            skyboxMaterial.SetColor("_Tint", Color.Lerp(initialSkyboxTint, newTheme.backgroundColor, t));
            RenderSettings.ambientLight = Color.Lerp(initialAmbientLight, newTheme.ambientLight, t);
            
            // Update particle systems
            if (backgroundParticles != null)
            {
                var main = backgroundParticles.main;
                main.startColor = Color.Lerp(currentTheme.particleColor.color, newTheme.particleColor.color, t);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final values are set
        ApplyTheme(newTheme, false);
        
        isTransitioning = false;
        currentTheme = newTheme;
    }
    
    private void ApplyTheme(HUDTheme theme, bool withTransition = true)
    {
        if (withTransition)
        {
            StartCoroutine(TransitionToTheme(theme));
            return;
        }
        
        // Apply colors immediately
        foreach (var image in themeableImages)
        {
            if (image != null)
            {
                image.color = GetTargetColor(image, theme);
            }
        }
        
        foreach (var text in themeableTexts)
        {
            if (text != null)
            {
                text.color = theme.textColor;
            }
        }
        
        // Update environment
        skyboxMaterial.SetColor("_Tint", theme.backgroundColor);
        RenderSettings.ambientLight = theme.ambientLight;
        
        // Update particle systems
        if (backgroundParticles != null)
        {
            var main = backgroundParticles.main;
            main.startColor = theme.particleColor;
        }
        
        currentTheme = theme;
    }
    
    private Color GetTargetColor(Image image, HUDTheme theme)
    {
        // Determine which theme color to use based on the image's purpose
        // You can add more tags or properties to customize this
        if (image.CompareTag("Primary"))
            return theme.primaryColor;
        if (image.CompareTag("Secondary"))
            return theme.secondaryColor;
        if (image.CompareTag("Accent"))
            return theme.accentColor;
        
        return theme.primaryColor;
    }
    
    private IEnumerator FadeMusicVolume(float targetVolume, float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }
        
        musicSource.volume = targetVolume;
    }
    
    public void SetParticleEmissionRate(float rate)
    {
        if (backgroundParticles != null)
        {
            var emission = backgroundParticles.emission;
            emission.rateOverTime = rate;
        }
    }
    
    public HUDTheme GetCurrentTheme()
    {
        return currentTheme;
    }
    
    public List<string> GetAvailableThemeNames()
    {
        List<string> themeNames = new List<string>();
        foreach (var theme in availableThemes)
        {
            themeNames.Add(theme.name);
        }
        return themeNames;
    }
} 