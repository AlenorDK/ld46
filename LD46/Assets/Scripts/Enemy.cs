using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

    public float searchTargetDelay = 2f;

    public float stealingBoxTime = 3f;
    public int damageAmount = 1;
    public float attackCooldown = 1f;

    public ParticleSystem blood;
    
    void Start()
    {
        health = Random.Range(minHealth, maxHealth + 1);
        anim.SetTrigger("Walk");
        agent.stoppingDistance = stoppingDistance;
        StartCoroutine(searchNewTarget());
    }

    IEnumerator searchNewTarget()
    {
        while (true)
        {
            yield return new WaitForSeconds(searchTargetDelay);
            if (state == EnemyState.MovingToPlayer || state == EnemyState.MovingToBox || state == EnemyState.Idle)
                FindTarget();
        }
    }

    void Update()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");
        boxObj = GameObject.FindWithTag("Box");

        if (boxObj == null || playerObj == null)
        {
            Debug.Log("Player or container not present.");
            return;
        }

        if (agent.path == null || agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            FindTarget();
        }
        
        transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        
        if (state == EnemyState.MovingToPlayer)
        {
            agent.SetDestination(playerObj.transform.position);
        }
        else if (state == EnemyState.MovingToBox)
        {
            agent.SetDestination(boxObj.transform.position);
        }
        else if (state == EnemyState.Hit || state == EnemyState.Down || health == 0)
        {
            agent.SetDestination(transform.position);
        }

        if (state == EnemyState.MovingToPlayer && agent.remainingDistance <= agent.stoppingDistance && agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            StartCoroutine(AttackPlayer());
        }
        else if (state == EnemyState.MovingToBox && agent.remainingDistance <= agent.stoppingDistance && agent.pathStatus == NavMeshPathStatus.PathComplete)
            StartCoroutine(StealBox());
    }

    void FindTarget()
    {
        NavMeshPath pathToPlayer = new NavMeshPath();
        agent.CalculatePath(playerObj.transform.position, pathToPlayer);

        float lengthToPlayer = 0.0F;
        if (pathToPlayer.corners.Length > 0)
        {
            Vector3 previousCorner = pathToPlayer.corners[0];
            int i = 1;
            while (i < pathToPlayer.corners.Length)
            {
                Vector3 currentCorner = pathToPlayer.corners[i];
                lengthToPlayer += Vector3.Distance(previousCorner, currentCorner);
                previousCorner = currentCorner;
                i++;
            }
        }


        NavMeshPath pathToBox = new NavMeshPath();
        agent.CalculatePath(boxObj.transform.position, pathToBox);
        
        float lengthToBox = 0.0F;
        if (pathToBox.corners.Length > 0)
        {
            Vector3 previousCorner2 = pathToBox.corners[0];
            int j = 1;
            while (j < pathToBox.corners.Length)
            {
                Vector3 currentCorner = pathToBox.corners[j];
                lengthToBox += Vector3.Distance(previousCorner2, currentCorner);
                previousCorner2 = currentCorner;
                j++;
            }
        }

        if (lengthToBox == 0 && lengthToPlayer == 0)
        {
            state = EnemyState.Idle;
            anim.SetTrigger("Idle");
        }
        else if (lengthToBox == 0)
            state = EnemyState.MovingToPlayer;
        else if (lengthToPlayer == 0)
            state = EnemyState.MovingToBox;
        else if (lengthToBox > lengthToPlayer)
        {
            state = EnemyState.MovingToPlayer;
        }
        else
            state = EnemyState.MovingToBox;
    }
    
    public void Damage(int dmg)
    {
        health -= dmg;
        if (health > 0)
        {
            StartCoroutine(PlayHit());
            blood.Play();
        }

        else if (health <= 0 && state != EnemyState.Down)
        {
            StartCoroutine(PlayDown());
        }
    }

    IEnumerator AttackPlayer()
    {
        state = EnemyState.AttackPlayer;
        do
        {
            playerObj.GetComponent<PlayerMovement>().Damage(damageAmount);
            yield return new WaitForSeconds(attackCooldown);
        } while (health > 0 && Vector3.Distance(transform.position, playerObj.transform.position) < 3f);
        FindTarget();
    }

    IEnumerator StealBox()
    {
        state = EnemyState.AttackBox;
        float stealingTime = 0f;
        
        boxObj.GetComponent<ContainerController>().stealingBox1.Play();
        boxObj.GetComponent<ContainerController>().stealingBox2.Play();
        
        while (stealingTime < stealingBoxTime && state == EnemyState.AttackBox && health > 0)
        {
            stealingTime += Time.deltaTime;
            yield return null;
        }
        
        boxObj.GetComponent<ContainerController>().stealingBox1.Stop();
        boxObj.GetComponent<ContainerController>().stealingBox2.Stop();
        
        if (stealingTime >= stealingBoxTime)
        {
            Destroy(boxObj);
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
        FindTarget();
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
