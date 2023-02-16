using System;
using Sirenix.OdinInspector;
using SpaceSim;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour {
    [InfoBox("Projectile sprite should be facing up")]
    
    
    [FormerlySerializedAs("characteristics")]
    public ProjectileCharacteristics Characteristics;
    
    public Rigidbody2D RigidBody { get; private set; }
    [HideInInspector] public Spacecraft FiringSpacecraft;
    
    [Header("Timings")]
    [SerializeField] private float lifeTime = 10;
    [SerializeField] private float armTime = 0.1f;
    [SerializeField] private bool canHitOwner = false;
    private float birthTime = float.MinValue;

    private void Awake() {
        RigidBody = GetComponent<Rigidbody2D>();
    }

    void Start() {
        birthTime = Time.time;
        Destroy(gameObject, lifeTime);
        
        //RigidBody.useAutoMass = false;
        //RigidBody.mass = 0;
        //RigidBody.velocity += (Vector2)transform.up * InitialImpulseSpeed;
        
        foreach (ProjectileFlightPattern projectileFlightPattern in Characteristics.FlightPattern) {
            projectileFlightPattern.OnFire(this, FiringSpacecraft);
        }
    }

    private void Update() {
        foreach (ProjectileFlightPattern projectileFlightPattern in Characteristics.FlightPattern) {
            projectileFlightPattern.Fly(this, FiringSpacecraft);
        }
    }

    private void OnCollisionEnter2D(Collision2D col) {
        if (birthTime + armTime > Time.time) {
            Debug.DrawRay(col.contacts[0].point, col.contacts[0].normal, Color.green, 5); // Draw a green line as it is not armed
            return;
        }
        
        
        //Try to find if the object has a spacecraft component
        Spacecraft spacecraft = col.gameObject.GetComponent<Spacecraft>();
        if (spacecraft != null) {
            if(spacecraft == FiringSpacecraft && !canHitOwner) return;
            
            foreach (ProjectileOnHitEffect characteristicsEffect in Characteristics.OnHitEffects) {
                characteristicsEffect.ApplyEffect(this, spacecraft, FiringSpacecraft);
            }
        }
        
        // Draw the debug location
        // Debug.DrawRay(col.contacts[0].point, col.contacts[0].normal, Color.red, 5);
        
        Detonate();
    }
    
    public void SetupProjectile(Spacecraft firingSpacecraft) {
        FiringSpacecraft = firingSpacecraft;
    }

    //Used to make the projectile blow up
    private void Detonate() {
        //Spawn the explosion
        if (Characteristics.ExplosionAnimation != null) {
            GameObject explosion = Instantiate(Characteristics.ExplosionAnimation, transform.position, transform.rotation);
            //explosion.transform.localScale = transform.localScale;
        }
        
        Destroy(gameObject);
    }

    public float PredictSpeedGivenImpulse(Vector2 offsetVelocity, Vector2 direction) {
        Vector2 velocity = offsetVelocity + direction * Characteristics.FlightPattern[0]!.velocity;
        return (velocity).magnitude;
    }
}
