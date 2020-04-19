using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int minHealth = 3;
    public int maxHealth = 5;
    public int health = 3;
    
    void Start()
    {
        health = Random.Range(minHealth, maxHealth + 1);
    }
    
    public void Damage(int dmg)
    {
        health -= dmg;
        if (health <= 0)
            Destroy(gameObject);
    }
}
