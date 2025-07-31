using UnityEngine;

public class Laser : MonoBehaviour
{
    public float damage = 100f;
    public float lifetimeSec = 10f;
    private float startTime = float.MaxValue;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime = Time.time; 
    }

    // Update is called once per frame
    void Update()
    {
       float curTime = Time.time;
       if (curTime - startTime >= lifetimeSec)
       {
            Destroy(gameObject); 
       }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("Player"))
            return; // Ignore collisions with the thing that fired us

        Destroy(gameObject);
    }

}
