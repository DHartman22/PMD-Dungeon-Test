using UnityEngine;

[CreateAssetMenu(fileName = "Room", menuName = "ScriptableObjects/Room", order = 1)]
[System.Serializable]
public class Room : ScriptableObject
{
    public int width = 2;
    public int height = 2;
    public Cell[] cells;
    public float rngWeight = 0;
    public Room(int width, int height)
    {
        Debug.Log("New room");
        this.width = width;
        this.height = height;

        cells = new Cell[width * height];
        int i = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                cells[i] = new Cell();
                cells[i].type = TerrainType.Ground;
                i++;
            }
        }
    }

    public Cell[] InitializeCells(int width, int height)
    {
        Debug.Log("Init cells");
        this.width = width;
        this.height = height;
        rngWeight = 1;

        cells = new Cell[width * height];
        int i = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                cells[i] = new Cell();
                cells[i].type = TerrainType.Ground;
                i++;
            }
        }
        return cells;
    }

   
    // Access it like a 2d array
    public Cell GetCell(int cellX, int cellY)
    {
        int index = cellX + (cellY * width);
        try
        {
            return cells[index];
        }
        catch(System.IndexOutOfRangeException ex)
        {
            Debug.Log(index + " out of range of room name " + this.name);
            return null;
        }
    }

    public void SetCell(int cellX, int cellY, TerrainType newType)
    {
        
        int index = cellX + (cellY * width);
        cells[index].type = newType;
    }

}
