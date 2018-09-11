using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    public GameObject PlayMenuObj;
    public GameObject MainMenuObj;
    public GameObject SettingsMenuObj;
    public InputField ipInputField;
    public InputField nameInputField;
    public Text playMenuStatusText;
    public Text mainMenuStatusText;

    private static int errorNum = 0;
    public static int ErrorNum { get { return errorNum; } set { errorNum = value; } }
    private readonly int maxNameLen = 15;

    byte shipType = 1;

    public void Start() {
        Cursor.visible = true;
        AudioListener.pause = false;
        if (!ipInputField || !playMenuStatusText || !nameInputField || !mainMenuStatusText) {
            Debug.LogError("required fields weren't found");
        }
        mainMenuStatusText.text = "";

        if (errorNum == 0) {
            MainMenuObj.SetActive(true);
            PlayMenuObj.SetActive(false);
            SettingsMenuObj.SetActive(false);

        }
        else if (errorNum == 6) { // Connection Timeout
            MainMenuObj.SetActive(false);
            PlayMenuObj.SetActive(true);
            playMenuStatusText.text = "Connection Timed Out";
            errorNum = 0;
            //Cursor.lockState = CursorLockMode.None;
        }

        else if (errorNum == 666) { // can't start host, probably port is taken
            mainMenuStatusText.text = string.Format("Cannot open socket. Please check your network,\n most probably port is occupied.");
        }
    }

    public static void LoadScene(int scene) {
        SceneManager.LoadScene(scene);
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void StartGame() {
        
        playMenuStatusText.text = "";
        string ipAddress = ipInputField.text;
        if (ipAddress != "" && ValidateIPv4(ipAddress) == false) {
            playMenuStatusText.text = "Invalid IP Address";
            return;
        }
        if (ipAddress == "") {
            Client.ServerIP = "127.0.0.1";
        } else {
            Client.ServerIP = ipAddress;
        }

        string playerName = nameInputField.text;
        if (playerName == "") {
            playMenuStatusText.text = "Enter a name";
            return;
        }
        if (playerName.Length > maxNameLen) {
            playMenuStatusText.text = "Maximum name length is " + maxNameLen + " characters";
            return;
        }

        Client.ClientInitData.PlayerName = playerName;
        Client.ClientInitData.ShipType = shipType;
        PlayMenuObj.SetActive(false);
        SceneManager.LoadScene(2);
    }

    public void OnOpenPlayMenu() {
        playMenuStatusText.text = "";
    }

    public bool ValidateIPv4(string ipString) {
        if (System.String.IsNullOrEmpty(ipString)) {
            return false;
        }
        string[] splitValues = ipString.Split('.');
        if (splitValues.Length != 4) {
            return false;
        }
        byte tempForParsing;
        return splitValues.All(r => byte.TryParse(r, out tempForParsing));
    }

    public void HostGame() {
        SceneManager.LoadScene(1);
    }

    public void SetShipType1() {
        shipType = 1;
        Debug.Log("set ship type to 1");
    }

    public void SetShipType2() {
        shipType = 2;
        Debug.Log("set ship type to 2");
    }

    public void SetShipType3() {
        shipType = 3;
        Debug.Log("set ship type to 3");
    }

}
