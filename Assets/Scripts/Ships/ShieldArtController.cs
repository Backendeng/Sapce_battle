using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SpaceSim {
    
    [RequireComponent(typeof(SpriteRenderer))]
    public class ShieldArtController : MonoBehaviour {
        [Required][SerializeField][HideIf("@spacecraft != null")]
        private Spacecraft spacecraft;
        private SpriteRenderer spriteRenderer;
        
        public Color healthyShieldColor = Color.blue;
        public Color damagedShieldColor = Color.red;
        public Color shieldFlashColor = Color.white;

        [SerializeField] private float timeUntilShieldHides = 5f;
        
        private Color oldHealthyColor;
        private Color oldDamageColor;
        private Color oldFlashColor;

        private Coroutine shieldHideCoroutine;
        private Coroutine shieldFlashCoroutine;
        private Color lastColor = Color.white;

        private void OnValidate() {
            if(spacecraft == null) {
                spacecraft = GetComponent<Spacecraft>();
                if(spacecraft == null) { spacecraft = GetComponentInParent<Spacecraft>(); }
            }
            
            if(spriteRenderer == null) {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            
            if(spriteRenderer != null) {
                if(oldHealthyColor != healthyShieldColor) {
                    spriteRenderer.color = healthyShieldColor;
                } else if(oldDamageColor != damagedShieldColor) {
                    spriteRenderer.color = damagedShieldColor;
                } else if(oldFlashColor != shieldFlashColor) {
                    spriteRenderer.color = shieldFlashColor;
                } else {
                    spriteRenderer.color = healthyShieldColor;
                }
            }

            oldHealthyColor = healthyShieldColor;
            oldDamageColor = damagedShieldColor;
            oldFlashColor = shieldFlashColor;
        }

        private void Awake() {
            OnValidate();

            lastColor = healthyShieldColor; //This might have to change if we want ships to spawn with less than full shields
        }

        private void Start() {
            spacecraft.OnShieldChanged += OnShieldChanged;
            spriteRenderer.color = Color.clear;
        }
        
        //Change the color of the shield based on the shield value
        private void OnShieldChanged(float shield, float maxShield, bool valueWentUp) {
            spriteRenderer.enabled = !(shield < 1); //Hide the shield if it's not active
            spriteRenderer.color = Color.Lerp(damagedShieldColor, healthyShieldColor, shield / maxShield);
            lastColor = spriteRenderer.color;
            
            if(!valueWentUp) {
                FlashShield();
            }
            
            BeginHidingShield();
        }
        
        //Coroutine to make shield flash when it's hit
        public void FlashShield() {
            if(shieldFlashCoroutine != null) {
                StopCoroutine(shieldFlashCoroutine);
                spriteRenderer.color = lastColor;
            }
            
            shieldFlashCoroutine = StartCoroutine(FlashShieldCoroutine());
        }

        public void BeginHidingShield() { 
            if(shieldHideCoroutine != null) {
                StopCoroutine(shieldHideCoroutine);
                spriteRenderer.color = lastColor;
            }
            
            shieldHideCoroutine = StartCoroutine(ShieldHide());
        }
        
        //Lerp the shield from its current color to alpha zero over the course of shield hide
        private IEnumerator ShieldHide() {
            while(shieldFlashCoroutine != null) {
                yield return null;
            }
            
            Color startColor = spriteRenderer.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);
            float startTime = Time.time;
            float endTime = startTime + timeUntilShieldHides;
            float t = 0;
            
            while(t < 1) {
                t = (Time.time - startTime) / (endTime - startTime);
                spriteRenderer.color = Color.Lerp(startColor, endColor, t);
                yield return null;
                
                while(shieldFlashCoroutine != null) {
                    yield return null;
                }
            }
            
            shieldHideCoroutine = null;
        }
        
        //Coroutine to make shield flash when it's hit, should save the shield color and restore it after the flash. Lerp the color to the flash color and back
        private IEnumerator FlashShieldCoroutine() {
            Color originalColor = spriteRenderer.color;
            Color currentColor = originalColor;
            float totalFlashTime = 0.25f;
            
            while (totalFlashTime > 0) {
                totalFlashTime -= Time.deltaTime;
                
                if(totalFlashTime > 0.15f) {
                    currentColor = Color.Lerp(currentColor, shieldFlashColor, Time.deltaTime * 10);
                } else {
                    currentColor = Color.Lerp(currentColor, originalColor, Time.deltaTime * 10);
                }
                
                spriteRenderer.color = currentColor;
                yield return null;
            }
            
            shieldFlashCoroutine = null;
        }
    }
}