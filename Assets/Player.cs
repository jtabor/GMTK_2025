using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Player : MonoBehaviour
{

    public GameObject controlledObject;
    private float zoomDegrees = 90;

    public Canvas mainUi;
    public List<GameObject> selectedTargets = new List<GameObject>();
    public GameObject[] selectedGoals;

    public Sprite targetSprite;
    public Sprite goalSprite;

    //DEBUG
    public GameObject[] turrets;

    private int curTurret= 0;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (controlledObject != null)
        {
            PlayerShip ship = controlledObject.GetComponent<PlayerShip>();
            if (ship != null)
            {
                // Handle WS movement (forward/back)
                Vector3 movement = Vector3.zero;
                
                if (Keyboard.current.wKey.isPressed)
                    movement += new Vector3(1f,0f,0f);
                if (Keyboard.current.sKey.isPressed)
                    movement += new Vector3(-1f,0f,0f);

                // Handle AD rotation
                float rotation = 0f;
                if (Keyboard.current.aKey.isPressed)
                    rotation += -1f; 
                if (Keyboard.current.dKey.isPressed)
                    rotation += 1f;

                ship.HandleControlInput(movement,rotation);

                // Handle QE turret rotation
                float turretRotation = 0f;
               
                if (Keyboard.current.qKey.isPressed)
                    turretRotation = -1f;
                
                if (Keyboard.current.eKey.isPressed)
                    turretRotation = 1f;
                
                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    curTurret = (curTurret + 1) % turrets.Length;
                    GameObject newTurret = Instantiate(turrets[curTurret]);
                    ship.ReplaceHardpoint(newTurret,0);

                }
                if (Keyboard.current.spaceKey.isPressed)
                {
                    PlayerShip ps = controlledObject.GetComponent<PlayerShip>();
                    if (ps != null)
                    {
                        ps.Fire(selectedTargets.ToArray()); 
                    }
                }
                if (Mouse.current.leftButton.wasPressedThisFrame){
                    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                    RaycastHit hit;
                    
                    if (Physics.Raycast(ray, out hit))
                    {
                        Debug.Log("MOUSE HIT!");
                        GameObject hitObject = hit.transform.gameObject;
                        Clickable clickable = hitObject.GetComponent<Clickable>();
                        if (clickable != null)
                        {
                            bool ctrlPressed = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
                            
                            if (ctrlPressed)
                            {
                                // Append to selectedTargets if not already present
                                if (!selectedTargets.Contains(hitObject))
                                {
                                    selectedTargets.Add(hitObject);
                                }
                            }
                            else
                            {
                                // Replace selectedTargets with just this one
                                selectedTargets.Clear();
                                selectedTargets.Add(hitObject);
                            }
                        }
                    }

                    Debug.Log("Selected: " + selectedTargets.Count);
                }

                Vector2 scrollDelta = Mouse.current.scroll.ReadValue();
                float scrollY = scrollDelta.y;
                bool rtMouse = Mouse.current.rightButton.isPressed;
                ship.HandleCameraInput(rtMouse,scrollY);

            }
        }
        
        // Draw target UI for selected targets
        DrawSelectedTargets();
    }
    
    void DrawSelectedTargets()
    {
        if (mainUi == null) return;
        
        MainUI mainUIScript = mainUi.GetComponent<MainUI>();
        if (mainUIScript == null) return;
        
        Camera cam = Camera.main;
        if (cam == null) return;
        
        foreach (GameObject target in selectedTargets)
        {
            if (target == null) continue;
            
            Collider collider = target.GetComponent<Collider>();
            if (collider == null) continue;
            
            // Get world space bounds
            Bounds bounds = collider.bounds;
            
                        // Draw the target UI
            mainUIScript.DrawTarget(targetSprite, bounds, target.name);
        }
    }



}
