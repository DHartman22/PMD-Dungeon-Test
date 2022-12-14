using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Room))]
public class RoomEditor : Editor
{
    private SerializedObject sRoom;
    private SerializedProperty cellsObj;
    private Room room = null;
    private Cell[] roomCells;
    
    
    int cellWidthTest = 30;
    public void OnEnable()
    {
        sRoom = new SerializedObject(target);
        //roomObj = serializedObject.FindProperty("room");
        
        cellsObj = serializedObject.FindProperty("cells");

       
    }

    


    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        
        sRoom.Update();
        
        EditorGUILayout.HelpBox("RNG Weight 1 = 100% chance to spawn when chosen by PlaceRoomAttempt().\nRNG Weight 0.5 = 50% chance to spawn when chosen by PlaceRoomAttempt().", MessageType.Info);
        SerializedProperty sWeight = sRoom.FindProperty("rngWeight");
        sWeight.floatValue = EditorGUILayout.FloatField("RNG Weight", sWeight.floatValue);

        SerializedProperty sWidth = sRoom.FindProperty("width");
        sWidth.intValue = EditorGUILayout.IntField("Room width", sWidth.intValue);

        SerializedProperty sHeight = sRoom.FindProperty("height");
        sHeight.intValue = EditorGUILayout.IntField("Room height", sHeight.intValue);

        cellWidthTest = EditorGUILayout.IntField("Editor view cell width", cellWidthTest);

        SerializedProperty sArray = sRoom.FindProperty("cells");
        

        roomCells = ((Room)target).cells;
        room = (Room)target;

        if(sWidth.intValue <= 0 || sHeight.intValue <= 0)
        {
            sWidth.intValue = 1;
            sHeight.intValue = 1;
            room.InitializeCells(sWidth.intValue, sHeight.intValue);
        }

        if (roomCells == null || roomCells.Length != sWidth.intValue * sHeight.intValue)
            roomCells = room.InitializeCells(sWidth.intValue, sHeight.intValue);


        EditorGUILayout.HelpBox("Room Layout, origin is the bottom-leftmost cell.", MessageType.Info);

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginVertical();
        for(int y = sHeight.intValue-1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for(int x = 0; x < sWidth.intValue; x++)
            {
                GUILayoutOption[] options = new GUILayoutOption[2];
                options[0] = GUILayout.Width(cellWidthTest);
                options[1] = GUILayout.Height(cellWidthTest);

                switch (room.GetCell(x, y).type)
                {
                    case TerrainType.Ground:
                        {
                            GUI.contentColor = Color.green;
                            break;
                        }
                    case TerrainType.Solid:
                        {
                            GUI.contentColor = Color.red;
                            break;
                        }
                    case TerrainType.Water:
                        {
                            GUI.contentColor = Color.cyan;
                            break;
                        }
                }

                room.SetCell(x,y, (TerrainType)EditorGUILayout.EnumPopup(room.GetCell(x,y).type, options));
                
            }
            EditorGUILayout.EndHorizontal();
        }
            EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        serializedObject.ApplyModifiedProperties();
    }
}
