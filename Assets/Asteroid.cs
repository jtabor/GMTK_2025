using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class Asteroid : MonoBehaviour
{
    public GameObject[] spawns;
    public float[] spawnsChances;
    public int numberOfSpawns = 1;
    public float maxHealth = 100f;

    private float curHealth = 100f;

    public float damageScale = 1f;
    public float breakDamageScale = 0.01f;
    public float breakAngleVariant = 0.5f; // Radian
    private float endTime = 0f;

    private Vector3 hitPos;
    private Vector3 hitDir;

    public GameObject gameLogic;
    private AudioManager audioManager;   
    public AudioClip asteroidDestroyedClip;
    public AudioClip asteroidHitClip;

    public bool hasLaserImmunity = false;
    public bool hasMissleImmunity = false;
    public bool hasCollisionImmunity = false;

    private enum DamageSource
    {
        Collision,
        Laser,
        Missle,
        Shield  //These do no damage to objects, only to the shield itself
    }

    void Awake()
    {
        transform.Rotate(Random.rotation.eulerAngles);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        curHealth = maxHealth;
        audioManager = gameLogic.GetComponent<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (curHealth <= 0)
        {
            // Debug.Log(endTime);
            endTime -= Time.deltaTime;
            
            if (endTime < 0) Destroy(this.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject otherObject = collision.gameObject;
        Laser laser = otherObject.GetComponent<Laser>();
        Rigidbody rb = otherObject.GetComponent<Rigidbody>();
        PlayerShip ship = otherObject.GetComponent<PlayerShip>();

        float damage = 0;
        
        if (laser != null && !hasLaserImmunity)
        {
            damage = laser.damage;
            DoDamage(damage, DamageSource.Laser, collision.relativeVelocity);
        }
        else if (rb != null && ship == null && !hasCollisionImmunity)
        {
            Vector3 relVel = collision.relativeVelocity;
            damage = rb.mass * relVel.magnitude; 
            // Debug.Log("Asteroid hit - Damage: " + damage);
            // Use the first contact point (assuming we are convex, this is good enough)
            hitPos = collision.contacts[0].point;
            hitDir = relVel.normalized;
            DoDamage(damage, DamageSource.Collision, collision.relativeVelocity);
        }
        else if (ship != null)
        {
            //Don't do damage if it has shields left
            if (!ship.HasShields() && rb != null)
            {
                Vector3 relVel = collision.relativeVelocity;
                damage = rb.mass * relVel.magnitude; 
                // Debug.Log("Asteroid hit - Damage: " + damage);
                // Use the first contact point (assuming we are convex, this is good enough)
                hitPos = collision.contacts[0].point;
                hitDir = relVel.normalized;
                DoDamage(damage, DamageSource.Collision, collision.relativeVelocity);
            }
        }
         
        
    }
    private void DoDamage(float damage, DamageSource source, Vector3 hitDir)
    {
        damage = Mathf.Min(GetComponent<DamageNumbers>().maxDamageTaken, damage);
        curHealth -= damage;
        Debug.Log("Asteroid new health: " + curHealth + " damage: " + damage);
        
        if (curHealth <= 0) {
            DestroyObject(source, hitDir, damage);
                    
            if (audioManager != null && asteroidDestroyedClip != null)
            {
                audioManager.PlayEffectClip(asteroidDestroyedClip, AudioManager.AudioSourceType.EFFECT, transform.position, -1f);
            }

        }
        else{

            if (audioManager != null && asteroidHitClip != null)
            {
                audioManager.PlayEffectClip(asteroidHitClip, AudioManager.AudioSourceType.EFFECT, transform.position, -1f);
            }
        }

    }
    private GameObject GenerateSpawn()
    {
        // TODO: random generate
        return spawns[0];
    }
    private void DestroyObject(DamageSource damageSource, Vector3 hitDir, float damage)
    {
        endTime = 0;
        ParticleSystem ps = gameObject.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ParticleSystem.ShapeModule psShape = ps.shape;
            psShape.position = transform.worldToLocalMatrix.MultiplyPoint(hitPos);
            psShape.rotation = Quaternion.FromToRotation(new Vector3(0, 0, -1), transform.worldToLocalMatrix.MultiplyPoint(hitDir)).eulerAngles;
            // psShape.rotation = new Vector3(-90, 0, 0);
            ps.Play();
            endTime = ps.main.startLifetime.constant + ps.main.duration;
        }
        // Disable it as collider
        Collider collider = gameObject.GetComponent<Collider>();
        collider.enabled = false;
        MeshRenderer renderer = gameObject.GetComponentInChildren<MeshRenderer>();
        renderer.enabled = false;
        // Spawn children
        for (int i = 0; i < numberOfSpawns; ++i)
        {
            GameObject spawnPre = GenerateSpawn();
            // TODO: add a random rotation
            GameObject instance = Instantiate(spawnPre, transform.position, transform.rotation);
            Rigidbody rb = instance.GetComponent<Rigidbody>();
            Debug.Log(Mathf.Rad2Deg * Random.Range(-breakAngleVariant, breakAngleVariant));
            rb.linearVelocity = GetComponent<Rigidbody>().linearVelocity + Quaternion.AngleAxis(Mathf.Rad2Deg * Random.Range(-breakAngleVariant, breakAngleVariant), new Vector3(0.0f, 1.0f, 0.0f)) * (hitDir.normalized * damage * breakDamageScale);
            // assume we progress half second
            instance.transform.position += rb.linearVelocity * 0.2f;
            // TODO: make them inherent angular momentum
        }
        // Destroy(gameObject);
    }
}
