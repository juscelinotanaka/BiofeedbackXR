using System;
using Unity.Mathematics;
using UnityEngine;

namespace VisualGraphs
{
    
    public class GraphAxis : MonoBehaviour
    {
        [SerializeField] private LineRenderer linePrefab;

        [Header("Axis Settings")]
        [SerializeField] private bool2 showAxis = new bool2(true);
        private float2 _size = new float2(1);

        [Header("Line Properties")]
        [SerializeField] private Color color = Color.red;
        [Range(0.01f, 0.2f)] [SerializeField] private float lineWidth = 0.1f;


        [SerializeField, HideInInspector] private LineRenderer[] axisLines;

        public void SetSize(float2 newSize)
        {
            _size = newSize;
            UpdateAxis();
        }

        private LineRenderer X
        {
            get => axisLines[0];
            set => axisLines[0] = value;
        }

        private LineRenderer Y
        {
            get => axisLines[1];
            set => axisLines[1] = value;
        }

        private void OnValidate()
        {
            UpdateAxis();
        }

        [ContextMenu("Recreate Axis")]
        public void CreateAxis()
        {
            foreach (LineRenderer line in axisLines)
            {
                if (line == null)
                    continue;

                if (Application.isPlaying)
                    Destroy(line.gameObject);
                else
                    DestroyImmediate(line.gameObject);
            }
            axisLines = new LineRenderer[2];
            X = Instantiate(linePrefab, transform, false);
            X.name = "X";

            Y = Instantiate(linePrefab, transform, false);
            Y.name = "Y";
        }

        public void UpdateAxis()
        {
            X.SetPosition(0, new Vector3(-0.1f, 0, 0));
            X.SetPosition(1, new Vector3(_size.x, 0, 0));
            X.widthMultiplier = lineWidth;
            X.startColor = X.endColor = color;

            Y.SetPosition(0, new Vector3(0, -0.1f, 0));
            Y.SetPosition(1, new Vector3(0, _size.y, 0));
            Y.widthMultiplier = lineWidth;
            Y.startColor = Y.endColor = color;
        }
    }
}