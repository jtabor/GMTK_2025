using System.Threading.Tasks;
using UnityEngine;

public class AstroidSpawner : MonoBehaviour
{
    public GameObject[] astroidPrefabs;
    public float spawnSpeed;
    public float spawnTime = 1;
    public float spawnVariant;
    public float speedVariant;
    public float angleVariant;
    public float angularVriant;
    public float rotationVriant;

    private float spawnTimer;
    private GameObject spawnBox;
    private float nextSpawnTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async Task Start()
    {
        spawnTimer = 0;
        spawnBox = transform.Find("Spawn Box").gameObject;
        nextSpawnTime = spawnTime + Random.Range(-spawnVariant, spawnVariant);
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer > nextSpawnTime)
        {
            nextSpawnTime = spawnTime + Random.Range(-spawnVariant, spawnVariant);
            spawnTimer = 0;
            int id = Random.Range(0, astroidPrefabs.Length);
            Vector3 position;
            Quaternion rotation;
            Renderer meshRenderer = spawnBox.GetComponent<Renderer>();
            Bounds meshBounds = meshRenderer.bounds;
            transform.GetPositionAndRotation(out position, out rotation);
            position[0] = Random.Range(meshBounds.min.x, meshBounds.max.x);
            position[1] = 0;
            position[2] = Random.Range(meshBounds.min.z, meshBounds.max.z);
            GameObject instance = Instantiate(astroidPrefabs[id], position, rotation);
            Rigidbody body = instance.GetComponent<Rigidbody>();
            Quaternion rotate = Quaternion.Euler(0, Mathf.Rad2Deg * Random.Range(-angleVariant, angleVariant), 0);
            body.linearVelocity = rotate * (transform.forward * (spawnSpeed + Random.Range(-speedVariant, speedVariant)));
            body.angularVelocity = new Vector3(Random.Range(-angularVriant, angularVriant), Random.Range(-angularVriant, angularVriant), Random.Range(-angularVriant, angularVriant));
            instance.transform.rotation = Quaternion.Euler(new Vector3(Mathf.Rad2Deg * Random.Range(-angularVriant, angularVriant), Mathf.Rad2Deg * Random.Range(-angularVriant, angularVriant), Mathf.Rad2Deg * Random.Range(-angularVriant, angularVriant)));
        }
    }
}
