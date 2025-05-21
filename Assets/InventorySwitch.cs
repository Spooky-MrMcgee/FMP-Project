using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySwitch : MonoBehaviour
{
    [SerializeField] GameObject inventoryObjectMiddle, inventoryObjectLeft, inventoryObjectRight;
    [SerializeField] Vector3 middlePos, leftPos, rightPos;
    bool moveLeft, moveRight;
    float step;
    [SerializeField] float itemMoveSpeed;
    private void Update()
    {
        if (moveLeft)
        {
            if (Vector3.Distance(inventoryObjectLeft.transform.GetChild(0).transform.position, inventoryObjectMiddle.transform.position) < 0.01f)
            {
                Destroy(inventoryObjectMiddle.transform.GetChild(0).gameObject);
                inventoryObjectLeft.transform.GetChild(0).parent = inventoryObjectMiddle.transform;
                Debug.Log("This is occuring");
                moveLeft = false;
                return;
            }
            step = itemMoveSpeed * Time.deltaTime;
            inventoryObjectMiddle.transform.GetChild(0).transform.position = Vector3.MoveTowards(inventoryObjectMiddle.transform.GetChild(0).transform.position, inventoryObjectRight.transform.position, step);
            inventoryObjectLeft.transform.GetChild(0).transform.position = Vector3.MoveTowards(inventoryObjectLeft.transform.GetChild(0).transform.position, inventoryObjectMiddle.transform.position, step);
        }

        if (moveRight)
        {
            if (Vector3.Distance(inventoryObjectRight.transform.GetChild(0).transform.position, inventoryObjectMiddle.transform.position) < 0.01f)
            {
                Destroy(inventoryObjectMiddle.transform.GetChild(0).gameObject);
                inventoryObjectRight.transform.GetChild(0).parent = inventoryObjectMiddle.transform;
                moveRight = false;
                return;
            }
            step = itemMoveSpeed * Time.deltaTime;
            inventoryObjectMiddle.transform.GetChild(0).transform.position = Vector3.MoveTowards(inventoryObjectMiddle.transform.GetChild(0).transform.position, inventoryObjectLeft.transform.position, step);
            inventoryObjectRight.transform.GetChild(0).transform.position = Vector3.MoveTowards(inventoryObjectRight.transform.GetChild(0).transform.position, inventoryObjectMiddle.transform.position, step);
        }
    }

    public bool SwitchItemsLeft()
    {
        moveLeft = true;
        return true;
    }

    public bool SwitchItemsRight()
    {
        moveRight = true;
        return true;
    }
}
