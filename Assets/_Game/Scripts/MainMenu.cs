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

    private static int errorNum = 0;
    public static int ErrorNum { get { return errorNum; } set { errorNum = value; } }

    public void Start() {
        if (!ipInputField || !playMenuStatusText || !nameInputField) {
            Debug.LogError("required fields weren't found");
        }

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
            Cursor.visible = true;
            //Cursor.lockState = CursorLockMode.None;
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
        PlayMenuObj.SetActive(false);
        if (ipAddress == "") {
            Client.ServerIP = "127.0.0.1";
        } else {
            Client.ServerIP = ipAddress;
        }

        string playerName = nameInputField.text;
        if (playerName == "") {
            playMenuStatusText.text = "Enter a name";
        }
        byte shipType = 1; //TODO: implement ship type selection

        Client.ClientInitData.PlayerName = playerName;
        Client.ClientInitData.ShipType = shipType;

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

}
