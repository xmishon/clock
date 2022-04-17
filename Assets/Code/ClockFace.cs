using UnityEngine;
using UnityEngine.UI;

namespace Clock
{
    public class ClockFace
    {
        private Image _hourArrow;
        private Image _minuteArrow;

        public ClockFace(Image hourArrow, Image minuteArrow)
        {
            _hourArrow = hourArrow;
            _minuteArrow = minuteArrow;
        }

        public void UpdateArrowPositions(int hours, int minutes)
        {
            float hourAngle = 360.0f / 12.0f * (hours + minutes / 60.0f);
            float minutesAngle = minutes / 60.0f * 360.0f;

            _hourArrow.transform.rotation = Quaternion.Euler(0, 0, -hourAngle);
            _minuteArrow.transform.rotation = Quaternion.Euler(0, 0, -minutesAngle);
        }
    }
}
