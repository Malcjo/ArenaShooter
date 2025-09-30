using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public interface IPlayerState
{
    void Enter();
    void Exit();
    void HandleInput();
    void Tick();
    void FixedTick();
}
public class PlayerStateMachine : MonoBehaviour
{
    public IPlayerState Current { get; private set; }

    [HideInInspector] public string CurrentStateName;
    public void SetState(IPlayerState next)
    {

        Current?.Exit(); // call exit on old state
        Current = next; //set current to new state
        Current?.Enter(); // enter into new state


        // update context if it's one of your PlayerContext-based states
        if (next != null &&
            next.GetType().GetField("ctx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(next) is PlayerContext ctx)
        {
            ctx.CurrentStateName = next.GetType().Name;
        }

    }
    //update and fixed update forwards a tick method
    // safe that it calls the tick once per frame and not risking a forgotten
    // component not disabled from running update
    private void Update()
    {
        Current?.HandleInput();
        Current?.Tick();
    }
    private void FixedUpdate()
    {
        Current?.FixedTick();
    }
}
