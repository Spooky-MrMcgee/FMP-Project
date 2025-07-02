using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseAcid : InventoryUse
{
    [SerializeField] WinePuzzle winePuzzle;
    public bool usedAcid;
    public override void Use()
    {
        winePuzzle = GameObject.Find("WineFridge.001").GetComponent<WinePuzzle>();
        if (PlayerInteraction.Instance.nearestInteractable == winePuzzle.wineFridge)
            usedAcid = true;
        StartCoroutine(useCooldown());
    }

    IEnumerator useCooldown()
    {
        yield return new WaitForSeconds(1f);
        usedAcid = false;
    }

    public override void Combine()
    {
        throw new System.NotImplementedException();
    }
}
