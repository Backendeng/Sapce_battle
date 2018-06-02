using UnityEngine;

[RequireComponent(typeof(Camera))]

public class PlayerCamera : MonoBehaviour {

    [Tooltip("Speed at which the camera rotates. (Camera uses Slerp for rotation.)")]
    public float rotateSpeed = 90.0f;

    private Transform target;
    private Vector3 startOffset;

    private Vector3 additional_offset = new Vector3(0, 0, -10); // when boosting
    private int boost_cam_speed = 3;
    private float cam_travel = 0;
    private float dec_cam_travel = 0;
    private bool decelerating = false;

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
        bool isBoosting = target.GetComponent<LocalPlayerShip>().boosting;

        if (target && !isBoosting) {
            cam_travel = 0f;
            if (!decelerating) {
                transform.position = target.TransformPoint(startOffset);
                transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotateSpeed * Time.deltaTime);
            } else {
                dec_cam_travel += boost_cam_speed * Time.deltaTime;
                transform.position = target.TransformPoint(Vector3.Lerp(startOffset + additional_offset, startOffset, dec_cam_travel));
                transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotateSpeed * Time.deltaTime);
                if (dec_cam_travel > 1) {
                    decelerating = false;
                    dec_cam_travel = 0f;
                }
            }
        }
  
        if (target && isBoosting) {
            cam_travel += 2 * boost_cam_speed * Time.deltaTime;
            transform.position = target.TransformPoint(Vector3.Lerp(startOffset, startOffset + additional_offset, cam_travel));
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotateSpeed * Time.deltaTime);
            decelerating = true; // used when done boosting
        } 

        
    }
}
