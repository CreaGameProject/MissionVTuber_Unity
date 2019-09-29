using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrabEffector : MonoBehaviour
{
    public int cachedFrameCount = 10;
    public int ignoreFrameCount = 3;
    public float velocityMagic = 1.0f;
    public float angularVelocityMagic = 30.0f;
    public float positionMagic = 30.0f;
    public Transform leftHandAnchor;
    public Transform rightHandAnchor;
    public Transform bothHandAnchor;
    public float releaseDelayVelocity;
    public float releaseDelay = 0.2f;

    public Material selectMaterial;
    public Material grabMaterial;

    GrabSensor.GrabInfo currentGrab;
    int oldLayer;                                       // remember old layer of the grabbed object, when released set this value back to the object
    Transform currentAnchor;
    public Material oldMaterial;
    private List<Vector3> velocityHistory = new List<Vector3>();
    private List<Vector3> angularVelocityHistory = new List<Vector3>();
    private List<Vector3> positionHistory = new List<Vector3>();
    public Transform positionSourceTransform;

    // debug
    public bool verbose = false;
    private Vector3 lastMaxVelocityPosition;
    private Vector3 lastMaxVelocity;

    void OnEnable()
    {
        GrabSensor sensor = GetComponent<GrabSensor>();
        if( sensor != null )
        {
            sensor.OnSelect += OnSelect;
            sensor.OnDeselect += OnDeselect;
            sensor.OnGrab += OnGrab;
            sensor.OnRelease += OnRelease;
        }
    }

    void OnDisable()
    {
        GrabSensor sensor = GetComponent<GrabSensor>();
        if (sensor != null)
        {
            sensor.OnSelect -= OnSelect;
            sensor.OnDeselect -= OnDeselect;
            sensor.OnGrab -= OnGrab;
            sensor.OnRelease -= OnRelease;
        }
    }

    void FixedUpdate()
    {
        if( currentGrab != null && currentGrab.obj != null )
        {
            GameObject obj = currentGrab.obj;

            // calculate delta position and delta rotation between fixed updates
            Quaternion dAng = currentAnchor.rotation * Quaternion.Inverse(obj.transform.rotation);
            Vector3 dPos = currentAnchor.position - obj.transform.position;
            float angle;
            Vector3 axis;
            dAng.ToAngleAxis(out angle, out axis);

            if (angle > 180.0f) angle -= 360.0f;

            // estimate next position and rotation and set to rigidbody, using approach from NewtonVR
            if (angle != 0)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                Vector3 velocityTarget = dPos * velocityMagic * Time.fixedDeltaTime;
                Vector3 angularTarget = (Time.fixedDeltaTime * angle * axis) * angularVelocityMagic;

                ApplyVelocity(rb, velocityTarget, angularTarget);

                velocityHistory.Add(velocityTarget);
                angularVelocityHistory.Add(angularTarget);
                if (positionSourceTransform == null)
                {
                    NeuronAnimatorInstance neuron = rightHandAnchor.GetComponentInParent<NeuronAnimatorInstance>();
                    positionSourceTransform = neuron.physicalReference.GetReferenceAnimator().GetBoneTransform(HumanBodyBones.RightHand);
                }
                positionHistory.Add(positionSourceTransform.position);

                if (velocityHistory.Count > cachedFrameCount)
                {
                    velocityHistory.RemoveAt(0);
                    angularVelocityHistory.RemoveAt(0);
                    positionHistory.RemoveAt(0);
                }
            }
        }

        if( lastMaxVelocity.magnitude > 0 )
        {
            Debug.DrawLine(lastMaxVelocityPosition, lastMaxVelocityPosition + lastMaxVelocity, Color.red);
        }
    }

    void ApplyVelocity( Rigidbody rb, Vector3 velocityTarget, Vector3 angularTarget)
    {
        rb.velocity = Vector3.MoveTowards(rb.velocity, velocityTarget, 10.0f);
        rb.angularVelocity = Vector3.MoveTowards(rb.angularVelocity, angularTarget, 10.0f);
    }

    void OnGrab(GrabSensor.GrabInfo info )
    {
        // on grab 
        if( verbose )
        {
            Debug.LogFormat("Grabbed {0} by {1} and {2} grabAngle = {3}", info.obj, info.left.name, info.right.name, info.grabAngle);
        }
        
        currentGrab = info;
        oldLayer = currentGrab.obj.layer;

        if (currentGrab.IsLeftHand)
        {
            currentAnchor = leftHandAnchor;
        }
        else if (currentGrab.IsRightHand)
        {
            currentAnchor = rightHandAnchor;
        }
        else
        {
            currentAnchor = bothHandAnchor;
        }

        currentAnchor.position = currentGrab.obj.transform.position;
        currentAnchor.rotation = currentGrab.obj.transform.rotation;

        velocityHistory.Clear();
        angularVelocityHistory.Clear();

        Renderer renderer = currentGrab.obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            GameObject.DestroyImmediate(renderer.material);
            renderer.material = grabMaterial;
        }

        currentGrab.obj.layer = LayerMask.NameToLayer("Grabbed");
        currentGrab.hand = currentAnchor;

        Interactable interact = currentGrab.obj.GetComponent<Interactable>();
        if ( interact != null )
        {
            interact.OnGrab(currentGrab);
        }
    }

    void OnRelease(GrabSensor.GrabInfo info )
    {
        if (verbose)
        {
            Debug.LogFormat("Released {0} by {1} and {2} grabAngle = {3}",info.obj, info.left.name, info.right.name, info.grabAngle);
        }

        if (currentGrab != null && currentGrab.obj != null)
        {
            GameObject obj = currentGrab.obj;
            if (currentGrab.obj.GetComponent<Rigidbody>().velocity.magnitude < releaseDelayVelocity)
            {
                Debug.LogFormat("Immediate change layer");
                ChangeLayer(currentGrab.obj, oldLayer);
            }
            else
            {
                StartCoroutine(DelayChangeLayer(currentGrab.obj, releaseDelay, oldLayer));
            }

            //Vector3 velocityTarget = Vector3.zero;
            //Vector3 angularTarget = Vector3.zero;
            //for( int i = 0; i < velocityHistory.Count; ++i )
            //{
            //    velocityTarget += velocityHistory[i];
            //    angularTarget += angularVelocityHistroy[i];
            //}
            ////velocityTarget /= velocityHistory.Count;
            ////angularTarget /= velocityHistory.Count;
            //ApplyVelocity(obj.GetComponent<Rigidbody>(), velocityTarget, angularTarget);

            Vector3 maxVelocity = Vector3.zero;
            Vector3 maxAngularVelocity = Vector3.zero;
            GetMaxVelocityInHistroy(out maxVelocity, out maxAngularVelocity);
            lastMaxVelocity = maxVelocity;
            lastMaxVelocityPosition = obj.transform.position;

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            ApplyVelocity(rb, maxVelocity, maxAngularVelocity);

            if( rb.useGravity == true )
            {
                // additional force by last several cached positions
                float curveLength = 0.0f;
                if (positionHistory.Count > 0 && positionHistory.Count - ignoreFrameCount > 0)
                {
                    for (int i = 1; i < positionHistory.Count; ++i)
                    {
                        curveLength += (positionHistory[i] - positionHistory[i - 1]).magnitude;
                    }
                    Vector3 forceDir = positionHistory[positionHistory.Count - 1] - positionHistory[positionHistory.Count - 2];
                    rb.AddForce(forceDir.normalized * curveLength * positionMagic, ForceMode.VelocityChange);
                    positionHistory.Clear();
                }
            }            

            // restore object material
            Renderer renderer = currentGrab.obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                GameObject.DestroyImmediate(renderer.material);
                renderer.material = oldMaterial;
                oldMaterial = null;
            }
        }
        currentGrab = null;
    }

    void GetMaxVelocityInHistroy( out Vector3 maxVelocity, out Vector3 maxAngularVelocity)
    {
        maxVelocity = Vector3.zero;
        maxAngularVelocity = Vector3.zero;

        if (velocityHistory.Count - ignoreFrameCount > 0)
        {
            maxVelocity = Vector3.zero;
            maxAngularVelocity = Vector3.zero;
            for (int i = 0; i < velocityHistory.Count - ignoreFrameCount; ++i)
            {
                Vector3 velocity = velocityHistory[i];
                if (maxVelocity == Vector3.zero || maxVelocity.magnitude < velocity.magnitude)
                {
                    maxVelocity = velocity;
                }

                Vector3 angularVelocity = angularVelocityHistory[i];
                if (maxAngularVelocity == Vector3.zero || maxAngularVelocity.magnitude < angularVelocity.magnitude)
                {
                    maxAngularVelocity = angularVelocity;
                }
            }

            Debug.LogFormat("use max veloctiy {0} in last {1}-{2} frames", maxVelocity.ToString("F4"), ignoreFrameCount, velocityHistory.Count - ignoreFrameCount);
            Debug.LogFormat("use max angularVeloctiy {0} in last {1}-{2} frames", maxAngularVelocity.ToString("F4"), ignoreFrameCount, velocityHistory.Count - ignoreFrameCount);
        }
        else
        {
            maxVelocity = Vector3.zero;
            maxAngularVelocity = Vector3.zero;
            Debug.LogWarning("Not velocity history");
        }
    }

    void OnSelect( GrabSensor.GrabInfo info )
    {
        if (verbose)
        {
            Debug.LogFormat("Select {0} by {1} and {2} selectAngle = {3}", info.obj, info.left.name, info.right.name, info.selectAngle );
        }

        Renderer renderer = info.obj.GetComponent<Renderer>();
        if( renderer != null )
        {
            oldMaterial = renderer.sharedMaterial;
            renderer.material = selectMaterial;
        }
    }

    void OnDeselect( GrabSensor.GrabInfo info )
    {
        if (verbose)
        {
            Debug.LogFormat("Deselect {0} by {1} and {2} selectAngle = {3}", info.obj, info.left.name, info.right.name, info.selectAngle );
        }

        Renderer renderer = info.obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            GameObject.DestroyImmediate(renderer.material);
            renderer.material = oldMaterial;
            oldMaterial = null;
        }
    }

    IEnumerator DelayChangeLayer( GameObject obj, float delay, int toLayer )
    {
        yield return new WaitForSeconds(delay);
        ChangeLayer(obj, toLayer);
    }

    void ChangeLayer( GameObject obj, int toLayer )
    {
        obj.layer = oldLayer;
    }
}
