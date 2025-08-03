using UnityEngine;

public class TriggerHelper : MonoBehaviour
{
    
    public bool isTriggered = false;
    public GameObject triggerObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        
        if(triggerObject == other.gameObject){
            isTriggered = true;
        }

    }
}
