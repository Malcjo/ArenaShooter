using UnityEngine;

[CreateAssetMenu(menuName = "Data/MovementStats")]
public class MovementStats : ScriptableObject
{
    [Header("Base")]
    public float moveSpeed = 9f, dashSpeed = 22f, slideSpeed = 12f;
    public float maxHorizSpeed = 20f, airMaxPerFrame = 6f;
    public float gravity = -22f, jumpForce = 8.5f;

    [Header("Quake")]
    public float groundFriction = 6f, groundStopSpeed = 2f, groundAccelQ = 14f;
    public float airAccelQ = 6f, airTurnRateDeg = 540f; public float airControlQ = 0f;

    [Header("Sensitivity")]
    public float mouseSensitivity = 0.08f, stickSensitivity = 120f;
}

public class PlayerStats : MonoBehaviour
{
    public MovementStats baseStats;

    [Header("Global movement buffs")]
    public float globalFlatBonus = 0f;
    public float globalPercentIncrease = 0f;   // 0.10 = +10%
    public float globalMultiplier = 1f;        // 1.05 = +5%

    [Header("Channel buffs")]
    public float groundPercentIncrease = 0f, dashPercentIncrease = 0f, slidePercentIncrease = 0f;
    public float maxHorizPercentIncrease = 0f, airPerFramePercentIncrease = 0f;

    float GlobalFactor => (1f + globalPercentIncrease) * globalMultiplier;

    public float EffectiveGroundMoveSpeed => (baseStats.moveSpeed + globalFlatBonus) * (1f + groundPercentIncrease) * GlobalFactor;
    public float EffectiveDashSpeed => (baseStats.dashSpeed + globalFlatBonus) * (1f + dashPercentIncrease) * GlobalFactor;
    public float EffectiveSlideSpeed => (baseStats.slideSpeed + globalFlatBonus) * (1f + slidePercentIncrease) * GlobalFactor;
    public float EffectiveMaxHorizSpeed => baseStats.maxHorizSpeed * (1f + maxHorizPercentIncrease) * GlobalFactor;
    public float EffectiveAirPerFrame => baseStats.airMaxPerFrame * (1f + airPerFramePercentIncrease) * GlobalFactor;

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
}
