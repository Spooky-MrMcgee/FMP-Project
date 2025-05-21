using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomDetails : MonoBehaviour
{
    public GameObject roomCollider, spawnPoint, cameraPoint, connectingSpawn;
    public Object[] collectibles;
    public Enemy[] enemies;
    public bool followPlayer;
    public bool lockX, lockZ;
    public Vector2 clampX, clampZ;
    public float orthographicSize;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
            PlayerManager.Instance.currentRoom = this;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            PlayerManager.Instance.previousRoom = this;
        }
    }
}
