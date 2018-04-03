using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerShip : PlayerShip {

    public float positionThreshold = 0.5f;
    public float rotationThreshold = 0.05f;

    public float sendInputRate = 0.05f;
    private float nextInputSendTime;

    private Vector3 lastLinearInput = new Vector3();
    private Vector3 lastAngularInput = new Vector3();


    public GameObject shadowPrefab;
    private Transform shadow;

    private void Start() {
        nextInputSendTime = Time.time;
        networkController = GameObject.Find("ClientNetworkController");
        if (networkController == null)
            Debug.LogError("ERROR! networkController not found");
        if (shadowPrefab != null)
            shadow = Instantiate(shadowPrefab, new Vector3(), new Quaternion()).transform;
    }

    private void Update() {

        // pass player input to the physics
        Vector3 linear_input = new Vector3(0.0f, 0.0f, input.throttle);
        Vector3 angular_input = new Vector3(input.pitch, input.yaw, input.roll);

        physics.SetPhysicsInput(linear_input, angular_input);

        if (incomingQueue.Count != 0) {
            NetMsg netMessage = incomingQueue.Dequeue();
            switch (netMessage.Type) {
                case (byte)NetMsg.MsgType.SC_MovementData:
                    //   SyncPositionWithServer((SC_MovementData)netMessage);
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

        if (Time.time > nextInputSendTime && (linear_input != lastLinearInput || lastAngularInput != angular_input) ) {
            networkController.GetComponent<Client>().SendInputToHost(entityID, input.throttle, angular_input);
            lastLinearInput = linear_input;
            lastAngularInput = angular_input;
            nextInputSendTime = Time.time + sendInputRate;
        }
        

        // shooting
        if ((Input.GetButton("RightTrigger") || Input.GetMouseButtonDown(0)) && Time.time > nextFire) {
            nextFire = Time.time + fireRate;
            Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            GetComponent<AudioSource>().Play();
        }

        if (isPlayer)
            activeShip = this;
    }

    private void SyncPositionWithServer(SC_MovementData message) {
        Vector3 pos = this.gameObject.GetComponent<Transform>().position;
        Quaternion rot = this.gameObject.GetComponent<Transform>().rotation;

        Vector3 newPos = pos;
        Quaternion newRot = rot;

        if (Vector3.Distance(message.Position, pos) > positionThreshold ) {
            newPos = message.Position;
        }

        if (Quaternion.Angle(message.Rotation, rot) > rotationThreshold) {
            newRot = message.Rotation;
        }
        
        if (newPos != pos || newRot != rot)
            GetComponent<Transform>().SetPositionAndRotation(newPos, newRot);
    }

    private void MoveShadow(SC_MovementData message) {
        Vector3 pos = this.gameObject.GetComponent<Transform>().position;
        Quaternion rot = this.gameObject.GetComponent<Transform>().rotation;

        shadow.GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);

    }


}
