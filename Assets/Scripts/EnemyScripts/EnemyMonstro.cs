using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMonstro : Enemy
{
    // EnemyMonstro inherits from the base enemy script and overrides all necessary attributes in order to have it function in a unique way.

    public override void Wander()
    {
        // Wander state is for moving around the room, will set distinct pathways for each room for the enemies to navigate
        nMA.isStopped = false;
        nMA.speed = enemySpeed;
        nMA.SetDestination(currentWaypoint.transform.position);
        if (Vector3.Distance(transform.position, currentWaypoint.transform.position) < 4f)
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
        transform.LookAt(new Vector3(player.transform.position.x, this.transform.position.y, player.transform.position.z));
    }

    public void HurtPlayer()
    {
        player.GetComponent<IDamageable>().TakeDamage(enemyDamage);
    }

    public override void Idle()
    {
       // Idle will occur when a monster is not navigating a specific path
    }

    public override void Chase()
    {
        nMA.isStopped = false;
        // Chase occurs upon the player attacking an enemy, causing them to lock into the players position and charge
        nMA.speed = chaseSpeed;
        nMA.SetDestination(GameObject.Find("Player").transform.position);

    }

    public override void Stagger()
    {
        // Stagger occurs when the enemy has taken enough damage, enemy will pause for a moment and then continue it's aggression
        StartCoroutine(StaggerTimer());
    }

    IEnumerator StaggerTimer()
    {
        nMA.isStopped = true;
        yield return new WaitForSeconds(1f);
        nMA.isStopped = false;
        enemyState = EnemyStates.Chase;

    }
}
