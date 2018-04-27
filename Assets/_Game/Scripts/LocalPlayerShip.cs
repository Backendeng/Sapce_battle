using UnityEngine;

[RequireComponent(typeof(ShipShootingClient))]

public class LocalPlayerShip : PlayerShip {
    public GameObject interShadowPrefab;
    public GameObject shadowPrefab;
    private Transform shadow;
    private NetworkEntity InterShadow;
    private ShipShootingClient shooting;

    private float lastReturnedInputTime;
    private float latency;

    public float sendStateRate = 0.05f;
   

    private new void Awake() {
        base.Awake();
    }

    private new void Start() {
        base.Start();

        if (isPlayer)
            activeShip = this;

        if (shadowPrefab != null)
            shadow = Instantiate(shadowPrefab, new Vector3(), new Quaternion()).transform;
        if (interShadowPrefab != null)
            InterShadow = Instantiate(interShadowPrefab, new Vector3(), new Quaternion()).GetComponent<NetworkEntity>();

        // shooting init
        shooting = GetComponent<ShipShootingClient>();
        shooting.Init(clientController, entityID);

        clientID = clientController.ClientID;

    }

    private void FixedUpdate() {
       
        HandleMessagesFromServer();

        // update the server with our position
        SendStateToServer(transform.position, transform.rotation, Velocity);

        
    }


    private void Update() {

        // get player input for movement
        Vector3 linearInput = new Vector3(0.0f, 0.0f, input.throttle);
        Vector3 angularInput = new Vector3(input.pitch, input.yaw, input.roll);

        // apply movement physics using player input
        physics.SetPhysicsInput(linearInput, angularInput);

        // shooting
        shooting.HandleShooting();

    }


    int timeStep = 0;
    int rate = 2;//2 for 0.06f 
    private void SendStateToServer(Vector3 pos, Quaternion rot, Vector3 vel) {
        if (timeStep < rate) {
            timeStep++;
            return;
        }
        timeStep = 0;

        //Debug.Log("DDDD sent " + Time.time);///////////////////
  
        clientController.SendStateToHost(entityID, pos, rot, vel);
 
    }

    private void HandleMessagesFromServer() {
        if (!isServer && incomingQueue.Count != 0) {
            NetMsg netMessage = incomingQueue.Dequeue();
            switch (netMessage.Type) {
                case (byte)NetMsg.MsgType.SC_MovementData:
                    MoveShadow((SC_MovementData)netMessage);
                    MoveInterShadow((SC_MovementData)netMessage);
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
          //  Debug.LogWarning("No shadow prefab connected to LocalPlayerShip");
        }
        
    }

    private void MoveInterShadow(SC_MovementData message) {
        if (InterShadow != null) {
            InterShadow.AddRecMessage(message);
        }
        else {
        //    Debug.LogWarning("No InterShadow prefab connected to LocalPlayerShip");
        }


    }

}

