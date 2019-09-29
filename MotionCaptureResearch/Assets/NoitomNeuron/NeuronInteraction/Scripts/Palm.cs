using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Palm : MonoBehaviour
{
    public CollisionDetector[] colDetectors;
    public Collider[] colliders;

    public HashSet<GameObject> touchedObjects = new HashSet<GameObject>();
    public Transform palmRayAnchor;
    public Transform grabAttachAnchor;

    void OnEnable()
    {
        for( int i = 0; i < colDetectors.Length; ++i )
        {
            colDetectors[i].OnDetectEnter += OnEnterCollision;
            colDetectors[i].OnDetectExit += OnExitCollision;
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < colDetectors.Length; ++i)
        {
            colDetectors[i].OnDetectExit -= OnExitCollision;
            colDetectors[i].OnDetectEnter -= OnEnterCollision;
        }
    }

    void OnEnterCollision(CollisionDetector reporter, Collider other)
    {
        touchedObjects.Add(other.gameObject);
    }

    void OnExitCollision(CollisionDetector reporter, Collider other)
    {
        touchedObjects.Add(other.gameObject);
    }
}
