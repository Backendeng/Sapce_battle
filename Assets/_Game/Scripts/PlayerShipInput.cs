using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipInput : MonoBehaviour {

    [Tooltip("use mouse and mousewheel for ship input. otherwise, uses regular control input")]
    public bool useMouseInput = false;

    [Tooltip("add roll when using yaw")]
    public bool addRoll = true;
    [Tooltip("amount of roll added when using yaw")]
    public float rollMul = 0.5f;
    [Tooltip("set mouse stick sensitivity")]
    public float mouseStickSensitivity = 0.5f;

    public float analogStickSensitivity = 0.5f;
 

    [Range(-1, 1)]
    public float pitch;
    [Range(-1, 1)]
    public float yaw;
    [Range(-1, 1)]
    public float roll;
    [Range(-1, 1)]
    public float strafe; // ?
    [Range(0, 1)]
    public float throttle;


	// Update is called once per frame
	private void Update ()
    {
		if (useMouseInput)
        {
            SetStickCommandsUsingMouse();
            throttle = Input.GetAxis("Vertical");
            roll = -Input.GetAxis("Horizontal");
        }
        else // controller with analog stick
        {
            pitch = Input.GetAxis("Vertical") * analogStickSensitivity;
            if (addRoll)
                roll = -Input.GetAxis("Horizontal") * rollMul;
            if (Input.GetButton("X"))
                throttle = 1;
            else
                throttle = 0;
            if (Input.GetAxis("LeftTrigger") > 0)
                roll = -Input.GetAxis("Horizontal") * analogStickSensitivity;
            else
            {
                yaw = Input.GetAxis("Horizontal") * analogStickSensitivity;
                roll = 0;
            }
        }
	}

    private void SetStickCommandsUsingMouse()
    {
        Vector3 mousePos = Input.mousePosition;

        // set pitch and yaw with mouse position relative to the center of the screen.
        // (0,0) is te center, (-1,-1) is the bottom left, (1,1) is the top right.
        pitch = (mousePos.y - (Screen.height * 0.5f)) / (Screen.height * 0.5f) * mouseStickSensitivity;
        yaw = (mousePos.x - (Screen.width * 0.5f)) / (Screen.width * 0.5f) * mouseStickSensitivity;

        // make sure the values don't exceed limits.
        pitch = -Mathf.Clamp(pitch, -1.0f, 1.0f);
        yaw = Mathf.Clamp(yaw, -1.0f, 1.0f);
    }
}
