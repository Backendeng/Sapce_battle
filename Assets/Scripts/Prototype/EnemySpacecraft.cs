using System.Linq;
using UnityEngine;
using SpaceSim;
using SpaceSim.ShipAI;
using UnityEngine.Serialization;

[RequireComponent(typeof(ShipAI))]
public class EnemySpacecraft : Spacecraft {
    [FormerlySerializedAs("range")] public float DetectionRange = 10f;
    [FormerlySerializedAs("enemyTag")] public string EnemyTag = "Enemy";

    [Range(0.1f, 45f)] public float maxInaccuracyDegrees = 5f;
    public float futurePredictionSeconds = 0.5f;

    public float chaseSpeed = 10f;

    private ShipAI shipAI;

    private Spacecraft markedTarget = null;
    private bool hasFoundTargetBefore = false;

    private float lastMagicMovement = float.MinValue;

    protected override void Awake() {
        base.Awake();

        shipAI = GetComponent<ShipAI>();
        Team = 1;
    }

    private void Update() {
        Spacecraft target = FindEnemyInRange(DetectionRange);

        if (target != null) {
            markedTarget = target;
            FireAtSpacecraft(target);

        } else if (markedTarget != null) {
            // no target but we have a marked target

            ChaseTarget(markedTarget);

        } else {
            // no target and no marked target
            shipAI.StopShip();
        }
    }

    private void ChaseTarget(Spacecraft target) {
        float velocitySignificance = 0.5f;

        Vector3 offset = new Vector3(10, 0, 0);
        //Get direction to target
        Vector3 direction = target.transform.position + offset - transform.position -
                            ((Vector3) rigidbody2D.velocity * velocitySignificance);
        direction.Normalize();

        if (RotateTowards2D(direction, 2)) {
            MagicMovement(target);

            //If velocity is pointed at target, accelerate
            if (Vector3.Dot(rigidbody2D.velocity.normalized, direction) < 0.5f) {
                //Not moving towards target
                ThrustForward();
            } else {
                //Are moving towards target
                if (rigidbody2D.velocity.magnitude < chaseSpeed) {
                    //Not moving fast enough
                    ThrustForward();
                } else {
                    //Moving fast enough
                    shipAI.StopShip();
                }
            }
        }


    }

    private Spacecraft FindEnemyInRange(float range) {
        //Change how this works for sure ...
        Spacecraft[] enemies = GameObject.FindGameObjectsWithTag(EnemyTag).Where(go => go.GetComponent<Spacecraft>())
            .Select(go => go.GetComponent<Spacecraft>()).ToArray();
        Spacecraft nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Spacecraft enemy in enemies) {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance) {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        return (nearestEnemy != null && shortestDistance <= range) ? nearestEnemy : null;
    }

    private void FireAtSpacecraft(Spacecraft enemy) {
        if (enemy == null) return;

        float timeForProjectileToHitTarget = Vector3.Distance(transform.position, enemy.transform.position) /
                                             weapons[0].Data.Projectile.GetComponent<Projectile>()
                                                 .PredictSpeedGivenImpulse(rigidbody2D.velocity, transform.up);

        Vector3 currentDirection = enemy.transform.position - transform.position;
        Vector3 futureDirection = (Vector2) currentDirection +
                                  (enemy.rigidbody2D.velocity - rigidbody2D.velocity) * timeForProjectileToHitTarget;

        Debug.DrawRay(transform.position, currentDirection, Color.green);
        Debug.DrawRay(transform.position, futureDirection, Color.yellow);

        bool isAimingAtTarget =
            RotateTowards2D(futureDirection,
                errorMargin: maxInaccuracyDegrees); //RotateTowardsDirection2D(futureDirection, maxTurnDegreesPerSecond, maxInaccuracyDegrees);

        //Draw ray forward
        Debug.DrawRay(transform.position, transform.right * DetectionRange, Color.red);

        if (isAimingAtTarget) FireAllWeapons();
    }

    private void MagicMovement(Spacecraft target) {
        //If offscreen, magically get closer to the target (no more than once every 10 seconds)
        if (Time.time - lastMagicMovement > 10 && !IsOnScreen() && Vector3.Distance(transform.position, target.transform.position) > 50) {
            //Magically move the ship closer to the target
            Vector3 directionToMeFromPlayer = transform.position -target.transform.position;
            directionToMeFromPlayer.Normalize();
            
            //Move towards target
            transform.position = target.transform.position + directionToMeFromPlayer * 50;
            
            //Set my velocity to the target's velocity
            rigidbody2D.velocity = target.rigidbody2D.velocity * 1.4f; //This should bring the enemy up to the target
            
            lastMagicMovement = Time.time;
        }
    }

//Returns true if this ship is off the main camera's view
    public bool IsOnScreen() {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        return screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
    }
}
