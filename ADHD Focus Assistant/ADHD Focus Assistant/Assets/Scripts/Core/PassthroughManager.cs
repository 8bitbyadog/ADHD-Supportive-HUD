using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the passthrough AR functionality for Meta Quest devices
/// Handles configuration and visual effects for passthrough mode
/// </summary>
public class PassthroughManager : MonoBehaviour
{
    [Header("Passthrough Settings")]
    [SerializeField] private bool enablePassthroughOnStart = true;
    [SerializeField] private float passthroughOpacity = 1.0f;
    [SerializeField] private float passthroughBrightness = 0.0f;
    [SerializeField] private float passthroughContrast = 0.0f;
    
    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1.0f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Debug")]
    [SerializeField] private bool verboseLogging = false;
    
    private bool _isPassthroughActive = false;
    private Coroutine _transitionCoroutine;
    
    private void Start()
    {
        if (enablePassthroughOnStart)
        {
            StartCoroutine(EnablePassthroughDelayed(0.5f));
        }
    }
    
    private IEnumerator EnablePassthroughDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnablePassthrough(true);
    }
    
    /// <summary>
    /// Enables or disables the passthrough functionality
    /// </summary>
    /// <param name="enable">Whether to enable or disable passthrough</param>
    /// <param name="immediate">Whether to transition immediately or use the transition duration</param>
    public void EnablePassthrough(bool enable, bool immediate = false)
    {
        if (_isPassthroughActive == enable)
            return;
        
        _isPassthroughActive = enable;
        
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
        }
        
        if (immediate)
        {
            SetPassthroughOpacity(enable ? passthroughOpacity : 0);
        }
        else
        {
            _transitionCoroutine = StartCoroutine(TransitionPassthrough(enable));
        }
        
        LogMessage($"Passthrough {(enable ? "enabled" : "disabled")}");
    }
    
    private IEnumerator TransitionPassthrough(bool enable)
    {
        float startOpacity = enable ? 0 : passthroughOpacity;
        float targetOpacity = enable ? passthroughOpacity : 0;
        float elapsedTime = 0;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / transitionDuration);
            float curveValue = transitionCurve.Evaluate(normalizedTime);
            float currentOpacity = Mathf.Lerp(startOpacity, targetOpacity, curveValue);
            
            SetPassthroughOpacity(currentOpacity);
            
            yield return null;
        }
        
        SetPassthroughOpacity(targetOpacity);
        _transitionCoroutine = null;
    }
    
    /// <summary>
    /// Sets the opacity of the passthrough
    /// </summary>
    /// <param name="opacity">Opacity value (0-1)</param>
    public void SetPassthroughOpacity(float opacity)
    {
        try
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (OVRManager.instance != null)
            {
                OVRManager.instance.isPassthroughEnabled = opacity > 0;
                
                if (opacity > 0)
                {
                    // Set passthrough parameters using OculusPassthroughLayer if available
                    var passthroughLayer = FindObjectOfType<OVRPassthroughLayer>();
                    if (passthroughLayer != null)
                    {
                        var currentStyle = passthroughLayer.colorMapEditorType;
                        var colorAdjustment = new OVRPassthroughLayer.ColorMapEditorColorAdjustment();
                        
                        colorAdjustment.Brightness = passthroughBrightness;
                        colorAdjustment.Contrast = passthroughContrast;
                        colorAdjustment.Posterize = 0;
                        colorAdjustment.Saturation = 0;
                        
                        passthroughLayer.SetColorMapControls(colorAdjustment);
                        passthroughLayer.edgeRenderingEnabled = false;
                        
                        // Use opacity value
                        var currentColor = passthroughLayer.edgeColor;
                        currentColor.a = opacity;
                        passthroughLayer.edgeColor = currentColor;
                    }
                    else
                    {
                        LogMessage("No OVRPassthroughLayer found. Creating one.");
                        
                        // Create a new GameObject with OVRPassthroughLayer if it doesn't exist
                        var passthroughObject = new GameObject("PassthroughLayer");
                        passthroughLayer = passthroughObject.AddComponent<OVRPassthroughLayer>();
                        
                        // Configure the layer
                        var colorAdjustment = new OVRPassthroughLayer.ColorMapEditorColorAdjustment();
                        colorAdjustment.Brightness = passthroughBrightness;
                        colorAdjustment.Contrast = passthroughContrast;
                        
                        passthroughLayer.SetColorMapControls(colorAdjustment);
                    }
                }
            }
            else
            {
                LogError("OVRManager instance not found. Cannot set passthrough opacity.");
            }
            #else
            LogMessage($"Setting passthrough opacity to {opacity} (Editor mode)");
            #endif
        }
        catch (System.Exception e)
        {
            LogError($"Error setting passthrough opacity: {e.Message}");
        }
    }
    
    /// <summary>
    /// Updates the passthrough visual settings
    /// </summary>
    /// <param name="brightness">Brightness adjustment (-1 to 1)</param>
    /// <param name="contrast">Contrast adjustment (-1 to 1)</param>
    public void UpdatePassthroughSettings(float brightness, float contrast)
    {
        passthroughBrightness = brightness;
        passthroughContrast = contrast;
        
        if (_isPassthroughActive)
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                var passthroughLayer = FindObjectOfType<OVRPassthroughLayer>();
                if (passthroughLayer != null)
                {
                    var colorAdjustment = new OVRPassthroughLayer.ColorMapEditorColorAdjustment();
                    colorAdjustment.Brightness = brightness;
                    colorAdjustment.Contrast = contrast;
                    
                    passthroughLayer.SetColorMapControls(colorAdjustment);
                }
                #endif
            }
            catch (System.Exception e)
            {
                LogError($"Error updating passthrough settings: {e.Message}");
            }
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && _isPassthroughActive)
        {
            // Re-enable passthrough when app resumes
            EnablePassthrough(true, true);
        }
    }
    
    private void OnDestroy()
    {
        // Ensure passthrough is disabled when the component is destroyed
        if (_isPassthroughActive)
        {
            EnablePassthrough(false, true);
        }
    }
    
    private void LogMessage(string message)
    {
        if (verboseLogging)
        {
            Debug.Log($"[PassthroughManager] {message}");
        }
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[PassthroughManager] {message}");
    }
}
