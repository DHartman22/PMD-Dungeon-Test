using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DungeonGenerator gen = (DungeonGenerator)target;
        base.OnInspectorGUI();
        if(GUILayout.Button("Generate"))
        {
            gen.Generate();
        }
    }
}
