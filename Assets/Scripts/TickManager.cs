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
    public List<AgentAction> actionsThisPhase;
    public List<AgentAction> actionsThisTick;
    public List<AgentAction> actionsAllTicks;
    public Queue<AgentAction> actionsThisPhaseQueue;
    public TickPhase phase;
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
    void Tick()
    {
        switch(phase)
        {
            case TickPhase.Player:
                {
                    //Wait for a successful player AgentAction event
                    agentController.awaitingAction = true;
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
    }

    public void NewAction(AgentAction action)
    {
        actionsThisPhase.Add(action);
        actionsThisTick.Add(action);
        actionsAllTicks.Add(action);

        action.Prepare();
    }

    void PlayerHasActed()
    {
        NextPhase();
        //Prepare all teammate actions, then execute in order of movement/attack, and team ID
        //Temp loop here
        for(int i = 0; i < actionsThisTick.Count; i++)
        {
            actionsThisTick[i].Execute();
        }
    }

    void NextPhase() //Progresses to the next phase 
    {
        if(phase == TickPhase.Misc)
        {
            Debug.Log("Waiting for completion");
            return;
        }
        else
        {
            phase += 1;
            actionsThisPhase.Clear();
        }
    }

    bool ActionsReady()
    {
        for (int i = 0; i < actionsThisPhase.Count; i++)
        {
            if (actionsThisPhase[i].state != AgentActionState.Ready)
            {
                return false;
            }
        }
        return true;
    }

    void ActionLoop()
    {
        for(int i = 0; i < actionsThisPhase.Count; i++)
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
                return false;
            }
        }

        return true;
    }

    void NewTick()
    {
        actionsThisTick.Clear();
        currentTick++;
        phase = TickPhase.Player;
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
        player = GameObject.FindObjectOfType<AgentController>();
        GameEvents.instance.onSuccessfulPlayerEvent += PlayerHasActed;
        GameEvents.instance.onGenerationComplete += Initialize;
        //Uncomment when random agent generation is done
        //agents = new List<Agent>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if(running)
        {
            Tick();
            if(ActionsReady())
            {
                ActionLoop();
            }

            if(actionsThisTick.Count != 0 && CheckForActionCompletion())
            {
                NewTick();
            }
            tickCounter.text = "Tick: " + currentTick;
        }
    }
}
