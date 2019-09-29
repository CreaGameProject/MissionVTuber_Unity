using UnityEngine;
using System.Collections;

public class Lever : Interactable {
    // working on NormallyClosed mode
    public delegate void OnEngagedhandler();
    public event OnEngagedhandler OnEngaged;

    // working on Slider mode
    public delegate void OnAdjustCompleteDelegate();
    public event OnAdjustCompleteDelegate OnAdjustComplete;

    [SerializeField]
    private WorkingMode workingMode;
    public WorkingMode Mode { get { return workingMode; } }
    public float LastValue;
    public float CurrentValue;
    public LeverPosition LastLeverPosition;
    public LeverPosition CurrentLeverPosition;
    public bool LeverEngaged = false;
    public float EngagedWaitTime = 1f;
    public Rigidbody Rigidbody;
    public Transform Axis;

    protected virtual float DeltaMagic { get { return 2f; } }
    protected Transform InitialAttachPoint;
    protected HingeJoint HingeJoint;

    protected Transform AttachedHand;
    protected bool IsAttached = false;
    protected bool EmitCompleteEvent = false;
    protected Quaternion Max, Mid, Min;
    protected float AngleRange;

    protected void Awake()
    {
        if (Rigidbody == null)
            Rigidbody = this.GetComponent<Rigidbody>();

        if (Rigidbody == null)
        {
            Debug.LogError("There is no rigidbody attached to this interactable.");
        }

        Rigidbody.maxAngularVelocity = 100f;

        if (HingeJoint == null)
        {
            HingeJoint = Rigidbody.gameObject.GetComponent<HingeJoint>();
        }

        Mid = HingeJoint.transform.localRotation;
        Max = Mid * Quaternion.AngleAxis(HingeJoint.limits.max, HingeJoint.axis);
        Min = Mid * Quaternion.AngleAxis(HingeJoint.limits.min, HingeJoint.axis);

        if (HingeJoint.useLimits)
        {
            AngleRange = (Mathf.Max(HingeJoint.limits.max, HingeJoint.limits.min)) - (Mathf.Min(HingeJoint.limits.max, HingeJoint.limits.min));
        }

        if (Mode == WorkingMode.Slider)
        {
            HingeJoint.useMotor = false;

            float m_diff = (1 - CurrentValue) * AngleRange;
            float angle = (m_diff + HingeJoint.limits.min);
            Transform t = Axis;
            Vector3 axis = t.up;
            Rigidbody.transform.RotateAround(t.position, axis, angle);
        }
        else if (Mode == WorkingMode.NormallyClosed)
        {
            HingeJoint.useMotor = true;
        }
    }

    protected void FixedUpdate()
    {
        if (IsAttached == true)
        {
            Vector3 PositionDelta = (AttachedHand.transform.position - InitialAttachPoint.position) * DeltaMagic;
            this.Rigidbody.AddForceAtPosition(PositionDelta, InitialAttachPoint.position, ForceMode.VelocityChange);
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
    }

    protected void Update()
    {
        LeverEngaged = false;
        LastValue = CurrentValue;
        LastLeverPosition = CurrentLeverPosition;

        CurrentValue = GetValue();
        CurrentLeverPosition = GetPosition();

        if (Mode == WorkingMode.NormallyClosed)
        {
            if (LastLeverPosition != LeverPosition.On && CurrentLeverPosition == LeverPosition.On)
            {
                LeverEngaged = true;
                Engage();
                if (OnEngaged != null)
                {
                    OnEngaged();
                }
            }
        }
        else if (Mode == WorkingMode.Slider)
        {
            if (EmitCompleteEvent)
            {
                if (OnAdjustComplete != null)
                {
                    OnAdjustComplete();
                }
                EmitCompleteEvent = false;
            }
        }
    }

    protected virtual void Engage()
    {
        //CanAttach = false;
        StartCoroutine(HoldPosition(EngagedWaitTime));
    }

    private IEnumerator HoldPosition(float time)
    {
        if (Mode == WorkingMode.NormallyClosed)
        {
            HingeJoint.useMotor = false;
        }

        yield return new WaitForSeconds(time);

        if (Mode == WorkingMode.NormallyClosed)
        {
            HingeJoint.useMotor = true;
        }
        //CanAttach = true;
    }

    public void BeginInteraction(Transform hand)
    {
        AttachedHand = hand;
        IsAttached = true;

        InitialAttachPoint = new GameObject(string.Format("[{0}] InitialAttachPoint", this.gameObject.name)).transform;
        InitialAttachPoint.position = hand.transform.position;
        InitialAttachPoint.rotation = hand.transform.rotation;
        InitialAttachPoint.localScale = Vector3.one * 0.25f;
        InitialAttachPoint.parent = this.transform;

        if (Mode == WorkingMode.NormallyClosed)
        {
            HingeJoint.useMotor = false;
        }
    }

    public void EndInteraction()
    {
        AttachedHand = null;
        IsAttached = false;
        EmitCompleteEvent = true;

        if (Mode == WorkingMode.NormallyClosed)
        {
            HingeJoint.useMotor = true;
        }

        if (InitialAttachPoint != null)
        {
            GameObject.Destroy(InitialAttachPoint.gameObject);
        }
    }

    private float GetValue()
    {
        float m_diff = 0.0f;
        if (HingeJoint.useLimits)
        {
            m_diff = HingeJoint.angle - HingeJoint.limits.min;
        }

        float val = 1 - (m_diff / AngleRange);
        if (val < 0.001f)
        {
            val = 0.0f;
        } else if (val > 1)
        {
            val = 1;
        }
        return val;
    }

    private LeverPosition GetPosition()
    {
        if (CurrentValue <= 0.05f)
        {
            return LeverPosition.Off;
        }
        else if (CurrentValue >= 0.95f)
        {
            return LeverPosition.On;
        }

        return LeverPosition.Mid;
    }

    public enum LeverPosition
    {
        Off,
        Mid,
        On
    }

    public enum WorkingMode
    {
        NormallyClosed,
        Slider
    }
}
