using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySwitch : MonoBehaviour
{
    [SerializeField] GameObject inventoryObjectMiddle, inventoryObjectLeft, inventoryObjectRight;
    Vector3 middlePos, leftPos, rightPos;
    public bool SwitchItemsLeft()
    {
        Debug.Log("Performing the switch");
        inventoryObjectMiddle.transform.GetChild(0).transform.position = rightPos;
        inventoryObjectLeft.transform.GetChild(0).transform.position = middlePos;
        inventoryObjectMiddle.transform.GetChild(0).parent = inventoryObjectRight.transform;
        inventoryObjectLeft.transform.GetChild(0).parent = inventoryObjectMiddle.transform;
        inventoryObjectMiddle.transform.GetChild(0).transform.localPosition = new Vector3(0, 0, 0);
        Destroy(inventoryObjectRight.transform.GetChild(0).gameObject);
        return true;
    }

    public bool SwitchItemsRight()
    {
        Debug.Log("Performing the switch");
        inventoryObjectMiddle.transform.GetChild(0).transform.position = leftPos;
        inventoryObjectRight.transform.GetChild(0).transform.position = middlePos;
        inventoryObjectMiddle.transform.GetChild(0).parent = inventoryObjectLeft.transform;
        inventoryObjectRight.transform.GetChild(0).parent = inventoryObjectMiddle.transform;
        inventoryObjectMiddle.transform.GetChild(0).transform.localPosition = new Vector3(0, 0, 0);
        Destroy(inventoryObjectLeft.transform.GetChild(0).gameObject);
        return true;
    }
}
