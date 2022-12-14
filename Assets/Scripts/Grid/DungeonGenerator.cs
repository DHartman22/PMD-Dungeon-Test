using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] List<Room> rooms = new List<Room>();

    [SerializeField] int roomAttempts;

    [SerializeField] int maxRooms;
    [SerializeField] int minRooms;
    [SerializeField] int minDistanceFromOtherRooms;

    [SerializeField] int maxTileSum; // Unimplemented

    [SerializeField] int tileSum;

    [SerializeField] int corridorAttempts;
    [SerializeField] int minCorridors;
    [SerializeField] int maxCorridors;
    [SerializeField] float baseTurnChance;
    [SerializeField] bool allowCorridorOverlap;

    // Used to avoid corridors running parallel to each other, should be 5
    [SerializeField] int maxNeighboringCorridorCells = 5;
    
    // Increments the turn chance by this variable each cell, the longer the corridor the more likely it will take a turn
    [SerializeField] float turnChancePerCellModifier;

    // Contains all GridCells marked as room walls except for the corners, used in corridor generation
    List<GridCell> roomSides;

    [SerializeField] int seed = 0;
    [SerializeField] bool randomizeSeed = false;
    static public DungeonGenerator instance;

    List<GameObject> lineRendererObjects;

    [SerializeField] Material lineMat;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void Generate()
    {
        // Needs to be done to move on
        LevelGridContainer.instance.ClearGrid();
        roomSides = new List<GridCell>();
        foreach(GameObject obj in lineRendererObjects)
        {
            Destroy(obj);
        }
        lineRendererObjects.Clear();
        tileSum = 0;

        int roomSuccessCount = 0;

        if(!randomizeSeed)
            Random.InitState(seed);

        

        for(int i = 0; i < roomAttempts; i++)
        {
            if(PlaceRoomAttempt()) roomSuccessCount++;
            if(roomSuccessCount >= maxRooms) break;
        }

        int hallwaySuccessCount = 0;
        for (int i = 0; i < corridorAttempts; i++)
        {
            if (PlaceCorridor()) hallwaySuccessCount++;
            if (hallwaySuccessCount >= maxCorridors) break;
        }

        GameEvents.instance.OnGenerationComplete();
        Debug.Log(roomSuccessCount + "/" + roomAttempts + " room attempts successful.");
        Debug.Log(hallwaySuccessCount + "/" + corridorAttempts + " corridor attempts successful.");

    }

    // Places room in a random starting position on the grid
    // If any tile in newRoom overlaps an existing room, return false and don't place room
    // Otherwise, place room
    bool PlaceRoomAttempt()
    {
        // Pick random room
        // Pick random starting position, rooms use bottom left cell as origin
        // If the room doesn't fit, return false
        // If any cell overlaps with a non-ungenerated cell, return false
        // Otherwise, place cells
        LevelGridContainer grid = LevelGridContainer.instance;
        Room newRoom = rooms[Random.Range(0, rooms.Count)];

        // Never place rooms on the border
        Vector2Int pos = new Vector2Int(Random.Range(1, grid.gridWidth-1),
            Random.Range(1, grid.gridHeight-1));

        if(grid.gridWidth <= pos.x + newRoom.width || grid.gridHeight <= pos.y + newRoom.height)
        {
            return false;
        }

        // Ensure the new room does not overlap other generated rooms or room walls
        // 
        for(int x = pos.x - minDistanceFromOtherRooms; x < pos.x + newRoom.width + minDistanceFromOtherRooms; x++)
        {
            for(int y = pos.y - minDistanceFromOtherRooms; y < pos.y + newRoom.height + minDistanceFromOtherRooms; y++)
            {
                try
                {
                    if (grid.gridCells[x][y].type != TerrainType.Ungenerated || grid.gridCells[x][y].location == Location.RoomWall)
                    {
                        return false;
                    }
                }
                catch (System.ArgumentOutOfRangeException ex)
                {
                    Debug.LogWarning(ex.Message);

                }
            }
        }

        // Place the room
        int localX = 0;
        int localY = 0;
        for (int globalX = pos.x; globalX < pos.x + newRoom.width; globalX++)
        {
            for (int globalY = pos.y; globalY < pos.y + newRoom.height; globalY++)
            {

                try
                {
                    grid.gridCells[globalX][globalY].type = newRoom.GetCell(localX, localY).type;
                    grid.gridCells[globalX][globalY].location = Location.Room;
                    localY++;

                }
                catch (System.NullReferenceException ex)
                {
                    Debug.LogError(ex.Message);
                }
            }
            localX++;
            localY = 0;
        }

        // Mark the surrounding area as room wall cells

        for (int globalX = pos.x-1; globalX <= pos.x + newRoom.width; globalX++)
            for (int globalY = pos.y-1; globalY <= pos.y + newRoom.height; globalY++)
                if((globalX == pos.x-1 || globalX == pos.x + newRoom.width) || (globalY == pos.y - 1 || globalY == pos.y + newRoom.height))
                {
                    // Ensure the border isn't out of the grid's range
                    if (globalX < 0 || globalX >= grid.gridWidth || globalY < 0 || globalY >= grid.gridHeight)
                    {
                        continue;
                    }
                    grid.gridCells[globalX][globalY].location = Location.RoomWall;
                    grid.gridCells[globalX][globalY].type = TerrainType.Solid;
                   

                    roomSides.Add(grid.gridCells[globalX][globalY]);
                }

        try
        {
            grid.gridCells[pos.x + newRoom.width][pos.y + newRoom.height].location = Location.RoomWallCorner;
            grid.gridCells[pos.x - 1][pos.y + newRoom.height].location = Location.RoomWallCorner;
            grid.gridCells[pos.x + newRoom.width][pos.y - 1].location = Location.RoomWallCorner;
            grid.gridCells[pos.x -1][pos.y -1].location = Location.RoomWallCorner;
        }
        catch (System.ArgumentOutOfRangeException ex)
        {
            Debug.LogError(ex.Message);
            Debug.LogError("Corner is out of range. Position:" + pos + " newRoom dimensions:" + newRoom.width + " " + newRoom.height);
        }



        tileSum += newRoom.width * newRoom.height;
        
        return true;
    }

    bool PlaceCorridor()
    {
        LevelGridContainer grid = LevelGridContainer.instance;

        // Start with a random room wall
        GridCell corridorStart = roomSides[Random.Range(0, roomSides.Count)];
        Vector2Int startCoords = corridorStart.GetGridPosition();


        // Find the "normal" of the room wall

        List<GridCell> cardinalNeighbors = grid.GetCardinalNeighboringGridCells(startCoords.x, startCoords.y);
        Vector2Int dir = Vector2Int.zero;
        foreach (GridCell gridCell in cardinalNeighbors)
        {
            if(gridCell.type == TerrainType.Ground)
            {
                // Find the direction this one is relative to corridorStart
                dir = corridorStart.GetGridPosition() - gridCell.GetGridPosition();
            }
            // Prevents new corridor from spawning right next to another
            if(gridCell.location == Location.Corridor || gridCell.location == Location.RoomExit)
            {
                return false;
            }
        }
        if (dir == Vector2Int.zero) return false;

        Vector2Int currentCoords = startCoords;
        int length = 0;

        // Ensure the initial while statement doesn't crash
        if ((currentCoords.x + dir.x) < 0 || (currentCoords.x + dir.x) >= grid.gridWidth ||
                (currentCoords.y + dir.y) < 0 || (currentCoords.y + dir.y) >= grid.gridHeight) return false;

        float turnChance = baseTurnChance;

        GameObject lineHolder = new GameObject(currentCoords.ToString());
        LineRenderer lineRenderer = lineHolder.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = 0;
        lineRenderer.startColor = new Color(Random.value, Random.value, Random.value);
        lineRenderer.endColor = lineRenderer.startColor;
        lineRenderer.material = lineMat;

        lineRendererObjects.Add(lineHolder);

        // Start the corridor at the current coords
        grid.gridCells[currentCoords.x][currentCoords.y].type = TerrainType.Ungenerated;
        grid.gridCells[currentCoords.x][currentCoords.y].location = Location.Corridor;
        
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(length, new Vector3(grid.GetCellCenterWorld(currentCoords).x, grid.GetCellCenterWorld(currentCoords).y, -1));
        length++;

        currentCoords += dir;

        bool turnedLastLoop = false; // Prevents a corridor from turning two times in a row

        // Ensures that we dont touch the borders of the grid with a hallway
        while (!(currentCoords.x + dir.x < 0 || currentCoords.x + dir.x >= grid.gridWidth ||
                currentCoords.y + dir.y < 0 || currentCoords.y + dir.y >= grid.gridHeight))
        {
            
            // Ensures corridor doesn't go inside rooms
            if (grid.gridCells[currentCoords.x][currentCoords.y].location == Location.Room)
            {
                Debug.Log("Corridor-room break at " + currentCoords);
                grid.gridCells[currentCoords.x][currentCoords.y].type = TerrainType.Water;
                grid.gridCells[currentCoords.x][currentCoords.y].location = Location.Corridor;
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(length, new Vector3(grid.GetCellCenterWorld(currentCoords).x, grid.GetCellCenterWorld(currentCoords).y, -1));

                length++;
                break;
            }

            if (grid.gridCells[currentCoords.x + dir.x][currentCoords.y + dir.y].location == Location.Corridor && !allowCorridorOverlap)
            {
                Debug.Log("Corridor-corridor break at " + currentCoords);
                grid.gridCells[currentCoords.x][currentCoords.y].type = TerrainType.Water;
                grid.gridCells[currentCoords.x][currentCoords.y].location = Location.Corridor;
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(length, new Vector3(grid.GetCellCenterWorld(currentCoords).x, grid.GetCellCenterWorld(currentCoords).y, -1));

                length++;
                break;
            }

            // Is the current GridCell bordering a room? If so end it here
            if ((grid.gridCells[currentCoords.x][currentCoords.y].location == Location.RoomWall) 
                || (grid.gridCells[currentCoords.x][currentCoords.y].location == Location.RoomWallCorner))
            {
               

                grid.gridCells[currentCoords.x][currentCoords.y].type = TerrainType.Water;
                grid.gridCells[currentCoords.x][currentCoords.y].location = Location.Corridor;
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(length, new Vector3(grid.GetCellCenterWorld(currentCoords).x, grid.GetCellCenterWorld(currentCoords).y, -1));

                length++;


                // If it's a corner, go one further and mark that as a room exit, otherwise mark the current position as the exit
                // Assumes that all rooms are squares
                if (grid.gridCells[currentCoords.x][currentCoords.y].location == Location.RoomWallCorner)
                {
                    currentCoords += dir;
                    grid.gridCells[currentCoords.x][currentCoords.y].type = TerrainType.Water;
                    grid.gridCells[currentCoords.x][currentCoords.y].location = Location.RoomExit;
                    length++;
                    currentCoords += dir;

                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(length, new Vector3(grid.GetCellCenterWorld(currentCoords).x, grid.GetCellCenterWorld(currentCoords).y, -1));
                    Debug.Log("Corridor-room corner break at " + currentCoords);

                }
                else
                {
                    grid.gridCells[currentCoords.x][currentCoords.y].location = Location.RoomExit;
                    Debug.Log("Corridor-room wall break at " + currentCoords);

                }
                break;
            }
             
            // Turn?
            float rand = Random.Range(0f, 1f);
            if (rand < turnChance && length > 0 && !turnedLastLoop)
            {
                turnChance = baseTurnChance;
                List<GridCell> cardinals = grid.GetCardinalNeighboringGridCells(currentCoords.x, currentCoords.y);

                cardinals.Remove(grid.gridCells[currentCoords.x + dir.x][currentCoords.y + dir.y]);
                cardinals.Remove(grid.gridCells[currentCoords.x - dir.x][currentCoords.y - dir.y]);

                GridCell newTarget = cardinals[Random.Range(0, 1)];
                dir = newTarget.GetGridPosition() - currentCoords;
                Debug.Log("Turning towards " + dir);
                turnedLastLoop = true;
                continue;
            }

            // Avoids parallel corridors
            int neighboringCorridorCells = 0;
            foreach(GridCell cell in grid.GetNeighboringGridCells(currentCoords.x, currentCoords.y))
            {
                if (cell.location == Location.Corridor || cell.location == Location.RoomExit) neighboringCorridorCells++;
            }
            if(neighboringCorridorCells >= maxNeighboringCorridorCells)
            {
                break;
            }

            grid.gridCells[currentCoords.x][currentCoords.y].type = TerrainType.Water;
            grid.gridCells[currentCoords.x][currentCoords.y].location = Location.Corridor;

            lineRenderer.positionCount++;
            lineRenderer.SetPosition(length, new Vector3(grid.GetCellCenterWorld(currentCoords).x, grid.GetCellCenterWorld(currentCoords).y, -1));
            length++;

            currentCoords += dir;

            turnChance += turnChancePerCellModifier;
            turnedLastLoop = false;
        }

        Debug.Log("Corridor length: " + length);

        return true;
    }

    // Start is called before the first frame update
    void Start()
    {
        lineRendererObjects = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

