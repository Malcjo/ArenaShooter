using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public PlayerContext ctx;
    public InputBuffer input;       // drag your existing InputBuffer here
    public WeaponRuntime weapon;    // drag your WeaponRuntime here

    void Awake()
    {
        if (!ctx) ctx = GetComponentInParent<PlayerContext>();
        if (!weapon) weapon = GetComponentInChildren<WeaponRuntime>();
    }
    void Update()
    {
        if (!ctx || !ctx.input || !weapon) return;
        if (ctx.input.Frame.fireHeld) weapon.TryFire(true);
    }
}
