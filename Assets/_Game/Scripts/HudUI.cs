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

    public Image FixedCrosshair;
    public Image LockingCircle;

    public Text KillCreditText;

    private ShipShootingClient shipShooting;


    private void Awake() {
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
        PlayerShip ship = PlayerShip.ActiveShip;
        if (ship != null) {
            leftPanelText.text = string.Format("SPEED: {0}\nBOOST: {1}\nDEATHS: {2}\nName: {3}", ship.Velocity.magnitude.ToString("000"), ship.Boost, ship.Deaths, ship.PlayerName);
            SetHealthBar(ship.Health);
            SetEnergyBar(ship.GetComponent<ShipShootingClient>().Energy);
            ScoreText.text = string.Format("SCORE: {0}", ship.Score);
        }

        if(gameManager.LocalPlayerLockCounter > 0) {
            lockedWarningText.enabled = true;
        }
        else {
            lockedWarningText.enabled = false;
        }

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

        // TODO: update killed messages on HUD here
        KillCreditText.text = gameManager.GetKillCreditText();
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

    public void EnableCrossharis() {
        FixedCrosshair.enabled = true;
        LockingCircle.enabled = true;
    }

    public void DisableCrosshairs() {
        FixedCrosshair.enabled = false;
        LockingCircle.enabled = false;
    }
}
