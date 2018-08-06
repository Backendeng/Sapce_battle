using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ServerUI : MonoBehaviour {

    private Text ipAddressText;
    public GameObject EscapeMenu;
    public Server serverController;

    void Start () {
        GameObject ipField = GameObject.Find("ipAddressText");
        if (ipField) {
            ipAddressText = ipField.GetComponent<Text>();
        }
        else {
            Debug.LogWarning("ip text field wasn't found");
        }

        ipAddressText.text = string.Format("Host Lan IP Address: {0}", 0/*Network.player.ipAddress.ToString()*/ );

        EscapeMenu.SetActive(false);
    }
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (EscapeMenu.activeSelf) {
                EscapeMenu.SetActive(false);
            }
            else {
                EscapeMenu.SetActive(true);
            }
        }
    }

    public void QuitServer() {
        // do some stuff before closing server instance
        serverController.CloseServer();
        SceneManager.LoadScene(0);
    }
}
