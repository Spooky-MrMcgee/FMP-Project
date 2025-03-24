using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMonstro : Enemy
{

    [SerializeField] int x = 0;

    public override void Wander()
    {
        // Wander state is for moving around the room, will set distinct pathways for each room for the enemies to navigate
        nMA.SetDestination(currentWaypoint.transform.position);
        if (Vector3.Distance(transform.position, currentWaypoint.transform.position) < 2f)
        {
            GetNextWaypoint();
        }
    }

    void GetNextWaypoint()
    {
        // Gets the next appropriate way point and reverses path once the enemy reaches the end of the waypoint
        if (currentWaypoint == waypoints[waypoints.Count - 1])
            reversePath = true;
        else if (currentWaypoint == waypoints[0])
            reversePath = false;

        if (!reversePath)
        {
            for (int y = 0; y <= waypoints.Count - 1; y++)
            {
                if (currentWaypoint == waypoints[y])
                {
                    currentWaypoint = waypoints[y + 1];
                    break;
                }
            }
        }
        else
        {
            for (int y = waypoints.Count - 1; y >= 0; y--)
            {
                if (currentWaypoint == waypoints[y])
                {
                    currentWaypoint = waypoints[y - 1];
                    break;
                }
            }
        }
    }

    public override void Attack()
    {
        nMA.isStopped = true;
    }

    public override void Idle()
    {
       // Idle will occur when a monster is not navigating a specific path
    }

    public override void Chase()
    {
        // Chase occurs upon the player attacking an enemy, causing them to lock into the players position and charge
        Debug.Log("Currently chasing");
        nMA.SetDestination(GameObject.Find("Player").transform.position);

    }

    public override void Stagger()
    {
        StartCoroutine(StaggerTimer());
    }

    IEnumerator StaggerTimer()
    {
        nMA.isStopped = true;
        yield return new WaitForSeconds(1f);
        nMA.isStopped = false;
        enemyState = EnemyStates.Chase;

    }
    public void AttackPlayer()
    {
        player.GetComponent<IDamageable>().TakeDamage(enemyDamage);
        // At the end of the attack animation an animation event will call AttackPlayer to deal damage to the player.
    }
}
