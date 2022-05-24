using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// track the path of the bullet. Only in UnityEditor
/// </summary>
#if UNITY_EDITOR
public class BulletDebugger : MonoBehaviour {

    private Transform me;
    private List<Vector3> Poses = new List<Vector3>();

    //color of the path
    public Color LineColor;
    //amount of frames being stored
    public int maxFrames;

    void Awake()
    {
        me = this.transform;
        Poses.Add(me.position);
    }

    void Update()
    {
        for (int i = 1; i < Poses.Count; i++)
        {
            Debug.DrawLine(Poses[i-1], Poses[i], LineColor);
        }
        if (Poses.Count > maxFrames)
        {
            Poses.RemoveRange(0, Poses.Count - maxFrames);
        }
    }

    public void AddPos(Vector3 pos)
    {
            Poses.Add(pos);
    }
    
    public void Clear()
    {
        Poses.Clear();
    }
}
#endif
