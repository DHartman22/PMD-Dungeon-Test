using UnityEngine;

public class PlayerChargingState : PlayerBaseState
{
    public override void EnterState(PlayerController player)
    {
        Debug.Log("Charging");
    }

    public override void UpdateState(PlayerController player)
    {
        //Prevents charge from accruing when not touching the stick
        if (player.pogoInput != Vector2.zero) 
            player.pogoChargeProgress += Time.deltaTime;

        if (player.pogoChargeProgress > player.pogoChargeTime)
            player.pogoChargeProgress = player.pogoChargeTime;

        float parentRotationDelta = Vector2.SignedAngle(Vector2.down, player.previousPogoInput);
        Debug.Log("Angle change: " + parentRotationDelta);
        player.rayParent.rotation = Quaternion.Euler(new Vector3(0, 0, parentRotationDelta));

        if(player.releaseChargeThisFrame)
        {
            RaycastHit2D pogoRayHitLeft = Physics2D.Raycast(player.leftRay.position, player.previousPogoInput, player.pogoRange, player.groundLayers.value);
            RaycastHit2D pogoRayHitRight = Physics2D.Raycast(player.rightRay.position, player.previousPogoInput, player.pogoRange, player.groundLayers.value);

            GameEvents.instance.OnSuccessfulPogo();
            

            player.pogoChargeProgress = 0;
            player.remainingCooldownTime = player.pogoCooldown;

            player.SwitchState(player.cooldownState);
            player.releaseChargeThisFrame = false;
        }
        //Keeps the previous input set to a direction when letting go of the stick
        if(player.pogoInput != Vector2.zero)
            player.previousPogoInput = player.pogoInput;
    }

}
