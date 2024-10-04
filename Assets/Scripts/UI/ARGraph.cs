using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RL.Graphs
{
    /// <summary>
    /// Accept/Reject visualization box graph.
    /// </summary>
    public class ARGraph : MonoBehaviour
    {
        [SerializeField, Range(0, 1)] float _xMin;
        public float xMin
        {
            get { return _xMin; }
            set
            {
                _xMin = value;
                Refit();
            }
        }
        [SerializeField, Range(0, 1)] float _xMax;
        public float xMax
        {
            get { return _xMax; }
            set
            {
                _xMax = value;
                Refit();
            }
        }
        [SerializeField, Range(0, 1)] float _yMin;
        public float yMin
        {
            get { return yMin; }
            set
            {
                _yMin = value;
                Refit();
            }
        }
        [SerializeField, Range(0, 1)] float _yMax;
        public float yMax
        {
            get { return _yMax; }
            set
            {
                _yMax = value;
                Refit();
            }
        }
        public float PlotThickness = 10f;
        public Color PlotColor = Color.red;
        [SerializeField] List<GameObject> points = new();

        [SerializeField] Color color;
        public Color Color
        {
            get { return color; }
            set { boxPlotFill.Color = value; }
        }

        public BoxPlotFill boxPlotFill;
        public Transform plotsContainer;
        public GameObject plotPrefab;

        void OnValidate()
        {
            boxPlotFill.Color = color;
            Refit();
        }

        public void SetBoundsY(float yMin, float yMax)
        {
            this.yMin = yMin;
            this.yMax = yMax;
        }

        public void PlotPoint(float value)
        {
            value = Mathf.Clamp01(value);
            var go = Instantiate(plotPrefab, plotsContainer);
            go.GetComponent<Image>().color = PlotColor;
            var rect = (RectTransform) go.transform;
            rect.anchorMin = new(0f, value);
            rect.anchorMax = new(1f, value);
            rect.sizeDelta = new(0f, PlotThickness);
            rect.anchoredPosition = Vector2.zero;
            points.Add(go);
        }

        public void RemovePoints()
        {
            foreach (GameObject go in points)
            {
                Destroy(go);
            }
            points.Clear();
        }

        public void Refit()
        {
            boxPlotFill.Refit(_xMin, _xMax, _yMin, _yMax);
        }
    }
}