using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private void Start()
    {
        PlayerCombat.Instance.enemyAttacked += HurtEnemy;
    }

    private void OnDisable()
    {
        PlayerCombat.Instance.enemyAttacked -= HurtEnemy;
    }

    void HurtEnemy(GameObject enemyToHurt, float damage)
    {
        Debug.Log("AAA");
        enemyToHurt.GetComponent<Enemy>().TakeDamage(damage);
    }
}
