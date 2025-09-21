using UnityEngine;
using System.Collections;


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

    [Header("QoL")]
    public bool autoHop = false; // leave off unless you want it

    // in MovementStats.cs
    [Header("Dash")] public float dashDuration = 0.18f;
    [Header("Slide")] public float crouchHeight = 1.0f;
    public float slideDuration = 0.5f;
    public float slideFriction = 8f;

}