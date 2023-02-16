using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace SpaceSim {
    
    [RequireComponent(typeof(Rigidbody2D))]
    public class Spacecraft : MonoBehaviour{
        #region Ship Status

        public int Team = 0; //0 is player team
        public float MaxShield = 100;
        protected float _shield;
        [ShowInInspector] [ProgressBar(0, "MaxShield", 0.1f,0.65f,0.95f)]
        public float Shield {
            get => _shield;
            set {
                float oldShield = _shield;
                _shield = Mathf.Clamp(value, 0, MaxShield);
                OnShieldChanged?.Invoke(_shield, MaxShield, oldShield < _shield);
            }
        }
        
        /// <summary>
        /// New Value, Max Value, Went up?
        /// </summary>
        public event Action<float, float, bool> OnShieldChanged;
        

        public float MaxArmor = 100;
        protected float _armor = 100;
        [ShowInInspector] [ProgressBar(0, "MaxArmor", (97f/255f),(204f/255f),(65f/255f))]
        public float Armor {
            get => _armor;
            set {
                float oldArmor = _armor;
                _armor = Mathf.Clamp(value, 0, MaxArmor);
                if (_armor <= 0) {
                    _armor = 0;
                    Die();
                }
                
                OnArmorChanged?.Invoke(_armor, MaxArmor, oldArmor < _armor);
            }
        }

        /// <summary>
        /// New Value, Max Value, Went up?
        /// </summary>
        public event Action<float, float, bool> OnArmorChanged;


        public float MaxFuel = 100;
        protected float _fuel = 100;
        
        [ShowInInspector] [ProgressBar(0, "MaxFuel", 1f, 1f, 0.25f)]
        public float Fuel {
            get => _fuel;
            set {
                float oldFuel = _fuel;
                _fuel = Mathf.Clamp(value, 0, MaxFuel);
                OnFuelChanged?.Invoke(_fuel, MaxFuel, oldFuel < _fuel);
            }
        }
        
        /// <summary>
        /// New Value, Max Value, Went up?
        /// </summary>
        public event Action<float, float, bool> OnFuelChanged;
        
    #endregion

    #region ShipSettings

    [Range(0.1f, 360f) ]
    public float RotationSpeedPerSecond = 30;
    
    //Movement in units per second
    public float FreeMovementUnitsPerSecond = 4;
    
    //Movement in units per second
    public float ThrustForce = 1;

    #endregion

    #region Ship Events
        
        public event Action OnDeath;
        public event Action OnSpawn;
        public event Action OnThrusterEngaged;
        public event Action OnManeuverEngaged;
        public event Action OnRotationEngaged;

        #endregion
        
        protected List<ShipWeapon> weapons = new List<ShipWeapon>();
        protected ShipWeapon selectedWeapon;
        
        [HideInInspector] public Rigidbody2D rigidbody2D;

        // ================================================== //
        
        protected virtual void OnValidate() {
            Armor = MaxArmor;
            Shield = MaxShield;
            Fuel = MaxFuel;
        }

        protected virtual void Awake() {
            OnValidate();
            weapons.AddRange(GetComponentsInChildren<ShipWeapon>());
            
            rigidbody2D = GetComponent<Rigidbody2D>();
            if(weapons.Count > 0) selectedWeapon = weapons[0];
        }

        protected virtual void Start() {
            OnSpawn?.Invoke();
        }

        
        // ================================================== //

    #region Movement Controls

        //Freely move the craft towards a direction without using rigidbody. Does not affect velocity
        public virtual void MoveInDirection(Vector2 direction, float speedMultiplier = 1) {
            direction = direction.normalized;
            
            transform.Translate(direction * speedMultiplier * FreeMovementUnitsPerSecond * Time.deltaTime);
            OnManeuverEngaged?.Invoke();
        }
        
        //Apply a force to the rigidbody
        public virtual void ThrustInDirection(Vector2 direction, float speedMultiplier = 1) {
            if(direction.magnitude < 0.01f) return;
            direction = direction.normalized;
            
            rigidbody2D.AddForce(direction * speedMultiplier * ThrustForce, ForceMode2D.Force);
            OnThrusterEngaged?.Invoke();
        }
        
        public virtual void ThrustForward(float speedMultiplier = 1) {
            ThrustInDirection(transform.up, speedMultiplier);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="speedMultiplier"></param>
        /// <param name="errorMargin">The error margin in degrees. If the craft is within this margin, it will return true</param>
        /// <returns>If we are facing the target direction</returns>
        public virtual bool RotateTowards2D(Vector2 direction, float speedMultiplier = 1, float errorMargin = 0.00001f) {
            direction = direction.normalized;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q,  speedMultiplier * RotationSpeedPerSecond * Time.deltaTime);
            OnRotationEngaged?.Invoke();
            
            return Quaternion.Angle(transform.rotation, q) < errorMargin;
        }
        
        public virtual bool RotateAwayFrom2D(Vector2 direction, float speedMultiplier = 1, float errorMargin = 0.00001f) {
            return RotateTowards2D(-direction, speedMultiplier, errorMargin);
        }

        // Turn towards the given direction
        public virtual void TurnInDirection2D(float leftRight) {
            leftRight = Mathf.Clamp(leftRight, -1, 1);
            transform.Rotate(0, 0, leftRight * RotationSpeedPerSecond * Time.deltaTime);
            OnRotationEngaged?.Invoke();
        }
        
    #endregion
    
    
        [Button, FoldoutGroup("Debug")]
        public virtual void FireAllWeapons() {
            weapons.ForEach(w => w.FireWeapon(this));
        }
        
        public virtual void FireSelectedWeapon() {
            if(selectedWeapon != null) selectedWeapon.FireWeapon(this);
        }

        public virtual void SelectNextWeapon() {
            if (weapons.Count == 0) return;
            int index = weapons.IndexOf(selectedWeapon);
            index++;
            if (index >= weapons.Count) index = 0;
            selectedWeapon = weapons[index];
        }
        
        [Button, FoldoutGroup("Debug")]
        public virtual void TakeDamage(float damage){
            if (Shield > 0){
                Shield -= damage;
                if (Shield < 0){
                    Armor += Shield;
                    Shield = 0;
                }
            }
            else{
                Armor -= damage;
            }
        }
        
        [Button, FoldoutGroup("Debug")]
        public virtual void Refuel(float amount){
            Fuel += amount;
            if (Fuel > MaxFuel){
                Fuel = MaxFuel;
            }
        }
        
        [Button, FoldoutGroup("Debug")]
        public virtual void Repair(float amount){
            Armor += amount;
            if (Armor > MaxArmor){
                Armor = MaxArmor;
            }
        }
        
        [Button, FoldoutGroup("Debug")]
        public virtual void RechargeShield(float amount){
            Shield += amount;
            if (Shield > MaxShield){
                Shield = MaxShield;
            }
        }
        
        [Button, FoldoutGroup("Debug")]
        public virtual void Die(){
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
        
        [Button, FoldoutGroup("Debug")]
        public virtual void TakeSomeDamage() {
            TakeDamage(10);
        }
    }
}