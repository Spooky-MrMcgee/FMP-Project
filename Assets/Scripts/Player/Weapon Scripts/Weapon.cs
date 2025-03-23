using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour 
{
    public int maxAmmoCount;
    public int currentAmmo;
    public bool needsAmmo;
    public int weaponDamage;
    public int ammoReserves;
    public float weaponSpeed;

    [SerializeField] private WeaponBP weaponBlueprint;

    private void Start()
    {
        maxAmmoCount = weaponBlueprint.maxAmmoCount;
        currentAmmo = weaponBlueprint.currentAmmo;
        needsAmmo = weaponBlueprint.needsAmmo;
        weaponDamage = weaponBlueprint.weaponDamage;
        weaponSpeed = weaponBlueprint.shotCooldown;
        foreach (PlayerManager.InventoryItems inventoryItem in PlayerManager.Instance.interactableItems)
        {
            if (inventoryItem.item ==  weaponBlueprint.ammoType)
            {
                ammoReserves = inventoryItem.quantity;
            }
        }
    }

    public void RemoveAmmo()
    {
        currentAmmo--;
    }

    public int Reload(int totalAmmo)
    {
        ammoReserves = totalAmmo;
        Debug.Log("Current ammo is" + ammoReserves);
        if (currentAmmo == maxAmmoCount)
            return ammoReserves;

        if (ammoReserves > 0)
        {
            if (ammoReserves < (maxAmmoCount - currentAmmo))
            {
                currentAmmo =+ ammoReserves;
                Debug.Log("AAA");
                ammoReserves = 0;
            }

            if (ammoReserves >= maxAmmoCount)
            {
                ammoReserves -= (maxAmmoCount - currentAmmo);
                Debug.Log("BBB");
                currentAmmo = maxAmmoCount;
            }

            if (ammoReserves < 0)
                ammoReserves = 0;
        }
        Debug.Log("Ammo reserves are now" + ammoReserves);
        return ammoReserves;
    }
}
