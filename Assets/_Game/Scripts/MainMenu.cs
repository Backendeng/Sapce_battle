using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    public GameObject PlayMenuObj;
    public GameObject MainMenuObj;
    public InputField ipInputField;
    public Text inputErrorText;

    private static int errorNum;
    public static int ErrorNum { get { return errorNum; } set { errorNum = value; } }

    public void Start() {
        if (!ipInputField || !inputErrorText) {
            Debug.LogError("required fields weren't found");
        }

        if (errorNum == 6) {
            MainMenuObj.SetActive(false);
            PlayMenuObj.SetActive(true);
            inputErrorText.text = "Connection Timed Out";
        }

    }

    public void Update() {
        
        if (PlayMenuObj.activeSelf == false) {
            inputErrorText.text = "";
        }
    }

    public static void LoadScene(int scene) {
        SceneManager.LoadScene(scene);
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void StartGame() {
        inputErrorText.text = "";
        string ipAddress = ipInputField.text;
        if (ValidateIPv4(ipAddress) == false) {
            inputErrorText.text = "Invalid IP Address";
            return;
        }
        
        
        //StartCoroutine(PingUpdate(ipAddress));


        Client.ServerIP = ipAddress;
        SceneManager.LoadScene(2);
    }

    System.Collections.IEnumerator PingUpdate(string ipAddress) {
        Ping serverPing = new Ping(ipAddress);
        yield return new WaitForSeconds(1f);

        inputErrorText.text = serverPing.time.ToString();

        if (serverPing.isDone == false) {
            inputErrorText.text = "Ping Failed.";
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
