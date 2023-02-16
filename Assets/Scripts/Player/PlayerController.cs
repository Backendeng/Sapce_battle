using System;
using System.Collections;
using System.Collections.Generic;
using QFSW.QC;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace SpaceSim {
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour {
        
        public static PlayerController Instance { get; private set; }

        private new Camera camera;
        private new Rigidbody2D rigidbody;
        public Spacecraft Spacecraft { get; private set; }

        public float speed = 10.0f;
        public float zoomSpeed = 10.0f;
        public Vector2 maxZoom = new Vector2(5, 20);
        
        public bool allowStrafeControls = false;
        [DisableIf("@!allowStrafeControls")]
        public float strafeSpeed = 10.0f;

        public float boostMultiplier = 1.5f;

        [FoldoutGroup("Keys")]
        public KeyCode strafeKey = KeyCode.Space;

        [FoldoutGroup("Keys")]
        public KeyCode boostKey = KeyCode.LeftShift;
        
        [FoldoutGroup("Keys")]
        public KeyCode menuKey = KeyCode.Escape;

        [FoldoutGroup("Keys")]
        public KeyCode fireKey = KeyCode.Space;
        
        
        private void OnValidate() {
            camera = GetComponentInChildren<Camera>();
            rigidbody = GetComponent<Rigidbody2D>();
            Spacecraft = GetComponent<Spacecraft>();
        }

        private void Awake() {
            if(Instance != null) {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }

        private void Start() {
            OnValidate();
            
            Spacecraft.OnDeath += () => {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); //Reload scene on death
            };
        }

        //Move the charactor with WASD
        //Zoom in and out with the mouse wheel
        private void Update() {
            ThrustControl();
            RotationControl();
            CameraControl();
            WeaponControl();
            
            if(Input.GetKeyDown(menuKey)) {
                SceneManager.LoadScene(0);
            }
        }

        private void ThrustControl() {
            float verticalInput = Input.GetAxis("Vertical");
            float horizontalInput = Input.GetAxis("Horizontal");
            bool isTryingToStrafe = Input.GetKey(strafeKey);
            bool isTryingToBoost = Input.GetKey(boostKey);
            
            if (isTryingToStrafe && allowStrafeControls) { //Is strafing
                Vector3 horizontalMovement = transform.right * horizontalInput * speed * Time.deltaTime;
                
                rigidbody.AddForce(horizontalMovement, ForceMode2D.Force); //move this to the proper spaceCraft controls
            } else { // Non strafe controls
                if (verticalInput < 0) { //Holding backwards
                    //Face the player away from velocity
                    Spacecraft.RotateAwayFrom2D(rigidbody.velocity);
                    return;
                }

                float speedMultiplier = isTryingToBoost ? boostMultiplier : 1;
                
                Vector3 forwardForce = verticalInput * transform.up;
                Spacecraft.ThrustInDirection(forwardForce, speedMultiplier);
                // rigidbody.AddForce(forwardForce * speed * speedMultiplier * Time.deltaTime, ForceMode2D.Force);
            }
        }

        private void RotationControl() {
            float horizontalInput = Input.GetAxis("Horizontal");
            bool isTryingToStrafe = Input.GetKey(strafeKey);
            
            if (!(isTryingToStrafe && allowStrafeControls)) { //Is not strafing
                Spacecraft.TurnInDirection2D(-horizontalInput);
            }
        }

        private void CameraControl() {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            camera.orthographicSize -= scrollInput * zoomSpeed;
            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, maxZoom.x, maxZoom.y);

            camera.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        private void WeaponControl() {
            if (Input.GetKey(fireKey)) {
                Spacecraft.FireSelectedWeapon();
            }
            
            if(Input.GetKeyDown(KeyCode.Tab)) {
                Spacecraft.SelectNextWeapon();
            }
        }
        
        [Command("player.setArmor", MonoTargetType.All)]
        private void ChangePlayerArmor(float newArmor) {
            //if this is above max, increase max
            
            if(newArmor > Spacecraft.MaxArmor) {
                Spacecraft.MaxArmor = newArmor;
            }
            
            Spacecraft.Armor = newArmor;
        }
        
        [Command("player.setShields", MonoTargetType.All)]
        private void ChangePlayerShields(float newShields) {
            //if this is above max, increase max
            
            if(newShields > Spacecraft.MaxShield) {
                Spacecraft.MaxShield = newShields;
            }
            
            Spacecraft.Shield = newShields;
        }
        
        [Command("player.setFuel", MonoTargetType.All)]
        private void ChangePlayerFuel(float newFuel) {
            //if this is above max, increase max
            
            if(newFuel > Spacecraft.MaxFuel) {
                Spacecraft.MaxFuel = newFuel;
            }
            
            Spacecraft.Fuel = newFuel;
        }
        
        [Command("world.KillAllEnemies", MonoTargetType.All)]
        private void KillAllEnemies() {
            foreach (var enemy in FindObjectsOfType<EnemySpacecraft>()) {
                enemy.Die();
            }
        }
    }
}