using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum TerrainType
{
    Ground, //Open terrain that anyone can walk on
    Solid, //Only ghost types can occupy these cells, also blocks LOS based attacks. Cannot corner cut
    Water, //Only water types can occupy these cells, but can be corner cut by anyone
    Ungenerated
}

public enum Location
{
    Room,
    Corridor,
    RoomWall,
    RoomWallCorner,
    RoomExit,
    Undetermined
}

// Stores information related to what's inside a given grid cell.
// Accessed during any action that requires information about certain cells
// e.g. using an attack, moving, etc

[System.Serializable]
public class Cell
{
    // Terrain type
    public TerrainType type;

    public void SetTerrainType(TerrainType type)
    {
        this.type = type;
    }

    public Cell(TerrainType type = TerrainType.Ungenerated)
    {
        this.type = type;
    }
    public Cell()
    {
        this.type = TerrainType.Ungenerated;
    }
}


[System.Serializable]
public class GridCell : Cell
{
    int x;
    int y;

    // Item?

    // Agent in cell, null if no agent is present
    public Agent agentInCell;

    // On generation, all bordering cells of each room are marked as room walls, preventing rooms from generating in this space
    public bool isRoomWall; 

    public Location location;

    public GridCell(int x, int y, TerrainType type = TerrainType.Ungenerated)
    {
        this.x = x;
        this.y = y;
        this.type = type;
        location = Location.Undetermined;
    }

    public GridCell(int x, int y, Cell cell)
    {
        this.x = x;
        this.y = y;
        this.type = cell.type;
        location = Location.Undetermined;
    }

    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(x, y);
    }

    

    public bool IsAgentInCell() {  return agentInCell != null; }


}


