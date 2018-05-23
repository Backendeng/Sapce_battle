using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    public GameObject PlayMenuObj;
    public GameObject MainMenuObj;
    public GameObject SettingsMenuObj;
    public InputField ipInputField;
    public Text playMenuStatusText;

    private bool pingResult = false;
    private bool pinging;
    private static int errorNum = 0;
    public static int ErrorNum { get { return errorNum; } set { errorNum = value; } }

    public void Start() {
        if (!ipInputField || !playMenuStatusText) {
            Debug.LogError("required fields weren't found");
        }

        if (errorNum == 0) {
            MainMenuObj.SetActive(true);
            PlayMenuObj.SetActive(false);
            SettingsMenuObj.SetActive(false);

        }
        else if (errorNum == 6) {
            MainMenuObj.SetActive(false);
            PlayMenuObj.SetActive(true);
            playMenuStatusText.text = "Connection Timed Out";
            errorNum = 0;
        }

    }

    public void Update() {
  
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
        if (ValidateIPv4(ipAddress) == false) {
            playMenuStatusText.text = "Invalid IP Address";
            return;
        }
        /*
        pinging = true;
        StartCoroutine(PingUpdate(ipAddress));
        while (pinging == true) { }; // no no no!
        if (pingResult == false) {
            return;
        }*/
        PlayMenuObj.SetActive(false);
        Client.ServerIP = ipAddress;
        SceneManager.LoadScene(2);
    }

    public void OnOpenPlayMenu() {
        playMenuStatusText.text = "";
    }

    System.Collections.IEnumerator PingUpdate(string ipAddress) {
        Ping serverPing = new Ping(ipAddress);
        yield return new WaitForSeconds(1f);

        playMenuStatusText.text = serverPing.time.ToString();

        if (serverPing.isDone == false) {
            playMenuStatusText.text = "Ping Failed.";
            pingResult = false;
            pinging = false;
        }
        else {
            playMenuStatusText.text = "Ping Returned. Connecting...";
            pingResult = true;
            pinging = false;
        }
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
