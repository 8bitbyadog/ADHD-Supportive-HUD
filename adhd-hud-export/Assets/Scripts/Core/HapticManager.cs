using UnityEngine;
using Oculus.Interaction.Input;

public class HapticManager : MonoBehaviour
{
    private static HapticManager instance;
    public static HapticManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<HapticManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("HapticManager");
                    instance = go.AddComponent<HapticManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("Haptic Settings")]
    [SerializeField] private bool enableHaptics = true;
    [SerializeField] private float defaultDuration = 0.1f;
    [SerializeField] private float defaultFrequency = 0.5f;
    [SerializeField] private float defaultAmplitude = 0.5f;

    [Header("Hand References")]
    [SerializeField] private HandRef rightHandRef;
    [SerializeField] private HandRef leftHandRef;

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

        LoadHapticSettings();
        FindHandReferences();
    }

    private void LoadHapticSettings()
    {
        enableHaptics = PlayerPrefs.GetInt("EnableHaptics", 1) == 1;
    }

    private void SaveHapticSettings()
    {
        PlayerPrefs.SetInt("EnableHaptics", enableHaptics ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void FindHandReferences()
    {
        if (rightHandRef == null || leftHandRef == null)
        {
            var handRefs = FindObjectsOfType<HandRef>();
            foreach (var handRef in handRefs)
            {
                if (handRef.Handedness == Handedness.Right)
                {
                    rightHandRef = handRef;
                }
                else if (handRef.Handedness == Handedness.Left)
                {
                    leftHandRef = handRef;
                }
            }
        }
    }

    public void PlayHapticFeedback(Handedness hand, float duration = -1, float frequency = -1, float amplitude = -1)
    {
        if (!enableHaptics) return;

        HandRef handRef = hand == Handedness.Right ? rightHandRef : leftHandRef;
        if (handRef != null)
        {
            var controller = handRef.GetComponent<HandPinchInteractor>();
            if (controller != null)
            {
                controller.SetHaptics(
                    duration >= 0 ? duration : defaultDuration,
                    frequency >= 0 ? frequency : defaultFrequency,
                    amplitude >= 0 ? amplitude : defaultAmplitude
                );
            }
        }
    }

    public void PlayHapticFeedbackBothHands(float duration = -1, float frequency = -1, float amplitude = -1)
    {
        PlayHapticFeedback(Handedness.Right, duration, frequency, amplitude);
        PlayHapticFeedback(Handedness.Left, duration, frequency, amplitude);
    }

    // Predefined haptic patterns
    public void PlayTaskCompleteHaptic()
    {
        PlayHapticFeedbackBothHands(0.1f, 0.8f, 0.7f);
    }

    public void PlayPomodoroCompleteHaptic()
    {
        PlayHapticFeedbackBothHands(0.2f, 0.5f, 0.8f);
    }

    public void PlayBreakReminderHaptic()
    {
        PlayHapticFeedbackBothHands(0.15f, 0.6f, 0.6f);
    }

    public void PlayWaterReminderHaptic()
    {
        PlayHapticFeedbackBothHands(0.1f, 0.7f, 0.5f);
    }

    public void PlayUIInteractionHaptic()
    {
        PlayHapticFeedbackBothHands(0.05f, 0.9f, 0.4f);
    }

    public void PlayErrorHaptic()
    {
        // Play three quick pulses
        StartCoroutine(PlayErrorPattern());
    }

    private System.Collections.IEnumerator PlayErrorPattern()
    {
        for (int i = 0; i < 3; i++)
        {
            PlayHapticFeedbackBothHands(0.05f, 0.9f, 0.8f);
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void ToggleHaptics()
    {
        enableHaptics = !enableHaptics;
        SaveHapticSettings();
    }

    // Debug methods
    public void TestHaptics()
    {
        // Test different haptic patterns
        PlayTaskCompleteHaptic();
        Invoke("PlayPomodoroCompleteHaptic", 0.5f);
        Invoke("PlayBreakReminderHaptic", 1f);
        Invoke("PlayWaterReminderHaptic", 1.5f);
        Invoke("PlayUIInteractionHaptic", 2f);
        Invoke("PlayErrorHaptic", 2.5f);
    }

    public void TestIndividualHands()
    {
        // Test right hand
        PlayHapticFeedback(Handedness.Right, 0.2f, 0.5f, 0.7f);
        // Test left hand after a delay
        Invoke(() => PlayHapticFeedback(Handedness.Left, 0.2f, 0.5f, 0.7f), 0.3f);
    }
} 