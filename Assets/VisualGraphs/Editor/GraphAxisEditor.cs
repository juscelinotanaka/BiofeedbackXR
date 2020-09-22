using UnityEditor;
using UnityEngine;

namespace VisualGraphs.Editor
{
    [CustomEditor(typeof(GraphAxis))]
    public class GraphAxisEditor : UnityEditor.Editor
    {
        private GraphAxis _graphAxis;

        private void OnEnable()
        {
            _graphAxis = target as GraphAxis;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(20f);

            if (GUILayout.Button("Update Axis"))
                _graphAxis.UpdateAxis();
        }
    }
}