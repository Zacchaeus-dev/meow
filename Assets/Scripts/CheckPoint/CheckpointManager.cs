using System;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }
    public static event Action OnRespawn;
    int currentIndex = 0;

    [SerializeField] Transform startPoint; // default spawn (empty marker)

    Vector3 lastPos;
    Quaternion lastRot;
    readonly HashSet<string> committed = new(); // locked in at a section
    readonly HashSet<string> solvedNow = new(); // solved since last section

    void Awake()
    {
        Instance = this;
        lastPos = startPoint.position;
        lastRot = startPoint.rotation;
    }

    public void MarkSolved(string id) => solvedNow.Add(id);
    public bool IsCommitted(string id) => committed.Contains(id);

    public void SetCheckpoint(int index, Vector3 pos, Quaternion rot)
    {
        if (index <= currentIndex) return; // don't go backwards
        currentIndex = index;
        lastPos = pos;
        lastRot = rot;
        //committed.UnionWith(solvedNow);
    }

    public void Respawn(GameObject player)
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false; // otherwise it snaps back

        var rb = player.GetComponent<Rigidbody>();
        if (rb) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

        player.transform.SetPositionAndRotation(lastPos, lastRot);

        if (cc) cc.enabled = true;

        //solvedNow.Clear();
        //OnRespawn?.Invoke();
    }
}
