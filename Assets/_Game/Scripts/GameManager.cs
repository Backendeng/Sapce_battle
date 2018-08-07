using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    private GameObject networkControllerObj;
    private Client clientController;
    private Server serverController;
    public GameObject RespawnMenu;

    private bool isServer;
    private readonly int initialHealth = 100;

    private Dictionary<int, PlayerData> PlayerDataDict = new Dictionary<int, PlayerData>();
    public Dictionary<int, PlayerShip> PlayerShipsDict = new Dictionary<int, PlayerShip>(); // For Client Use Only
    private Dictionary<int, int> ClientsDict = new Dictionary<int, int>(); // (ClientID, PlayerID)
    private Dictionary<int, int> PlayersDict = new Dictionary<int, int>(); // (PlayerID, ClientID)

    private int localPlayerLockCounter = 0;
    public int LocalPlayerLockCounter { set { localPlayerLockCounter = value; } get { return localPlayerLockCounter; } }

    private class PlayerData {
        public string Name { get; set; }
        public int Health { get; set; }
        public int Score { get; set; }
        public PlayerData(string name, int health, int score) { Name = name; Health = health; Score = score; }
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
        PlayerData playerData = new PlayerData(playerName, initialHealth, 0);
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
    }

    public void RemovePlayer(int playerID) {
        ClientsDict.Remove(GetClientID(playerID));
        PlayersDict.Remove(playerID);
        if (isServer) {
            PlayerDataDict.Remove(playerID);
        }
        else {
            PlayerDataDict.Remove(playerID);
            PlayerShipsDict.Remove(playerID);
        }
        Debug.Log("Removed player with playerID = " + playerID + " from the Game Manager.");
    }

    // called on server when updating, and on client when receiving SC_PlayerData message from server
    public void UpdatePlayerData(int playerID, int health, int score) {
        // Update PlayerDataDict
        if (PlayerDataDict.ContainsKey(playerID)) {
            PlayerDataDict[playerID].Health = health;
            PlayerDataDict[playerID].Score = score;
        }
        else {
            Debug.LogError("UpdatePlayerData: playerID = " + playerID + " does not exist in PlayerDataDict");
            /*
            PlayerData playerData = new PlayerData(health, score);
            PlayerDataDict.Add(playerID, playerData);
            Debug.Log("UpdatePlayerData: Adding Player to PlayerDataDict. playerID = " + playerID);
            */
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
                Debug.Log("Updating local player ship data: health = " + health + " , score = " + score);
            }
            else {
                if (!PlayerShipsDict.ContainsKey(playerID)) {
                    Debug.LogWarning("PlayerShipsDict doesn't contain the ship of playerID = " + playerID);
                    return;
                }
                PlayerShip ship = PlayerShipsDict[playerID];
                ship.Health = health;
                ship.Score = score;
                Debug.Log("Updating remote player ship data: health = " + health + " , score = " + score);
            }
        }

        if (health == 0) {
            KillPlayer(playerID);
        }
    }

    // called on server only
    public void UpdatePlayerHealth(int playerID, int health) {
        if (PlayerDataDict.ContainsKey(playerID)) {
           
            PlayerDataDict[playerID].Health = health; // player ships also hold Health
            Debug.Log(playerID + "send health update" + PlayerDataDict[playerID].Health);
            SC_PlayerData playerDataMessage = new SC_PlayerData(playerID, Time.time, health, PlayerDataDict[playerID].Score);
            serverController.EnqueueReliable(playerDataMessage);
        }
        else {
            Debug.LogError("playerID = " + playerID + " does not exist in PlayerDataDict");
        }

        if (health == 0) {
            KillPlayer(playerID);
        }
    }

    public void SendAllGameDataToNewClient(int clientID) {

        foreach (KeyValuePair<int, PlayerData> kvp in PlayerDataDict) {
            SC_PlayerData playerDataMessage = new SC_PlayerData(kvp.Key, Time.time, kvp.Value.Health, kvp.Value.Score);
            serverController.SendMsgToClient(clientID, playerDataMessage);
            //Debug.Log("SendAllGameDataToNewClient: sending data of clientID = " + clientID);
        }
    }

    public void KillPlayer(int playerID) {
        Debug.Log("Player with playerID = " + playerID + " killed.");
        if (isServer) {
            serverController.RespawnPlayer(PlayersDict[playerID]);
            // TODO: update score
        }
        else {  // client
            PlayerShip ship = PlayerShipsDict[playerID];
            ship.RespawnOnClientStart();
            if (PlayerShip.ActiveShip.PlayerID == playerID) {
                Debug.Log("LOCAL PLAYER DIED");
                localPlayerLockCounter = 0; //disable all lock warnnings 
              
                RespawnMenu.SetActive(true); // activate RespawnMenu
            }
        }
    }

    public string GetName(int playerID) {
        if (!PlayerShipsDict.ContainsKey(playerID)) {
            Debug.LogError("playerID = " + playerID + " does not exist in PlayerShipsDict");
        }
        return PlayerShipsDict[playerID].PlayerName;
    }


}
