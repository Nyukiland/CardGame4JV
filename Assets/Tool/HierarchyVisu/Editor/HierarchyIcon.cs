using UnityEditor;
using UnityEngine;


namespace CustomTool.HierarchyIcon
{
    [InitializeOnLoad]
    public class HierarchyIcon
    {
        static HierarchyIcon()
        {
            EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyIcon;
        }

        private static void DrawHierarchyIcon(int instanceID, Rect selectionRect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (obj == null) return;

            Texture2D icon = EditorGUIUtility.GetIconForObject(obj);

            if (icon == null) return;

            float iconScaleChange = Mathf.Max(icon.width, icon.height) / 16;

            Rect r = new Rect(selectionRect.xMax - 16, selectionRect.yMin, icon.width / iconScaleChange, icon.height / iconScaleChange);
            GUI.DrawTexture(r, icon);
        }
    }
}