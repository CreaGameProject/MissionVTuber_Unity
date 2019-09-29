using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionDetector : MonoBehaviour
{
    public delegate void DetectDelegate(CollisionDetector reporter, Collider other);
    public event DetectDelegate OnDetectEnter;
    public event DetectDelegate OnDetectExit;

    void OnTriggerEnter( Collider other )
    {
        if( other.GetComponent<Rigidbody>() != null )
        {
            DetectDelegate callback = OnDetectEnter;
            if (callback != null)
            {
                callback(this, other);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null)
        {
            DetectDelegate callback = OnDetectExit;
            if (callback != null)
            {
                callback(this, other);
            }
        }
    }
}
