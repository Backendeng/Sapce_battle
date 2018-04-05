using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerShip : PlayerShip {
    public GameObject shadowPrefab;
    private Transform shadow;

    private float lastReturnedInputTime;
    private float latency;

    public float sendStateRate = 0.05f;
    private float nextStateSendTime;

    public new void Start() {
        base.Start();
        if (shadowPrefab != null)
            shadow = Instantiate(shadowPrefab, new Vector3(), new Quaternion()).transform;

        nextStateSendTime = Time.time;
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

        if (isPlayer)
            activeShip = this;
    }
  
    private void HandleShooting() {
        if ((Input.GetButton("RightTrigger") || Input.GetMouseButtonDown(0)) && Time.time > nextFire) {
            nextFire = Time.time + fireRate;
            //Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            SendMissileToServer(shotSpawn.position, shotSpawn.rotation);
            GetComponent<AudioSource>().Play();
        }
    }

    private void SendMissileToServer(Vector3 pos, Quaternion rot) {
        clientController.SendMissileShotToHost(entityID, pos, rot);
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

