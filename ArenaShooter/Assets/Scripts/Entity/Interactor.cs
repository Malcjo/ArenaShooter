using UnityEngine;

[RequireComponent(typeof(PlayerContext))]
public class Interactor : MonoBehaviour
{
    public float interactRange = 2.2f;
    public LayerMask interactMask = ~0; // set to "Interactable" in Inspector if you like

    PlayerContext ctx;

    void Awake() { ctx = GetComponent<PlayerContext>(); }

    void Update()
    {
        if (!ctx || ctx.input.Frame.InteractPressedEdge == false) return;

        // Ray from camera center
        var cam = ctx.cam;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out var hit, interactRange, interactMask, QueryTriggerInteraction.Ignore))
        {
            var ent = hit.collider.GetComponentInParent<Entity>();
            if (ent is IInteractable inter && inter.CanInteract(ctx))
            {
                inter.Interact(ctx);
                // consume input one frame later automatically via InputBuffer.Update()
            }
        }
    }
}
