#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WeaponRuntime))]
public class WeaponRuntimeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // draw the normal inspector first
        base.OnInspectorGUI();

        var wr = (WeaponRuntime)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Editor Utilities", EditorStyles.boldLabel);

        if (GUILayout.Button("Clean Sockets (Delete All Children)"))
        {
            CleanSockets(wr);
        }

        if (GUILayout.Button("Rebuild Only"))
        {
            // make it undoable
            Undo.RecordObject(wr.gameObject, "Rebuild Weapon");
            wr.Rebuild();
            EditorUtility.SetDirty(wr);
        }

        if (GUILayout.Button("Clean + Rebuild"))
        {
            CleanSockets(wr);
            Undo.RecordObject(wr.gameObject, "Clean + Rebuild Weapon");
            wr.Rebuild();
            EditorUtility.SetDirty(wr);
        }

        EditorGUILayout.EndVertical();
    }

    private void CleanSockets(WeaponRuntime wr)
    {
        Transform[] sockets = new Transform[]
        {
            wr.receiverSocket,
            wr.barrelSocket,
            wr.magazineSocket,
            wr.stockSocket,
            wr.gripSocket,
            wr.sightSocket,
            wr.foregripSocket,
            wr.weaponFrameSocket,
        };

        foreach (var socket in sockets)
        {
            if (!socket) continue;

            // delete children from last to first
            for (int i = socket.childCount - 1; i >= 0; i--)
            {
                var child = socket.GetChild(i).gameObject;
                Undo.DestroyObjectImmediate(child);
            }
        }
    }
}
#endif
