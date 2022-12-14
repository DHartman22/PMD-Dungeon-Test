using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCooldownState : PlayerBaseState
{
    public override void EnterState(PlayerController player)
    {
        Debug.Log("CooldownEnter");
    }

    public override void UpdateState(PlayerController player)
    {
        if (player.remainingCooldownTime > 0)
        {
            Debug.Log("Cooldown");
            player.remainingCooldownTime -= Time.deltaTime;
        }
        else
        {
            player.SwitchState(player.readyState);
        }
    }
}
