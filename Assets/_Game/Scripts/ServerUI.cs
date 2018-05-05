using UnityEngine;
using UnityEngine.UI;

public class ServerUI : MonoBehaviour {

    private Text ipAddressText;

    // Use this for initialization
    void Start () {
        GameObject ipField = GameObject.Find("ipAddressText");
        if (ipField) {
            ipAddressText = ipField.GetComponent<Text>();
        }
        else {
            Debug.LogWarning("ip text field wasn't found");
        }

        ipAddressText.text = string.Format("Host Lan IP Address: {0}", Network.player.ipAddress.ToString() );

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
