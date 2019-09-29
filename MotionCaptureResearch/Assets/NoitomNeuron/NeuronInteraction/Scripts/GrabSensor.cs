using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class GrabSensor : MonoBehaviour
{
    #region declaration
    public enum FingerGroup
    {
        LeftThumb,
        LeftFingers,
        LeftPalm,
        RightThumb,
        RightFingers,
        RightPalm,
        NumOfGroups
    }

    public enum ObjectSelectPolicy
    {
        SelectMinSqrDistFromFingers,
        SelectMiddleOfFingers
    }

    public class GrabInfo
    {
        public GameObject obj;
        public Finger left;
        public Finger right;
        public int leftGroup;
        public int rightGroup;
        public float selectAngle;
        public float grabAngle;
        public Transform hand;

        public bool IsLeftHand
        {
            get { return GrabSensor.IsAllLeftHand(leftGroup, rightGroup); }
        }

        public bool IsRightHand
        {
            get { return GrabSensor.IsAllRightHand(leftGroup, rightGroup); }
        }

        public bool IsBothHand
        {
            get { return GrabSensor.IsSameHand(leftGroup, rightGroup); }
        }

        public bool IsFingersInsideCollider()
        {
            bool ret = obj.GetComponent<Collider>().bounds.Contains(left.fingerTipBone.position);
            ret = ret || obj.GetComponent<Collider>().bounds.Contains(right.fingerTipBone.position);
            return ret;
        }

        public bool IsMidPointInsideCollider()
        {
            return obj.GetComponent<Collider>().bounds.Contains((left.fingerJointBone.position + right.fingerJointBone.position) * 0.5f );
        }
    };

    public delegate void GrabReleaseDelegate(GrabInfo obj);
    public event GrabReleaseDelegate OnSelect;
    public event GrabReleaseDelegate OnDeselect;
    public event GrabReleaseDelegate OnGrab;
    public event GrabReleaseDelegate OnRelease;
    #endregion

    #region setup
    // detectors
    public Finger leftThumb;
    public Finger leftIndex;
    public Finger leftMiddle;
    public Finger leftRing;
    public Finger leftPinky;
    public Finger rightThumb;
    public Finger rightIndex;
    public Finger rightMiddle;
    public Finger rightRing;
    public Finger rightPinky;
    public Finger leftPalm1;
    public Finger leftPalm2;
    public Finger rightPalm1;
    public Finger rightPalm2;

    // hand colliders
    public Collider[] toggleColliders;
    public float indexPokingDeltaAngle;

    // layer config
    public string[] ignoreLayers = { "Body", "Detector" };

    // collider config
    public float colliderEnableVelocity;

    // grab and release config
    public float rayDistance = 0.1f;
    public int grabFrameRequired = 10;
    public int releaseFrameRequired = 10;

    #endregion

    // working context
    public enum State
    {
        Idle,
        Selected,
        Grabbed
    }

    private Finger[][] fingerGroups;
    private int ingoreLayerMask;
    private GrabInfo currentSelect = new GrabInfo();
    public State currentState;
    public float grabAngle;
    public float releaseAngle;

    // debug
    public GameObject selectObject;
    public float currentAngle;
    public float handVelocity;

    #region properties
    GameObject GrabbedObject
    {
        get { return currentSelect != null ? currentSelect.obj : null; }
    }
    #endregion

    #region interface
    public static bool IsAllLeftHand(int leftGroup, int rightGroup)
    {
        return leftGroup <= (int)FingerGroup.LeftPalm && rightGroup <= (int)FingerGroup.LeftPalm;
    }

    public static bool IsAllRightHand(int leftGroup, int rightGroup)
    {
        return leftGroup > (int)FingerGroup.LeftPalm && rightGroup > (int)FingerGroup.LeftPalm;
    }

    public static bool IsSameHand(int leftGroup, int rightGroup)
    {
        if (leftGroup <= (int)FingerGroup.LeftPalm && rightGroup <= (int)FingerGroup.LeftPalm
            || leftGroup > (int)FingerGroup.LeftPalm && rightGroup > (int)FingerGroup.LeftPalm)
        {
            return true;
        }

        return false;
    }

    public static bool IsPalm( int group )
    {
        return group == (int)FingerGroup.LeftPalm || group == (int)FingerGroup.RightPalm;
    }
    #endregion

    #region internal
    void FixedUpdate()
    {   
        handVelocity = GetMaxFingerVelocity();

        //if( handVelocity > colliderEnableVelocity )
        //{
        //    if( currentState == State.Idle )
        //    {
        //        EnableColliders(true);
        //    }
        //}
        //else
        //{
        //    EnableColliders(false);
        //}

        //if( currentState == State.Idle )
        {
            if (handVelocity < colliderEnableVelocity)
            {
                EnableColliders(false);
                if( leftIndex.BendAngle > leftMiddle.BendAngle + indexPokingDeltaAngle )
                {
                    leftIndex.pushCollider.enabled = true;
                }

                if( rightIndex.BendAngle > rightMiddle.BendAngle + indexPokingDeltaAngle )
                {
                    rightIndex.pushCollider.enabled = true;
                }
            }
            else
            {
                EnableColliders(true);
            }
        }

        switch ( currentState )
        {
            case State.Idle:
                {
                    currentSelect = GetGrabInfoByDetector();
                    // currentSelect = GetGrabInfoByRaycast();
                    if (currentSelect != null && currentSelect.obj != null)
                    {
                        //GrabInfo detectorInfo = GetGrabInfoByDetector();
                        //if (detectorInfo == null || detectorInfo.obj == null || detectorInfo.obj != currentSelect.obj)
                        //{
                        //    return;
                        //}

                        Transform midTransform = GetMiddleTransform(currentSelect);
                        currentState = State.Selected;
                        currentAngle = GetAngle(currentSelect.left.fingerTipBone, currentSelect.right.fingerTipBone, midTransform);
                        currentSelect.selectAngle = currentAngle;

                        // EnableColliders(touchedWith > 0);

                        GrabReleaseDelegate callback = OnSelect;
                        if (callback != null)
                        {
                            callback(currentSelect);
                        }
                    }
                }
                return;
            case State.Selected:
                {
                    Vector3 vleft = currentSelect.left.fingerRayAnchor.position;
                    Vector3 vRight = currentSelect.right.fingerRayAnchor.position;

                    Transform midTransform = GetMiddleTransform(currentSelect);
                    float angle = GetAngle(currentSelect.left.fingerTipBone, currentSelect.right.fingerTipBone, midTransform);
                    currentAngle = angle;
                    //if( !currentSelect.IsMidPointInsideCollider() )
                    {
                        GrabInfo info = GetGrabInfoByDetector();

                        if (info == null || currentSelect != null && currentSelect.obj != info.obj)
                        {
                            GrabReleaseDelegate callback = OnDeselect;
                            if (callback != null)
                            {
                                callback(currentSelect);
                            }

                            currentState = State.Idle;
                            // currentSelect.selectAngle = angle;
                            currentSelect = null;

                            return;
                        }
                    }

                    //if ( angle > currentAngle )
                    //{
                    //    currentAngle = angle;
                    //}

                    if (currentSelect.selectAngle - angle > grabAngle )
                    {
                        currentState = State.Grabbed;
                        currentSelect.grabAngle = angle;
                        currentSelect.left.pushCollider.enabled = false;
                        currentSelect.right.pushCollider.enabled = false;

                        GrabReleaseDelegate callback = OnGrab;
                        if (callback != null)
                        {
                            callback(currentSelect);
                        }
                    }

                    Debug.DrawLine(vleft, midTransform.position, Color.red);
                    Debug.DrawLine(vRight, midTransform.position, Color.red);
                }
                return;
            case State.Grabbed:
                {
                    Vector3 vleft = currentSelect.left.fingerRayAnchor.position;
                    Vector3 vRight = currentSelect.right.fingerRayAnchor.position;
                    Transform midTransform = GetMiddleTransform(currentSelect);
                    float angle = GetAngle(currentSelect.left.fingerRayAnchor, currentSelect.right.fingerRayAnchor, midTransform);
                    currentAngle = angle;
                    //if ( angle < currentAngle )
                    //{
                    //    currentAngle = angle;
                    //}
                    if (angle - currentSelect.grabAngle > releaseAngle )
                    {
                        GrabReleaseDelegate callback = OnRelease;
                        if (callback != null)
                        {
                            callback(currentSelect);
                        }

                        currentState = State.Idle;
                        // currentSelect.selectAngle = currentAngle;
                        currentSelect = null;
                    }
                    else if (angle < currentSelect.grabAngle)
                    {
                        currentSelect.grabAngle = angle;
                    }

                    Debug.DrawLine(vleft, midTransform.position, Color.red);
                    Debug.DrawLine(vRight, midTransform.position, Color.red);
                }
                return;
        }
    }

    Transform GetMiddleTransform( GrabInfo grab )
    {
        if (IsAllLeftHand(grab.leftGroup, grab.rightGroup))
        {
            if (IsPalm(grab.leftGroup))
            {
                return grab.right.fingerPalmBone;
            }
            else if (IsPalm(grab.rightGroup))
            {
                return grab.left.fingerPalmBone;
            }
            else
            {
                return leftPalm1.fingerPalmBone;
            }
        }
        else if (IsAllRightHand(grab.leftGroup, grab.rightGroup))
        {
            if (IsPalm(grab.leftGroup))
            {
                return grab.right.fingerPalmBone;
            }
            else if (IsPalm(grab.rightGroup))
            {
                return grab.left.fingerPalmBone;
            }
            else
            {
                return rightPalm1.fingerPalmBone;
            }
        }
        return null;
    }

    GrabInfo GetGrabInfoByRaycast()
    {
        // check all fingers
        for( int iFingerGroupLeft = 0; iFingerGroupLeft < fingerGroups.Length; ++iFingerGroupLeft)
        {
            for( int iFingerGroupRight = iFingerGroupLeft + 1; iFingerGroupRight < fingerGroups.Length; ++iFingerGroupRight )
            {
                GrabInfo info = GetGrabInfoOfFingersByRayCast(iFingerGroupLeft, iFingerGroupRight);
                if( info != null )
                {
                    return info;
                }
            }
        }

        return null;
    }

    GrabInfo GetGrabInfoOfFingersByRayCast(int iFingerGroupLeft, int iFingerGroupRight)
    {
        Finger[] leftFingers = fingerGroups[iFingerGroupLeft];
        Finger[] rightFingers = fingerGroups[iFingerGroupRight];

        for (int iFingerLeft = 0; iFingerLeft < leftFingers.Length; ++iFingerLeft)
        {
            for (int iFingerRight = 0; iFingerRight < rightFingers.Length; ++iFingerRight)
            {               
                Finger leftFinger = leftFingers[iFingerLeft];
                Finger rightFinger = rightFingers[iFingerRight];

                Vector3 leftPosition = leftFinger.fingerRayAnchor.position;
                Vector3 rightPosition = rightFinger.fingerRayAnchor.position;

                Ray ray = new Ray(leftPosition, rightPosition - leftPosition);
                float lenth = Vector3.Distance(leftPosition, rightPosition);

                //if (IsSameHand(iFingerGroupLeft, iFingerGroupRight))
                //{
                //    Debug.DrawLine(leftPosition, rightPosition, Color.red);
                //}

                RaycastHit hitInfo;
                if (Physics.Raycast( ray, out hitInfo, lenth, ingoreLayerMask))
                {
                    bool sameHand = IsSameHand(iFingerGroupLeft, iFingerGroupRight);

                    // same hand fingers or both hand actually touched object
                    if ( sameHand || leftFinger.touchedObjects.Contains(hitInfo.collider.gameObject) && rightFinger.touchedObjects.Contains(hitInfo.collider.gameObject ) )
                    {
                        GrabInfo info = new GrabInfo();
                        info.left = leftFinger;
                        info.right = rightFinger;
                        info.obj = hitInfo.collider.gameObject;
                        info.grabAngle = 0.0f;
                        info.leftGroup = iFingerGroupLeft;
                        info.rightGroup = iFingerGroupRight;

                        selectObject = info.obj;

                        return info;
                    }
                }
            }
        }

        return null;
    }

    GrabInfo GetGrabInfoByDetector()
    {
        for (int iFingerGroupLeft = 0; iFingerGroupLeft < fingerGroups.Length; ++iFingerGroupLeft)
        {
            for (int iFingerGroupRight = iFingerGroupLeft + 1; iFingerGroupRight < fingerGroups.Length; ++iFingerGroupRight)
            {
                // ignore thumb palm grab
                if( iFingerGroupLeft == (int)FingerGroup.LeftThumb && iFingerGroupRight == (int)FingerGroup.LeftPalm
                    || iFingerGroupLeft == (int)FingerGroup.LeftPalm && iFingerGroupRight == (int)FingerGroup.LeftThumb
                    || iFingerGroupLeft == (int)FingerGroup.RightThumb && iFingerGroupRight == (int)FingerGroup.RightPalm
                    || iFingerGroupLeft == (int)FingerGroup.RightPalm && iFingerGroupRight == (int)FingerGroup.RightThumb )
                {
                    continue;
                }

                GrabInfo info = GetGrabInfoOfFingersByDetector( iFingerGroupLeft, iFingerGroupRight );
                if (info != null)
                {
                    return info;
                }
            }
        }

        return null;
    }

    GrabInfo GetGrabInfoOfFingersByDetector( int iFingerGroupLeft, int iFingerGroupRight )
    {
        Finger[] leftFingers = fingerGroups[iFingerGroupLeft];
        Finger[] rightFingers = fingerGroups[iFingerGroupRight];

        for (int iFingerLeft = 0; iFingerLeft < leftFingers.Length; ++iFingerLeft)
        {
            for (int iFingerRight = 0; iFingerRight < rightFingers.Length; ++iFingerRight)
            {
                Finger leftFinger = leftFingers[iFingerLeft];
                Finger rightFinger = rightFingers[iFingerRight];

                GameObject[] grabbedObjects = GetIntersectObjects(leftFinger, rightFinger);
                if( grabbedObjects != null && grabbedObjects.Length > 0 )
                {
                    GrabInfo grab = new GrabInfo();
                    grab.left = leftFinger;
                    grab.right = rightFinger;
                    grab.obj = grabbedObjects[0];
                    grab.grabAngle = 0.0f;
                    grab.leftGroup = iFingerGroupLeft;
                    grab.rightGroup = iFingerGroupRight;
                    selectObject = grab.obj;

                    // Debug.LogFormat("{0} grabbed by detector {1} and {2}", grab.obj, grab.left.name, grab.right.name);

                    return grab;
                }
            }
        }

        return null;
    }

    GameObject[] GetIntersectObjects(Finger left, Finger right)
    {
        List<GameObject> res = left.touchedObjects.Intersect(right.touchedObjects).ToList();
        return res != null ? res.ToArray() : null;
    }

    float GetAngle( Transform left, Transform right, Transform middle )
    {
        Vector3 vLeft = left.position - middle.position;
        Vector3 vRight = right.position - middle.position;
        float angle = Mathf.Acos( Vector3.Dot(vLeft.normalized, vRight.normalized) ) / Mathf.PI * 180.0f;
        // Debug.LogFormat("left.name = {0} right.name = {1}, middle.name = {2}, angle = {3}", left.name, right.name, middle.name, angle);
        return angle > 0.0f ? angle : 180.0f + angle;
    }

    void EnableColliders( bool value )
    {
        for( int i = 0; i < toggleColliders.Length; ++i )
        {
            toggleColliders[i].enabled = value;
        }
    }

    float GetMaxFingerVelocity()
    {
        float max = 0.0f;
        float value = leftThumb.fingerTipBone.GetComponent<Rigidbody>().velocity.magnitude; if (value > max) max = value;
        value = leftIndex.fingerTipBone.GetComponent<Rigidbody>().velocity.magnitude; if (value > max) max = value;
        value = leftMiddle.fingerTipBone.GetComponent<Rigidbody>().velocity.magnitude; if (value > max) max = value;
        value = leftRing.fingerTipBone.GetComponent<Rigidbody>().velocity.magnitude; if (value > max) max = value;
        value = leftPinky.fingerTipBone.GetComponent<Rigidbody>().velocity.magnitude; if (value > max) max = value;
        value = rightThumb.fingerTipBone.GetComponent<Rigidbody>().velocity.magnitude; if (value > max) max = value;
        value = rightIndex.fingerTipBone.GetComponent<Rigidbody>().velocity.magnitude; if (value > max) max = value;
        value = rightMiddle.fingerTipBone.GetComponent<Rigidbody>().velocity.magnitude; if (value > max) max = value;
        value = rightRing.fingerTipBone.GetComponent<Rigidbody>().velocity.magnitude; if (value > max) max = value;
        value = rightPinky.fingerTipBone.GetComponent<Rigidbody>().velocity.magnitude; if (value > max) max = value;
        return max;
    }

    void Start()
    {
        fingerGroups = new Finger[(int)FingerGroup.NumOfGroups][]
        {
            new Finger[] { leftThumb },
            new Finger[] { leftIndex, leftMiddle, leftRing, leftPinky },
            new Finger[] { leftPalm1, leftPalm2 },
            new Finger[] { rightThumb },
            new Finger[] { rightIndex, rightMiddle, rightRing, rightPinky },
            new Finger[] { rightPalm1, rightPalm2 }
        };

        ingoreLayerMask = 0;
        for (int i = 0; i < ignoreLayers.Length; ++i)
        {
            ingoreLayerMask |= LayerMask.NameToLayer(ignoreLayers[i]);
        }
    }
    #endregion
}
