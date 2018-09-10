using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClientUI : MonoBehaviour {

    public Client clientController;
    public GameObject StartScreen;
    public GameObject EscapeMenu;
    public GameObject RespawnScreen;
    public GameObject HUD;
    public HudUI hudUI;
    public Text escapeMenuStatusText;
    public GameManager gameManager;
    public GameObject ScoreBoardView;
    public Text RespawnScreenKilledByText;

    private bool started = false;
    private float stopConnectionTime;

    public void Start() {
        stopConnectionTime = Time.time + 10f;

        EscapeMenu.SetActive(false);
        HUD.SetActive(false);
        StartScreen.SetActive(true);

    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (EscapeMenu.activeSelf) {
                EscapeMenu.SetActive(false);
                Cursor.visible = false;
            }
            else {
                EscapeMenu.SetActive(true);
                Cursor.visible = true;
            }
        }
        if (!Input.GetKeyDown(KeyCode.Escape)) {
            if (Input.GetKey(KeyCode.Tab)) {
                ScoreBoardView.SetActive(true);
                Cursor.visible = true;
            }
            else if (RespawnScreen.activeSelf == false) {
                ScoreBoardView.SetActive(false);
                Cursor.visible = false;
            }
        }

        
        if (started == false) {
            if (clientController.PlayerAvatarCreated == false) {
                if (Time.time > stopConnectionTime) {
                    //StartScreen.SetActive(false);
                    //escapeMenuStatusText.text = "Connection Timed Out. Please Quit And Try Again.";
                    //EscapeMenu.SetActive(true);
                    Cursor.visible = true;
                }
            }
            else {
                HUD.SetActive(true);
                StartScreen.SetActive(false);
                started = true;
            }
        }
    }


    public void QuitGame() {
        // do some stuff before closing client instance
        SceneManager.LoadScene(0);
    }

    public void EnableRespawnScreen() {
        RespawnScreen.SetActive(true);
        ScoreBoardView.SetActive(true);
        if(gameManager.localPlayersKiller != "") {
            RespawnScreenKilledByText.text = string.Format("You Were Blasted By {0}", gameManager.localPlayersKiller);
        }
        else {
            RespawnScreenKilledByText.text = "WASTED!";
        }
    }

    public void DisableRespawnScreen() {
        RespawnScreen.SetActive(false);
        ScoreBoardView.SetActive(false);
        gameManager.localPlayersKiller = "";
    }
}
