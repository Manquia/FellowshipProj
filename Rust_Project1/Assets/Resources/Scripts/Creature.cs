using UnityEngine;
using System.Collections;
using System;

public class Creature : MonoBehaviour
{
    public FFPath movePath;

    #region Events
    struct CommandEvent
    {
        public enum Type
        {
            Additive,
            Override,
        }

        public Type type;
        public Vector3 point;
    }

    struct ScareEvent
    {
        public Vector3 origin;
        public float strength;
    }

    struct DamageEvent
    {
       public Vector3 origin;
    }
    #endregion

    #region Start/Destroy

    // Use this for initialization
    void Start ()
    {
        FFMessageBoard<CommandEvent>.Connect(OnCommandEvent, gameObject);
        FFMessageBoard<ScareEvent>.Connect(OnScareEvent, gameObject);
        FFMessageBoard<DamageEvent>.Connect(OnDamageEvent, gameObject);
    }
    void OnDestroy()
    {
        FFMessageBoard<CommandEvent>.Disconnect(OnCommandEvent, gameObject);
        FFMessageBoard<ScareEvent>.Disconnect(OnScareEvent, gameObject);
        FFMessageBoard<DamageEvent>.Disconnect(OnDamageEvent, gameObject);
    }
#endregion
    private void OnCommandEvent(CommandEvent e)
    {
        if(e.type == CommandEvent.Type.Additive)
        {
            AddPointToPath(e.point);
        }
        else if(e.type == CommandEvent.Type.Override)
        {
            ClearPath();
            AddPointToPath(e.point);
        }
    }

    private void OnScareEvent(ScareEvent e)
    {

    }

    private void OnDamageEvent(DamageEvent e)
    {
    }

    void AddPointToPath(Vector3 point)
    {
        var newPoints = new Vector3[movePath.points.Length + 1];

        for(int i = 0; i < movePath.points.Length; ++i)
        {
            newPoints[i] = movePath.points[i];
        }
        newPoints[newPoints.Length - 1] = point;
        movePath.points = newPoints;
    }
    void ClearPath()
    {
        movePath.points = null;
    }

    // Update is called once per frame
    void Update ()
    {

	
	}
}
