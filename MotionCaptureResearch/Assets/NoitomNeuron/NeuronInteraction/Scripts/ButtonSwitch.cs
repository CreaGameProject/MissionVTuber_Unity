using UnityEngine;
using System.Collections;

public class ButtonSwitch : MonoBehaviour {

    public enum ButtonSwitchState
    {
        ON,
        OFF
    };

    public delegate void OnStateChangedDelegate(ButtonSwitch btn, ButtonSwitchState newState);

    public event OnStateChangedDelegate OnStateChangedTo;

    public bool isOn = true;
    public bool LastStateIsOn = true;
    private bool FixedStateIsOn = true;

    public ButtonSwitchState State {
        get { return isOn? ButtonSwitchState.ON: ButtonSwitchState.OFF; }
    }

    public Transform OnButton;
    public Renderer OnButtonRenderer;
    public Transform OffButton;
    public Renderer OffButtonRenderer;

    private Rigidbody Rigidbody;
    private float ForceMagic = 100f;

    private void Awake()
    {
        Rigidbody = this.GetComponent<Rigidbody>();
        SetRotation(isOn);
    }

    private void FixedUpdate()
    {
        float angle = this.transform.localEulerAngles.x;
        if (angle > 180)
            angle -= 360;

        if (angle > -7.5f)
        {
            if (angle < -0.2f)
            {
                Rigidbody.AddForceAtPosition(this.transform.forward * ForceMagic, OnButton.position);
            }
            else if ((angle > -0.2f && angle < -0.1f) || angle > 0.1f)
            {
                SetRotation(true);
            }
        }
        else if (angle < -7.5f)
        {
            if (angle > -14.8f)
            {
                Rigidbody.AddForceAtPosition(this.transform.forward * ForceMagic, OffButton.position);
            }
            else if ((angle < -14.8f && angle > -14.9f) || angle < -15.1)
            {
                SetRotation(false);
            }
        }
    }

    private void Update()
    {
        LastStateIsOn = isOn;
        if (OnStateChangedTo!= null && isOn != FixedStateIsOn)
        {
            OnStateChangedTo(this, FixedStateIsOn? ButtonSwitchState.ON: ButtonSwitchState.OFF);
        }
        isOn = FixedStateIsOn;
    }

    private void SetRotation(bool forState)
    {
        FixedStateIsOn = forState;
        //print("SetRotation FixedStateIsOn " + FixedStateIsOn);
        if (FixedStateIsOn == true)
        {
            this.transform.localEulerAngles = Vector3.zero;
            OnButtonRenderer.material.color = Color.yellow;
            OffButtonRenderer.material.color = Color.white;
        }
        else
        {
            this.transform.localEulerAngles = new Vector3(-15, 0, 0);
            OnButtonRenderer.material.color = Color.white;
            OffButtonRenderer.material.color = Color.red;
        }

        Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.velocity = Vector3.zero;
    }
}
