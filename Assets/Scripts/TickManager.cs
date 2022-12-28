using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TickPhase
{
    Player,
    Ally,
    Enemy,
    Misc
}

public enum PhaseStage
{
    Initializing,
    Preparing,
    Executing,
    Finished
}

public class TickManager : MonoBehaviour
{
    public static TickManager instance;
    [SerializeField] bool running;
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

    public List<Agent> allAgents;
    public List<Agent> allyAgents;
    public List<Agent> enemyAgents;

    public List<AgentAction> actionsThisPhase;
    public List<AgentAction> actionsThisTick;
    public List<AgentAction> actionsAllTicks;
    public Queue<AgentAction> actionsThisPhaseQueue;
    public TickPhase phase;
    public PhaseStage phaseStage;
    int currentTick = 0;
    int currentActionIndex = 0;

    AgentController player;

    [SerializeField] TextMeshProUGUI tickCounter;
    public AgentController agentController;
    // Asks every agent what they want to do during the upcoming tick
    // Player action -> All Teammate actions -> All enemy attacks -> enemy movement -> misc
    // First, waits for player to perform an AgentAction
    // Second, all teammates perform their action based on what the player just did/whatever AI state they are in
    // Third, asks every enemy if they want to attack. Enemies that wish to spend their turn attacking attack
    // Fourth, enemies who wish to move perform their movements.
    // Finally, miscellaneous events such as picking up an item or displaying the staircase UI happen ?
    // 

    // One Tick = four phases
    // One Phase = four stages

    // Runs once at the start of every phase
    void PhaseInit()
    {
        switch(phase)
        {
            case TickPhase.Player:
                {
                    //Wait for a successful player AgentAction event
                    agentController.currentAgent.RequestAction();
                    break;
                }
            case TickPhase.Ally:
                {
                    //Wait for all allies to have an AgentAction event called
                    foreach(Agent ally in allyAgents)
                    {
                        ally.RequestAction();
                    }
                    break;
                }
            case TickPhase.Enemy:
                {
                    NextPhase();

                    break;
                }
            case TickPhase.Misc:
                {
                    NextPhase();
                    break;
                }
        }
        phaseStage = PhaseStage.Preparing;
    }

    public void NewAction(AgentAction action)
    {
        actionsThisPhase.Add(action);
        actionsThisTick.Add(action);
        actionsAllTicks.Add(action);

        action.Prepare();
    }

    void NextPhase() //Progresses to the next phase 
    {
        if(phase == TickPhase.Misc)
        {
            if(CheckForActionCompletion())
            {
                NewTick();
            }
            return;
        }
        else
        {
            phase += 1;
            actionsThisPhase.Clear();
            phaseStage = PhaseStage.Initializing;
        }
    }

    bool ActionsReady()
    {
        if (phase == TickPhase.Player && actionsThisPhase.Count == 0)
        {
            // Wait for at least one action to come through
            return false;
        }

        if (actionsThisPhase.Count == 0)
        {
            phaseStage = PhaseStage.Finished;
            return false;
        }

        for (int i = 0; i < actionsThisPhase.Count; i++)
        {
            if (actionsThisPhase[i].state != AgentActionState.Ready)
            {
                return false;
            }
        }

        // Ensure all agents who belong to this phase have returned an action
        // This might not be necessary
        phaseStage = PhaseStage.Executing;
        return true;
    }

    void ActionLoop()
    {
        // Runs Execute if it hasn't been done so already
        // Execute() sets the state to Loop so it only runs once
        if (actionsThisPhase[currentActionIndex].state == AgentActionState.Ready)
            actionsThisPhase[currentActionIndex].Execute();
        // Exit conditions
        if (actionsThisPhase[currentActionIndex].state == AgentActionState.Complete)
        {
            if(actionsThisPhase.Count > currentActionIndex + 1)
            {
                currentActionIndex++;
                return;
            }
            else
            {
                phaseStage = PhaseStage.Finished;
                currentActionIndex = 0;
                return;
            }
        }

        actionsThisPhase[currentActionIndex].ExecuteLoop();

        for (int i = 0; i < actionsThisPhase.Count; i++)
        {
            if (actionsThisPhase[i].state == AgentActionState.Loop)
            {
                actionsThisPhase[i].ExecuteLoop();
            }
        }
    }

    bool CheckForActionCompletion()
    {
        for (int i = 0; i < actionsThisTick.Count; i++)
        {
            if (actionsThisTick[i].state != AgentActionState.Complete)
            {
                Debug.LogWarning("Action from agent " + actionsThisTick[i].owner.name + " of type "
                    + actionsThisTick[i].type.ToString() + " is marked as " + actionsThisTick[i].state.ToString());
                return false;
            }
        }

        return true;
    }

    void NewTick()
    {
        actionsThisTick.Clear();
        actionsThisPhase.Clear();

        currentTick++;
        phase = TickPhase.Player;
        phaseStage = PhaseStage.Initializing;
    }

    private void Initialize()
    {
        running = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        actionsThisTick = new List<AgentAction>();
        actionsAllTicks = new List<AgentAction>();
        actionsThisPhase = new List<AgentAction>();

        player = GameObject.FindObjectOfType<AgentController>();
        //GameEvents.instance.onSuccessfulPlayerEvent += PlayerHasActed;
        GameEvents.instance.onGenerationComplete += Initialize;
        //Uncomment when random agent generation is done
        //agents = new List<Agent>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if(running)
        {
            if(phaseStage == PhaseStage.Initializing)
                PhaseInit();

            switch(phaseStage)
            {
                case PhaseStage.Preparing:
                    {
                        ActionsReady();
                        break;
                    }
                case PhaseStage.Executing:
                    {
                        ActionLoop();
                        break;
                    }
                case PhaseStage.Finished:
                    {
                        NextPhase();
                        break;
                    }
            }
            tickCounter.text = "Tick: " + currentTick;
        }
    }
}
