using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryUse : MonoBehaviour, IUsable
{
    // Used as a base class for all inventory usable items.
    public abstract void Use();
    public abstract void Combine();
}
