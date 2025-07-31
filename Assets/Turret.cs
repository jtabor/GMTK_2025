using UnityEngine;

public class Turret : MonoBehaviour
{
    
    public GameObject[] bulletSpawns; 
    public GameObject projectile;
    public float cooldownTimeMs = 500f;
    public float muzzleVelocity = 10f;
    
    private float lastFireTime = 0f;


    void Start()
    {
        GameObject[] allGameObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        System.Collections.Generic.List<GameObject> bulletSpawnList = new System.Collections.Generic.List<GameObject>();
        
        foreach (GameObject obj in allGameObjects)
        {
            if (obj.name == "BulletSpawn")
            {
                bulletSpawnList.Add(obj);
            }
        }
        
        bulletSpawns = bulletSpawnList.ToArray();
    }

    void Update()
    {
            
    }
    
    public void Fire(GameObject[] targets)
    {
        if (Time.time * 1000f - lastFireTime < cooldownTimeMs)
            return;
            
        if (projectile == null || bulletSpawns == null || bulletSpawns.Length == 0)
            return;
            
        foreach (GameObject spawnPoint in bulletSpawns)
        {
            if (spawnPoint != null)
            {
                GameObject bullet = Instantiate(projectile, spawnPoint.transform.position, spawnPoint.transform.rotation);
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                if (bulletRb != null)
                {
                    bulletRb.isKinematic = false;
                    bulletRb.linearVelocity = spawnPoint.transform.right * muzzleVelocity;
                }
            }
        }
        
        lastFireTime = Time.time * 1000f;
    }
    
    public float GetCooldownPercentage()
    {
        float timeSinceLastFire = Time.time * 1000f - lastFireTime;
        float remainingCooldown = cooldownTimeMs - timeSinceLastFire;
        
        if (remainingCooldown <= 0f)
            return 0f;
            
        return remainingCooldown / cooldownTimeMs;
    }
}
