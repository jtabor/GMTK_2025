using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class Player : MonoBehaviour
{

    public GameObject controlledObject;
    private float zoomDegrees = 90;
    public TextMeshProUGUI modeText;
 
    public Canvas mainUi;
    public List<GameObject> selectedTargets = new List<GameObject>();
    public List<GameObject> selectedGoals = new List<GameObject>();
    public GameObject selectedTractor;

    public Sprite targetSprite;
    public Sprite tractorSprite;

    //DEBUG
    public GameObject[] turrets;

    private int curTurret= 0;

    public enum SelectionMode{
        TARGET,
        TRACTOR,
        GOAL
    }
    private string[] modeStrings = {"TRGT", "TRCTR","GL"};
    public SelectionMode curMode = SelectionMode.TARGET;
    


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
                
                if (Keyboard.current.tKey.wasPressedThisFrame)
                {
                    ship.ToggleTractorState();      
                }


                if (Keyboard.current.escapeKey.isPressed)
                {
                    if (curMode == SelectionMode.TARGET)
                    {
                        selectedTargets.Clear();
                    }
                    else if (curMode == SelectionMode.TRACTOR)
                    {
                        selectedTractor = null;
                    }
                }

                if (Mouse.current.leftButton.wasPressedThisFrame){
                    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                    RaycastHit hit;
                    
                    if (Physics.Raycast(ray, out hit))
                    {
                        GameObject hitObject = hit.transform.gameObject;
                        Clickable clickable = hitObject.GetComponent<Clickable>();
                        if (clickable != null)
                        {
                            bool ctrlPressed = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
                            
                            if (curMode == SelectionMode.TARGET)
                            {
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
                            else if (curMode == SelectionMode.TRACTOR)
                            {
                                selectedTractor = hitObject;
                            }
                            
                        }
                    }
                }
                if (selectedTractor != null)
                {
                    ship.tractorTarget = selectedTractor;
                }
                Vector2 scrollDelta = Mouse.current.scroll.ReadValue();
                float scrollY = scrollDelta.y;
                bool rtMouse = Mouse.current.rightButton.isPressed;
                ship.HandleCameraInput(rtMouse,scrollY);

            }
        }
        

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            int modeCount = System.Enum.GetValues(typeof(SelectionMode)).Length;
            curMode = (SelectionMode)(((int)curMode + 1) % modeCount);
        }
        modeText.text = "MODE: " + modeStrings[(int)curMode]; 

        if (selectedTractor != null) 
        {
            DrawGameObject(tractorSprite, selectedTractor, "TRCTR TRGT");
        }
        // Draw target UI for selected targets
        DrawSelectedTargets();
    }
    void DrawGameObject(Sprite cornerSprite, GameObject go, string text)
    {
        if (mainUi == null) return;
       
        MainUI mainUIScript = mainUi.GetComponent<MainUI>();
        if (mainUIScript == null) return;
       
        Collider collider = go.GetComponent<Collider>();
        if (collider == null) return;
        
        // Get world space bounds
        Bounds bounds = collider.bounds;
        
                    // Draw the target UI
        mainUIScript.DrawTarget(cornerSprite, bounds, text);
    

    }
    void DrawSelectedTargets()
    {
        
        Camera cam = Camera.main;
        if (cam == null) return;
        
        foreach (GameObject target in selectedTargets)
        {
            if (target == null) continue;
            DrawGameObject(targetSprite, target, target.name); 
        }
    }



}
