using UnityEngine;
using System.Collections;

public class SliderButton : Interactable
{
    public delegate void OnAdjustCompleteDelegate();
    public event OnAdjustCompleteDelegate OnAdjustComplete;

    [Tooltip("Set to zero when the slider is at StartPoint. Set to one when the slider is at EndPoint.")]
    public float CurrentValue = 0;

    public Rigidbody rigidBody;

    [Tooltip("A transform at the position of the zero point of the slider")]
    public Transform startPoint;

    [Tooltip("A transform at the position of the one point of the slider")]
    public Transform endPoint;

    private float lastValue = 0f;
    private float TotalDistance;
    //protected float AttachedPositionMagic = 3000f;

    //private Vector3 SliderPath;
    //private Transform PickupTransform;
    private bool IsAttached = false;
    private bool EmitCompleteEvent = false;

    public void setCurrentValue(float value, bool updateSlider = true)
    {
        if (value < 0.01f)
        {
            value = 0.0f;
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }
        else if (value > 1)
        {
            value = 1;
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }
        lastValue = CurrentValue;
        CurrentValue = value;
        if (updateSlider)
        {
            rigidBody.transform.position = Vector3.Lerp(startPoint.position, endPoint.position, CurrentValue);
        }
    }

    void Awake()
    {
        if (startPoint == null)
        {
            Debug.LogError("This slider has no StartPoint.");
        }
        if (endPoint == null)
        {
            Debug.LogError("This slider has no EndPoint.");
        }

        //SliderPath = endPoint.position - startPoint.position;
        TotalDistance = Vector3.Distance(startPoint.position, endPoint.position);
        if (TotalDistance <= 0)
        {
            Debug.LogError("Distance form StartPoint to EndPoint is less than 0.");
        }

        setCurrentValue(CurrentValue, true);
        //Debug.Log("init pos " + rigidBody.transform.position + " " + CurrentValue);
    }

    void Update()
    {
        Vector3 pos = rigidBody.transform.position;
        float distance = Vector3.Distance(startPoint.position, pos);
        Vector3 diff = pos - startPoint.position;
        float synclastic = Vector3.Dot(rigidBody.transform.up, diff);
        if (synclastic < 0)
        {
            distance = 0;
        }
        float val = distance / TotalDistance;
        if (val != CurrentValue)
        {
            setCurrentValue(val, false);
        }

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
        IsAttached = true;
    }

    public override void OnRelease(GrabSensor.GrabInfo grab)
    {
        IsAttached = false;
        EmitCompleteEvent = true;
        print("release  ...");
    }
}
