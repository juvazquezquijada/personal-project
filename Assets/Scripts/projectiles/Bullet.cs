using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bullet : MonoBehaviour
{
    

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Demon"))
        {
            Destroy(gameObject);
            // Deal damage to enemy
            Demon enemy = other.gameObject.GetComponent<Demon>();
            GetComponent<SphereCollider>().enabled = false;
            
               enemy.TakeDamage();
               
            

            // Destroy bullet
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Zombie"))
        {
            // Deal damage to enemy
            Zombie enemy = other.gameObject.GetComponent<Zombie>();
            
                enemy.TakeDamage();
                CanvasManager.Instance.UpdateScore(10);
            GetComponent<SphereCollider>().enabled = false;

            // Destroy bullet
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Soldier"))
        {
            // Deal damage to enemy
            Soldier enemy = other.gameObject.GetComponent<Soldier>();
            
            CanvasManager.Instance.UpdateScore(10);
            GetComponent<SphereCollider>().enabled = false;
            enemy.TakeDamage();
            // Destroy bullet
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            // Deal damage to enemy
            CyberTitan enemy = other.gameObject.GetComponent<CyberTitan>();

            
            GetComponent<SphereCollider>().enabled = false;
            enemy.TakeDamage();
            // Destroy bullet
            Destroy(gameObject);
        }

        else if (other.gameObject.CompareTag("Wall"))
        {
            // Destroy bullet
            Destroy(gameObject);
        }
    }
}
