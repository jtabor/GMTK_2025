using UnityEngine;

public class Clickable : MonoBehaviour
{
    //NOTE - this is an empty class that we should add to the parent of all clickable objects
    //After a raycast hit we'll check for it and ignore objects that don't have it.
    public string objectName = "UNTITLED";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objectName = objectName.ToUpper();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
