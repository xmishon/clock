using System.Collections;
using System.Collections.Generic;
using System;
using Extensions;
using UnityEngine;
using UnityEngine.UI;
using Clock;
using UnityEngine.EventSystems;
using TMPro;

public class AlarmSetupView : MonoBehaviour
{
    public event Action<int> minutesChanged;
    public event Action<int> hoursChanged;

    [SerializeField]
    private GraphicRaycaster _graphicRaycaster;
    [SerializeField]
    private EventSystem _eventSystem;
    [SerializeField]
    private Image _hourArrow;
    [SerializeField]
    private Image _minuteArrow;
    [SerializeField]
    private TMP_InputField _hoursInputField;
    [SerializeField]
    private TMP_InputField _minutesInputField;
    [SerializeField]
    private TextMeshProUGUI _amPmButtonText;

    private int Minute
    {
        get
        {
            return _minute;
        }
        set
        {
            if(value >= 59)
                _minute = 59; 
            else if (value <= 0)
                _minute = 0;
            else
                _minute = value;
            minutesChanged?.Invoke(_minute);
            _clockFace.UpdateArrowPositions(_hour, _minute);
            UpdateMinutesText(_minute);
        }
    }
    private int Hour
    {
        get
        {
            return _hour;
        }
        set
        {
            if (value > 23)
                _hour = 23;
            else if (value < 0)
                _hour = 0;
            else
                _hour = value;
            hoursChanged?.Invoke(_hour);
            _clockFace.UpdateArrowPositions(_hour, _minute);
            UpdateHoursText(_hour);
        }
    }
    private bool AM
    {
        get
        {
            return _am;
        }
        set
        {
            _am = value;
            _amPmButtonText.text = _am ? "AM" : "PM";
            if (Hour > 11 && AM)
            {
                Hour -= 12;
            }
            if (Hour < 12 && !AM)
            {
                Hour += 12;
            }

        }
    }

    private PointerEventData _eventData;
    private ClockFace _clockFace;
    private bool _isDragging;
    private bool _dragMinutes;
    private Vector3 _touchPosition;
    private int _minute;
    private int _hour;
    private bool _am;
    private int _previousMinute;

    public void UpdateMinutesInput(string minutes)
    {
        int result;
        if (int.TryParse(minutes, out result))
        {
            if (result < 0 || result > 59)
            {
                _minutesInputField.text = "";
            }
            else
            {
                Minute = result;
            }
        }
        else
        {
            _minutesInputField.text = "";
        }
    }

    public void UpdatetHoursInput(string hours)
    {
        int result;
        if (int.TryParse(hours, out result))
        {
            if (result < 0 || result > 23)
            {
                _hoursInputField.text = "";
            }
            else
            {
                if (result < 12)
                {
                    AM = true;
                }
                else
                {
                    AM = false;
                }
                Hour = result;
            }
        }
        else
        {
            _hoursInputField.text = "";
        }
    }

    public void SwitchAmPm()
    {
        AM = !AM;
    }

    public void SetupAlarm()
    {
        AlarmManager.Instance.SetupAlarm(_hour, _minute);
    }

    private void UpdateHoursText(int hours)
    {
        _hoursInputField.text = hours.ToString();
    }

    private void UpdateMinutesText(int minutes)
    {
        _minutesInputField.text = minutes.ToString();
    }

    private void Start()
    {
        _clockFace = new ClockFace(_hourArrow, _minuteArrow);
        AM = true;
        Hour = AlarmManager.Instance.GetAlarmHour();
        Minute = AlarmManager.Instance.GetAlarmMinute();
        _clockFace.UpdateArrowPositions(Hour, Minute);
        UpdateHoursText(Hour);
        UpdateMinutesText(Minute);
    }

    private void Update()
    {
        IdentifyDragging();
        if (_isDragging)
        {
            double arrowAngle = CalculateArrowAngle(_dragMinutes ? _minuteArrow.transform.position : _hourArrow.transform.position, _touchPosition);
            if (_dragMinutes)
            {
                Minute = GetMinutesFromAngle(arrowAngle);
                if (Minute < 15 && _previousMinute > 45)
                {
                    if (Hour >= 23)
                        Hour -= 23;
                    else
                        Hour++;
                } else if (Minute > 45 && _previousMinute < 15)
                {
                    if (Hour <= 0)
                        Hour += 23;
                    else
                        Hour--;
                }
                _previousMinute = Minute;
                UpdateMinutesText(Minute);
            }
            else
            {
                Hour = GetHoursFromAngle(arrowAngle);
                UpdateHoursText(Hour);
            }
        }
    }

    private void IdentifyDragging()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.touches[0];
            _touchPosition = touch.position;
            if (touch.phase == TouchPhase.Began)
            {
                _eventData = new PointerEventData(_eventSystem);
                _eventData.position = _touchPosition;
                List<RaycastResult> raycastResults = new List<RaycastResult>();
                _graphicRaycaster.Raycast(_eventData, raycastResults);
                foreach (var result in raycastResults)
                {
                    if (result.gameObject.CompareTag("Arrow"))
                    {
                        if (result.gameObject.GetComponent<Image>().Equals(_hourArrow))
                        {
                            _dragMinutes = false;
                            Debug.Log("Drag hours");
                        }
                        else if (result.gameObject.GetComponent<Image>().Equals(_minuteArrow))
                        {
                            _dragMinutes = true;
                            Debug.Log("Drag minutes");
                        }
                        _isDragging = true;
                        break;
                    }
                }
            }
        }
        else
        {
            _isDragging = false;
        }
    }

    private double CalculateArrowAngle(Vector3 centerPosition, Vector3 fingerPosition)
    {
        Vector3 fingerVector = fingerPosition - centerPosition;
        if (Vector3.Angle(Vector3.right, fingerVector) < 90.0d)
        {
            return Vector3.Angle(Vector3.up, fingerVector);
        }
        else
        {
            return 360.0d - Vector3.Angle(Vector3.up, fingerVector);
        }
    }

    private int GetMinutesFromAngle(double angle)
    {
        return (int)(angle / 6);
    }

    private int GetHoursFromAngle(double angle)
    {
        return AM ? (int)(angle / 30) : (int)(angle / 30) + 12;
    }
}
