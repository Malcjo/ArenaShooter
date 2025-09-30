using UnityEngine;

public class CursorGuard : MonoBehaviour
{
    void Start() { Lock(); }
    void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked) Lock();
    }
    void Lock() { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
}