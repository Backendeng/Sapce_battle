using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SpaceSim {
    
    public class EngineArtController : MonoBehaviour{
        
        private int framesSinceLastThrust = 0;
        private bool playing = false;
        [SerializeField] private List<ParticleSystem> thrusterParticles;
        private Spacecraft spacecraft;
        

        private void Awake() {
            FindSpacecraft();
            spacecraft.OnThrusterEngaged += OnThrusterEngaged;
            thrusterParticles.ForEach(p => p.Stop());
        }

        private void Update() {
            if (playing && framesSinceLastThrust > 5) {
                StopThrust();
            } else {
                framesSinceLastThrust++;
            }
        }

        private void FindSpacecraft() {
            if(spacecraft == null) spacecraft = GetComponent<Spacecraft>();
            if (spacecraft == null) spacecraft = GetComponentInParent<Spacecraft>();
        }
        
        private void OnThrusterEngaged() {
            framesSinceLastThrust = 0;
            
            if(playing) return;
            StartThrust();
        }
        
        private void StopThrust() {
            if(!playing) return;
            thrusterParticles.ForEach(p => p.Stop());
            playing = false;
        }
        
        private void StartThrust() {
            if(playing) return;
            thrusterParticles.ForEach(p => p.Play());
            playing = true;
        }
        
        private void OnDestroy() {
            spacecraft.OnThrusterEngaged -= OnThrusterEngaged;
        }
    }
}