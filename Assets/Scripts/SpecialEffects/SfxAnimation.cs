using System.Collections.Generic;
using UnityEngine;

namespace SpaceSim.SpecialEffects {
    public class SfxAnimation : MonoBehaviour {
        private bool playOnce = true;
        
        private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

        private void Awake() {
            particleSystems = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
            particleSystems.Add(GetComponent<ParticleSystem>());
        }
        
        private void Update() {
            //if all particle systems are stopped, destroy the game object
            if (particleSystems.TrueForAll(p => !p.IsAlive())) {
                if (playOnce) {
                    Destroy(gameObject);
                }
            }
        }
    }
}