using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A simplified theme manager for initial setup
/// Provides basic theme switching functionality without dependencies
/// </summary>
public class SimpleThemeManager : MonoBehaviour
{
    [System.Serializable]
    public class Theme
    {
        public string name = "Default Theme";
        public Color primaryColor = Color.blue;
        public Color secondaryColor = Color.cyan;
        public Color backgroundColor = Color.black;
        public Color textColor = Color.white;
    }
    
    [Header("Theme Settings")]
    [SerializeField] private List<Theme> availableThemes = new List<Theme>();
    [SerializeField] private Theme defaultTheme;
    [SerializeField] private float transitionDuration = 0.5f;
    
    [Header("UI Elements")]
    [SerializeField] private List<Image> primaryColorElements = new List<Image>();
    [SerializeField] private List<Image> secondaryColorElements = new List<Image>();
    [SerializeField] private List<Image> backgroundElements = new List<Image>();
    [SerializeField] private List<Text> textElements = new List<Text>();
    
    [Header("High Contrast Mode")]
    [SerializeField] private bool highContrastByDefault = false;
    [SerializeField] private Color highContrastTextColor = Color.white;
    [SerializeField] private Color highContrastBackgroundColor = Color.black;
    
    private Theme _currentTheme;
    private bool _isHighContrastMode = false;
    private Coroutine _transitionCoroutine;
    
    private void Start()
    {
        // Initialize with default theme
        if (defaultTheme != null)
        {
            _currentTheme = defaultTheme;
        }
        else if (availableThemes.Count > 0)
        {
            _currentTheme = availableThemes[0];
        }
        else
        {
            _currentTheme = new Theme();
        }
        
        // Apply initial theme
        ApplyTheme(_currentTheme, false);
        
        // Set high contrast mode if needed
        _isHighContrastMode = highContrastByDefault;
        if (_isHighContrastMode)
        {
            ApplyHighContrastMode();
        }
    }
    
    /// <summary>
    /// Changes the active theme with a smooth transition
    /// </summary>
    /// <param name="theme">The theme to change to</param>
    public void ChangeTheme(Theme theme)
    {
        if (theme == null)
            return;
            
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
        }
        
        _currentTheme = theme;
        _transitionCoroutine = StartCoroutine(TransitionToTheme(theme));
    }
    
    /// <summary>
    /// Changes the active theme by index
    /// </summary>
    /// <param name="themeIndex">Index of the theme in the availableThemes list</param>
    public void ChangeThemeByIndex(int themeIndex)
    {
        if (themeIndex >= 0 && themeIndex < availableThemes.Count)
        {
            ChangeTheme(availableThemes[themeIndex]);
        }
    }
    
    /// <summary>
    /// Enables or disables high contrast mode
    /// </summary>
    /// <param name="enabled">Whether to enable high contrast mode</param>
    public void SetHighContrastMode(bool enabled)
    {
        _isHighContrastMode = enabled;
        
        if (enabled)
        {
            ApplyHighContrastMode();
        }
        else
        {
            ApplyTheme(_currentTheme, true);
        }
    }
    
    private IEnumerator TransitionToTheme(Theme targetTheme)
    {
        float elapsedTime = 0f;
        
        // Store initial colors
        Dictionary<Image, Color> startPrimaryColors = new Dictionary<Image, Color>();
        Dictionary<Image, Color> startSecondaryColors = new Dictionary<Image, Color>();
        Dictionary<Image, Color> startBackgroundColors = new Dictionary<Image, Color>();
        Dictionary<Text, Color> startTextColors = new Dictionary<Text, Color>();
        
        foreach (var element in primaryColorElements)
        {
            if (element != null)
                startPrimaryColors[element] = element.color;
        }
        
        foreach (var element in secondaryColorElements)
        {
            if (element != null)
                startSecondaryColors[element] = element.color;
        }
        
        foreach (var element in backgroundElements)
        {
            if (element != null)
                startBackgroundColors[element] = element.color;
        }
        
        foreach (var element in textElements)
        {
            if (element != null)
                startTextColors[element] = element.color;
        }
        
        // Perform transition
        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;
            
            // Update primary color elements
            foreach (var element in primaryColorElements)
            {
                if (element != null && startPrimaryColors.ContainsKey(element))
                {
                    element.color = Color.Lerp(startPrimaryColors[element], targetTheme.primaryColor, t);
                }
            }
            
            // Update secondary color elements
            foreach (var element in secondaryColorElements)
            {
                if (element != null && startSecondaryColors.ContainsKey(element))
                {
                    element.color = Color.Lerp(startSecondaryColors[element], targetTheme.secondaryColor, t);
                }
            }
            
            // Update background elements
            foreach (var element in backgroundElements)
            {
                if (element != null && startBackgroundColors.ContainsKey(element))
                {
                    element.color = Color.Lerp(startBackgroundColors[element], targetTheme.backgroundColor, t);
                }
            }
            
            // Update text elements
            foreach (var element in textElements)
            {
                if (element != null && startTextColors.ContainsKey(element))
                {
                    element.color = Color.Lerp(startTextColors[element], targetTheme.textColor, t);
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final values are set exactly
        ApplyTheme(targetTheme, false);
        
        _transitionCoroutine = null;
    }
    
    private void ApplyTheme(Theme theme, bool immediate)
    {
        if (theme == null)
            return;
            
        if (immediate)
        {
            // Apply immediately to all elements
            foreach (var element in primaryColorElements)
            {
                if (element != null)
                    element.color = theme.primaryColor;
            }
            
            foreach (var element in secondaryColorElements)
            {
                if (element != null)
                    element.color = theme.secondaryColor;
            }
            
            foreach (var element in backgroundElements)
            {
                if (element != null)
                    element.color = theme.backgroundColor;
            }
            
            foreach (var element in textElements)
            {
                if (element != null)
                    element.color = theme.textColor;
            }
        }
        else
        {
            // Start transition
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
            }
            
            _transitionCoroutine = StartCoroutine(TransitionToTheme(theme));
        }
    }
    
    private void ApplyHighContrastMode()
    {
        // Override colors with high contrast values
        foreach (var element in backgroundElements)
        {
            if (element != null)
                element.color = highContrastBackgroundColor;
        }
        
        foreach (var element in textElements)
        {
            if (element != null)
                element.color = highContrastTextColor;
        }
    }
}
