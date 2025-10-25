using UnityEngine;

public class UpgradeZoneInteract : Entity
{
    public string prompt = "Open Upgrades (E)";
    protected override void Awake()
    {
        base.Awake();
        canBeDamaged = false;
        canInteract = true;
        maxHP = 1f; // irrelevant
    }
    public override string Prompt => prompt;

    public override void Interact(PlayerContext ctx)
    {
        // Open your upgrade UI / weapon-part picker
        onInteracted?.Invoke();
        // Example: pause, switch to WeaponWheelState, or show canvas
        // ctx.fsm.SetState(new WeaponWheelState(ctx));
    }
}
