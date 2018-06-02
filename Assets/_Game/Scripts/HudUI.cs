using UnityEngine;
using UnityEngine.UI;

public class HudUI : MonoBehaviour {

    public Image HealthBarImage;
    public Text HealthBarText;

    public Image EnergyBarImage;
    public Text EnergyBarText;

    public Text ScoreText;

    private Text text;


    private void Awake()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        if (text != null && PlayerShip.ActiveShip != null) {
            text.text = string.Format("SPEED: {0}\nBOOST: {1}\nDEATHS: {2}", PlayerShip.ActiveShip.Velocity.magnitude.ToString("000"), PlayerShip.ActiveShip.Boost, 0 /* TODO: get deaths here */);

            SetHealthBar(PlayerShip.ActiveShip.Health);
            SetEnergyBar(PlayerShip.ActiveShip.GetComponent<ShipShootingClient>().Energy);
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
}
