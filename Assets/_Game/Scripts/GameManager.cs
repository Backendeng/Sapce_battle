using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    private GameObject networkControllerObj;
    private Client clientController;
    private Server serverController;
    //public LocalPlayerShip localPlayerShip;
    private bool isServer;

    private int initialHealth = 100;

    

    private class PlayerData {
        public int Health { get; set; }
        public int Score { get; set; }
        public PlayerData(int health, int score) { Health = health; Score = score; }
    };

    private Dictionary<int, PlayerData> PlayerDataDict = new Dictionary<int, PlayerData>();
    Dictionary<int, PlayerShip> PlayerShipsDict = new Dictionary<int, PlayerShip>(); // For Client Use Only

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

        if (!isServer) {

        }
    }

    public int GetHealth(int clientID) {
        if (PlayerDataDict.ContainsKey(clientID)) {
            return PlayerDataDict[clientID].Health;
        }
        else {
            Debug.LogError("client id = " + clientID + " does not exist in PlayerDataDitc");
            return -1;
        }
    }

    public int GetScore(int clientID) {
        if (PlayerDataDict.ContainsKey(clientID)) {
            return PlayerDataDict[clientID].Score;
        }
        else {
            Debug.LogError("client id = " + clientID + " does not exist in PlayerDataDitc");
            return -1;
        }
    }

    public void AddPlayerData(int clientID) {
        // Called On Server Only
        if (PlayerDataDict.ContainsKey(clientID)) {
            Debug.LogWarning("PlayerDataDict already contains player data with client id = " + clientID);
            return;
        }
        PlayerData playerData = new PlayerData(initialHealth, 0);
        PlayerDataDict.Add(clientID, playerData);
    }

    public void AddPlayerShip(int clientID, GameObject shipObj) {
        // Called On Client Only
        if (PlayerShipsDict.ContainsKey(clientID)) {
            Debug.LogWarning("PlayerShipsDict already contains player ship with client id = " + clientID);
            return;
        }
        PlayerShipsDict.Add(clientID, shipObj.GetComponent<PlayerShip>());
        Debug.Log("ship was added to PlayerShipsDict for client id = " + clientID);
    }

    // called on server when updating, and on client when receiving SC_PlayerData message from server
    public void UpdatePlayerData(int clientID, int health, int score) {
        if (PlayerDataDict.ContainsKey(clientID)) {
            PlayerDataDict[clientID].Health = health;
            PlayerDataDict[clientID].Score = score;
        }
        else {
            PlayerData playerData = new PlayerData(health, score);
            PlayerDataDict.Add(clientID, playerData);
        }

        if (!isServer) {
            if (!PlayerShip.ActiveShip) // Active Ship yet to be initiated.
                return;

            if (PlayerShip.ActiveShip.ClientID == clientID) {
                PlayerShip.ActiveShip.Health = health;
                PlayerShip.ActiveShip.Score = score;
                Debug.Log("Updating local player ship data: health = " + health + " , score = " + score);
            }
            else {
                if (!PlayerShipsDict.ContainsKey(clientID)) {
                    Debug.LogWarning("PlayerShipsDict doesn't contain the ship of client id = " + clientID);
                    return;
                }
                PlayerShip ship = PlayerShipsDict[clientID];
                ship.Health = health;
                ship.Score = score;
                Debug.Log("Updating remote player ship data: health = " + health + " , score = " + score);
            }
        }

        if (isServer && health == 0) {
            KillPlayer(clientID);
        }
    }

    // called on server only
    public void UpdatePlayerHealth(int clientID, int health) {
        if (PlayerDataDict.ContainsKey(clientID)) {
            PlayerDataDict[clientID].Health = health;

            SC_PlayerData playerDataMessage = new SC_PlayerData(clientID, Time.time, health, PlayerDataDict[clientID].Score);
            serverController.EnqueueReliable(playerDataMessage);
        }
        else {
            Debug.LogError("client id = " + clientID + " does not exist in PlayerDataDitc");
        }

        if (health == 0) {
            KillPlayer(clientID);
        }
    }

    public void SendAllGameDataToNewClient(int clientID) {

        foreach (KeyValuePair<int, PlayerData> kvp in PlayerDataDict) {
            SC_PlayerData playerDataMessage = new SC_PlayerData(kvp.Key, Time.time, kvp.Value.Health, kvp.Value.Score);
            serverController.EnqueueReliable(playerDataMessage);
        }
    }

    public void KillPlayer(int clientID) {
        Debug.Log("Player with client id = " + clientID + " killed.");
        if (isServer) {
            // TODO: do not kill by destroying the player ship object. make new death logic and network message.
        }
        // TODO: respawn logic

    }


}
