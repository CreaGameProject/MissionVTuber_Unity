using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Finger : MonoBehaviour
{
    #region declaration
    #endregion

    private float referenceAngle;
    public List<GameObject> touchedObjects = new List<GameObject>();

    public CollisionDetector colDetector;
    public Collider pushCollider;
    // public Collider fingerCollider;

    public Transform fingerTipBone;
    public Transform fingerJointBone;
    public Transform fingerPalmBone;
    public Transform fingerRayAnchor;

    public float bendAngle;

    public void OnEnable()
    {
        if( colDetector != null )
        {
            colDetector.OnDetectEnter += OnEnterCollision;
            colDetector.OnDetectExit += OnExitCollision; 
        }
    }

    public void OnDisable()
    {
        if (colDetector != null)
        {
            colDetector.OnDetectEnter -= OnEnterCollision;
            colDetector.OnDetectExit -= OnExitCollision;
        }
    }

    #region properties
    public float BendAngle
    {
        get
        {
            Vector3 vTip = fingerTipBone.position - fingerJointBone.position;
            Debug.DrawLine(fingerTipBone.position, fingerJointBone.position, Color.blue);
            Vector3 vPalm = fingerPalmBone.position - fingerJointBone.position;
            Debug.DrawLine(fingerPalmBone.position, fingerJointBone.position, Color.blue);

            return Mathf.Acos(Vector3.Dot(vTip.normalized, vPalm.normalized)) / Mathf.PI * 180.0f;
        }
    }

    public float BendAngleOffset
    {
        get
        {
            return bendAngle - referenceAngle;
        }
    }
    
    public Ray tipRay
    {
        get
        {
            return new Ray(fingerTipBone.position, fingerTipBone.position - fingerJointBone.position);
        }
    }
    #endregion

    #region interface
    public void Reset()
    {
        referenceAngle = bendAngle;
    }

    public bool IsTouched( GameObject obj )
    {
        return touchedObjects.Contains(obj);
    }
    #endregion

    #region internal
    private void OnEnterCollision(CollisionDetector reporter, Collider other)
    {
        touchedObjects.Add(other.gameObject);
    }

    private void OnExitCollision(CollisionDetector reporter, Collider other)
    {
        touchedObjects.Remove(other.gameObject);
    }

    void Update()
    {
        //Vector3 vTip = fingerTipBone.position - fingerJointBone.position;
        //Debug.DrawLine(fingerTipBone.position, fingerJointBone.position, Color.blue);
        //Vector3 vPalm = fingerPalmBone.position - fingerJointBone.position;
        //Debug.DrawLine(fingerPalmBone.position, fingerJointBone.position, Color.blue);

        //bendAngle = Mathf.Acos(Vector3.Dot(vTip.normalized, vPalm.normalized)) / Mathf.PI * 180.0f;
    }
    #endregion

}
