using UnityEngine;
using System.Collections;

public class RotationKnob : Interactable
{
    public delegate void OnAdjustCompleteDelegate();
    public event OnAdjustCompleteDelegate OnAdjustComplete;

    public float CurrentValue = 0f;
    public Rigidbody Rigidbody;
    public Transform Indicator;

    //public Transform InteractionPoint;
    protected Transform AttachedHand;
    protected bool IsAttached = false;
    protected bool EmitCompleteEvent = false;

    protected float AttachedRotationMagic = 20f;
    protected float AttachedPositionMagic = 3000f;

    //protected Transform PickupTransform;
    //protected HingeJoint HingeJoint;

    protected Vector3 IndicatorZeroPos;
    protected Vector3 IndicatorZeroRight;
    protected Vector3 IndicatorZeroPlaneNormal;

    public void setCurrentValue(float val)
    {
        CurrentValue = val;
    }

    void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.maxAngularVelocity = 100f;

        IndicatorZeroPos = Indicator.forward;
        IndicatorZeroRight = Indicator.right;
        IndicatorZeroPlaneNormal = Indicator.up;
    }

    void Update()
    {
        Vector3 vecAngle = Vector3.ProjectOnPlane(Indicator.forward, IndicatorZeroPlaneNormal);
        float angle = Vector3.Angle(IndicatorZeroPos, vecAngle);
        float angle90 = Vector3.Angle(IndicatorZeroRight, vecAngle);
        if (angle90 > 90)
        {
            angle = 360 - angle;
        }

        CurrentValue = angle;// Rigidbody.transform.localEulerAngles.x;

        if (EmitCompleteEvent)
        {
            if (OnAdjustComplete != null)
            {
                OnAdjustComplete();
            }
            EmitCompleteEvent = false;
        }
    }

    public override void OnGrab(GrabSensor.GrabInfo grab)
    {
        BeginInteraction(grab.hand);
    }

    // call before release
    public override void OnRelease(GrabSensor.GrabInfo grab)
    {
        EndInteraction();
        EmitCompleteEvent = true;
    }

    public void BeginInteraction(Transform hand)
    {
        AttachedHand = hand;
        IsAttached = true;
    }

    public void EndInteraction()
    {
        AttachedHand = null;
        IsAttached = false;
    }
}
