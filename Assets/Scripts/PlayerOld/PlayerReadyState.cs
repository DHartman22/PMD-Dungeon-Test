using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReadyState : PlayerBaseState
{
    public override void EnterState(PlayerController player)
    {
        Debug.Log("Ready");
        
    }

    public override void UpdateState(PlayerController player)
    {
        if(player.pogoInput != Vector2.zero)
        {
            player.SwitchState(player.chargingState);
        }
    }
}
