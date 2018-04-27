using UnityEngine;

public class Target : MonoBehaviour {
    private Server serverController;
    private int clientID;

    private float health = 100f;
    public float Health { get { return health; } }
    
    public void Init(Server serverController, int clientID) {
        this.serverController = serverController;
        this.clientID = clientID;
    }

    public void TakeDamage(float damage) {
        health -= damage;
    }

    void OnTriggerEnter(Collider other) {
        // hit by a projectile of another player
        if (other.CompareTag("Projectile") && other.GetComponent<Projectile>().ClientID != clientID) {
            TakeDamage(10);
        }
    }
}
