using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour {
    private bool loggingEnabled = false;
    private bool showUnsmoothedShadowEnabled = false;
    private bool showInterpolatedShadowEnabled = false;
    private static bool useXboxController = false;
    
    public static bool UseMouseInput { get { return !useXboxController; } }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    

    public void ToggleSaveLogs() {
        if (loggingEnabled) {
            loggingEnabled = false;
        }
        else {
            loggingEnabled = true;
        }
        Logger.LogEnabled = loggingEnabled;
        Debug.Log("saving logs is: " + loggingEnabled);
    }

    public void ToggleUnsmoothedShadow() {
        if (showUnsmoothedShadowEnabled) {
            showUnsmoothedShadowEnabled = false;
        }
        else {
            showUnsmoothedShadowEnabled = true;
        }
        LocalPlayerShip.showUnsmoothedShadow = showUnsmoothedShadowEnabled;
        Debug.Log("show unsmoothed shadow is: " + showUnsmoothedShadowEnabled);
    }

    public void ToggleInterpolatedShadow() {
        if (showInterpolatedShadowEnabled) {
            showInterpolatedShadowEnabled = false;
        }
        else {
            showInterpolatedShadowEnabled = true;
        }
        LocalPlayerShip.showInterpolatedShadow = showInterpolatedShadowEnabled;
        Debug.Log("show lerped shadow is: " + showInterpolatedShadowEnabled);
    }

    public void ToggleControls() {
        if (useXboxController) {
            useXboxController = false;

        }
        else {
            useXboxController = true;
        }
        PlayerShipInput.useMouseInput = !useXboxController;
        Debug.Log("using mouse input is: " + !useXboxController);
    }


}
