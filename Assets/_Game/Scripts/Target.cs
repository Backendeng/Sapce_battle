using UnityEngine;

public class Target : MonoBehaviour {
    private Server serverController;
    private int clientID = -1; // default value for non-client network entities with a target script
    private int entityID;

    private float health = 100f;
    public float Health { get { return health; } }
    
    public void Init(Server serverController, int entityID, int clientID) {
        this.serverController = serverController;
        this.entityID = entityID;
        this.clientID = clientID;
    }

    public void TakeDamage(float damage) {
        if (health > 0) {
            health = Mathf.Clamp(health - damage, 0f, 100f);
        }
    }

    void OnTriggerEnter(Collider other) {
        // hit by a projectile of another player
        if (other.CompareTag("Projectile") && other.GetComponent<Projectile>().ClientID != clientID) {
            Debug.Log("Target was hit: entityID = " + entityID + ", clientID = " + clientID);
            TakeDamage(10);

            if (health == 0) {
                Debug.Log("Target is dead!: entityID = " + entityID + ", clientID = " + clientID);

                // update game manager on server
            }
        }
    }
}
