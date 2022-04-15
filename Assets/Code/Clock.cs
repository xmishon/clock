using System.Collections;
using System.Collections.Generic;
using System;
using Extensions;
using UnityEngine;
using UnityEngine.UI;
using clock;

public class Clock : MonoBehaviour
{
    public DateTime dateTime;

    [SerializeField]
    private Image _hourArrow;
    [SerializeField]
    private Image _minuteArrow;
    [SerializeField]
    private List<string> _ntpServers;
    [SerializeField]
    private CanvasScaler _rootCanvasScaler;
    [SerializeField]
    private double _updateIntervalInMinutes = 60.0d;

    private int _hours;
    private int _minutes;
    private DateTime _lastUpdateTime;

    private void Start()
    {
        UpdateTimeFromNetwork();
        _lastUpdateTime = dateTime;
    }

    private void Update()
    {
        TimeTick();
        UpdateArrowPositions();
        if (dateTime.Subtract(_lastUpdateTime) > TimeSpan.FromMinutes(_updateIntervalInMinutes))
        {
            UpdateTimeFromNetwork();
            _lastUpdateTime = dateTime;
        }
    }

    private void TimeTick()
    {
        dateTime = dateTime.AddSeconds(Time.deltaTime);
        _hours = dateTime.Hour;
        _minutes = dateTime.Minute;
    }

    private void UpdateArrowPositions()
    {
        float hourAngle = 360.0f / 12.0f * (_hours + _minutes / 60.0f);
        float minutesAngle = _minutes / 60.0f * 360.0f;

        _hourArrow.transform.rotation = Quaternion.Euler(0, 0, -hourAngle);
        _minuteArrow.transform.rotation = Quaternion.Euler(0, 0, -minutesAngle);
    }

    private void OnRectTransformDimensionsChange()
    {
        Debug.Log("Dimensions changed!");
        int height = Screen.height;
        int width = Screen.width;
        _rootCanvasScaler.matchWidthOrHeight = width > height ? 1.0f : 0.0f;
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
