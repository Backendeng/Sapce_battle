using UnityEngine;
using UnityEngine.UI;

public class HudUI : MonoBehaviour
{
    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        if (text != null && PlayerShip.ActiveShip != null)
        {
            text.text = string.Format("THR: {0}\nSPD: {1}", (PlayerShip.ActiveShip.Throttle * 100.0f).ToString("000"), PlayerShip.ActiveShip.Velocity.magnitude.ToString("000"));
        }
    }
}