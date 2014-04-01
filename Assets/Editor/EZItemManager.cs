using UnityEngine;
using UnityEditor;
using System.Collections;

public class EZItemManager : EditorWindow
{
    private const string menuItemLocation = "Assets/EZ Item Manager";

    [MenuItem(menuItemLocation)]
    private static void showEditor()
    {
        EditorWindow.GetWindow<EZItemManager>(false, "EZ Item Manager");
    }

    [MenuItem(menuItemLocation, true)]
    private static bool showEditorValidator()
    {
        return true;
    }
}
