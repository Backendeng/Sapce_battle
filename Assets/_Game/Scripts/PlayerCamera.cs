using UnityEngine;

[RequireComponent(typeof(Camera))]

public class PlayerCamera : MonoBehaviour {

    [Tooltip("Speed at which the camera rotates. (Camera uses Slerp for rotation.)")]
    public float rotateSpeed = 90.0f;

    private Transform target;
    private Vector3 startOffset;

    private void Start ()
    {
        target = transform.parent;

        if (!target)
            Debug.LogWarning(name + ": PlayerCamera has no target");
        if (!transform.parent)
            Debug.LogWarning(name + ": PlayerCamera has no parent to derive the initial offset from.");

        startOffset = transform.localPosition;
        transform.SetParent(null);

	}

    private void FixedUpdate() {
        if (target) {
            transform.position = target.TransformPoint(startOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotateSpeed * Time.deltaTime);
        }
    }
}
