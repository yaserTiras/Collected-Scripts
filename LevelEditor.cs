using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    private float CellSize = 100f; 
    private float CellSpacing = 2f; 
    private Vector2 scrollPosition;

    public override void OnInspectorGUI()
    {
        Level level = (Level)target;

        level.Rows = EditorGUILayout.IntField("Rows", level.Rows);
        level.Cols = EditorGUILayout.IntField("Cols", level.Cols);

        level.environment = (GameObject)EditorGUILayout.ObjectField("Environment", level.environment, typeof(GameObject), true);

        GUILayout.Space(10);

        int requiredCount = level.Rows * level.Cols;
        while (level.nodeData.Count < requiredCount)
            level.nodeData.Add(new VehicleValues());
        while (level.nodeData.Count > requiredCount)
            level.nodeData.RemoveAt(level.nodeData.Count - 1);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < level.Rows; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < level.Cols; j++)
            {
                GUILayout.Space(CellSpacing);
                VehicleValues node = level.GetNode(i, j);

                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Car Transform", GUILayout.Width(CellSize));
                GUILayout.Space(10); 
                node.car = (Transform)EditorGUILayout.ObjectField(node.car, typeof(Transform), true, GUILayout.Width(CellSize * 1.5f - 10)); 
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Color", GUILayout.Width(CellSize));
                GUILayout.Space(10);
                node.color = (VehicleColorValue)EditorGUILayout.EnumPopup(node.color, GUILayout.Width(CellSize - 10));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Direction", GUILayout.Width(CellSize));
                GUILayout.Space(10);
                node.direction = (CarDirection)EditorGUILayout.EnumPopup(node.direction, GUILayout.Width(CellSize - 10));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Width", GUILayout.Width(CellSize));
                GUILayout.Space(10);
                node.width = EditorGUILayout.FloatField(node.width, GUILayout.Width(CellSize * 0.6f));
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Height", GUILayout.Width(CellSize));
                GUILayout.Space(10);
                node.height = EditorGUILayout.FloatField(node.height, GUILayout.Width(CellSize * 0.6f));
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(CellSpacing);
                EditorGUILayout.EndVertical();

                GUILayout.Space(CellSpacing);
                level.SetNode(i, j, node);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(level);
        }
    }

}
