using UnityEngine;

public class Turret : MonoBehaviour
{
    
    public GameObject[] bulletSpawns; 
    public GameObject projectile;
    public float cooldownTimeMs = 500f;
    public float muzzleVelocity = 10f;
    
    private float lastFireTime = 0f;
    
    public bool hasGimbal = false;
    private Quaternion initialRotation;
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
        initialRotation = transform.rotation; 
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
        
        Vector3 targetDirection = Vector3.right; // Default direction
        //TODO This lead calculation is wrong, but close, probably okay? 
        // If hasGimbal and we have targets, calculate aiming direction
        if (hasGimbal && targets != null && targets.Length > 0 && targets[0] != null)
        {
            GameObject target = targets[0];
            Rigidbody targetRb = target.GetComponent<Rigidbody>();
            
            if (targetRb != null)
            {
                // Calculate target leading (predictive aiming)
                Vector3 targetPosition = target.transform.position;
                Vector3 targetVelocity = targetRb.linearVelocity;
                Vector3 turretPosition = transform.position;
                
                // Calculate intercept point
                Vector3 toTarget = targetPosition - turretPosition;
                float distance = toTarget.magnitude;
                float timeToHit = distance / muzzleVelocity;
                
                // Predict where target will be
                Vector3 predictedPosition = targetPosition + targetVelocity * timeToHit;
                targetDirection = (predictedPosition - turretPosition).normalized;
            }
            else
            {
                // No rigidbody, just aim directly at target
                targetDirection = (target.transform.position - transform.position).normalized;
            }
            
            // Rotate turret to face target
            Vector3 newZ = Vector3.Cross(targetDirection,Vector3.up).normalized;
            transform.rotation = Quaternion.LookRotation(newZ, Vector3.up);
        }
        else
        {
            // transform.rotation = initialRotation;
            transform.rotation = transform.parent.rotation;
            targetDirection = transform.rotation*(new Vector3(1f,0f,0f));
        }

        foreach (GameObject spawnPoint in bulletSpawns)
        {
            if (spawnPoint != null)
            {
                GameObject bullet = Instantiate(projectile, spawnPoint.transform.position, spawnPoint.transform.rotation);
                bullet.tag = "Player";
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                if (bulletRb != null)
                {
                    bulletRb.isKinematic = false;
                    // Use calculated target direction for bullet velocity
                    if (hasGimbal)
                    {
                        bulletRb.linearVelocity = targetDirection * muzzleVelocity;
                    }
                    else
                    {
                        bulletRb.linearVelocity = spawnPoint.transform.right * muzzleVelocity;
                    }
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
