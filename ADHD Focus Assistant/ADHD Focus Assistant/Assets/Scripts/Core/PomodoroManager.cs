using UnityEngine;
using System;

public class PomodoroManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float workDuration = 25f * 60f; // 25 minutes in seconds
    [SerializeField] private float breakDuration = 5f * 60f; // 5 minutes in seconds
    [SerializeField] private float longBreakDuration = 15f * 60f; // 15 minutes in seconds
    [SerializeField] private int sessionsUntilLongBreak = 4;

    [Header("References")]
    [SerializeField] private HUDManager hudManager;

    private float currentTime;
    private bool isRunning;
    private bool isWorkMode;
    private int completedSessions;
    private float progress;

    private void Start()
    {
        if (hudManager == null)
        {
            hudManager = FindObjectOfType<HUDManager>();
        }
        ResetTimer();
    }

    private void Update()
    {
        if (isRunning)
        {
            UpdateTimer();
        }
    }

    private void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        progress = 1 - (currentTime / (isWorkMode ? workDuration : breakDuration));

        if (currentTime <= 0)
        {
            OnTimerComplete();
        }

        // Update HUD
        hudManager.UpdatePomodoroDisplay(currentTime, isWorkMode);
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        isRunning = false;
        isWorkMode = true;
        currentTime = workDuration;
        completedSessions = 0;
        progress = 0;
        hudManager.UpdatePomodoroDisplay(currentTime, isWorkMode);
    }

    private void OnTimerComplete()
    {
        if (isWorkMode)
        {
            completedSessions++;
            if (completedSessions >= sessionsUntilLongBreak)
            {
                // Start long break
                isWorkMode = false;
                currentTime = longBreakDuration;
                completedSessions = 0;
            }
            else
            {
                // Start regular break
                isWorkMode = false;
                currentTime = breakDuration;
            }
        }
        else
        {
            // Start work session
            isWorkMode = true;
            currentTime = workDuration;
        }

        // Play notification sound or haptic feedback
        PlayNotification();
    }

    private void PlayNotification()
    {
        // TODO: Implement notification sound or haptic feedback
    }

    // Public methods for external control
    public void ToggleTimer()
    {
        if (isRunning)
        {
            PauseTimer();
        }
        else
        {
            StartTimer();
        }
    }

    public void SkipCurrentSession()
    {
        OnTimerComplete();
    }

    // Debug methods
    public void SetShortDurationForTesting()
    {
        workDuration = 60f; // 1 minute
        breakDuration = 30f; // 30 seconds
        longBreakDuration = 45f; // 45 seconds
        ResetTimer();
    }
} 