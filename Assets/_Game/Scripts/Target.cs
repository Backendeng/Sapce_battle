using UnityEngine;

public class Target : MonoBehaviour {
    private Server serverController;
    private GameManager gameManager;
    private PlayerShip playerShip;
    private int clientID = -1; // default value for non-client network entities with a target script
    private int playerID;

    public void Start() {
        playerShip = GetComponentInParent<PlayerShip>();
        serverController = playerShip.ServerController;
        clientID = playerShip.ClientID;
        playerID = playerShip.PlayerID;
        gameManager = serverController.gameManager;

        if (!playerShip || !serverController || !gameManager) {
            Debug.LogError("target component is missing some object refs.");
        }
    }

    public void TakeDamage(int damage) {
        int health = playerShip.Health;
        Debug.Log("damg " + health +  " "+damage);
        if (health > 0) {
            health = Mathf.Clamp(health - damage, 0, 100);
            playerShip.Health = health;
            gameManager.UpdatePlayerHealth(playerID, health);
        }
    }

    void OnTriggerEnter(Collider other) {
        // hit by a projectile of another player
        if (other.CompareTag("Projectile") /* && other.GetComponent<Projectile>().OwnerID != playerID */) {
            Debug.Log("Target was hit by a projectile: playerID = " + playerID + ", clientID = " + clientID);
            TakeDamage(Projectile.Damage);
            return;
        }
        Debug.Log("Target was hit " + other.name + ", tag: " + other.tag);/////TODO: error
    }
}
