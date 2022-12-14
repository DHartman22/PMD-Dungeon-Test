using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// When attached to an agent, turns player input into AgentActions
public class AgentController : MonoBehaviour
{
    public bool awaitingAction;
    public Vector2Int input;
    public bool yMode; // Lock player in place
    public bool rMode; // Diagonal only mode
    public Agent currentAgent; // Enventually want this to take in any agent, for now just the player

    public void OnMove(InputValue value)
    {
        input = new Vector2Int((int)value.Get<Vector2>().x, (int)value.Get<Vector2>().y);
    }

    public void OnLock(InputValue value)
    {
        yMode = value.Get<float>() > 0.5f;
    }

    public void OnDiagonalLock(InputValue value)
    {
        rMode = value.Get<float>() > 0.5f;
    }

    void ProcessInput()
    {
        if (input == Vector2Int.zero)
            return;

        if (rMode) //Only allow diagonals
        {
            if (input.magnitude != new Vector2Int(1, 1).magnitude) //Any diagonal will have this magnitude 
                return;
        }
        
        currentAgent.facing = input;
        
        if(yMode)
        {
            //Do not finalize input, only update facing direction
            return;
        }
        if(currentAgent.MoveAgent(input))
        {
            awaitingAction = false;
            Debug.Log("Player move action complete");
        }
        
    }

    private void Start()
    {
        currentAgent = GetComponent<Agent>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(awaitingAction)
        {
            ProcessInput();
        }
    }
}
