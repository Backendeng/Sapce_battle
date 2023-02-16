using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace SpaceSim.ShipAI {
    [RequireComponent(typeof(Spacecraft))]
    public class ShipAI : MonoBehaviour {
        
        private Spacecraft spacecraft;
        
        private void Awake() {
            spacecraft = GetComponent<Spacecraft>();
        }
        
        /// <summary>
        /// Slows ship and returns true when stopped
        /// </summary>
        /// <param name="velocityMargin"></param>
        /// <returns></returns>
        public bool StopShip(float velocityMargin = 0.1f) {
            if (spacecraft.rigidbody2D.velocity.magnitude > velocityMargin ) { //Is moving
                bool isFacing = spacecraft.RotateAwayFrom2D(spacecraft.rigidbody2D.velocity);
                if(isFacing) { //Is facing away from velocity
                    spacecraft.ThrustForward();
                    return false;
                }
            }
            
            return true;
        }
        
    }
}