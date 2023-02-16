using System;
using UnityEngine;

namespace SpaceSim {
    
    [RequireComponent(typeof(Rigidbody2D))]
    public class SpaceRigidBody : MonoBehaviour {

        private Rigidbody2D rb;

        private void Awake() {
            rb = GetComponent<Rigidbody2D>();
        }

        private void OnValidate() {
            rb = GetComponent<Rigidbody2D>();
            SetupSettings();
        }

        public void SetupSettings() {
            rb.gravityScale = 1;
            rb.drag = 0;
            rb.angularDrag = 0.05f;
            rb.simulated = true;
            rb.useAutoMass = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.StartAwake;
            rb.interpolation = RigidbodyInterpolation2D.None;
        }
    }
}