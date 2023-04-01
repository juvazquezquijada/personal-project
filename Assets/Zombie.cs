using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
	public float moveSpeed = 3f;
	public float knockbackForce = 10;
	private Transform player;
	private bool isDead = false;
    public float destroyTime = 20f;

	 void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
	
    void Update()
    {
        if (isDead) return; // Don't do anything if the enemy is dead

        // Move towards the player
        transform.LookAt(player);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return; // Don't do anything if the enemy is dead

        if (other.gameObject.CompareTag("Bullet"))
        {
            // Apply knockback force
            Vector3 knockbackDirection = (transform.position - other.transform.position).normalized;
            GetComponent<Rigidbody>().AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

            // Die
            Die();
        }
        
    }

    private void Die()
    
{
    isDead = true;

    // Disable the enemy's collider and renderer
    GetComponent<BoxCollider>().enabled = false;
   
    // Apply a force to launch the enemy in the air
    Rigidbody rb = GetComponent<Rigidbody>();
    rb.isKinematic = false;
    Vector3 knockbackDirection = transform.up + transform.forward * 0.5f;
    rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

    // Destroy the enemy after a delay
    Destroy(gameObject, destroyTime);
}

    }



