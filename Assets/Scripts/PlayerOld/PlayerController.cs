using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public Vector2 pogoInput;
    public Vector2 previousPogoInput; //Stores the input from the last frame
    public bool releaseChargeThisFrame;

    private Rigidbody2D rgd;

    private PlayerBaseState state;

    public PlayerReadyState readyState = new PlayerReadyState();
    public PlayerChargingState chargingState = new PlayerChargingState();
    public PlayerCooldownState cooldownState = new PlayerCooldownState();

    [SerializeField] float raycastGap;
    [SerializeField] public float pogoRange;
    [SerializeField] float pogoForce;
    [SerializeField] float midAirForceMultiplier;
    [SerializeField] public float pogoChargeProgress;
    [SerializeField] public float pogoChargeTime;
    [SerializeField] public float pogoCooldown;
    [SerializeField] public float remainingCooldownTime;

    [SerializeField] bool canPogo;
    [SerializeField] bool charging;

    [SerializeField] public LayerMask groundLayers;
    [SerializeField] public Transform rayParent;
    [SerializeField] public Transform leftRay;
    [SerializeField] public Transform rightRay;

    [SerializeField] GameObject pogoIndicator;
    [SerializeField] Image fillSprite;
    [SerializeField] Color chargeColor;
    [SerializeField] Color cooldownColor;

    void PerformPogo()
    {
        //Raycast in the direction of pogoInput with a short range
        RaycastHit2D pogoRayHitLeft = Physics2D.Raycast(leftRay.position, previousPogoInput, pogoRange, groundLayers.value);
        Debug.DrawLine(leftRay.position, leftRay.position + (Vector3)(pogoInput * pogoRange), Color.red);

        RaycastHit2D pogoRayHitRight = Physics2D.Raycast(rightRay.position, previousPogoInput, pogoRange, groundLayers.value);
        Debug.DrawLine(rightRay.position, rightRay.position + (Vector3)(pogoInput * pogoRange), Color.blue);

        //How close is the normal to the input direction?
        float degreeDiffLeft = Vector2.Angle(previousPogoInput.normalized, pogoRayHitLeft.normal) / 180;
        float degreeDiffRight = Vector2.Angle(previousPogoInput.normalized, pogoRayHitRight.normal) / 180;
        //Get the average of the two normals
        float degreeDiff = (degreeDiffLeft + degreeDiffRight) / 2;

        if (float.IsInfinity(degreeDiff) || degreeDiff == 0)
            degreeDiff = midAirForceMultiplier;
        else if (pogoRayHitLeft.collider == null) degreeDiff = degreeDiffRight;
        else if (pogoRayHitRight.collider == null) degreeDiff = degreeDiffLeft;

        Debug.Log("Averaged degree diff:" + degreeDiff);
        rgd.AddForce((-previousPogoInput * pogoForce * (pogoChargeProgress / pogoChargeTime)) * degreeDiff);
    }

    void UpdateSprites()
    {
        pogoIndicator.SetActive(state == chargingState);

        fillSprite.fillAmount = pogoChargeProgress / pogoChargeTime;
        fillSprite.color = chargeColor;

        if(remainingCooldownTime > 0)
        {
            fillSprite.fillAmount = remainingCooldownTime/pogoCooldown;
            fillSprite.color = cooldownColor;
        }
    }

    public void OnPogo(InputValue value)
    {
        pogoInput = value.Get<Vector2>();
    }

    public void OnConfirm(InputValue value)
    {
        if(state == chargingState)
            releaseChargeThisFrame = true;
    }

    public void SwitchState(PlayerBaseState newState)
    {
        state = newState;
        state.EnterState(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        rgd = GetComponent<Rigidbody2D>();
        GameEvents.instance.onSuccessfulPogo += PerformPogo;
        state = readyState;
        state.EnterState(this);
    }

    // Update is called once per frame
    void Update()
    {
        if(canPogo)
        {
            state.UpdateState(this);
        }
        Debug.Log("input:" + pogoInput);
        UpdateSprites();
    }

}
