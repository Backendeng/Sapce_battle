using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ShipShootingClient))]
[RequireComponent(typeof(Kino.AnalogGlitch))]


public class LocalPlayerShip : PlayerShip {
    public GameObject interShadowPrefab;
    public GameObject shadowPrefab;
    private Transform shadow;
    private NetworkEntity InterShadow;

    public Kino.AnalogGlitch cameraGlitch;
    private readonly float glitchDuration = 1f;
    private float glitchEndTime;


    private ShipShootingClient shooting;

    private float lastReturnedInputTime;
    private float latency;

    public float sendStateRate = 0.05f;
    public static bool showUnsmoothedShadow = false;
    public static bool showInterpolatedShadow = false;

    public float boost_energy;
    private float boost_per_sec = 15f;
    public bool boosting = false;

    private AudioSource enginesSound;
    private float enginesSoundVolume = 0.08f;


    private new void Awake() {
        base.Awake();
    }

    private new void Start() {
        base.Start();
        activeShip = this;
        boost_energy = 100f;

        if (shadowPrefab != null && showUnsmoothedShadow)
            shadow = Instantiate(shadowPrefab, new Vector3(), new Quaternion()).transform;

        if (interShadowPrefab != null && showInterpolatedShadow)
            InterShadow = Instantiate(interShadowPrefab, new Vector3(), new Quaternion()).GetComponent<NetworkEntity>();

        ClientID = clientController.ClientID;

        // shooting init
        shooting = GetComponent<ShipShootingClient>();
        shooting.Init(clientController, EntityID);

        enginesSound = GetComponents<AudioSource>()[1];
    }

    private void FixedUpdate() {
       
        HandleMessagesFromServer();

        // update the server with our position
        SendStateToServer(transform.position, transform.rotation, Velocity);
    }


    private void Update() {

        if (!cameraGlitch) {
            cameraGlitch = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Kino.AnalogGlitch>();
        }

        if (cameraGlitch.enabled && Time.time > glitchEndTime) {
            cameraGlitch.enabled = false;
        }

        // get player input for movement
        float throttle = input.throttle;

        if (input.boost_pressed && boost_energy > 0) {
            boosting = true;
            throttle *= 1.5f;
            boost_energy -= Time.deltaTime * boost_per_sec;
        } else {
            boosting = false;
        }
        
        if (!input.boost_pressed && boost_energy < 100f)
            boost_energy += Time.deltaTime * boost_per_sec;

        // boost_energy = Mathf.Round(Mathf.Clamp(boost_energy, 0f, 100f));
        Boost = Mathf.Round(boost_energy);

        Vector3 linearInput = new Vector3(0.0f, 0.0f, throttle);
        Vector3 angularInput = new Vector3(input.pitch, input.yaw, input.roll);

        // apply movement physics using player input
        physics.SetPhysicsInput(linearInput, angularInput);

        if (Throttle > 0 && enginesSound.volume < enginesSoundVolume) {
            enginesSound.volume += Time.deltaTime * 0.05f;
        }
        else if (enginesSound.volume > 0) {
            enginesSound.volume -= Time.deltaTime * 0.1f;
        }

        if (IsDead) {
            shooting.ResetEnergy();
            boost_energy = 100f;

            return;
        }
          

        // shooting
        shooting.HandleShooting();
    }


    float rate = 0.06f;
    float nextSendTime = 0;
    float lastSent = -1;
    int lastFrameCount = 0;
    private void SendStateToServer(Vector3 pos, Quaternion rot, Vector3 vel) {
        if (Time.fixedTime < nextSendTime - 0.01f)
            return;
        nextSendTime = Time.fixedTime + 0.06f;
        //Debug.Log("DDDD sent " + Time.fixedTime);///////////////////
        clientController.SendStateToHost(EntityID, pos, rot, vel);

    }

    private void HandleMessagesFromServer() {
        if (!isServer && incomingQueue.Count != 0) {
            NetMsg netMessage = incomingQueue.Dequeue();
            switch (netMessage.Type) {
                case (byte)NetMsg.MsgType.SC_MovementData:
                    if (showUnsmoothedShadow) MoveShadow((SC_MovementData)netMessage);
                    if (showInterpolatedShadow) MoveInterShadow((SC_MovementData)netMessage);

                    SC_MovementData msg = (SC_MovementData)netMessage;
                    if(msg.TimeStamp == -1) { // respawn
                        GetComponent<Transform>().SetPositionAndRotation(msg.Position, msg.Rotation);
                        RespawnOnClientEnd();
                    }
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
        shadow.GetComponent<Transform>().SetPositionAndRotation(message.Position, message.Rotation);
    }

    private void MoveInterShadow(SC_MovementData message) {
        InterShadow.AddRecMessage(message);
    }

    public void ShakeCamera(int intensity) {
        cameraGlitch.enabled = true;
        glitchEndTime = Time.time + glitchDuration;        
    }


}

