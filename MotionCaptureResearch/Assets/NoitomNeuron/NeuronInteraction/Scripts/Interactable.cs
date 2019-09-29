using UnityEngine;
using System.Collections;

public class Interactable : MonoBehaviour
{
    public Transform grabAnchor;
    public Transform hand;
    public Rigidbody[] parts;

    // call after selected
    public void OnSelected(GrabSensor.GrabInfo grab)
    {
        Debug.LogFormat("OnSelected");
    }

    // call before deselected
    public void OnDeselected(GrabSensor.GrabInfo grab)
    {
        Debug.LogFormat("OnSelected");
    }

    // call after grabbed
    public virtual void OnGrab( GrabSensor.GrabInfo grab)
    {
        SetToGrabAnchor( grab );

        Debug.LogFormat("OnSelected");
    }

    // call before release
    public virtual void OnRelease(GrabSensor.GrabInfo grab)
    {
        Debug.LogFormat("OnSelected");
    }

    private void SetToGrabAnchor(GrabSensor.GrabInfo grab)
    {
        transform.rotation = grab.hand.rotation * Quaternion.Inverse(grabAnchor.localRotation);
        transform.position = grab.hand.position + Vector3.Scale(grabAnchor.localPosition, transform.lossyScale);

        Rigidbody[] rigids = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rigids.Length; ++i)
        {
            rigids[i].velocity = Vector3.zero;
            rigids[i].angularVelocity = Vector3.zero;
        }
    }

    void Update()
    {
        // debug
        if( Input.GetKeyDown( KeyCode.Return ) )
        {
            GrabSensor.GrabInfo grab = new GrabSensor.GrabInfo();
            grab.hand = GameObject.Find("Robot_RightHandMiddle1").transform;
            OnGrab(grab);
        }
    }
}
