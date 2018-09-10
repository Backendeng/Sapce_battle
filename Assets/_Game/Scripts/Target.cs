using UnityEngine;

public class Target : MonoBehaviour {
    private Server serverController;
    private GameManager gameManager;
    private PlayerShip playerShip;
    private int clientID = -1; // default value for non-client network entities with a target script
    private int playerID;
    private readonly float collisionDamageFactor = 2f;
    private float collisionCooldown = 1f;
    private float nextCollisionDamage;
    private float minCollisionSpeed = 20f;

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

    public void TakeDamage(int damage, int hitterID = 0) {
        int health = playerShip.Health;
        if (health == 0) {
            return;
        }
        Debug.Log("damage: " + damage);
        health = Mathf.Clamp(health - damage, 0, 100);
        playerShip.Health = health;
        gameManager.UpdatePlayerHealth(playerID, health);

        if (playerShip.Health == 0 && hitterID != playerShip.PlayerID && gameManager.IsValidPlayerID(hitterID)) {
            gameManager.AddScore(hitterID);
            // TODO: send kill message to clients
            gameManager.SendPlayerKilledMsg(hitterID, playerShip.PlayerID, 1); // TODO: change 1 to weapon type
            gameManager.AddKillCredit(hitterID, playerShip.PlayerID, 1);
        }
    }

    void OnTriggerEnter(Collider other) {
        // hit by a projectile of another player
        if (other.CompareTag("Projectile") /* && other.GetComponent<Projectile>().OwnerID != playerID */) {
            Debug.Log("Target was hit by a projectile: playerID = " + playerID + ", clientID = " + clientID);
            TakeDamage(Projectile.Damage, other.GetComponent<Projectile>().OwnerID);
            return;
        }

        if (other.CompareTag("SpaceStructure") && Time.time > nextCollisionDamage) {
            float speed = playerShip.Velocity.magnitude;
            if (speed > minCollisionSpeed) {
                TakeDamage((int)(speed * collisionDamageFactor));
                nextCollisionDamage = Time.time + collisionCooldown;
                Debug.Log("the ship hit a structure:" + other.name + ", tag: " + other.tag + " speed = " + playerShip.Velocity.magnitude + ", damage: " + (int)(playerShip.Velocity.magnitude * collisionDamageFactor));
                return;
            }
        }

        //Debug.Log("Target was hit " + other.name + ", tag: " + other.tag);
    }
}
