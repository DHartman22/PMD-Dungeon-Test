using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum AgentActionType
{
    Movement,
    Attack,
    Wait
}

public enum AgentActionState
{
    Prepare,
    Execute,
    Loop,
    Complete
}

public abstract class AgentAction
{
    public AgentActionType type;
    public Agent owner;
    public AgentActionState state;

    // Locks in the action, e.g. updating grid information for where the agent will move
    public abstract void Prepare();

    // Performs the action for the player to see, e.g. actually moving the agent
    public abstract void Execute();

    // Continues execution, runs every frame until action has finished executing
    public abstract void ExecuteLoop();

    // Marks this action as completed
    public abstract void Complete(); 

}

public class MovementAction : AgentAction
{
    const float MOVE_TIME = .15f;
    public Vector2Int dir;
    public float moveTimeProgress = 0f;
    public MovementAction(Vector2Int dir, Agent owner)
    {
        this.dir = dir;
        this.owner = owner;
    }

    public override void Prepare()
    {
        state = AgentActionState.Prepare;
        // Update the position on the grid in data
        LevelGridContainer.instance.MoveAgentToNewCell(LevelGridContainer.instance.gridCells[owner.coords.x][owner.coords.y],
            LevelGridContainer.instance.gridCells[owner.coords.x + dir.x][owner.coords.y + dir.y]);
        
    }

    public override void Execute()
    {
        state = AgentActionState.Execute;
        // Start moving the sprite

        state = AgentActionState.Loop;
    }
    public override void ExecuteLoop()
    {
        LevelGridContainer levelGrid = LevelGridContainer.instance;
        state = AgentActionState.Loop;
        // Lerp the sprite until it reaches its destination
        moveTimeProgress += Time.deltaTime;
        if (moveTimeProgress > MOVE_TIME) // Sets the 
        {
            moveTimeProgress = MOVE_TIME;
            owner.transform.position = levelGrid.GetCellCenterWorld(owner.coords + dir);
            Complete();
            return;
        }
        else
        {
            owner.transform.position = Vector3.Lerp(levelGrid.GetCellCenterWorld(owner.coords), levelGrid.GetCellCenterWorld(owner.coords + dir), moveTimeProgress/MOVE_TIME);
        }
        
    }

    public override void Complete()
    {
        state = AgentActionState.Complete;
        owner.coords += dir;
    }

}

public class AttackAction : AgentAction
{
    public Vector2 dir;
    // public Move move
    public float atk = 15;
    public AttackAction(Vector2 dir, Agent owner)
    {
        this.dir = dir;
        this.owner = owner;
    }

    public override void Prepare()
    {
        throw new System.NotImplementedException();
    }

    public override void Execute()
    {
        throw new System.NotImplementedException();
    }
    public override void ExecuteLoop()
    {

    }

    public override void Complete()
    {

    }
}