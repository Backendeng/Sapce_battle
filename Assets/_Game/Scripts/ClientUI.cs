using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientUI : MonoBehaviour {

    public GameObject EscapeMenu;
    public MouseCrosshair mouseCrosshair;

    public void Start() {
        if (!EscapeMenu || !mouseCrosshair) {
            Debug.LogError("some ClientUI objects are missing");
        }
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (EscapeMenu.activeSelf) {
                EscapeMenu.SetActive(false);
            }
            else {
                EscapeMenu.SetActive(true);
            }
            
        }
    }


    public void QuitGame() {
        // do some stuff before closing client instance
        

        SceneManager.LoadScene(0);
    }
}
