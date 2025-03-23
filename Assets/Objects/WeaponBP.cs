using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WeaponBP")]
public class WeaponBP : InteractableItem
{
    public int maxAmmoCount;
    public int currentAmmo;
    public bool needsAmmo;
    public int weaponDamage;
    public int ammoReserves;
    public float shotCooldown;
    public InteractableItem ammoType;
}
