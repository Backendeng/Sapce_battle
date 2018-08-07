using UnityEngine;

public class EngineEmission : MonoBehaviour {

    public PlayerShip ship;

    public ParticleSystem[] engines = new ParticleSystem[5];

    private readonly float engineEmissionFactor = 1.3f;
    private readonly float minEmissionRate = 2f;

    void Update() {
        float speed = ship.Velocity.magnitude;
        float throttle = ship.Throttle;
        foreach (ParticleSystem engine in engines) {
            ParticleSystem.EmissionModule emission = engine.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(minEmissionRate + throttle * speed * engineEmissionFactor);
        }
        
    }
}
