using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ThemeSelector : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Transform themeContainer;
    [SerializeField] private GameObject themeButtonPrefab;
    [SerializeField] private Image previewImage;
    [SerializeField] private TextMeshProUGUI themeNameText;
    [SerializeField] private TextMeshProUGUI themeDescriptionText;
    
    [Header("Preview Settings")]
    [SerializeField] private float previewDuration = 2f;
    [SerializeField] private ParticleSystem previewParticles;
    [SerializeField] private AudioClip previewSound;
    [SerializeField] private float previewVolume = 0.5f;
    
    [Header("Animation")]
    [SerializeField] private float buttonSpacing = 10f;
    [SerializeField] private float buttonAnimationDelay = 0.1f;
    [SerializeField] private AnimationCurve buttonAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private ThemeManager themeManager;
    private AudioSource audioSource;
    private HUDTheme previewTheme;
    private Coroutine previewCoroutine;
    
    private void Awake()
    {
        themeManager = FindObjectOfType<ThemeManager>();
        audioSource = gameObject.AddComponent<AudioSource>();
        
        if (themeManager != null)
        {
            CreateThemeButtons();
        }
    }
    
    private void CreateThemeButtons()
    {
        var themeNames = themeManager.GetAvailableThemeNames();
        float delay = 0f;
        
        foreach (var themeName in themeNames)
        {
            GameObject buttonObj = Instantiate(themeButtonPrefab, themeContainer);
            ThemeButton themeButton = buttonObj.AddComponent<ThemeButton>();
            
            // Set up button
            themeButton.Initialize(themeName, this);
            
            // Animate button appearance
            StartCoroutine(AnimateButtonAppearance(buttonObj, delay));
            delay += buttonAnimationDelay;
        }
    }
    
    private IEnumerator AnimateButtonAppearance(GameObject button, float delay)
    {
        // Initial state
        button.transform.localScale = Vector3.zero;
        button.GetComponent<CanvasGroup>().alpha = 0f;
        
        yield return new WaitForSeconds(delay);
        
        // Animate
        float elapsed = 0f;
        float duration = 0.5f;
        
        while (elapsed < duration)
        {
            float t = buttonAnimationCurve.Evaluate(elapsed / duration);
            button.transform.localScale = Vector3.one * t;
            button.GetComponent<CanvasGroup>().alpha = t;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Final state
        button.transform.localScale = Vector3.one;
        button.GetComponent<CanvasGroup>().alpha = 1f;
    }
    
    public void PreviewTheme(string themeName)
    {
        if (previewCoroutine != null)
        {
            StopCoroutine(previewCoroutine);
        }
        
        previewTheme = themeManager.GetAvailableThemes().Find(t => t.name == themeName);
        if (previewTheme != null)
        {
            previewCoroutine = StartCoroutine(ShowThemePreview());
        }
    }
    
    private IEnumerator ShowThemePreview()
    {
        // Update preview UI
        if (previewImage != null && previewTheme.previewImage != null)
        {
            previewImage.sprite = previewTheme.previewImage;
        }
        
        if (themeNameText != null)
        {
            themeNameText.text = previewTheme.name;
        }
        
        if (themeDescriptionText != null)
        {
            themeDescriptionText.text = previewTheme.description;
        }
        
        // Play preview effects
        if (previewParticles != null)
        {
            var main = previewParticles.main;
            main.startColor = previewTheme.particleColor;
            previewParticles.Play();
        }
        
        if (previewSound != null)
        {
            audioSource.PlayOneShot(previewSound, previewVolume);
        }
        
        // Wait for preview duration
        yield return new WaitForSeconds(previewDuration);
        
        // Reset preview if needed
        if (previewParticles != null)
        {
            previewParticles.Stop();
        }
    }
    
    public void ApplyTheme(string themeName)
    {
        if (themeManager != null)
        {
            themeManager.ChangeTheme(themeName);
        }
    }
}

// Helper class for theme buttons
public class ThemeButton : MonoBehaviour
{
    private Button button;
    private TextMeshProUGUI buttonText;
    private string themeName;
    private ThemeSelector themeSelector;
    private bool isHovered;
    
    public void Initialize(string name, ThemeSelector selector)
    {
        themeName = name;
        themeSelector = selector;
        
        // Set up components
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        
        if (buttonText != null)
        {
            buttonText.text = themeName;
        }
        
        // Add listeners
        button.onClick.AddListener(OnClick);
        
        // Add hover events
        var eventTrigger = gameObject.AddComponent<EventTrigger>();
        
        var pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        pointerEnter.callback.AddListener(OnPointerEnter);
        eventTrigger.triggers.Add(pointerEnter);
        
        var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener(OnPointerExit);
        eventTrigger.triggers.Add(pointerExit);
    }
    
    private void OnClick()
    {
        themeSelector.ApplyTheme(themeName);
    }
    
    private void OnPointerEnter(BaseEventData data)
    {
        if (!isHovered)
        {
            isHovered = true;
            themeSelector.PreviewTheme(themeName);
        }
    }
    
    private void OnPointerExit(BaseEventData data)
    {
        isHovered = false;
    }
} 