using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum AgentActionType
{
    Movement, //Movement actions can be executed in sync with other movement actions that share a phase
    Attack, // Attack actions must be executed one at a time individually
    Wait // Wait actions are executed one at a time after attacks, used for statuses like sleep/paralysis
}

public enum AgentActionState
{
    Prepare,
    Ready,
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

    // Performs any final bits of logic then calls Complete()
    public abstract void FinishLoop(); 

    public void Complete()
    {
        state = AgentActionState.Complete;
        owner.ReportSuccess();
    }

}

public class MovementAction : AgentAction
{
    const float MOVE_TIME = .25f;
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
        state = AgentActionState.Ready;
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
        // Lerp the sprite until it reaches its destination
        moveTimeProgress += Time.deltaTime;
        if (moveTimeProgress > MOVE_TIME) // Sets the 
        {
            
            FinishLoop();
            return;
        }
        else
        {
            owner.transform.position = Vector3.Lerp(levelGrid.GetCellCenterWorld(owner.coords), levelGrid.GetCellCenterWorld(owner.coords + dir), moveTimeProgress/MOVE_TIME);
        }
        
    }

    public override void FinishLoop()
    {
        moveTimeProgress = MOVE_TIME;
        owner.transform.position = LevelGridContainer.instance.GetCellCenterWorld(owner.coords + dir);

        // Apply movement to owner's coords
        owner.coords += dir;
        Complete();
    }

}

public class SwapAction : AgentAction
{
    const float MOVE_TIME = .15f;
    public Vector2Int dir;
    public float moveTimeProgress = 0f;
    Agent victim;
    public SwapAction(Agent owner, Agent victim)
    {
        this.dir = dir;
        this.owner = owner;
        this.victim = victim;
    }

    public override void Prepare()
    {
        state = AgentActionState.Prepare;
        // Update the position on the grid in data
        Vector2Int originalOwnerCoords = owner.coords;
        LevelGridContainer.instance.MoveAgentToNewCell(LevelGridContainer.instance.gridCells[owner.coords.x][owner.coords.y],
            LevelGridContainer.instance.gridCells[owner.coords.x + dir.x][owner.coords.y + dir.y]);
        state = AgentActionState.Ready;

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
            FinishLoop();
            return;
        }
        else
        {
            owner.transform.position = Vector3.Lerp(levelGrid.GetCellCenterWorld(owner.coords), levelGrid.GetCellCenterWorld(owner.coords + dir), moveTimeProgress / MOVE_TIME);
        }

    }

    public override void FinishLoop()
    {
        owner.coords += dir;
        Complete();
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

    public override void FinishLoop()
    {
        Complete();
    }
}

public class WaitAction : AgentAction
{
    public WaitAction(Agent owner)
    {
        this.owner = owner;
    }

    public override void Prepare()
    {
        state = AgentActionState.Prepare;
        state = AgentActionState.Ready;

    }

    public override void Execute()
    {
        state = AgentActionState.Execute;

        state = AgentActionState.Loop;

    }
    public override void ExecuteLoop()
    {
        FinishLoop();
    }

    public override void FinishLoop()
    {
        Debug.Log(owner.name + " completed WaitAction.");
        Complete();
    }
}