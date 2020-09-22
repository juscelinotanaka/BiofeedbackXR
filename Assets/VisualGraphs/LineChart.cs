using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace VisualGraphs
{
    public class LineChart : MonoBehaviour
    {
        private static LineChart _instance;

        [Header("Line Properties")]
        [SerializeField] private LineRenderer linePrefab;
        [SerializeField] private Color color = Color.red;
        [Range(0.01f, 0.2f)] [SerializeField] private float lineWidth = 0.1f;

        [SerializeField] private GraphAxis graphAxis;

        [SerializeField] private bool2 plotAxis = math.bool2(true, false);
        [SerializeField] private float2 size;
        [SerializeField, HideInInspector] private float min;
        [SerializeField, HideInInspector] private float max;
        [SerializeField, HideInInspector] private LineRenderer _line;

        [SerializeField] private int maxPoints = 100;

        [SerializeField] private bool dynamicMinMax = true;

        public readonly Queue<float> _values = new Queue<float>(0);

        public int Axis => plotAxis[0] ? 0 : 1;
        public int Other => plotAxis[1] ? 0 : 1;

        private void Awake()
        {
            if (_instance != null)
            {
                _instance = this;
            }

            if (dynamicMinMax)
            {
                min = float.MaxValue;
                max = float.MinValue;
            }
        }

        #region Static Calls

        public static void InsertData(float value)
        {
            _instance.Insert(value);
        } 

        #endregion

        private void OnValidate()
        {
            if (graphAxis != null)
            {
                graphAxis.SetSize(size);
            }

            UpdateAll();
        }

        [ContextMenu("Create Line")]
        public void CreateAxis()
        {
            if (_line != null)
            {
                if (Application.isPlaying)
                    Destroy(_line.gameObject);
                else
                    DestroyImmediate(_line.gameObject);
            }

            _line = Instantiate(linePrefab, transform, false);
            _line.name = "Line";

            UpdateAll();
        }

        private void UpdateAll()
        {
            UpdateVisual();
            UpdateLine();
        }

        private void UpdateVisual()
        {
            _line.widthMultiplier = lineWidth;
            _line.startColor = _line.endColor = color;
        }

        public void Insert(float value)
        {
            _values.Enqueue(value);
            while (_values.Count > maxPoints)
                _values.Dequeue();

            if (dynamicMinMax)
            {
                if (value < min)
                    min = value;

                if (value > max)
                    max = value;
            }


            if (_line.positionCount != _values.Count)
            {
                _line.positionCount = _values.Count;
            }

            UpdateLine();
        }

        private void UpdateLine()
        {
            int i = 0;
            float step = size[Axis] / _values.Count;
            float3 current = float3.zero;
            NativeArray<float3> positions = new NativeArray<float3>(_values.Count, Allocator.Temp);
            foreach (float value in _values)
            {
                current.x = i * step;
                current.y = math.lerp(0, size[Other], math.unlerp(min, max, value));
                positions[i++] = current;
            }
            _line.SetPositions(positions.Reinterpret<Vector3>().ToArray());

            positions.Dispose();
        }

        public static float InverseLerp(float a, float b, float value)
        {
            return Math.Abs(a - b) > 0.0001f ? Mathf.Clamp01((value - a) / (b - a)) : 0.0f;
        }
    }
}
