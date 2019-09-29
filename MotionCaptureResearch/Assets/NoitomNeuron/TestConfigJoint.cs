using UnityEngine;
using System.Collections;

public class TestConfigJoint : MonoBehaviour
{
    ConfigurableJoint joint;

    void Start()
    {
        if( joint == null )
        {
            joint = GetComponent<ConfigurableJoint>();
        }
    }

    void Update()
    {
    }
}
