using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuildingData))]
public class BuildingDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BuildingData data = (BuildingData)target;

        // 기본 필드 먼저 출력
        data.width = EditorGUILayout.IntField("Width", data.width);
        data.height = EditorGUILayout.IntField("Height", data.height);

        // 배열 크기 맞추기
        int size = data.width * data.height;
        if (data.patternFlat == null || data.patternFlat.Length != size)
        {
            data.patternFlat = new bool[size];
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pattern (2D Grid)");

        // 2D 형태로 출력
        for (int y = data.height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < data.width; x++)
            {
                int index = y * data.width + x;

                data.patternFlat[index] = GUILayout.Toggle(
                    data.patternFlat[index],
                    "",
                    GUILayout.Width(20),
                    GUILayout.Height(20)
                );
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(data);
        }
    }
}