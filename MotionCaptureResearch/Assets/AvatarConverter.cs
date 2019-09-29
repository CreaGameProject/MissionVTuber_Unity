using System.Collections;
using System;
using UnityEngine;

public class AvatarConverter : MonoBehaviour
{
    [System.Serializable]
    public class JointPairs
    {
        public GameObject getJoint;
        public GameObject setJoint;
        [HideInInspector] public Quaternion difference;
        public JointPairs[] child;
    }

    [SerializeField] private JointPairs Root;
    void Awake()
    {
        VerticalSearch(Root, (JointPairs pairs) =>
        {
            pairs.difference = pairs.setJoint.transform.localRotation * Quaternion.Inverse(pairs.getJoint.transform.localRotation);
        });
    }

    // Update is called once per frame
    void Update()
    {
        VerticalSearch(Root, (JointPairs pairs) =>
        {
             pairs.setJoint.transform.localRotation = pairs.getJoint.transform.localRotation * pairs.difference;
        });
    }

    private void VerticalSearch(JointPairs root,Action<JointPairs> action)
    {
        action(root);
        foreach (JointPairs pairs in root.child)
        {
            VerticalSearch(pairs, action);
        }
    }
}
