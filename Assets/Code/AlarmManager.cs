using System;
using System.IO;
using UnityEngine;


public class AlarmManager
{
    private const string ACTION_SET_ALARM = "android.intent.action.SET_ALARM";
    private const string EXTRA_HOUR = "android.intent.extra.alarm.HOUR";
    private const string EXTRA_MINUTES = "android.intent.extra.alarm.MINUTES";
    private const string EXTRA_MESSAGE = "android.intent.extra.alarm.MESSAGE";
    private const string FILE_NAME = "SavedAlarm.data";

    private int _hour;
    private int _minute;
    private string _path;

    public static AlarmManager Instance
    {
        get
        {
            if (_instance == null)
                Instance = new AlarmManager();
            return _instance;
        }
        set
        {
            if (_instance != null)
                Debug.Log("An instance of the AlarmManager already exists");
            else
                _instance = value;
        }
    }

    private static AlarmManager _instance;

    public AlarmManager()
    {
        _path = Application.persistentDataPath + "/" + FILE_NAME;
        DataItem savedAlarm = LoadData();
        _hour = savedAlarm.hour;
        _minute = savedAlarm.minute;
    }

    public int GetAlarmHour()
    {
        return _hour;
    }
    
    public int GetAlarmMinute()
    {
        return _minute;
    }

    public bool CheckAlarm(int hour, int minute)
    {
        if (hour == _hour && minute == _minute)
        {
            return true;
        }
        else
            return false;
    }

    public void SetupAlarm(int hour, int minute)
    {
        Debug.Log("Setting up an alarm in my application");
        _hour = hour;
        _minute = minute;
        SaveData(new DataItem(hour, minute));
#if UNITY_ANDROID && !UNITY_EDITOR
        CreateAndroidAlarm(hour, minute, "My alarm");
#endif
    }

    private void CreateAndroidAlarm(int hour, int minute, string message)
    {
        Debug.Log("Setting up an alarm in android system");
        var intent = new AndroidJavaObject("android.content.Intent", ACTION_SET_ALARM);
        intent
            //.Call<AndroidJavaObject>("putExtra", EXTRA_MESSAGE, message)
            .Call<AndroidJavaObject>("putExtra", EXTRA_HOUR, hour.ToString())
            .Call<AndroidJavaObject>("putExtra", EXTRA_MINUTES, minute.ToString());
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            unityPlayer.GetStatic<AndroidJavaObject>("currentActivity").Call("startActivity", intent);
        }
    }

    private DataItem LoadData()
    {
        try
        {
            var str = File.ReadAllText(_path);
            return JsonUtility.FromJson<DataItem>(str);
        }
        catch
        {
            return new DataItem();
        }
    }

    private void SaveData(DataItem item)
    {
        var str = JsonUtility.ToJson(item);
        File.WriteAllText(_path, str);
    }

    [Serializable]
    public struct DataItem
    {
        public int hour;
        public int minute;

        public DataItem(int h = -1, int m = -1)
        {
            hour = h;
            minute = m;
        }
    }
}
