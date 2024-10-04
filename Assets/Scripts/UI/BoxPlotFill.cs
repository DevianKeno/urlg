using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RL.Graphs
{
    public class BoxPlotFill : MonoBehaviour
    {
        public Color Color
        {
            set
            {
                fill.color = value;
            }
        }

        public Image fill;
        public RectTransform parentRect;
        public RectTransform fillRect;

        public void Refit(float xMin, float xMax, float yMin, float yMax)
        {
            fillRect.anchorMin = new Vector2(xMin, yMin);
            fillRect.anchorMax = new Vector2(xMax, yMax);

            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;
        }
    }
}