using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("Asset Reference")]
    public MovementStats baseStats; // assign MovementStats asset in Inspector

    // ---------- Global movement buffs ----------
    [Header("Global movement buffs")]
    public float globalFlatBonus = 0f;
    public float globalPercentIncrease = 0f;   // 0.10 = +10%
    public float globalMultiplier = 1f;        // 1.05 = +5%

    // Channel buffs
    [Header("Channel buffs")]
    public float groundPercentIncrease = 0f;
    public float dashPercentIncrease = 0f;
    public float slidePercentIncrease = 0f;
    public float maxHorizPercentIncrease = 0f;
    public float airPerFramePercentIncrease = 0f;

    // Slide friction modifiers (let upgrades tweak friction feel)
    [Header("Slide friction modifiers")]
    [Tooltip("Additive change to slide friction (can be negative).")]
    public float slideFrictionFlatBonus = 0f;
    [Tooltip("Percent change to slide friction. 0.20 = +20%, -0.25 = -25%.")]
    public float slideFrictionPercentIncrease = 0f;

    // ---------- Dash charges (runtime) ----------
    [Header("Dash Charges")]
    public int maxDashCharges = 2;
    public float dashRefillCooldown = 0.9f;
    public bool groundedRequiredForDashRefill = true;

    public int DashCharges { get; private set; }
    public bool IsDashing { get; set; }

    CharacterController _cc;          // for grounded checks during refill
    Coroutine _dashRefillRoutine;

    void Awake()
    {
        if (baseStats == null)
            Debug.LogError("PlayerStats: 'baseStats' is not assigned. Create a MovementStats asset and drag it here.");
        _cc = GetComponent<CharacterController>();
    }

    void Start()
    {
        DashCharges = maxDashCharges;  // start full
    }

    public bool HasDashCharge => DashCharges > 0;

    public bool TryConsumeDashCharge()
    {
        if (DashCharges <= 0) return false;
        DashCharges--;
        if (DashCharges == 0) StartDashRefillIfNeeded();
        return true;
    }

    public void StartDashRefillIfNeeded()
    {
        if (_dashRefillRoutine != null) return;
        _dashRefillRoutine = StartCoroutine(RefillDash());
    }

    IEnumerator RefillDash()
    {
        if (groundedRequiredForDashRefill)
            while (_cc != null && !_cc.isGrounded) yield return null;

        yield return new WaitForSeconds(dashRefillCooldown);
        DashCharges = maxDashCharges;
        _dashRefillRoutine = null;
    }

    // ---------- Effective values ----------
    float GlobalFactor => (1f + globalPercentIncrease) * globalMultiplier;

    public float EffectiveGroundMoveSpeed => (baseStats.moveSpeed + globalFlatBonus) * (1f + groundPercentIncrease) * GlobalFactor;
    public float EffectiveDashSpeed => (baseStats.dashSpeed + globalFlatBonus) * (1f + dashPercentIncrease) * GlobalFactor;
    public float EffectiveSlideSpeed => (baseStats.slideSpeed + globalFlatBonus) * (1f + slidePercentIncrease) * GlobalFactor;
    public float EffectiveMaxHorizSpeed => baseStats.maxHorizSpeed * (1f + maxHorizPercentIncrease) * GlobalFactor;
    public float EffectiveAirPerFrame => baseStats.airMaxPerFrame * (1f + airPerFramePercentIncrease) * GlobalFactor;

    // Slide friction with modifiers
    public float SlideFrictionEffective
        => Mathf.Max(0f, (baseStats.slideFriction + slideFrictionFlatBonus) * (1f + slideFrictionPercentIncrease));

    // Pass-throughs for non-buffed values
    public float Gravity => baseStats.gravity;
    public float JumpForce => baseStats.jumpForce;
    public float GroundFriction => baseStats.groundFriction;
    public float GroundStopSpeed => baseStats.groundStopSpeed;
    public float GroundAccelQ => baseStats.groundAccelQ;
    public float AirAccelQ => baseStats.airAccelQ;
    public float AirTurnRateRad => baseStats.airTurnRateDeg * Mathf.Deg2Rad;
    public float AirControlQ => baseStats.airControlQ;
    public float MouseSensitivity => baseStats.mouseSensitivity;
    public float StickSensitivity => baseStats.stickSensitivity;

    // Config from asset (dash/slide/crouch)
    public float DashDuration => baseStats.dashDuration;
    public float SlideDuration => baseStats.slideDuration;
    public float CrouchHeight => baseStats.crouchHeight;

    // QoL toggle
    public bool AutoHop => baseStats.autoHop;
}

