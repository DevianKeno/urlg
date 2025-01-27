/*

Component Title: Box Plot Fill
Data written: September 30, 2024
Date revised: October 4, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    A UI element representing the box fill of a graph.

Data Structures:
    Color: the color of the fill
*/

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

        /// <summary>
        /// Sets new values for the box fill and updates it visually.
        /// </summary>
        public void Refit(float xMin, float xMax, float yMin, float yMax)
        {
            fillRect.anchorMin = new Vector2(xMin, yMin);
            fillRect.anchorMax = new Vector2(xMax, yMax);

            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;
        }
    }
}