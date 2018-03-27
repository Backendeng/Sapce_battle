using UnityEngine;
using UnityEngine.UI;

public class MouseCrosshair : MonoBehaviour
{
    private Image crosshair;

    private void Awake()
    {
        crosshair = GetComponent<Image>();
    }

    private void Update()
    {
        if (crosshair != null && PlayerShip.ActiveShip != null)
        {
            crosshair.enabled = PlayerShip.ActiveShip.UsingMouseInput;

            if (crosshair.enabled)
            {
                crosshair.transform.position = Input.mousePosition;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}