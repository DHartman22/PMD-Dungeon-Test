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
        if(status == AgentStatus.Paused)
        {
            status = AgentStatus.Normal;
            return;
        }
        switch(team)
        {
            case Team.Ally:
                {
                    if(TickManager.instance.phase == TickPhase.Player)
                    {
                        // do nothing
                        TickManager.instance.agentController.awaitingAction = true;
                        break;
                    }
                    else
                    {
                        // AI stuff
                        MoveAgent(Vector2Int.right);
                    }
                    break;
                }
            case Team.Enemy:
                {
                    MoveAgent(Vector2Int.left);
                    break;
                }
        }
    }

    public void ApplyStatus(AgentStatus newStatus)
    {
        // Add log
        status = newStatus;
    }

    public bool MoveAgent(Vector2Int direction, bool ignoreCollision = false)
    {

        GridCell target = LevelGridContainer.instance.gridCells[coords.x + direction.x][coords.y + direction.y];
        if (IsTerrainPassable(target.type) || ignoreCollision)
        {
            if (target.IsAgentInCell())
            {
                if (TickManager.instance.phase == TickPhase.Player)
                {
                    switch (target.agentInCell.team)
                    {
                        case Team.Ally:
                            {
                                //swap places if (this) can inhabit cell
                                // can only be done by the player
                                SwapAction action = new SwapAction(this, target.agentInCell);
                                TickManager.instance.NewAction(action);

                                return true;
                            }
                        case Team.Enemy:
                            {
                                // Push enemy?
                                return false;
                            }
                    }
                }
            }
            else
            {
                MovementAction action = new MovementAction(direction, this);
                TickManager.instance.NewAction(action);
                return true;
            }
        }
        return false;
    }

    private bool IsTerrainPassable(TerrainType type)
    {
        if(type == TerrainType.Solid)
            return false;
        //if (type == TerrainType.Ground)
        //    return true;
        //if (type == TerrainType.Water) //Add a type check for this later
        //    return true;



        return true;
    }

    private bool IsTerrainPassable(GridCell targetCell)
    {
        if (targetCell.type == TerrainType.Solid)
            return false;
        

        if(targetCell.IsAgentInCell())
        {
            if (TickManager.instance.phase == TickPhase.Player)
            {
                switch(targetCell.agentInCell.team)
                {
                    case Team.Ally:
                        {
                            //swap places if (this) can inhabit cell
                            // can only be done by the player
                            return false;
                        }
                    case Team.Enemy:
                        {
                            // Push enemy?
                            return false;
                        }
                }   
            }
        }


        return true;
    }

    
   

    public void ReportSuccess()
    {
        if(TickManager.instance.agentController.currentAgent == this)
        {
            GameEvents.instance.OnSuccessfulPlayerEvent();
        }
        switch (team)
        {
            case Team.Ally:
                {
                    GameEvents.instance.OnSuccessfulAllyEvent();
                    break;
                }
            case Team.Enemy:
                {
                    GameEvents.instance.OnSuccessfulEnemyEvent();
                    break;
                }
        }
        return;
    }

    public Vector2Int GetGridPosition()
    {
        return coords; 
    }

    void InitializeGridPosition()
    {
        LevelGridContainer.instance.gridCells[coords.x][coords.y].agentInCell = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameEvents.instance.onGenerationComplete += InitializeGridPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
