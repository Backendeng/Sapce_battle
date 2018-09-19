using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net;

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

        string hostName = Dns.GetHostName();

        string ip = "";
        foreach (IPAddress i in Dns.GetHostEntry(hostName).AddressList) {
            if (i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                ip = i.ToString();
            }
        }
        
        ipAddressText.text = string.Format("Host Lan IP: {0}", ip);

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
