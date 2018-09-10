using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    private GameObject networkControllerObj;
    private Client clientController;
    private Server serverController;
    public GameObject RespawnMenu;
    public ScoreBoard scoreBoard;
    public ClientUI clientUI;

    private bool isServer;
    private readonly int initialHealth = 100;
    private readonly int killScorePoints = 10;

    private Dictionary<int, PlayerData> PlayerDataDict = new Dictionary<int, PlayerData>();
    public Dictionary<int, PlayerShip> PlayerShipsDict = new Dictionary<int, PlayerShip>();
    private Dictionary<int, int> ClientsDict = new Dictionary<int, int>(); // (ClientID, PlayerID)
    private Dictionary<int, int> PlayersDict = new Dictionary<int, int>(); // (PlayerID, ClientID)

    private string lastKillCredit = "";
    public string localPlayersKiller = "";
    private readonly float killCreditTimeout = 5f;
    private float lastKillCreditTime;

    private int localPlayerLockCounter = 0;
    public int LocalPlayerLockCounter { set { localPlayerLockCounter = value; } get { return localPlayerLockCounter; } }

    private class PlayerData {
        public string Name { get; set; }
        public int Health { get; set; }
        public int Score { get; set; }
        public int Deaths { get; set; }
        public PlayerData(string name, int health, int score, int deaths) { Name = name; Health = health; Score = score; Deaths = deaths; }
    };

    void Start () {
        // setting up serverController / clientController referecnce 
        networkControllerObj = GameObject.Find("ServerNetworkController");
        if (networkControllerObj != null) {
            isServer = true;
            serverController = networkControllerObj.GetComponent<Server>();
            if (serverController == null) {
                Debug.LogError("server controller wasn't found for the Game Manager");
            }
        }
        else {
            networkControllerObj = GameObject.Find("ClientNetworkController");
            if (networkControllerObj != null) {
                isServer = false;
                clientController = networkControllerObj.GetComponent<Client>();
                if (clientController == null) {
                    Debug.LogError("client controller wasn't found for the Game Manager");
                }
            }
            else {
                Debug.LogWarning("ERROR! networkController not found for the Game Manager");
            }
        }
        scoreBoard.RefreshScoreBoard();
    }

    public int GetClientID(int playerID) {
        if (!PlayersDict.ContainsKey(playerID)) {
            Debug.LogError("PlayersDict doesn't contain playerID = " + playerID);
        }
        return PlayersDict[playerID];
    }

    public int GetPlayerID(int clientID) {
        if (!ClientsDict.ContainsKey(clientID)) {
            Debug.LogError("ClientsDict doesn't contain clientID = " + clientID);
        }
        return ClientsDict[clientID];
    }

    public int GetHealth(int playerID) {
        if (PlayerDataDict.ContainsKey(playerID)) {
            return PlayerDataDict[playerID].Health;
        }
        else {
            Debug.LogError("playerID = " + playerID + " does not exist in PlayerDataDitc");
            return -1;
        }
    }

    public int GetScore(int playerID) {
        if (PlayerDataDict.ContainsKey(playerID)) {
            return PlayerDataDict[playerID].Score;
        }
        else {
            Debug.LogError("playerID = " + playerID + " does not exist in PlayerDataDitc");
            return -1;
        }
    }

    public void AddPlayer(int playerID, int clientID, GameObject shipObj, string playerName) {
        if (PlayerDataDict.ContainsKey(playerID)) {
            Debug.LogWarning("PlayerDataDict already contains player data for playerID = " + playerID);
            return;
        }
        PlayerData playerData = new PlayerData(playerName, initialHealth, 0, 0);
        PlayerDataDict.Add(playerID, playerData);
        ClientsDict.Add(clientID, playerID);
        PlayersDict.Add(playerID, clientID);

        if (PlayerShipsDict.ContainsKey(playerID)) {
            Debug.LogWarning("PlayerShipsDict already contains player ship with playerID = " + playerID);
            return;
        }
        PlayerShipsDict.Add(playerID, shipObj.GetComponent<PlayerShip>());
        Debug.Log("ship was added to PlayerShipsDict for playerID = " + playerID);

        Debug.Log("Adding Player to PlayerDataDict. playerID = " + playerID + ", clientID = " + clientID + ", name = " + playerName);

        scoreBoard.RefreshScoreBoard();
    }

    public void RemovePlayer(int playerID) {
        ClientsDict.Remove(GetClientID(playerID));
        PlayersDict.Remove(playerID);
        PlayerDataDict.Remove(playerID);
        PlayerShipsDict.Remove(playerID);
        Debug.Log("Removed player with playerID = " + playerID + " from the Game Manager.");
    }

    // called on server when updating, and on client when receiving SC_PlayerData message from server
    // currently used from client only. (server uses more specific functions)
    public void UpdatePlayerData(int playerID, int health, int score, int deaths) {
        // Update PlayerDataDict
        if (PlayerDataDict.ContainsKey(playerID)) {
            PlayerDataDict[playerID].Health = health;
            PlayerDataDict[playerID].Score = score;
            PlayerDataDict[playerID].Deaths = deaths;
        }
        else {
            Debug.LogError("UpdatePlayerData: playerID = " + playerID + " does not exist in PlayerDataDict");
        }
        // Update PlayerShip Fields
        if (!isServer) {
            if (!PlayerShip.ActiveShip) // Active Ship yet to be initiated.
                return;
  
            if (PlayerShip.ActiveShip.PlayerID == playerID) {
                // if local ship got damaged, make a shake effect
                int delta_health = health - PlayerShip.ActiveShip.Health;
                if (delta_health < 0) {
                    PlayerShip.ActiveShip.ShakeCamera(-delta_health);
                }
                PlayerShip.ActiveShip.Health = health;
                PlayerShip.ActiveShip.Score = score;
                PlayerShip.ActiveShip.Deaths = deaths;
                Debug.Log("Updating local player ship data: health = " + health + " , score = " + score);
            }
            else {
                PlayerShip ship = GetShip(playerID);
                ship.Health = health;
                ship.Score = score;
                ship.Deaths = deaths;
                Debug.Log("Updating remote player ship data: health = " + health + " , score = " + score);
            }
        }
        else { // Server
            SendPlayerDataToClients(playerID);
        }

    }

    // called on server only
    public void UpdatePlayerHealth(int playerID, int health) {
        if (PlayerDataDict.ContainsKey(playerID)) {
            GetShip(playerID).Health = health;
            PlayerDataDict[playerID].Health = health;
            SendPlayerDataToClients(playerID);
            Debug.Log("player: " + playerID + " health update:" + PlayerDataDict[playerID].Health);
        }
        else {
            Debug.LogError("playerID = " + playerID + " does not exist in PlayerDataDict");
        }

        if (health == 0) {
            KillPlayer(playerID);
        }
    }

    public void AddKillCredit(int killerID, int victimID, byte weapon) {
        if (killerID > 0 && killerID != victimID) {
            lastKillCredit = string.Format("{0} killed {1}", GetName(killerID), GetName(victimID));
        }
        else if (killerID == victimID) {
            lastKillCredit = string.Format("{0} killed himself", GetName(victimID));
        }
        else if (killerID <= 0) {
            lastKillCredit = string.Format("{0} crashed", GetName(victimID));
        }

        lastKillCreditTime = Time.time;

        if (PlayerShip.ActiveShip.PlayerID == victimID) {
            localPlayersKiller = GetName(killerID); // for the death\respawn screen of the killed player
        }

        //TODO: use the weapon type info to show a matching icon in the kill message
    }

    public string GetKillCreditText() {
        if (Time.time < lastKillCreditTime + killCreditTimeout) {
            return lastKillCredit;
        }
        return "";
    }

    // called on server only
    public void AddScore(int playerID) {
        if (PlayerDataDict.ContainsKey(playerID)) {
            GetShip(playerID).Score += killScorePoints;
            PlayerDataDict[playerID].Score += killScorePoints;
            Debug.Log("player: " + playerID + " got " + killScorePoints + " score points, now has: " + PlayerDataDict[playerID].Score);
            SendPlayerDataToClients(playerID);
        }
        else {
            Debug.LogError("playerID = " + playerID + " does not exist in PlayerDataDict");
        }
        scoreBoard.RefreshScoreBoard();
    }

    public void SendAllGameDataToNewClient(int clientID) {

        foreach (KeyValuePair<int, PlayerData> kvp in PlayerDataDict) {
            SC_PlayerData playerDataMessage = new SC_PlayerData(kvp.Key, Time.time, kvp.Value.Health, kvp.Value.Score, kvp.Value.Deaths);
            serverController.SendMsgToClient(clientID, playerDataMessage);
            //Debug.Log("SendAllGameDataToNewClient: sending data of clientID = " + clientID);
        }
    }

    public void KillPlayer(int playerID) {
        PlayerShip ship = GetShip(playerID);
        if (!ship) {
            Debug.LogError("can't get ship of playerID = " + playerID);
        }

        Debug.Log("Player with playerID = " + playerID + " killed.");
        if (isServer) {
            serverController.RespawnPlayer(GetClientID(playerID));
            // Score is added where the hit is detected (target script and missile script)
            PlayerDataDict[playerID].Deaths++;
            ship.Deaths = PlayerDataDict[playerID].Deaths;
            SendPlayerDataToClients(playerID);
            // kill msg is sent through the TakeDamage func
        }
        else {  // client
            ship.RespawnOnClientStart();
            if (PlayerShip.ActiveShip.PlayerID == playerID) {
                Debug.Log("LOCAL PLAYER DIED");
                localPlayerLockCounter = 0; //disable all lock warnnings 
                clientUI.EnableRespawnScreen();
            }
        }
        scoreBoard.RefreshScoreBoard();
    }

    public string GetName(int playerID) {
        if (GetShip(playerID)) {
            return GetShip(playerID).PlayerName;
        }
        Debug.LogError("playerID " + playerID + " wasn't found in GetName()");
        return "";
    }

    public PlayerShip GetShip(int playerID) {
        if (!PlayerShipsDict.ContainsKey(playerID)) {
            return null;
        }

        if (isServer) {
            return serverController.GetShipOnServer(playerID);
        }
        else { // Client
            if (!PlayerShipsDict.ContainsKey(playerID)) {
                Debug.LogError("playerID = " + playerID + " does not exist in PlayerShipsDict");
                return null;
            }
            return PlayerShipsDict[playerID];
        }
    }

    public bool IsValidPlayerID(int playerID) {
        if (PlayersDict.ContainsKey(playerID)) {
            return true;
        }
        return false;
    }

    // Called on server only
    private void SendPlayerDataToClients(int playerID) {
        SC_PlayerData playerDataMessage = new SC_PlayerData(playerID, Time.time, PlayerDataDict[playerID].Health, PlayerDataDict[playerID].Score, PlayerDataDict[playerID].Deaths);
        serverController.EnqueueReliable(playerDataMessage);
    }

    // Called on server only
    public void SendPlayerKilledMsg(int killerID, int victimID, byte weapon) {
        MSG_PlayerKilled playerKilledMsg = new MSG_PlayerKilled(killerID, victimID, weapon, Time.time);
        serverController.EnqueueReliable(playerKilledMsg);
    }

}
