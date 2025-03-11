using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

[Serializable]
public class TaskData
{
    public string title;
    public bool isCompleted;
    public DateTime createdAt;
    public DateTime? dueDate;
    public int priority;
}

[Serializable]
public class PomodoroSession
{
    public DateTime startTime;
    public DateTime endTime;
    public bool isWorkMode;
    public float duration;
    public bool wasCompleted;
}

[Serializable]
public class HealthData
{
    public DateTime date;
    public int waterIntake;
    public List<DateTime> breakTimes;
    public List<DateTime> waterReminderTimes;
}

[Serializable]
public class AppData
{
    public List<TaskData> tasks = new List<TaskData>();
    public List<PomodoroSession> pomodoroHistory = new List<PomodoroSession>();
    public List<HealthData> healthHistory = new List<HealthData>();
}

public class DataManager : MonoBehaviour
{
    private static DataManager instance;
    public static DataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DataManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("DataManager");
                    instance = go.AddComponent<DataManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private AppData currentData = new AppData();
    private string dataPath;

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

        dataPath = Path.Combine(Application.persistentDataPath, "appdata.json");
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            if (File.Exists(dataPath))
            {
                string json = File.ReadAllText(dataPath);
                currentData = JsonUtility.FromJson<AppData>(json);
                Debug.Log("Data loaded successfully");
            }
            else
            {
                Debug.Log("No data file found, starting fresh");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading data: {e.Message}");
        }
    }

    private void SaveData()
    {
        try
        {
            string json = JsonUtility.ToJson(currentData, true);
            File.WriteAllText(dataPath, json);
            Debug.Log("Data saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving data: {e.Message}");
        }
    }

    // Task Management
    public void AddTask(TaskData task)
    {
        currentData.tasks.Add(task);
        SaveData();
    }

    public void UpdateTask(int index, TaskData updatedTask)
    {
        if (index >= 0 && index < currentData.tasks.Count)
        {
            currentData.tasks[index] = updatedTask;
            SaveData();
        }
    }

    public void RemoveTask(int index)
    {
        if (index >= 0 && index < currentData.tasks.Count)
        {
            currentData.tasks.RemoveAt(index);
            SaveData();
        }
    }

    public List<TaskData> GetTasks()
    {
        return currentData.tasks;
    }

    // Pomodoro History
    public void AddPomodoroSession(PomodoroSession session)
    {
        currentData.pomodoroHistory.Add(session);
        SaveData();
    }

    public List<PomodoroSession> GetPomodoroHistory(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (startDate == null && endDate == null)
        {
            return currentData.pomodoroHistory;
        }

        return currentData.pomodoroHistory.FindAll(session =>
            (!startDate.HasValue || session.startTime >= startDate.Value) &&
            (!endDate.HasValue || session.endTime <= endDate.Value)
        );
    }

    // Health Tracking
    public void AddHealthData(HealthData data)
    {
        // Find existing data for the same date
        var existingData = currentData.healthHistory.Find(d => d.date.Date == data.date.Date);
        if (existingData != null)
        {
            existingData.waterIntake = data.waterIntake;
            existingData.breakTimes = data.breakTimes;
            existingData.waterReminderTimes = data.waterReminderTimes;
        }
        else
        {
            currentData.healthHistory.Add(data);
        }
        SaveData();
    }

    public HealthData GetHealthData(DateTime date)
    {
        return currentData.healthHistory.Find(d => d.date.Date == date.Date);
    }

    public List<HealthData> GetHealthHistory(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (startDate == null && endDate == null)
        {
            return currentData.healthHistory;
        }

        return currentData.healthHistory.FindAll(data =>
            (!startDate.HasValue || data.date >= startDate.Value) &&
            (!endDate.HasValue || data.date <= endDate.Value)
        );
    }

    // Statistics
    public Dictionary<string, float> GetPomodoroStatistics(DateTime startDate, DateTime endDate)
    {
        var sessions = GetPomodoroHistory(startDate, endDate);
        var stats = new Dictionary<string, float>
        {
            { "totalSessions", sessions.Count },
            { "completedSessions", sessions.Count(s => s.wasCompleted) },
            { "totalWorkTime", sessions.Where(s => s.isWorkMode).Sum(s => s.duration) },
            { "totalBreakTime", sessions.Where(s => !s.isWorkMode).Sum(s => s.duration) }
        };

        return stats;
    }

    public Dictionary<string, float> GetHealthStatistics(DateTime startDate, DateTime endDate)
    {
        var data = GetHealthHistory(startDate, endDate);
        var stats = new Dictionary<string, float>
        {
            { "averageWaterIntake", data.Average(d => d.waterIntake) },
            { "totalBreaks", data.Sum(d => d.breakTimes.Count) },
            { "averageBreaksPerDay", data.Count > 0 ? data.Average(d => d.breakTimes.Count) : 0 }
        };

        return stats;
    }

    // Debug methods
    public void ClearAllData()
    {
        currentData = new AppData();
        SaveData();
        Debug.Log("All data cleared");
    }

    public void TestDataPersistence()
    {
        // Add test task
        var task = new TaskData
        {
            title = "Test Task",
            isCompleted = false,
            createdAt = DateTime.Now,
            priority = 1
        };
        AddTask(task);

        // Add test Pomodoro session
        var session = new PomodoroSession
        {
            startTime = DateTime.Now.AddMinutes(-30),
            endTime = DateTime.Now,
            isWorkMode = true,
            duration = 30 * 60,
            wasCompleted = true
        };
        AddPomodoroSession(session);

        // Add test health data
        var healthData = new HealthData
        {
            date = DateTime.Now,
            waterIntake = 4,
            breakTimes = new List<DateTime> { DateTime.Now.AddMinutes(-15) },
            waterReminderTimes = new List<DateTime> { DateTime.Now.AddMinutes(-30) }
        };
        AddHealthData(healthData);

        Debug.Log("Test data added successfully");
    }
} 