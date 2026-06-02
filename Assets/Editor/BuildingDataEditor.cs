using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuildingData))]
public class BuildingDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BuildingData data = (BuildingData)target;

        // =========================
        // Size
        // =========================

        data.width =
            EditorGUILayout.IntField(
                "Width",
                data.width);

        data.height =
            EditorGUILayout.IntField(
                "Height",
                data.height);

        EditorGUILayout.Space();

        // =========================
        // Work
        // =========================

        EditorGUILayout.LabelField(
            "Work",
            EditorStyles.boldLabel);

        data.jobType =
            (JobType)EditorGUILayout.EnumPopup(
                "Job Type",
                data.jobType);

        data.workSlotCost =
            EditorGUILayout.IntField(
                "Work Slot Cost",
                data.workSlotCost);

        data.workSlotCost =
            Mathf.Max(1, data.workSlotCost);

        EditorGUILayout.Space();

        // =========================
        // Pattern Grid
        // =========================

        int size = data.width * data.height;

        if (data.patternFlat == null ||
            data.patternFlat.Length != size)
        {
            data.patternFlat = new bool[size];
        }

        EditorGUILayout.LabelField(
            "Pattern (2D Grid)",
            EditorStyles.boldLabel);

        for (int y = data.height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < data.width; x++)
            {
                int index =
                    y * data.width + x;

                data.patternFlat[index] =
                    GUILayout.Toggle(
                        data.patternFlat[index],
                        "",
                        GUILayout.Width(20),
                        GUILayout.Height(20));
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(data);
        }
    }
}