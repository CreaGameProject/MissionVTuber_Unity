using UnityEngine;
using System.Collections;

public class ResetPosition : MonoBehaviour {

    Vector3 oldPosition;
    Quaternion oldRotation;

	// Use this for initialization
	void Start () {
        oldPosition = transform.position;
        oldRotation = transform.rotation;

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.position = oldPosition;
            transform.rotation = oldRotation;
            Rigidbody r = GetComponent<Rigidbody>();
            r.velocity = Vector3.zero;
            r.angularVelocity = Vector3.zero;
        }
	}
}
