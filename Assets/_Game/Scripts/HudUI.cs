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

    private Text leftPanelText;
    public Text lockedWarningText;

    private ShipShootingClient shipShooting;


    private void Awake()
    {
        leftPanelText = GetComponent<Text>();
        

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
            leftPanelText.text = string.Format("SPEED: {0}\nBOOST: {1}\nDEATHS: {2}\nName: {3}", PlayerShip.ActiveShip.Velocity.magnitude.ToString("000"), PlayerShip.ActiveShip.Boost, 0 /* TODO: get deaths here */, PlayerShip.ActiveShip.PlayerName);
        }

        if(gameManager.LocalPlayerLockCounter > 0) {
            lockedWarningText.enabled = true;
        }
        else {
            lockedWarningText.enabled = false;
        }

        SetHealthBar(PlayerShip.ActiveShip.Health);
        SetEnergyBar(PlayerShip.ActiveShip.GetComponent<ShipShootingClient>().Energy);
        UpdateEnemyTarget();
        
        if (LockedTargetCircle.enabled) {
            Vector3 screenPoint = shipShooting.TargetScreenPoint();
            if (screenPoint == Vector3.zero) {
                LockedTargetCircle.enabled = false;
            }
            else {
                LockedTargetCircle.transform.position = screenPoint;
            }
            
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
        int targetPlayerID = shipShooting.lockTargetID;
        if (targetPlayerID == -1) {
            EnemyTarget.SetActive(false);
            LockedTargetCircle.enabled = false;
            return;
        }

        EnemyTarget.SetActive(true);
        EnemyHealthBarImage.fillAmount = gameManager.GetHealth(targetPlayerID) / 100f;
        EnemyText.text = string.Format("TARGET LOCKED:\nENEMY: {0}", gameManager.GetName(targetPlayerID));

        LockedTargetCircle.enabled = true;

    }
}
