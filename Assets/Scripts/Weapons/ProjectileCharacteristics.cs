using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace SpaceSim {
    
    [CreateAssetMenu(fileName = "New Projectile Characteristics", menuName = "SpaceSim/ProjectileCharacteristics")]
    public class ProjectileCharacteristics : ScriptableObject{
        [FormerlySerializedAs("onHitEffects")]
        [SerializeReference] 
        public List<ProjectileOnHitEffect> OnHitEffects = new List<ProjectileOnHitEffect>();
        
        [FormerlySerializedAs("flightPattern")] 
        [SerializeReference] 
        public List<ProjectileFlightPattern> FlightPattern = new List<ProjectileFlightPattern>();

        public GameObject ExplosionAnimation;

    }

    [Serializable]
    public class ProjectileOnHitEffect {
        public virtual void ApplyEffect(Projectile projectile, Spacecraft victim, Spacecraft owner) {
            Debug.Log($"Projectile On Hit Effect '{GetType().Name}' Not Implemented But Called");
        }
    }
    
    [Serializable]
    public class ProjectileFlightPattern {
        public float velocity;
        
        public virtual void Fly(Projectile projectile, Spacecraft owner) {
            Debug.Log($"Projectile Flight Pattern [Fly] '{GetType().Name}' Not Implemented But Called");
        }
        
        public virtual void OnFire(Projectile projectile, Spacecraft owner) {
            Debug.Log($"Projectile Flight Pattern [Fire] '{GetType().Name}' Not Implemented But Called");
        }
    }
    
    [Serializable]
    public class DoDamage : ProjectileOnHitEffect {
        [SerializeField] 
        private float damage = 25;
        
        public override void ApplyEffect(Projectile projectile, Spacecraft victim, Spacecraft owner) {
            victim.TakeDamage(damage);
        }
    }
    
    [Serializable]
    public class ApplyForce : ProjectileOnHitEffect {
        public float force;
    }
    
    [Serializable]
    public class Heal : ProjectileOnHitEffect {
        public float heal;
    }
    
    [Serializable]
    public class DoAoeDamage : ProjectileOnHitEffect {
        public float damage;
        public float radius;
    }
    
    [Serializable]
    public class Homing : ProjectileFlightPattern {
        public float turnSpeed;
        public float searchDistance = 50;
        public float thrusterPower = 0.2f;
        public float maxVelocity = 5;

        public override void Fly(Projectile projectile, Spacecraft owner) {
            //Search for the nearest enemy in front of projectile within 100 units

            Spacecraft nearestEnemy = null;
            float nearestDistance = float.MaxValue;
            List<Spacecraft> allNearbySpaceCraft = Physics2D.OverlapCircleAll(projectile.transform.position, searchDistance)
                .Select(c => c.GetComponent<Spacecraft>())
                .Where(c => c != null)
                .ToList();

            foreach (Spacecraft spacecraft in allNearbySpaceCraft) {
                if (spacecraft == owner) continue;
                if (spacecraft.Team == owner.Team) continue;
                float distance = Vector3.Distance(projectile.transform.position, spacecraft.transform.position);
                if (distance < nearestDistance) {
                    nearestDistance = distance;
                    nearestEnemy = spacecraft;
                }
            }
            
            if(nearestEnemy == null) return;
            
            //Point at the nearest enemy
            Vector3 direction = nearestEnemy.transform.position - projectile.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle-90f, Vector3.forward);
            projectile.transform.rotation = Quaternion.Slerp(projectile.transform.rotation, rotation, turnSpeed * Time.deltaTime);

            float maxSpeed = maxVelocity;

            //Apply force towards the nearest enemy
            projectile.RigidBody.AddForce(projectile.transform.up * thrusterPower * Time.time, ForceMode2D.Force);
            
            projectile.RigidBody.velocity = Vector3.ClampMagnitude(projectile.RigidBody.velocity, maxSpeed);
            
        }


        public override void OnFire(Projectile projectile, Spacecraft owner) {
            projectile.RigidBody.velocity += (Vector2)projectile.transform.up * velocity;
        }
    }
    
    [Serializable]
    public class InstantVelocityForward : ProjectileFlightPattern {
        
        public override void Fly(Projectile projectile, Spacecraft owner) {
            
        }
        
        public override void OnFire(Projectile projectile, Spacecraft owner) {
            projectile.RigidBody.velocity += (Vector2)projectile.transform.up * velocity;
        }
    }
    
    
}