using UnityEditor;
using UnityEngine;

namespace VisualGraphs.Editor
{
    [CustomEditor(typeof(LineChart))]
    [CanEditMultipleObjects]
    public class LineChartEditor : UnityEditor.Editor
    {
        private float _newValue;
        private LineChart _chart;

        private const string MinRandomChartKey = "min_random_chart";
        private const string MaxRandomChartKey = "max_random_chart";
        private const string RandRandomChartKey = "rand_random_chart";

        private SerializedProperty _min;
        private SerializedProperty _max;
        private SerializedProperty _dynamicMinMax;

        private float MinRandom
        {
            get => EditorPrefs.GetFloat(MinRandomChartKey);
            set => EditorPrefs.SetFloat(MinRandomChartKey, value);
        }

        private float MaxRandom
        {
            get => EditorPrefs.GetFloat(MaxRandomChartKey);
            set => EditorPrefs.SetFloat(MaxRandomChartKey, value);
        }

        private bool Randomize
        {
            get => EditorPrefs.GetInt(RandRandomChartKey) == 1;
            set => EditorPrefs.SetInt(RandRandomChartKey, value ? 1 : 0);
        }

        private void OnEnable()
        {
            _chart = target as LineChart;
            _min = serializedObject.FindProperty("min");
            _max = serializedObject.FindProperty("max");
            _dynamicMinMax = serializedObject.FindProperty("dynamicMinMax");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            GUI.enabled = !_dynamicMinMax.boolValue;
            float cachedLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 30;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_min);
            EditorGUILayout.PropertyField(_max);
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
            EditorGUIUtility.labelWidth = cachedLabelWidth;

            GUILayout.Space(20f);
            GUILayout.Label("Test area", EditorStyles.boldLabel);
            Randomize = EditorGUILayout.Toggle("Randomize", Randomize);
            GUI.enabled = Randomize;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MinMax", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
            MinRandom = EditorGUILayout.FloatField(MinRandom);
            MaxRandom = EditorGUILayout.FloatField(MaxRandom);
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            GUILayout.Space(20f);

            _newValue = EditorGUILayout.FloatField("Value", _newValue);

            if (GUILayout.Button("Insert Value"))
            {
                _chart.Insert(_newValue);
                if (Randomize)
                {
                    _newValue = Random.Range(MinRandom, MaxRandom);
                    Debug.Log(_newValue);
                }
            }

            GUILayout.Label($"Total Items: {_chart._values.Count}");

            serializedObject.ApplyModifiedProperties();
        }
    }
}