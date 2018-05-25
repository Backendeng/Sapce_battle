using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipPhysics : MonoBehaviour {
    
    [Tooltip("X: lateral thrust\n Y: vertical thrust\n Z: longitudinal thrust")]
    public Vector3 linearForce = new Vector3(50.0f, 50.0f, 200.0f);

    [Tooltip("X: pitch\n Y: yaw\n Z: roll")]
    public Vector3 angularForce = new Vector3(75.0f, 75.0f, 35.0f);

    [Tooltip("multiplier for longitudinal thrust when reverse thrust is requested")]
    [Range(0.0f, 1.0f)]
    public float reverseMultiplier = 1.0f;

    [Tooltip("multiplier for all forces. can be used to keep force numbers smaller and more readable")]
    public float forceMultiplier = 100.0f;
    
    private Vector3 appliedLinearForce = Vector3.zero;
    private Vector3 appliedAngularForce = Vector3.zero;

    public Rigidbody Rigidbody { get; private set; }

    void Awake ()
    {
        Rigidbody = GetComponent<Rigidbody>();
        if (Rigidbody == null)
        {
            Debug.LogWarning(name + ": PlayerShipPhysics has no rigidbody.");
        }
	}

    void FixedUpdate()
    {
        if (Rigidbody != null)
        {
            Rigidbody.AddRelativeForce(appliedLinearForce * forceMultiplier, ForceMode.Force);
            Rigidbody.AddRelativeTorque(appliedAngularForce * forceMultiplier, ForceMode.Force);
        }
    }

    public void SetPhysicsInput(Vector3 linearInput, Vector3 angularInput)
    {
        appliedLinearForce = MultiplyByComponent(linearInput, linearForce);
        appliedAngularForce = MultiplyByComponent(angularInput, angularForce);
    }

    private Vector3 MultiplyByComponent(Vector3 a, Vector3 b)
    {
        Vector3 vec;
        vec.x = a.x * b.x;
        vec.y = a.y * b.y;
        vec.z = a.z * b.z;

        return vec;
    }
}
