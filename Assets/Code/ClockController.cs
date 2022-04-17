using System.Collections;
using System.Collections.Generic;
using System;
using Extensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Clock;

public class ClockController : MonoBehaviour
{
    public DateTime dateTime;

    [SerializeField]
    private Image _hourArrow;
    [SerializeField]
    private Image _minuteArrow;
    [SerializeField]
    private List<string> _ntpServers;
    [SerializeField]
    private List<CanvasScaler> _rootCanvasScalers = new List<CanvasScaler>();
    [SerializeField]
    private double _updateIntervalInMinutes = 60.0d;
    [SerializeField]
    private GameObject _notification;
    [SerializeField]
    private AudioSource _audioSource;

    private int _hours;
    private int _minutes;
    private DateTime _lastUpdateTime;
    private ClockFace _clockFace;

    public void CallNotification()
    {
        AlarmManager.Instance.SetupAlarm(-1, -1);
        _notification.SetActive(true);
        _audioSource.Play();
    }

    public void CloseNotification()
    {
        _audioSource.Stop();
        _notification.SetActive(false);
    }

    private void Start()
    {
        _clockFace = new ClockFace(_hourArrow, _minuteArrow);
        UpdateTimeFromNetwork();
        _lastUpdateTime = dateTime;
    }

    private void Update()
    {
        TimeTick();
        _clockFace.UpdateArrowPositions(_hours, _minutes);
        if (dateTime.Subtract(_lastUpdateTime) > TimeSpan.FromMinutes(_updateIntervalInMinutes))
        {
            UpdateTimeFromNetwork();
            _lastUpdateTime = dateTime;
        }
        if (AlarmManager.Instance.CheckAlarm(_hours, _minutes))
        {
            CallNotification();
        }
    }

    private void TimeTick()
    {
        dateTime = dateTime.AddSeconds(Time.deltaTime);
        _hours = dateTime.Hour;
        _minutes = dateTime.Minute;
    }

    private void OnRectTransformDimensionsChange()
    {
        Debug.Log("Dimensions changed!");
        int height = Screen.height;
        int width = Screen.width;
        foreach (CanvasScaler canvasScaler in _rootCanvasScalers)
        {
            canvasScaler.matchWidthOrHeight = width > height ? 1.0f : 0.0f;
        }
    }

    private void UpdateTimeFromNetwork()
    {
        DateTime[] dateTimes = new DateTime[_ntpServers.Count];

        for (int i = 0; i < _ntpServers.Count; i++)
        {
            dateTimes[i] = new NetworkDateTimeSupplier(_ntpServers[i]).GetNetworkTime();
        }
        long average = 0;
        foreach (DateTime dt in dateTimes)
        {
            average += dt.TotalMilliseconds();
        }
        average /= dateTimes.Length;
        dateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)average);
        Debug.Log("Time updated from network!");
    }
}
