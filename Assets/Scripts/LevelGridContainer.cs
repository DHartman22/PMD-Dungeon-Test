using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGridContainer : MonoBehaviour
{
    public static LevelGridContainer instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public Grid grid;
    public Tilemap tiles;
    public TileBase wall1;
    public TileBase wall2;
    public TileBase ground1;
    public TileBase ground2;

    public TileBase water1;



    public Transform gridStart;
    public float cellSize = .24f;
    public int gridWidth = 64;
    public int gridHeight = 64;
    public Vector2Int cellTest;
    public List<List<GridCell>> gridCells = new List<List<GridCell>>();


    public void MoveAgentToNewCell(GridCell originalCell, GridCell newCell)
    {
        if(originalCell.IsAgentInCell() && newCell.IsAgentInCell())
        {
            //Swap their position and put the newCell agent in paused state
            //This temp thing may cause problems?
            //Should only trigger for teammates swapping places
            Agent temp = newCell.agentInCell;
            newCell.agentInCell = originalCell.agentInCell;
            originalCell.agentInCell = temp;
        }
        else
        {
            newCell.agentInCell = originalCell.agentInCell;
            originalCell.agentInCell = null;
        }
    }

    public Vector3 GetCellCenterWorld(Vector2Int coords)
    {
        return grid.GetCellCenterWorld(new Vector3Int(coords.x, coords.y));
    }

    void ApplySprites()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                switch (gridCells[x][y].type)
                {
                    case TerrainType.Ground:
                        {
                            if(gridCells[x][y].location == Location.Corridor)
                                tiles.SetTile(new Vector3Int(x, y, 0), ground2);
                            else
                                tiles.SetTile(new Vector3Int(x, y, 0), ground1);
                            break;
                        }
                    case TerrainType.Solid:
                        {
                            tiles.SetTile(new Vector3Int(x, y, 0), wall1);

                            break;
                        }
                    case TerrainType.Water:
                        {
                            tiles.SetTile(new Vector3Int(x, y, 0), water1);

                            break;
                        }
                    case TerrainType.Ungenerated:
                        {
                            tiles.SetTile(new Vector3Int(x, y, 0), wall2);

                            break;
                        }
                }
                
            }
        }
    }

    public List<GridCell> GetNeighboringGridCells(int x, int y)
    {
        List<GridCell> neighbors = new List<GridCell>();

        if(GetGridCellAtPos(x, y + 1) != null)
            neighbors.Add(GetGridCellAtPos(x, y+1));


        if(GetGridCellAtPos(x + 1, y + 1) != null)
            neighbors.Add(GetGridCellAtPos(x+1, y+1));
        
        if(GetGridCellAtPos(x + 1, y) != null)
            neighbors.Add(GetGridCellAtPos(x+1, y));
        
        if(GetGridCellAtPos(x + 1, y - 1) != null)
            neighbors.Add(GetGridCellAtPos(x+1, y - 1));
        
        if(GetGridCellAtPos(x, y - 1) != null)
            neighbors.Add(GetGridCellAtPos(x, y - 1));
        
        if(GetGridCellAtPos(x - 1, y - 1) != null)
            neighbors.Add(GetGridCellAtPos(x-1, y - 1));
        
        if(GetGridCellAtPos(x - 1, y) != null)
            neighbors.Add(GetGridCellAtPos(x-1, y));
        
        if(GetGridCellAtPos(x - 1, y + 1) != null)
            neighbors.Add(GetGridCellAtPos(x -1, y + 1));

        return neighbors;
    }

    public List<GridCell> GetCardinalNeighboringGridCells(int x, int y)
    {
        List<GridCell> neighbors = new List<GridCell>();

        if (GetGridCellAtPos(x, y + 1) != null)
            neighbors.Add(GetGridCellAtPos(x, y + 1));

        if (GetGridCellAtPos(x + 1, y) != null)
            neighbors.Add(GetGridCellAtPos(x + 1, y));

        if (GetGridCellAtPos(x, y - 1) != null)
            neighbors.Add(GetGridCellAtPos(x, y - 1));

        if (GetGridCellAtPos(x - 1, y) != null)
            neighbors.Add(GetGridCellAtPos(x - 1, y));


        return neighbors;
    }

    public List<GridCell> GetCellsAtLocation(Location loc)
    {
        List<GridCell> cells = new List<GridCell>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (gridCells[x][y].location == loc)
                {
                    cells.Add(gridCells[x][y]);
                }
            }
        }

        return cells;
    }

    GridCell GetGridCellAtPos(int x, int y)
    {
        if(x > 0 && x < gridWidth && y > 0 && y < gridHeight)
        {
            return gridCells[x][y];
        }
        else
        {
            return null;
        }
    }

    public void ClearGrid()
    {
        tiles.ClearAllTiles();
        gridCells = new List<List<GridCell>>(gridWidth);
        for (int i = 0; i < gridWidth; i++)
        {
            gridCells.Insert(i, new List<GridCell>(gridHeight));
            for (int j = 0; j < gridHeight; j++)
            {
                gridCells[i].Add(new GridCell(i, j));
            }
        }
        //gridCells[2][1].type = TerrainType.Solid;
    }

    // Start is called before the first frame update
    void Start()
    {
        ClearGrid();
        GameEvents.instance.onGenerationComplete += ApplySprites;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {

        if(gridCells.Count > 0)
            Gizmos.DrawWireSphere(GetCellCenterWorld(gridCells[1][0].GetGridPosition()), .1f);


        for(int i = 0; i < gridWidth; i++)
            for(int j = 0; j < gridHeight; j++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(new Vector3((cellSize * i) + cellSize/2, (cellSize * j) + cellSize / 2) + gridStart.position, new Vector3(cellSize, cellSize));
                if (gridCells[i][j].IsAgentInCell())
                {
                    
                    if(gridCells[i][j].agentInCell.name == "AA")
                    {
                        Gizmos.color = Color.green;
                    }
                    else
                    {
                        Gizmos.color = Color.white;
                    }
                    Gizmos.DrawWireSphere(GetCellCenterWorld(new Vector2Int(i, j)), cellSize);
                }
            }
    }
}
