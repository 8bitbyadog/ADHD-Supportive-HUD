using UnityEngine;
using System;

public class HealthTracker : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int dailyWaterGoal = 8; // cups
    [SerializeField] private float breakReminderInterval = 30f * 60f; // 30 minutes in seconds
    [SerializeField] private float waterReminderInterval = 60f * 60f; // 1 hour in seconds

    [Header("References")]
    [SerializeField] private HUDManager hudManager;

    private int currentWaterIntake;
    private float lastBreakTime;
    private float lastWaterReminderTime;
    private bool isBreakReminderActive;
    private bool isWaterReminderActive;

    private void Start()
    {
        if (hudManager == null)
        {
            hudManager = FindObjectOfType<HUDManager>();
        }
        ResetTracker();
    }

    private void Update()
    {
        CheckBreakReminder();
        CheckWaterReminder();
    }

    private void CheckBreakReminder()
    {
        if (Time.time - lastBreakTime >= breakReminderInterval)
        {
            isBreakReminderActive = true;
            ShowBreakReminder();
        }
    }

    private void CheckWaterReminder()
    {
        if (Time.time - lastWaterReminderTime >= waterReminderInterval)
        {
            isWaterReminderActive = true;
            ShowWaterReminder();
        }
    }

    public void RecordWaterIntake(int cups)
    {
        currentWaterIntake = Mathf.Min(currentWaterIntake + cups, dailyWaterGoal);
        lastWaterReminderTime = Time.time;
        isWaterReminderActive = false;
        UpdateHUDDisplay();
    }

    public void RecordBreak()
    {
        lastBreakTime = Time.time;
        isBreakReminderActive = false;
    }

    private void ShowBreakReminder()
    {
        // TODO: Implement visual and/or haptic break reminder
        Debug.Log("Time for a break!");
    }

    private void ShowWaterReminder()
    {
        // TODO: Implement visual and/or haptic water reminder
        Debug.Log("Time to drink some water!");
    }

    private void UpdateHUDDisplay()
    {
        hudManager.UpdateHealthDisplay(currentWaterIntake);
    }

    public void ResetTracker()
    {
        currentWaterIntake = 0;
        lastBreakTime = Time.time;
        lastWaterReminderTime = Time.time;
        isBreakReminderActive = false;
        isWaterReminderActive = false;
        UpdateHUDDisplay();
    }

    // Debug methods
    public void SetShortIntervalsForTesting()
    {
        breakReminderInterval = 60f; // 1 minute
        waterReminderInterval = 120f; // 2 minutes
        ResetTracker();
    }

    public void SimulateWaterIntake()
    {
        RecordWaterIntake(1);
    }
} 