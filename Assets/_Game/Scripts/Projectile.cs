using System.Collections;
using UnityEngine;

public class Projectile : NetworkEntity {
    public float speed;
    public GameObject projExplosion;
    public float timeout = 5.0f;

    public new void Start() {
        base.Start();
        if (isServer) {
            GetComponent<Rigidbody>().velocity = transform.forward * speed;
        }
    }
	
	private void Update ()
    {
		
	}

    private void Awake()
    {
        Destroy(gameObject, timeout);
    }

    public void OnBecameInvisible()
    {
        //Destroy(gameObject);
    }

    // when the projectile hits something
    void OnTriggerEnter(Collider other)
    {
        // ignore bullet to bullet collision
        if (other.GetComponent<Projectile>())
            return;

        // ignore collision with boundary or other projectiles
        if (other.name == "Boundary")
            return;
        if (other.GetComponent<Projectile>() )
            return;

        if (projExplosion)
            Instantiate(projExplosion, transform.position, transform.rotation);

        if (other.CompareTag("Player"))
        {
            // make hitting effects
            //Instantiate(playerExplosion, other.transform.position, other.transform.rotation);
        }
        //gameController.AddScore(scoreValue);

        //Destroy(gameObject);
    }

    private void OnDestroy() {
        
    }
}
