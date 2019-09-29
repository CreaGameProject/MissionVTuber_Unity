using UnityEngine;
using System.Collections;

public class PushButton : MonoBehaviour
{

    public delegate void OnButtonPushedDelegate();
    public delegate void OnButtonReleasedDelegate();

    public event OnButtonPushedDelegate OnButtonPushed;
    public event OnButtonReleasedDelegate OnButtonReleased;

    public float Tolerance = 0.01f;
    public Rigidbody Rigidbody;

    protected float Percent = 0f;
    protected Vector3 StartPos;
    protected bool isClicked = false;

    void Awake()
    {
        StartPos = Rigidbody.position;
        Percent = 0f;
    }

    void Update()
    {
        Vector3 diff = Rigidbody.position - StartPos;
        float dot = Vector3.Dot(Rigidbody.transform.forward, diff);
        float len = 0;
        if (dot < 0)
        {
            len = Mathf.Sqrt(dot * dot);
        }
        
        Percent = len / Tolerance;
        if ( Percent > 1f)
        {
            Percent = 1;
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
            if (isClicked == false)
            {
                isClicked = true;
                if (OnButtonPushed != null)
                {
                    OnButtonPushed();
                }
            }
        }
        else if (Percent < 0.25f)
        {
            if (isClicked)
            {
                isClicked = false;
                if (OnButtonReleased != null)
                {
                    OnButtonReleased();
                }
            }
        }
    }
}
