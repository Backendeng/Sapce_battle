using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour {

    [Tooltip("Speed at which the camera rotates. (Camera uses Slerp for rotation.)")]
    public float rotateSpeed = 90.0f;

    [Tooltip("check this box if the parented object is using FixedUpdate for movement (smoothes movement)")]
    public bool usedFixedUpdate = true;

    private Transform target;
    private Vector3 startOffset;

    private void Start ()
    {
        target = transform.parent;

        if (!target)
            Debug.LogWarning(name + ": PlayerCamera will not function correctly without a target");
        if (!transform.parent)
            Debug.LogWarning(name + ": PlayerCamera will not function correctly without a parent to derive the initial offset from.");

        startOffset = transform.localPosition;
        transform.SetParent(null);

	}


    private void FixedUpdate()
    {
        if (usedFixedUpdate)
            UpdateCamera();
    }

    void Update ()
    {
        if (!usedFixedUpdate)
            UpdateCamera();
	}

    private void UpdateCamera()
    {
        if (target)
        {
            transform.position = target.TransformPoint(startOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotateSpeed * Time.deltaTime);
        }
    }
}
