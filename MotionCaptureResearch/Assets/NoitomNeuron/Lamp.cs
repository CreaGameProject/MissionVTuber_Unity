using UnityEngine;
using System.Collections;

public class Lamp : MonoBehaviour
{
    public GrabSensor grab;

    void OnEnable()
    {
        if (grab != null)
        {
            grab.OnGrab += OnGrab;
            grab.OnRelease += OnRelease;
        }
    }

    void OnDisable()
    {
        if (grab != null)
        {
            grab.OnGrab -= OnGrab;
            grab.OnRelease -= OnRelease;
        }
    }

    void OnGrab(GrabSensor.GrabInfo grabInfo)
    {
        if( grabInfo.obj == gameObject )
        {
            FixedJoint fj = gameObject.AddComponent<FixedJoint>();
            fj.connectedBody = GameObject.Find("Robot_RightHand").GetComponent<Rigidbody>();
        }
    }

    void OnRelease(GrabSensor.GrabInfo grabInfo)
    {
        if (grabInfo.obj == gameObject)
        {
            GameObject.DestroyImmediate(GetComponent<FixedJoint>());
        }
    }
}
