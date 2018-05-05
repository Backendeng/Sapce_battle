using UnityEngine;
using UnityEngine.UI;

public class MouseCrosshair : MonoBehaviour
{
    public Image crosshair;
    public GameObject escapeMenu;

    private void Awake()
    {
        crosshair = GetComponent<Image>();
    }

    private void Update()
    {
        if (crosshair != null && PlayerShip.ActiveShip != null)
        {
            
            if (escapeMenu.activeSelf) {
                crosshair.enabled = false;
            }
            else {
                crosshair.enabled = PlayerShip.ActiveShip.UsingMouseInput;
            }
                

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
