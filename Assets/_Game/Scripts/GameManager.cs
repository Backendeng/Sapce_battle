using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    private GameObject networkControllerObj;
    private Client clientController;
    private Server serverController;

    private bool isServer;
    private readonly int initialHealth = 100;

    private Dictionary<int, PlayerData> PlayerDataDict = new Dictionary<int, PlayerData>();
    public Dictionary<int, PlayerShip> PlayerShipsDict = new Dictionary<int, PlayerShip>(); // For Client Use Only
    private Dictionary<int, int> ClientsDict = new Dictionary<int, int>(); // (ClientID, PlayerID)
    private Dictionary<int, int> PlayersDict = new Dictionary<int, int>(); // (PlayerID, ClientID)

    private int localPlayerLockCounter = 0;
    public int LocalPlayerLockCounter { set { localPlayerLockCounter = value; } get { return localPlayerLockCounter; } }

    private class PlayerData {
        public int Health { get; set; }
        public int Score { get; set; }
        public PlayerData(int health, int score) { Health = health; Score = score; }
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

    public void AddPlayerData(int playerID, int clientID) {
        if (PlayerDataDict.ContainsKey(playerID)) {
            Debug.LogWarning("PlayerDataDict already contains player data for playerID = " + playerID);
            return;
        }
        PlayerData playerData = new PlayerData(initialHealth, 0);
        PlayerDataDict.Add(playerID, playerData);
        ClientsDict.Add(clientID, playerID);
        PlayersDict.Add(playerID, clientID);
        Debug.Log("Adding Player to PlayerDataDict. playerID = " + playerID + ", clientID = " + clientID);
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

    public void AddPlayerShip(int playerID, GameObject shipObj) {
        // Called On Client Only
        if (PlayerShipsDict.ContainsKey(playerID)) {
            Debug.LogWarning("PlayerShipsDict already contains player ship with playerID = " + playerID);
            return;
        }
        PlayerShipsDict.Add(playerID, shipObj.GetComponent<PlayerShip>());
        Debug.Log("ship was added to PlayerShipsDict for playerID = " + playerID);
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

        if (isServer && health == 0) {
            KillPlayer(playerID);
        }
    }

    // called on server only
    public void UpdatePlayerHealth(int playerID, int health) {
        if (PlayerDataDict.ContainsKey(playerID)) {
            PlayerDataDict[playerID].Health = health;

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
            // TODO: do not kill by destroying the player ship object. make new death logic and network message.
        }
        // TODO: respawn logic

    }


    }
