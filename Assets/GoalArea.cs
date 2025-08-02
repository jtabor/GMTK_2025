using UnityEngine;

public class GoalArea : MonoBehaviour
{
    public GameObject border;
    public GameObject debug;
    Vector3[] corners = new Vector3[8];
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (border == null) return;
        
        // Get the actual bounds of this object
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null) return;
        
        Bounds bounds = renderer.bounds;
        
        // Calculate the 8 corner points of the box
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        
        corners[0] = new Vector3(min.x, min.y, min.z); // Bottom-left-back
        corners[1] = new Vector3(max.x, min.y, min.z); // Bottom-right-back
        corners[2] = new Vector3(min.x, min.y, max.z); // Bottom-left-front
        corners[3] = new Vector3(max.x, min.y, max.z); // Bottom-right-front
        corners[4] = new Vector3(min.x, max.y, min.z); // Top-left-back
        corners[5] = new Vector3(max.x, max.y, min.z); // Top-right-back
        corners[6] = new Vector3(min.x, max.y, max.z); // Top-left-front
        corners[7] = new Vector3(max.x, max.y, max.z); // Top-right-front
        // Create borders for top face (4 edges connecting the 4 highest points)
        CreateBorderBetweenPoints(corners[4], corners[5]); // Top-back edge
        CreateBorderBetweenPoints(corners[5], corners[7]); // Top-right edge
        CreateBorderBetweenPoints(corners[7], corners[6]); // Top-front edge
        CreateBorderBetweenPoints(corners[6], corners[4]); // Top-left edge
        
        // Create borders for bottom face (4 edges connecting the 4 lowest points)
        CreateBorderBetweenPoints(corners[0], corners[1]); // Bottom-back edge
        CreateBorderBetweenPoints(corners[1], corners[3]); // Bottom-right edge
        CreateBorderBetweenPoints(corners[3], corners[2]); // Bottom-front edge
        CreateBorderBetweenPoints(corners[2], corners[0]); // Bottom-left edge
    }
    
        private void CreateBorderBetweenPoints(Vector3 worldPoint1, Vector3 worldPoint2)
    {
        
        Debug.DrawLine(worldPoint1,worldPoint2, Color.red);
        
        Vector3 centerPosition = (worldPoint1 + worldPoint2) * 0.5f;
        Vector3 direction = (worldPoint2 - worldPoint1).normalized;
        float distance = Vector3.Distance(worldPoint1, worldPoint2);

        if (distance <= Mathf.Epsilon)
            return;
        
        GameObject borderInstance = Instantiate(border, null);
        borderInstance.transform.position = centerPosition;
        Vector3 borderOriginalScale = borderInstance.transform.localScale; 

        borderInstance.transform.localScale = new Vector3(borderOriginalScale.x * distance , borderOriginalScale.y , borderOriginalScale.z );
        Vector3 offset = borderInstance.transform.position - transform.position;
        Vector3 rotatedOffset = transform.rotation*offset;

        if (direction != Vector3.zero)
        {
            borderInstance.transform.rotation = Quaternion.FromToRotation(Vector3.right, direction)*transform.rotation;
            borderInstance.transform.position = transform.position + rotatedOffset;  
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(corners[4], corners[5],Color.red); // Top-back edge
        Debug.DrawLine(corners[5], corners[7],Color.red); // Top-right edge
        Debug.DrawLine(corners[7], corners[6],Color.red); // Top-front edge
        Debug.DrawLine(corners[6], corners[4],Color.red); // Top-left edge
        
        // Create borders for bottom face (4 edges connecting the 4 lowest points,Color.red)
        Debug.DrawLine(corners[0], corners[1],Color.red); // Bottom-back edge
        Debug.DrawLine(corners[1], corners[3],Color.red); // Bottom-right edge
        Debug.DrawLine(corners[3], corners[2],Color.red); // Bottom-front edge
        Debug.DrawLine(corners[2], corners[0],Color.red); // Bottom-left edge
    }
}
