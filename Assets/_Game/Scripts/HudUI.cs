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

    public Image SpeedBarImage;
    public Image BoostBarImage;

    public Text ScoreText;
    public Text DeathsText;
    public Text NameText;

    public GameObject EnemyTarget;
    public Image EnemyHealthBarImage;
    public Text EnemyText;
    public Image LockedTargetCircle;

    public Text lockedWarningText;

    public Image FixedCrosshair;
    public Image LockingCircle;

    public Text KillCreditText;

    private ShipShootingClient shipShooting;


    private void Start() {
        
        shipShooting = PlayerShip.ActiveShip.GetComponent<ShipShootingClient>();

        if (!shipShooting) {
            Debug.LogError("ShipShootingClient wasn't found");
        }
        LockedTargetCircle.enabled = false;
        EnemyTarget.SetActive(false);

        NameText.text = PlayerShip.ActiveShip.PlayerName;
    }

    void Update()
    {
        PlayerShip ship = PlayerShip.ActiveShip;
        if (ship != null) {
            SetHealthBar(ship.Health);
            SetEnergyBar(ship.GetComponent<ShipShootingClient>().Energy);
            ScoreText.text = ship.Score.ToString();
            DeathsText.text = ship.Deaths.ToString();
            SetSpeedBar(ship.Velocity.magnitude);
            SetBoostBar(ship.Boost);

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
        HealthBarText.text = health.ToString();
    }

    private void SetEnergyBar(int energy) {
        EnergyBarImage.fillAmount = energy / 100f;
        EnergyBarText.text = energy.ToString();
    }

    private void SetSpeedBar(float speed) {
        SpeedBarImage.fillAmount = speed / 100f;
    }

    private void SetBoostBar(float boost) {
        BoostBarImage.fillAmount = boost / 100f;
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
        EnemyText.text = gameManager.GetName(targetPlayerID);

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
