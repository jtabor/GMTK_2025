using UnityEngine;

public class TriggerHelperGroup : MonoBehaviour
{
    
    public bool isTriggered = false;
    public GameObject[] triggerObjects;

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
        for (int i = 0; i < triggerObjects.Length; i++)
        {
            if(triggerObjects[i] == other.gameObject){
                isTriggered = true;
            }
        }
    }
}
