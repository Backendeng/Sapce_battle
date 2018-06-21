using UnityEngine;

public class GameSettings : MonoBehaviour {
    private bool loggingEnabled = false;
    private bool showUnsmoothedShadowEnabled = false;
    private bool showInterpolatedShadowEnabled = false;
    private bool useXboxController = false;
    private bool useAutoPilot = false;
    private bool shipLerp = true;
    private bool missileLerp = true;

    public void Start() {
        Logger.LogEnabled = loggingEnabled;
        PlayerShipInput.useAutoPilot = useAutoPilot;
        LocalPlayerShip.showUnsmoothedShadow = showUnsmoothedShadowEnabled;
        LocalPlayerShip.showInterpolatedShadow = showInterpolatedShadowEnabled;
        PlayerShipInput.useMouseInput = !useXboxController;
        RemotePlayerShipClient.doLerp = shipLerp;
        Missile.doLerp = missileLerp;
    }

    public void ToggleSaveLogs() {
        loggingEnabled = !loggingEnabled;

        Logger.LogEnabled = loggingEnabled;
        Debug.Log("saving logs is: " + loggingEnabled);
    }

    public void ToggleUnsmoothedShadow() {
        showUnsmoothedShadowEnabled = !showUnsmoothedShadowEnabled;

        LocalPlayerShip.showUnsmoothedShadow = showUnsmoothedShadowEnabled;
        Debug.Log("show unsmoothed shadow is: " + showUnsmoothedShadowEnabled);
    }

    public void ToggleInterpolatedShadow() {
        showInterpolatedShadowEnabled = !showInterpolatedShadowEnabled;

        LocalPlayerShip.showInterpolatedShadow = showInterpolatedShadowEnabled;
        Debug.Log("show lerped shadow is: " + showInterpolatedShadowEnabled);
    }

    public void ToggleControls() {
        useXboxController = !useXboxController;

        PlayerShipInput.useMouseInput = !useXboxController;
        Debug.Log("using mouse input is: " + !useXboxController);
    }

    public void ToggleAutoPilot() {
        useAutoPilot = !useAutoPilot;
        PlayerShipInput.useAutoPilot = useAutoPilot;
        Debug.Log("using autopilot is: " + useAutoPilot);
    }

    public void ToggleShipLerp() {
        shipLerp = !shipLerp;
        RemotePlayerShipClient.doLerp = shipLerp;
        Debug.Log("ships lerp is: " + shipLerp);
    }

    public void ToggleMissileLerp() {
        missileLerp = !missileLerp;
        Missile.doLerp = missileLerp;
        Debug.Log("missile lerp is: " + missileLerp);
    }


}
