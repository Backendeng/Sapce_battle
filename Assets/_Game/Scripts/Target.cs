using UnityEngine;

public class Target : MonoBehaviour {
    private Server serverController;
    private int clientID;

    public float health = 100f;
    
    public void Init(Server serverController, int clientID) {
        this.serverController = serverController;
        this.clientID = clientID;
    }

    public void TakeDamage(float damage) {

    }

    void OnTriggerEnter(Collider other) {

    }
}
