using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] protected float enemyHealth, enemyDamage, enemySpeed, attackRange;
    [SerializeField] protected List<GameObject> waypoints = new List<GameObject>();
    [SerializeField] protected float staggerLimit;
    protected NavMeshAgent nMA;
    [SerializeField] protected GameObject currentWaypoint;
    [SerializeField] protected bool reversePath;
    [SerializeField] protected int waypointIndex;
    [SerializeField] protected Animator animator;
    protected GameObject player;
    float totalDamageTakenSinceStagger;
    [SerializeField] EnemyStates previousState;
    bool stateChanged = false;

    private void Awake()
    {
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

    protected enum EnemyStates
    {
        Idle,
        Wandering,
        Chase,
        Attacking,
        Staggered,
        Dead,
    }

    [SerializeField] protected EnemyStates enemyState;

    // Update is called once per frame
    void Update()
    {
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
    }

    public void HandleAnimations(string animationTrigger)
    {
        Debug.Log(animationTrigger);
        Debug.Log(animator.parameterCount);
        for (int i = 0; i < animator.parameterCount; i++)
        {
            AnimatorControllerParameter animController;
            animController = animator.GetParameter(i);
            if (animController.name.ToString() == animationTrigger)
            {
                Debug.Log(animController.name.ToString() + ", " + animationTrigger);
                animator.SetBool(animationTrigger, true);
            }
            else
                animator.SetBool(animController.name, false);
        }
    }

    public abstract void Attack();
    public abstract void Wander();
    public abstract void Idle();
    public abstract void Chase();
    public abstract void Stagger();

    public void TakeDamage(float damageTaken)
    {
        Debug.Log("Damage taken innit");
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
        gameObject.SetActive(false);
    }
}
