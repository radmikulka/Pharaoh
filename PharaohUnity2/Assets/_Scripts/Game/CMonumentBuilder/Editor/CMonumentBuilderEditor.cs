using UnityEngine;
using UnityEditor;

namespace CMonumentBuilder
{
    [CustomEditor(typeof(CMonumentBuilder))]
    public class CMonumentBuilderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            CMonumentBuilder builder = (CMonumentBuilder)target;

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate", GUILayout.Height(30)))
            {
                int group = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName("CMonumentBuilder Generate");
                builder.Generate();
                Undo.CollapseUndoOperations(group);
            }

            if (GUILayout.Button("Clean Generated"))
            {
                int group = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName("CMonumentBuilder Clean");
                builder.CleanGenerated();
                Undo.CollapseUndoOperations(group);
            }
        }
    }
}
