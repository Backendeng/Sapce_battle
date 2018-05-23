using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour {
    private bool loggingEnabled = false;
    private bool showUnsmoothedShadowEnabled = false;
    private bool showInterpolatedShadowEnabled = false;
    

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    

    public void toggleSaveLogs() {
        if (loggingEnabled) {
            loggingEnabled = false;
        }
        else {
            loggingEnabled = true;
        }
        Logger.LogEnabled = loggingEnabled;
    }

    public void toggleUnsmoothedShadow() {
        if (showUnsmoothedShadowEnabled) {
            showUnsmoothedShadowEnabled = false;
        }
        else {
            showUnsmoothedShadowEnabled = true;
        }
        LocalPlayerShip.showUnsmoothedShadow = showUnsmoothedShadowEnabled;
    }

    public void toggleInterpolatedShadow() {
        if (showInterpolatedShadowEnabled) {
            showInterpolatedShadowEnabled = false;
        }
        else {
            showInterpolatedShadowEnabled = true;
        }
        LocalPlayerShip.showInterpolatedShadow = showInterpolatedShadowEnabled;
    }


}
