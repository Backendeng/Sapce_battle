using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SpaceSim {
    
    [CreateAssetMenu(fileName = "ShipWeaponData", menuName = "SpaceSim/ShipWeaponData")]
    public class ShipWeaponData : ScriptableObject {
        [Required]
        public GameObject Projectile;

        [Range(0.1f, 10f)]
        public float WeaponCooldownTime = 2f;

        private void OnValidate() {
            if(Projectile != null && Projectile.GetComponent<Projectile>() == null && Projectile.GetComponent<Rigidbody2D>() == null) {
                Debug.LogError("Projectile does not have a Projectile component");
            }
        }
    }
}