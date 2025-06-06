using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseBandages : InventoryUse
{
    [SerializeField] float healthIncrease;
    public override void Use()
    {
        if (PlayerManager.Instance.health + healthIncrease > PlayerManager.Instance.maxPlayerHealth)
            PlayerManager.Instance.health = PlayerManager.Instance.maxPlayerHealth;
        else
            PlayerManager.Instance.health += healthIncrease;
    }

    public override void Combine()
    {
        throw new System.NotImplementedException();
    }
}

