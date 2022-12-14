using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    Player,
    Ally,
    Enemy,
    Neutral
}

public enum AgentStatus
{
    Normal,
    Paused
}

public enum Type // Temporary, just lists relevant ones for movement
{
    Normal,
    Ghost,
    Water,
    Flying
}

public class Agent : MonoBehaviour
{

    public Team team;
    public AgentStatus status;
    public int health = 100;
    public int def = 5;
    public Vector2Int coords;
    public Vector2Int facing;


    // Called when TickManager decides it's this agent's turn to move
    public void RequestAction()
    {
        switch(team)
        {
            case Team.Player:
                {
                    // 
                    GetComponent<AgentController>().awaitingAction = true;
                    break;
                }
        }
    }

    public bool MoveAgent(Vector2Int direction)
    {
        GridCell target = LevelGridContainer.instance.gridCells[coords.x + direction.x][coords.y + direction.y];
        if (IsTerrainPassable(target.type))
        {
            MovementAction action = new MovementAction(direction, this);
            TickManager.instance.NewAction(action);
            if(team == Team.Player)
            {
                GameEvents.instance.OnSuccessfulPlayerEvent(); 
            }
            return true;
             
        }
        return false;
    }

    private bool IsTerrainPassable(TerrainType type)
    {
        if(type == TerrainType.Solid)
            return false;
        if (type == TerrainType.Ground)
            return true;
        if (type == TerrainType.Water) //Add a type check for this later
            return true;
        else
            return false;
    }

    public Vector2Int GetGridPosition()
    {
        return coords; 
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
