using UnityEngine;
using System.Collections;

public class CenterOfMassModifier : MonoBehaviour
{
    public Rigidbody rigid;
    public Transform centerOfMass; 

    void Start()
    {
        if( rigid == null )
        {
            rigid = GetComponent<Rigidbody>();
        }

        if( rigid != null && centerOfMass != null )
        {
            rigid.centerOfMass = centerOfMass.localPosition;
        }
    }
}
