using System;
using TMPro;
using UnityEngine;

namespace SpaceSim.UI.HUD {
    public class ShipStatsUI : MonoBehaviour{
        [SerializeField] private RectTransform armorBar;
        [SerializeField] private RectTransform shieldBar;
        [SerializeField] private RectTransform fuelBar;
        
        private float barSize = 100;
        
        private PlayerController playerController;

        private void Start() {
            playerController = PlayerController.Instance;
            
            playerController.Spacecraft.OnArmorChanged += UpdateArmorBar;
            playerController.Spacecraft.OnShieldChanged += UpdateShieldBar;
            playerController.Spacecraft.OnFuelChanged += UpdateFuelBar;
            
            barSize = armorBar.sizeDelta.x;
        }
        
        private void UpdateArmorBar(float armor, float maxArmor, bool didValueIncrease) {
            //Set the width of the bar to the percentage of the current armor. When below 1 disable the bar
            armorBar.sizeDelta = new Vector2(armor / maxArmor * barSize, armorBar.sizeDelta.y);
            armorBar.gameObject.SetActive(armor > 0);
        }
        
        private void UpdateShieldBar(float shield, float maxShield, bool didValueIncrease) {
            //Set the width of the bar to the percentage of the current shield. When below 1 disable the bar
            shieldBar.sizeDelta = new Vector2(shield / maxShield * barSize, shieldBar.sizeDelta.y);
            shieldBar.gameObject.SetActive(shield > 0);
        }
        
        private void UpdateFuelBar(float fuel, float maxFuel, bool didValueIncrease) {
            //Set the width of the bar to the percentage of the current fuel. When below 1 disable the bar
            fuelBar.sizeDelta = new Vector2(fuel / maxFuel * barSize, fuelBar.sizeDelta.y);
            fuelBar.gameObject.SetActive(fuel > 0);
        }
    }
}