using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AchievementData))]
public class AchievementDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty conditionTypeProperty = serializedObject.FindProperty("conditionType");
        SerializedProperty taskCategoryProperty = serializedObject.FindProperty("taskCategory");

        // Draw default inspector fields except for conditionType
        EditorGUILayout.PropertyField(serializedObject.FindProperty("achievementName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("targetValue"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("iconSprite"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));

        // Draw conditionType field
        EditorGUILayout.PropertyField(conditionTypeProperty);

        // Check if conditionType is TasksOfTypeCompleted
        if ((AchievementConditionType)conditionTypeProperty.enumValueIndex == AchievementConditionType.TasksOfTypeCompleted)
        {
            // Show additional field for selecting TaskCategory
            EditorGUILayout.PropertyField(taskCategoryProperty);
        }
        else
        {
            // Hide the taskCategory field
            taskCategoryProperty.intValue = 0; // Reset taskCategory value
            serializedObject.ApplyModifiedProperties(); // Apply changes before making taskCategory private

            // Make taskCategory private
            SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(taskCategoryProperty, true);
            EditorGUI.EndDisabledGroup();

            // Restore serializedObject state
            serializedObject.FindProperty("m_Script").objectReferenceValue = scriptProperty.objectReferenceValue;
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}