using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using static UnityEngine.UI.GridLayoutGroup;
public class MainUI : MonoBehaviour
{
    private Canvas canvas;
    public float SELECT_MIN_SIZE = 0.01f;
    private List<GameObject> corners;
    private GameObject targetLabel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvas = GetComponent<Canvas>();
        corners = new List<GameObject>();
        corners.Add(new GameObject("Corner"));
        corners.Add(new GameObject("Corner"));
        corners.Add(new GameObject("Corner"));
        corners.Add(new GameObject("Corner"));
        targetLabel = new GameObject("Text");
    }
    public void DisableSelectionRender()
    {
        foreach (GameObject go in corners)
        {
            go.SetActive(false);
        }
        targetLabel.SetActive(false);
    }
    public void EnableSelectionRender()
    {
        foreach (GameObject go in corners)
        {
            go.SetActive(true);
        }
        targetLabel.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawTarget(Sprite cornerSprite, Bounds objectBounds, string title)
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        
        // Get all 8 corners of the bounding box in world space
        Vector3 worldMin = objectBounds.min;
        Vector3 worldMax = objectBounds.max;
        Vector3[] worldCorners = new Vector3[8];
        worldCorners[0] = new Vector3(worldMin.x, worldMin.y, worldMin.z);
        worldCorners[1] = new Vector3(worldMax.x, worldMin.y, worldMin.z);
        worldCorners[2] = new Vector3(worldMin.x, worldMax.y, worldMin.z);
        worldCorners[3] = new Vector3(worldMax.x, worldMax.y, worldMin.z);
        worldCorners[4] = new Vector3(worldMin.x, worldMin.y, worldMax.z);
        worldCorners[5] = new Vector3(worldMax.x, worldMin.y, worldMax.z);
        worldCorners[6] = new Vector3(worldMin.x, worldMax.y, worldMax.z);
        worldCorners[7] = new Vector3(worldMax.x, worldMax.y, worldMax.z);
        
        // Convert to screen space and find screen bounds
        Vector2 screenMin = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 screenMax = new Vector2(float.MinValue, float.MinValue);
        
        foreach (Vector3 corner in worldCorners)
        {
            Vector3 screenPoint = cam.WorldToScreenPoint(corner);
            if (screenPoint.z > 0) // In front of camera
            {
                screenMin.x = Mathf.Min(screenMin.x, screenPoint.x);
                screenMin.y = Mathf.Min(screenMin.y, screenPoint.y);
                screenMax.x = Mathf.Max(screenMax.x, screenPoint.x);
                screenMax.y = Mathf.Max(screenMax.y, screenPoint.y);
            }
        }

        if (screenMin.x == float.MaxValue || screenMin.y == float.MaxValue || 
            screenMax.x == float.MinValue || screenMax.y == float.MinValue)
        {
            return; // Object is not visible or behind camera
        }
        
        // Convert to Canvas local coordinates
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 upperLeft, lowerRight;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            new Vector2(screenMin.x, screenMax.y),
            cam, 
            out upperLeft);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, 
            new Vector2(screenMax.x, screenMin.y), 
            cam, 
            out lowerRight);
        float width = Mathf.Abs(lowerRight.x - upperLeft.x);
        float height = Mathf.Abs(upperLeft.y - lowerRight.y);
        float minDimension = Mathf.Min(width, height);

        if (minDimension < SELECT_MIN_SIZE)
        {
            float scale = SELECT_MIN_SIZE / minDimension;
            Vector2 center = (upperLeft + lowerRight) / 2f;
            Vector2 halfSize = new Vector2(width / 2f, height / 2f) * scale;
            upperLeft = center + new Vector2(-halfSize.x, halfSize.y);
            lowerRight = center + new Vector2(halfSize.x, -halfSize.y);
        }
        // Draw four corners using screen coordinates (pixels)
        // Upper left corner
        DrawCornerSprite(corners[0], cornerSprite, upperLeft, 0f);
        // Upper right corner  
        DrawCornerSprite(corners[1], cornerSprite, new Vector2(lowerRight.x, upperLeft.y), 90f);
        // Lower right corner
        DrawCornerSprite(corners[2], cornerSprite, lowerRight, 180f);
        // Lower left corner
        DrawCornerSprite(corners[3], cornerSprite, new Vector2(upperLeft.x, lowerRight.y), 270f);
        
        // Draw title above the bounding box, center justified
        Vector2 titlePos = new Vector2((upperLeft.x + lowerRight.x) / 2f, upperLeft.y + 20f);
        DrawText(title, titlePos, TextAnchor.MiddleCenter);
    }
    //TODO: This should cache the gameobjects and move them instead of drawing new ones each time. 
    void DrawCornerSprite(GameObject corner, Sprite sprite, Vector2 position, float rotation)
    {
        // Create a temporary GameObject for the corner sprite
        corner.transform.SetParent(canvas.transform, false);
        if (!corner.GetComponent<Image>())
        {
            corner.AddComponent<Image>();

        }
        Image image = corner.GetComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        
        RectTransform rectTransform = corner.GetComponent<RectTransform>();
        Vector2 anchorAndPivot = new Vector2(0.5f,0.5f);
        rectTransform.anchorMin = anchorAndPivot;
        rectTransform.anchorMax = anchorAndPivot;
        rectTransform.pivot = anchorAndPivot;
        rectTransform.anchoredPosition = position;
        rectTransform.localRotation = Quaternion.Euler(0, 0, -rotation);
        rectTransform.sizeDelta = new Vector2(35,35);
        
        // Clean up after a frame (for temporary UI elements)
        // Destroy(corner, Time.deltaTime * 10.0f);
    }
    
    void DrawText(string text, Vector2 position, TextAnchor alignment)
    {
        // Create a temporary GameObject for the text
        GameObject textObj = targetLabel;
        textObj.transform.SetParent(canvas.transform, false);

        if (!textObj.GetComponent<Text>())
        {
            textObj.AddComponent<Text>();

        }

        Text textComponent = textObj.GetComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = 14;
        textComponent.color = Color.white;
        textComponent.alignment = alignment;
        textComponent.raycastTarget = false;
        
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        Vector2 anchorAndPivot = new Vector2(0.5f,0.5f);
        rectTransform.anchorMin = anchorAndPivot;
        rectTransform.anchorMax = anchorAndPivot;
        rectTransform.pivot = anchorAndPivot; 
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(200, 30);
        
        // Clean up after a frame (for temporary UI elements)
        // Destroy(textObj, Time.deltaTime * 10.0f);
    }

}
