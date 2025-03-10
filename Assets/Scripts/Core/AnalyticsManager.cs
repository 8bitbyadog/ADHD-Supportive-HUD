using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

[Serializable]
public class UsageEvent
{
    public string eventType;
    public DateTime timestamp;
    public Dictionary<string, object> parameters;
}

[Serializable]
public class UserFeedback
{
    public DateTime timestamp;
    public string feedbackType;
    public string message;
    public int rating;
    public Dictionary<string, object> metadata;
}

[Serializable]
public class AnalyticsData
{
    public List<UsageEvent> events = new List<UsageEvent>();
    public List<UserFeedback> feedback = new List<UserFeedback>();
    public Dictionary<string, int> featureUsageCount = new Dictionary<string, int>();
    public Dictionary<string, float> averageSessionDuration = new Dictionary<string, float>();
}

public class AnalyticsManager : MonoBehaviour
{
    private static AnalyticsManager instance;
    public static AnalyticsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AnalyticsManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AnalyticsManager");
                    instance = go.AddComponent<AnalyticsManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private AnalyticsData currentData = new AnalyticsData();
    private string analyticsPath;
    private DateTime sessionStartTime;

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

        analyticsPath = Path.Combine(Application.persistentDataPath, "analytics.json");
        LoadAnalytics();
        sessionStartTime = DateTime.Now;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            // App resumed
            sessionStartTime = DateTime.Now;
        }
        else
        {
            // App paused
            RecordSessionEnd();
        }
    }

    private void LoadAnalytics()
    {
        try
        {
            if (File.Exists(analyticsPath))
            {
                string json = File.ReadAllText(analyticsPath);
                currentData = JsonUtility.FromJson<AnalyticsData>(json);
                Debug.Log("Analytics loaded successfully");
            }
            else
            {
                Debug.Log("No analytics file found, starting fresh");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading analytics: {e.Message}");
        }
    }

    private void SaveAnalytics()
    {
        try
        {
            string json = JsonUtility.ToJson(currentData, true);
            File.WriteAllText(analyticsPath, json);
            Debug.Log("Analytics saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving analytics: {e.Message}");
        }
    }

    // Event Tracking
    public void TrackEvent(string eventType, Dictionary<string, object> parameters = null)
    {
        var usageEvent = new UsageEvent
        {
            eventType = eventType,
            timestamp = DateTime.Now,
            parameters = parameters ?? new Dictionary<string, object>()
        };

        currentData.events.Add(usageEvent);

        // Update feature usage count
        if (!currentData.featureUsageCount.ContainsKey(eventType))
        {
            currentData.featureUsageCount[eventType] = 0;
        }
        currentData.featureUsageCount[eventType]++;

        SaveAnalytics();
    }

    // Session Tracking
    private void RecordSessionEnd()
    {
        var sessionDuration = (DateTime.Now - sessionStartTime).TotalSeconds;
        var sessionKey = DateTime.Now.ToString("yyyy-MM-dd");

        if (!currentData.averageSessionDuration.ContainsKey(sessionKey))
        {
            currentData.averageSessionDuration[sessionKey] = 0;
        }

        // Update average session duration
        var currentAverage = currentData.averageSessionDuration[sessionKey];
        var newCount = currentData.featureUsageCount.GetValueOrDefault("session_start", 0);
        currentData.averageSessionDuration[sessionKey] = 
            (currentAverage * newCount + (float)sessionDuration) / (newCount + 1);

        TrackEvent("session_end", new Dictionary<string, object>
        {
            { "duration", sessionDuration }
        });
    }

    // User Feedback
    public void SubmitFeedback(string feedbackType, string message, int rating, Dictionary<string, object> metadata = null)
    {
        var feedback = new UserFeedback
        {
            timestamp = DateTime.Now,
            feedbackType = feedbackType,
            message = message,
            rating = rating,
            metadata = metadata ?? new Dictionary<string, object>()
        };

        currentData.feedback.Add(feedback);
        SaveAnalytics();
    }

    // Analytics Reports
    public Dictionary<string, object> GenerateDailyReport(DateTime date)
    {
        var report = new Dictionary<string, object>();
        var dateKey = date.ToString("yyyy-MM-dd");

        // Get events for the day
        var dailyEvents = currentData.events.FindAll(e => e.timestamp.Date == date);
        report["totalEvents"] = dailyEvents.Count;

        // Get feature usage
        var featureUsage = new Dictionary<string, int>();
        foreach (var evt in dailyEvents)
        {
            if (!featureUsage.ContainsKey(evt.eventType))
            {
                featureUsage[evt.eventType] = 0;
            }
            featureUsage[evt.eventType]++;
        }
        report["featureUsage"] = featureUsage;

        // Get average session duration
        if (currentData.averageSessionDuration.ContainsKey(dateKey))
        {
            report["averageSessionDuration"] = currentData.averageSessionDuration[dateKey];
        }

        // Get feedback for the day
        var dailyFeedback = currentData.feedback.FindAll(f => f.timestamp.Date == date);
        report["feedbackCount"] = dailyFeedback.Count;

        if (dailyFeedback.Count > 0)
        {
            var averageRating = dailyFeedback.Average(f => f.rating);
            report["averageRating"] = averageRating;
        }

        return report;
    }

    public Dictionary<string, object> GenerateWeeklyReport(DateTime startDate)
    {
        var report = new Dictionary<string, object>();
        var endDate = startDate.AddDays(7);

        // Get events for the week
        var weeklyEvents = currentData.events.FindAll(e => 
            e.timestamp >= startDate && e.timestamp < endDate);
        report["totalEvents"] = weeklyEvents.Count;

        // Get feature usage
        var featureUsage = new Dictionary<string, int>();
        foreach (var evt in weeklyEvents)
        {
            if (!featureUsage.ContainsKey(evt.eventType))
            {
                featureUsage[evt.eventType] = 0;
            }
            featureUsage[evt.eventType]++;
        }
        report["featureUsage"] = featureUsage;

        // Get average session durations
        var sessionDurations = new List<float>();
        for (var date = startDate; date < endDate; date = date.AddDays(1))
        {
            var dateKey = date.ToString("yyyy-MM-dd");
            if (currentData.averageSessionDuration.ContainsKey(dateKey))
            {
                sessionDurations.Add(currentData.averageSessionDuration[dateKey]);
            }
        }
        if (sessionDurations.Count > 0)
        {
            report["averageSessionDuration"] = sessionDurations.Average();
        }

        // Get feedback for the week
        var weeklyFeedback = currentData.feedback.FindAll(f => 
            f.timestamp >= startDate && f.timestamp < endDate);
        report["feedbackCount"] = weeklyFeedback.Count;

        if (weeklyFeedback.Count > 0)
        {
            var averageRating = weeklyFeedback.Average(f => f.rating);
            report["averageRating"] = averageRating;
        }

        return report;
    }

    // Debug methods
    public void ClearAnalytics()
    {
        currentData = new AnalyticsData();
        SaveAnalytics();
        Debug.Log("Analytics cleared");
    }

    public void TestAnalytics()
    {
        // Track some test events
        TrackEvent("app_start");
        TrackEvent("feature_used", new Dictionary<string, object>
        {
            { "feature_name", "task_completion" },
            { "task_id", 1 }
        });

        // Submit test feedback
        SubmitFeedback("usability", "The HUD is very helpful!", 5, new Dictionary<string, object>
        {
            { "feature", "task_display" }
        });

        Debug.Log("Test analytics added successfully");
    }
} 