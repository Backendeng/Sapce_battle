using UnityEngine;

public class Target : MonoBehaviour {
    private Server serverController;
    private GameManager gameManager;
    private PlayerShip playerShip;
    private int clientID = -1; // default value for non-client network entities with a target script
    private int entityID;

    public void Start() {
        playerShip = GetComponentInParent<PlayerShip>();
        serverController = playerShip.ServerController;
        clientID = playerShip.ClientID;
        entityID = playerShip.EntityID;
        gameManager = serverController.gameManager;

        if (!playerShip || !serverController || !gameManager) {
            Debug.LogError("target component is missing some object refs.");
        }
    }

    public void TakeDamage(int damage) {
        int health = playerShip.Health;
        if (health > 0) {
            health = Mathf.Clamp(health - damage, 0, 100);
            playerShip.Health = health;
            gameManager.UpdatePlayerHealth(clientID, health);
        }
    }

    void OnTriggerEnter(Collider other) {
        // hit by a projectile of another player
        if (other.CompareTag("Projectile") && other.GetComponent<Projectile>().ClientID != clientID) {
            Debug.Log("Target was hit: entityID = " + entityID + ", clientID = " + clientID);
            TakeDamage(Projectile.Damage);
        }
    }
}
