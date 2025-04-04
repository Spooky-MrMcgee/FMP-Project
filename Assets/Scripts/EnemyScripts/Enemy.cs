using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Processors;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    [Header("Enemy Stats")]
    [SerializeField] protected float enemyHealth;
    [SerializeField] protected float enemyDamage;
    [SerializeField] protected float enemySpeed;
    [SerializeField] protected float chaseSpeed;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float staggerLimit;

    [Header("Enemy Pathfinding")]
    [SerializeField] protected List<GameObject> waypoints = new List<GameObject>();
    protected NavMeshAgent nMA;
    [SerializeField] protected GameObject currentWaypoint;
    [SerializeField] protected bool reversePath;
    [SerializeField] protected int waypointIndex;

    [Header("Enemy Objects")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected EnemyStates enemyState;
    protected GameObject player;
    float totalDamageTakenSinceStagger;
    public bool isDead = false;
    protected enum EnemyStates
    {
        Idle,
        Wandering,
        Chase,
        Attacking,
        Staggered,
        Dead,
    }


    private void Awake()
    {
        // Assigns all the necessary start variables before setting it to wander along its respective pathways.
        nMA = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player");
        waypointIndex = 0;
        foreach (Transform child in GameObject.FindGameObjectWithTag("EnemyPathway").transform)
        {
            waypoints.Add(child.gameObject);
            if (currentWaypoint == null)
                currentWaypoint = child.gameObject;
            else if (Vector3.Distance(transform.position, child.transform.position) < Vector3.Distance(transform.position, currentWaypoint.transform.position))
                currentWaypoint = child.gameObject;
        }
        foreach (GameObject waypoint in waypoints)
        {
            if (currentWaypoint == waypoint)
                break;
            waypointIndex++;
        }
        enemyState = EnemyStates.Wandering;
    }



    // Update is called once per frame
    void Update()
    {
        if (isDead)
            return;
        // Handles enemy states and assigns their appropriate animation states to them.
        #region States Handler
        switch (enemyState)
        {
            case EnemyStates.Idle:
                HandleAnimations("isIdle");
                Idle();
                break;

            case EnemyStates.Wandering:
                HandleAnimations("isWandering");
                Wander();
                break;

            case EnemyStates.Chase:
                HandleAnimations("isChasing");
                Chase();
                break;

            case EnemyStates.Attacking:
                HandleAnimations("isAttacking");
                Attack();
                break;

            case EnemyStates.Staggered:
                HandleAnimations("isStaggered");
                Stagger();
                break;

            case EnemyStates.Dead:
                HandleAnimations("isDead");
                Die();
                break;
        }


        if (Vector3.Distance(transform.position, player.transform.position) <= attackRange)
            enemyState = EnemyStates.Attacking;
        else if (Vector3.Distance(transform.position, player.transform.position) >= attackRange && enemyState == EnemyStates.Attacking)
            enemyState = EnemyStates.Chase;
        #endregion
    }

    // Takes the current state and uses a string associated with it in order to enable/disable the appropriate booleans for animation events to occur.
    public void HandleAnimations(string animationTrigger)
    {
        for (int i = 0; i < animator.parameterCount; i++)
        {
            AnimatorControllerParameter animController;
            animController = animator.GetParameter(i);
            if (animController.name.ToString() == animationTrigger)
                animator.SetBool(animationTrigger, true);
            else
                animator.SetBool(animController.name, false);
        }
    }

    // Functions that allow enemies to act distinctly based on their type. Here to be overridden in their respective subclasses.
    #region Abstract Functions
    public abstract void Attack();
    public abstract void Wander();
    public abstract void Idle();
    public abstract void Chase();
    public abstract void Stagger();
    #endregion
   
    public void TakeDamage(float damageTaken)
    {
        // Handles damage being taken and aggroes/kills the enemy in response.
        totalDamageTakenSinceStagger += damageTaken;
        enemyHealth -= damageTaken;
        enemyState = EnemyStates.Chase;

        if (totalDamageTakenSinceStagger > staggerLimit)
        {
            enemyState = EnemyStates.Staggered;
            totalDamageTakenSinceStagger = 0;
        }

        if (enemyHealth <= 0)
            enemyState = EnemyStates.Dead;
    }

    public void Die()
    {
        isDead = true;
        nMA.isStopped = true;
    }
}
