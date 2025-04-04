using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour 
{
    [Header("Weapon Stats")]
    public int maxAmmoCount;
    public int currentAmmo;
    public bool needsAmmo;
    public int weaponDamage;
    public int ammoReserves;
    public float weaponSpeed;

    [SerializeField] private WeaponBP weaponBlueprint;

    private void Start()
    {
        // Assigns all of the weapon details based on the scriptable object blueprint.
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
        // Reloads based on current amount of ammo.
        ammoReserves = totalAmmo;
        if (currentAmmo == maxAmmoCount)
            return ammoReserves;

        if (ammoReserves > 0)
        {
            if (ammoReserves < (maxAmmoCount - currentAmmo))
            {
                currentAmmo =+ ammoReserves;;
                ammoReserves = 0;
            }

            if (ammoReserves >= maxAmmoCount)
            {
                ammoReserves -= (maxAmmoCount - currentAmmo);
                currentAmmo = maxAmmoCount;
            }

            if (ammoReserves < 0)
                ammoReserves = 0;
        }
        return ammoReserves;
    }
}
