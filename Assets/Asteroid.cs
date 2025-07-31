using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public float maxHealth = 100f;
    private float curHealth = 100f;
    public float damageScale = 1f; 
    
    private enum DamageSource
    {
        Collision,
        Laser,
        Missle
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       curHealth = maxHealth; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject otherObject = collision.gameObject;
        Laser laser = otherObject.GetComponent<Laser>();
        Rigidbody rb = otherObject.GetComponent<Rigidbody>();

        float damage = 0;

        if (laser != null)
        {
            damage = laser.damage;
            DoDamage(damage,DamageSource.Laser);
        }
        else if (rb != null)
        {
            Vector3 relVel = collision.relativeVelocity;
            damage = rb.mass*relVel.magnitude; 
            Debug.Log("Asteroid hit - Damage: " + damage);
            DoDamage(damage,DamageSource.Collision);
        }
        
    }
    private void DoDamage(float damage, DamageSource source)
    {
        curHealth -= damage;
        Debug.Log("Asteroid new health: " + curHealth);
        if (curHealth <= 0) {
            Debug.Log("Asteroid Destroyed");
            DestroyObject(source);
        }
    }
    private void DestroyObject(DamageSource damageSource)
    {
       Destroy(gameObject); 
    }
}
