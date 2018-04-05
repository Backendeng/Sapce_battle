using UnityEngine;

public class LocalPlayerShip : PlayerShip {
    public GameObject shadowPrefab;
    private Transform shadow;

    private float lastReturnedInputTime;
    private float latency;

    public float sendStateRate = 0.05f;
    private float nextStateSendTime;

    // Shooting : Ray
    float rayRange = 50f;

    GameObject playerCamera;


    public new void Start() {
        base.Start();

        if (isPlayer)
            activeShip = this;

        if (shadowPrefab != null)
            shadow = Instantiate(shadowPrefab, new Vector3(), new Quaternion()).transform;

        //nextStateSendTime = Time.time;
        playerCamera = GameObject.FindWithTag("MainCamera");

    }

    private void Update() {
        // get player input for movement
        Vector3 linearInput = new Vector3(0.0f, 0.0f, input.throttle);
        Vector3 angularInput = new Vector3(input.pitch, input.yaw, input.roll);

        // apply movement physics using player input
        physics.SetPhysicsInput(linearInput, angularInput);

        HandleMessagesFromServer();

        // shooting
        HandleShooting();

        // update the server with our position
        SendStateToServer(transform.position, transform.rotation);

        
    }
  
    private void HandleShooting() {
        // Secondary Shot - Missiles
        if ( Time.time > nextFire2 && (Input.GetButton("LeftTrigger") || Input.GetMouseButtonDown(1))) {
            nextFire2 = Time.time + fireRate2;
            //Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            SendMissileToServer(shotSpawn.position, shotSpawn.rotation);
            GetComponent<AudioSource>().Play();
        }
        // Primary Shot - Ray
        if (Time.time > nextFire1 && (Input.GetButton("RightTrigger") || Input.GetMouseButtonDown(0))) {
            nextFire1 = Time.time + fireRate1;
            ShootRay();
        }
    }

    private void SendMissileToServer(Vector3 pos, Quaternion rot) {
        clientController.SendMissileShotToHost(entityID, pos, rot);
    }

    private void ShootRay() {
        
        RaycastHit hit;

        if (Physics.Raycast(shotSpawn.position, playerCamera.transform.forward, out hit, rayRange)) {
            Debug.Log("Hit: " + hit.transform.name);
        }
    }

    private void SendStateToServer(Vector3 pos, Quaternion rot) {
        if (Time.time > nextStateSendTime) {
            clientController.SendStateToHost(entityID, pos, rot);
            nextStateSendTime = Time.time + sendStateRate;
        }
    }

    private void HandleMessagesFromServer() {
        if (!isServer && incomingQueue.Count != 0) {
            NetMsg netMessage = incomingQueue.Dequeue();
            switch (netMessage.Type) {
                case (byte)NetMsg.MsgType.SC_MovementData:
                    MoveShadow((SC_MovementData)netMessage);
                    break;
                case (byte)NetMsg.MsgType.SC_EntityDestroyed:
                    Destroy(gameObject);
                    break;
                default:
                    Debug.Log("ERROR! LocalPlayerShip on Client reveived an invalid NetMsg message. NetMsg Type: " + netMessage.Type);
                    break;
            }
        }
    }

    private void MoveShadow(SC_MovementData message) {
        Vector3 pos = this.gameObject.GetComponent<Transform>().position;
        Quaternion rot = this.gameObject.GetComponent<Transform>().rotation;
        if (shadow != null) {
            shadow.GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);
        }
        else {
            Debug.LogWarning("No shadow prefab connected to LocalPlayerShip");
        }
        
    }

}

