using UnityEngine;
using UnityEngine.UI;

public class MouseCrosshair : MonoBehaviour
{
    public Image crosshair;
    public GameObject escapeMenu;

    private void Awake()
    {
        crosshair = GetComponent<Image>();

        crosshair.enabled = true; // PlayerShipInput.useMouseInput;
        
        if (crosshair.enabled) {
            Cursor.visible = false;
            //Cursor.lockState = CursorLockMode.Confined;
        }
        else {
            Cursor.visible = true;
            //Cursor.lockState = CursorLockMode.None;
        }
    }

    private void Update() {
        crosshair.transform.position = Input.mousePosition;
    }
}
