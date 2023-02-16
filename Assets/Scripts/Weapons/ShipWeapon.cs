using Sirenix.OdinInspector;
using UnityEngine;

namespace SpaceSim {
    public class ShipWeapon : MonoBehaviour {
        
        [Required, SerializeField]
        public ShipWeaponData Data;
        private float lastFiredTime = 0f;
        
        public void FireWeapon(Spacecraft firingSpacecraft) {
            if (Time.time - lastFiredTime > Data.WeaponCooldownTime) {
                lastFiredTime = Time.time;
                GameObject projectile = Instantiate(Data.Projectile, transform.position, transform.rotation);
                projectile.GetComponent<Rigidbody2D>().velocity += firingSpacecraft.rigidbody2D.velocity; //Assign the velocity of the ship to the projectile
                projectile.GetComponent<Projectile>().SetupProjectile(firingSpacecraft);
            }
        }
        
        

    }
}