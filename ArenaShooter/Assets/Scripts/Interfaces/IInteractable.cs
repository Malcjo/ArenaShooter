public interface IInteractable
{
    bool CanInteract(PlayerContext ctx);
    void Interact(PlayerContext ctx);
    string Prompt { get; }
}
