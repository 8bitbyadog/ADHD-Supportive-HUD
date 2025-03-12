using System.Collections;
using UnityEngine;
using Meta.XR;

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
    
    public void SetPassthroughOpacity(float opacity)
    {
        try
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (Meta.XR.XRSystem.Instance != null)
            {
                Meta.XR.XRSystem.Instance.PassthroughEnabled = opacity > 0;
                
                if (opacity > 0)
                {
                    var passthroughLayer = FindObjectOfType<Meta.XR.PassthroughLayer>();
                    if (passthroughLayer != null)
                    {
                        var colorAdjustment = new Meta.XR.PassthroughLayer.ColorAdjustment();
                        colorAdjustment.Brightness = passthroughBrightness;
                        colorAdjustment.Contrast = passthroughContrast;
                        colorAdjustment.Saturation = 0;
                        
                        passthroughLayer.SetColorAdjustment(colorAdjustment);
                        passthroughLayer.EnableEdgeRendering = false;
                        
                        var currentColor = passthroughLayer.EdgeColor;
                        currentColor.a = opacity;
                        passthroughLayer.EdgeColor = currentColor;
                    }
                    else
                    {
                        LogMessage("No PassthroughLayer found. Creating one.");
                        
                        var passthroughObject = new GameObject("PassthroughLayer");
                        passthroughLayer = passthroughObject.AddComponent<Meta.XR.PassthroughLayer>();
                        
                        var colorAdjustment = new Meta.XR.PassthroughLayer.ColorAdjustment();
                        colorAdjustment.Brightness = passthroughBrightness;
                        colorAdjustment.Contrast = passthroughContrast;
                        
                        passthroughLayer.SetColorAdjustment(colorAdjustment);
                    }
                }
            }
            else
            {
                LogError("XRSystem instance not found. Cannot set passthrough opacity.");
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
    
    public void UpdatePassthroughSettings(float brightness, float contrast)
    {
        passthroughBrightness = brightness;
        passthroughContrast = contrast;
        
        if (_isPassthroughActive)
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                var passthroughLayer = FindObjectOfType<Meta.XR.PassthroughLayer>();
                if (passthroughLayer != null)
                {
                    var colorAdjustment = new Meta.XR.PassthroughLayer.ColorAdjustment();
                    colorAdjustment.Brightness = brightness;
                    colorAdjustment.Contrast = contrast;
                    
                    passthroughLayer.SetColorAdjustment(colorAdjustment);
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
            EnablePassthrough(true, true);
        }
    }
    
    private void OnDestroy()
    {
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
