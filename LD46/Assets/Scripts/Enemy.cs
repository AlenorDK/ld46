using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int minHealth = 3;
    public int maxHealth = 5;
    public int health = 3;

    public GameObject playerObj;

    public EnemyState state = EnemyState.Idle;
    
    void Start()
    {
        health = Random.Range(minHealth, maxHealth + 1);
    }

    void Update()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");

        transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }
    
    public void Damage(int dmg)
    {
        health -= dmg;
        if (health > 0)
        {
            state = EnemyState.Hit;
        }

        if (health <= 0)
        {
            Destroy(gameObject);
            state = EnemyState.Down;
        }
    }
}

public enum EnemyState
{
    Idle = 0,
    MovingToPlayer = 1,
    MovingToBox = 2,
    AttackPlayer = 3,
    AttackBox = 4,
    Hit = 5,
    Down = 6
}
