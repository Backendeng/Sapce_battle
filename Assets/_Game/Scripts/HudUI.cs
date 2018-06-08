using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GameManager))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Text))]

public class HudUI : MonoBehaviour {

    public GameManager gameManager;
    public Client clientController;

    public Image HealthBarImage;
    public Text HealthBarText;

    public Image EnergyBarImage;
    public Text EnergyBarText;

    public Text ScoreText;

    public GameObject EnemyTarget;
    public Image EnemyHealthBarImage;
    public Text EnemyText;
    public Image LockedTargetCircle;

    private Text text;

    private ShipShootingClient shipShooting;


    private void Awake()
    {
        text = GetComponent<Text>();
        

    }

    private void Start() {
        
        shipShooting = PlayerShip.ActiveShip.GetComponent<ShipShootingClient>();

        if (!shipShooting) {
            Debug.LogError("ShipShootingClient wasn't found");
        }
        LockedTargetCircle.enabled = false;
        EnemyTarget.SetActive(false);
    }

    void Update()
    {
        if (PlayerShip.ActiveShip != null) {
            text.text = string.Format("SPEED: {0}\nBOOST: {1}\nDEATHS: {2}", PlayerShip.ActiveShip.Velocity.magnitude.ToString("000"), PlayerShip.ActiveShip.Boost, 0 /* TODO: get deaths here */);
        }
        SetHealthBar(PlayerShip.ActiveShip.Health);
        SetEnergyBar(PlayerShip.ActiveShip.GetComponent<ShipShootingClient>().Energy);
        UpdateEnemyTarget();
        
        if (LockedTargetCircle.enabled) {
            LockedTargetCircle.transform.position = shipShooting.TargetScreenPoint();
        }
    }

    private void SetHealthBar(int health) {
        HealthBarImage.fillAmount = health / 100f;
        HealthBarText.text = string.Format("HEALTH: {0}", health);
    }

    private void SetEnergyBar(int energy) {
        EnergyBarImage.fillAmount = energy / 100f;
        EnergyBarText.text = string.Format("ENERGY: {0}", energy);
    }

    private void UpdateEnemyTarget() {
        if (shipShooting.lockTargetID == -1) {
            EnemyTarget.SetActive(false);
            LockedTargetCircle.enabled = false;
            return;
        }
        int targetClientID = clientController.GetShipClientID(shipShooting.lockTargetID);
        
        if (targetClientID == -1) {
            Debug.LogError("invalid locked target id");
        }

        EnemyTarget.SetActive(true);
        EnemyHealthBarImage.fillAmount = gameManager.GetHealth(targetClientID) / 100f;
        EnemyText.text = string.Format("TARGET LOCKED:\nENEMY: {0}", "<NAME>"); // TODO: get name here from game manager

        LockedTargetCircle.enabled = true;

    }
}
