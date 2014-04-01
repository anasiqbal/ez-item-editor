using UnityEngine;
using UnityEditor;
using System.Collections;

public class EZItemManagerWindow : EditorWindow
{
    private const string menuItemLocation = "Assets/EZ Item Manager";

    [MenuItem(menuItemLocation)]
    private static void showEditor()
    {
        EditorWindow.GetWindow<EZItemManagerWindow>(false, "EZ Item Manager");
    }

    [MenuItem(menuItemLocation, true)]
    private static bool showEditorValidator()
    {
        return true;
    }
}
