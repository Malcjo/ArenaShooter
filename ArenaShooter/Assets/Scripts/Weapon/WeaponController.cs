using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public InputBuffer input;       // drag your existing InputBuffer here
    public WeaponRuntime weapon;    // drag your WeaponRuntime here

    void Update()
    {
        if (!input || !weapon) return;
        if (input.Frame.fireHeld) weapon.TryFire(true);
    }
}
