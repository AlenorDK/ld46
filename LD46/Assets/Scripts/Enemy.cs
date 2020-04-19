using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public int minHealth = 3;
    public int maxHealth = 5;
    public int health = 3;

    public GameObject playerObj;
    public GameObject boxObj;

    public EnemyState state = EnemyState.Idle;

    public NavMeshAgent agent;

    public Animator anim;

    public float hitDownTime = 0.2f;
    public float respawnTime = 3f;
    public float stoppingDistance = 3f;

    void Start()
    {
        health = Random.Range(minHealth, maxHealth + 1);
        anim.SetTrigger("Walk");
        agent.stoppingDistance = stoppingDistance;
    }

    void Update()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");
        boxObj = GameObject.FindWithTag("Box");
        
        transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(transform.position, playerObj.transform.position, NavMesh.AllAreas, path);
        
        if (state == EnemyState.MovingToPlayer)
        {
            agent.SetDestination(playerObj.transform.position);
        }
        else if (state == EnemyState.MovingToBox)
        {
            agent.SetDestination(boxObj.transform.position);
        }
        else if (state == EnemyState.Hit || state == EnemyState.Down)
        {
            agent.SetDestination(transform.position);
        }
    }
    
    public void Damage(int dmg)
    {
        health -= dmg;
        if (health > 0)
        {
            StartCoroutine(PlayHit());
        }

        if (health <= 0)
        {
            StartCoroutine(PlayDown());
        }
    }

    IEnumerator PlayHit()
    {
        state = EnemyState.Hit;
        anim.SetTrigger("Hit");
        yield return new WaitForSeconds(hitDownTime);
        state = EnemyState.MovingToPlayer;
        anim.SetTrigger("Walk");
    }

    IEnumerator PlayDown()
    {
        state = EnemyState.Down;
        anim.SetTrigger("Down");
        yield return new WaitForSeconds(respawnTime);
        state = EnemyState.MovingToPlayer;
        health = Random.Range(minHealth, maxHealth + 1);
        anim.SetTrigger("Walk");
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
