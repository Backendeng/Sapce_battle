using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PlayerShipInput : MonoBehaviour {

    [Tooltip("use mouse and mousewheel for ship input. otherwise, uses regular control input")]
    public static bool useMouseInput = true;

    [Tooltip("useautomatic ship input. overrides player controls")]
    public static bool useAutoPilot = false;

    [Tooltip("add roll when using yaw")]
    public bool addRoll = false;

    [Tooltip("amount of roll")]
    private float mouseRollMul = GameSettings.Controls.mouseRollMul;
    private float analogStickRollMul = GameSettings.Controls.analogStickRollMul;

    [Tooltip("set mouse stick sensitivity")]
    private float mouseStickSensitivity = GameSettings.Controls.mouseStickSensitivity;
    [Tooltip("set analog stick sensitivity")]
    private float analogStickSensitivity = GameSettings.Controls.analogStickSensitivity;

    private int crosshairYOffset = 15;

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
    public bool boost_pressed = false;

    private bool disableInput = false;


    public void DisableInput() {
        yaw = 0f;
        throttle = 0f;
        roll = 0;
        pitch = 0;
        disableInput = true;
    }

    public void EnableInput() {
        disableInput = false;
    }

	// Update is called once per frame
	private void Update ()
    {
        if (disableInput)
            return;
        
        if (useAutoPilot) {
            yaw = 0.05f;
            throttle = 1.0f;
            roll = 0;
            pitch = 0;
            return;
        }

		if (useMouseInput)
        {
            SetStickCommandsUsingMouse();
            throttle = Mathf.Clamp(Input.GetAxis("Vertical"), -0.3f, 1f); // restricting max backwards speed
            roll = -Input.GetAxis("Horizontal") * mouseRollMul;
        }
        else
        {
            pitch = -Input.GetAxis("Vertical") * analogStickSensitivity;
            if (addRoll)
                roll = -Input.GetAxis("Horizontal") * analogStickRollMul;
            if (Input.GetButton("X"))
                throttle = 1;
            else
                throttle = 0;
            if (Input.GetAxis("LeftTrigger") > 0)
                roll = -Input.GetAxis("Horizontal") * analogStickSensitivity * analogStickRollMul;
            else
            {
                yaw = Input.GetAxis("Horizontal") * analogStickSensitivity;
                roll = 0;
            }
        }

        if (Input.GetKey(KeyCode.Space) || Input.GetButton("A")) {
            boost_pressed = true;
        } else {
            boost_pressed = false;
        }

        if (GameSettings.VR_Enabled && Input.GetKey(KeyCode.LeftControl)) {
            //InputTracking.Recenter();
        }

	}

    private void SetStickCommandsUsingMouse()
    {
        Vector3 mousePos = Input.mousePosition;

        // set pitch and yaw with mouse position relative to the center of the screen.
        // (0,0) is te center, (-1,-1) is the bottom left, (1,1) is the top right.
        pitch = (mousePos.y + crosshairYOffset - (Screen.height * 0.5f)) / (Screen.height * 0.5f) * mouseStickSensitivity;
        yaw = (mousePos.x - (Screen.width * 0.5f)) / (Screen.width * 0.5f) * mouseStickSensitivity;

        // make sure the values don't exceed limits.
        pitch = -Mathf.Clamp(pitch, -1.0f, 1.0f);
        yaw = Mathf.Clamp(yaw, -1.0f, 1.0f);
    }
}
