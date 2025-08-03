using UnityEngine;

public class InitialSpeed : MonoBehaviour
{
    public Vector3 initialVelocity;
    Rigidbody rb;
    float maxDistance = 90;
    Transform startT;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       rb = gameObject.GetComponent<Rigidbody>();
       rb.linearVelocity = initialVelocity;
        startT = gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(gameObject.transform.position, startT.position);
        if (dist > maxDistance)
        {
            Destroy(gameObject);
        }
    }
}
