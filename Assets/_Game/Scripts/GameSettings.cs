using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour {
    private bool loggingEnabled = false;
    private bool showUnsmoothedShadowEnabled = false;
    private bool showInterpolatedShadowEnabled = false;
    private bool useXboxController = false;
    private bool useAutoPilot = false;
    private bool shipLerp = true;
    private bool missileLerp = true;

    public Text statusText;
    public InputField portInputField;
    private int defaultPort = 8888;

    public void Start() {
        statusText.text = "";
        Logger.LogEnabled = loggingEnabled;
        PlayerShipInput.useAutoPilot = useAutoPilot;
        LocalPlayerShip.showUnsmoothedShadow = showUnsmoothedShadowEnabled;
        LocalPlayerShip.showInterpolatedShadow = showInterpolatedShadowEnabled;
        PlayerShipInput.useMouseInput = !useXboxController;
        RemotePlayerShipClient.doLerp = shipLerp;
        Missile.doLerp = missileLerp;
    }

    private void Update() {
        if (portInputField.text != "" && Server.inPort != System.Convert.ToInt32(portInputField.text)) {
            int port = System.Convert.ToInt32(portInputField.text);
            if (port <= 1023 || port > 99999) {
                statusText.text = "Invalid Port";
            }
            else {
                Server.inPort = port;
                Client.outPort = port;
                Debug.Log("port set to " + port);
                statusText.text = "";
            }
        }
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

    public void SetSettings() {
        if (portInputField.text == "") {
            Server.inPort = defaultPort;
            Client.outPort = defaultPort;
            Debug.Log("port set to " + defaultPort);
        }
    }

}
