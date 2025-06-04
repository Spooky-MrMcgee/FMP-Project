using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseBottle : InventoryUse
{
    [SerializeField] WinePuzzle winePuzzle;
    public bool usedBottle;
    public override void Use()
    {
        usedBottle = true;
        StartCoroutine(useCooldown());
    }

    IEnumerator useCooldown()
    {
        yield return new WaitForSeconds(1f);
        usedBottle = false;
    }

    public override void Combine()
    {
        throw new System.NotImplementedException();
    }
}
