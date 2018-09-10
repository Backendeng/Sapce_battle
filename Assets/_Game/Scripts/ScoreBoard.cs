using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ScoreBoard : MonoBehaviour {
    private const int maxEntries = 10;
    private static int maxNameSize = 20; // also defined in messages handler

    public GameManager gameManager;
    public Text ScoreBoardText;
    public Text[] entryName = new Text[maxEntries];
    public Text[] entryScore = new Text[maxEntries];
    public Text[] entryDeaths = new Text[maxEntries];
    public HudUI hudUI;
    
    private Dictionary<int, float> playerScores = new Dictionary<int, float>(); // (PlayerID, Score-Deaths*0.001)


	void Start () {
        if (!gameManager || !ScoreBoardText)
            Debug.LogError("null object in ScoreBoard script");
    }

    public void OnEnable() {
        if (hudUI) {
            hudUI.DisableCrosshairs();
        }
    }

    public void OnDisable() {
        if (hudUI) {
            hudUI.EnableCrossharis();
        }
    }

    public void RefreshScoreBoard() {
        playerScores.Clear();
        foreach (KeyValuePair<int, PlayerShip> kvp in gameManager.PlayerShipsDict) {
            PlayerShip ship = kvp.Value;
            playerScores.Add(kvp.Key, ship.Score - ship.Deaths * 0.001f); // the ship.Deaths * 0.001 adds a secondary sorting by the deaths
        }

        int index = 0;
        foreach (KeyValuePair<int, float> kvp in playerScores.OrderByDescending(p => p.Value)) {
            PlayerShip ship = gameManager.GetShip(kvp.Key);
            if (!ship) {
                continue;
            }
                
            entryName[index].text = ship.PlayerName;
            entryScore[index].text = ship.Score.ToString();
            entryDeaths[index].text = ship.Deaths.ToString();

            index++;
            if (index >= maxEntries - 1)
                break;
        }
        while (index < maxEntries) {
            entryName[index].text = "";
            entryScore[index].text = "";
            entryDeaths[index].text = "";
            index++;
        }
        Debug.Log("Refreshed Score Board");
    }
}
