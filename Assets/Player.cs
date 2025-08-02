using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class Player : MonoBehaviour
{
    public GameObject gameLogic;

    public GameObject controlledObject;
    private float zoomDegrees = 90;
    public TextMeshProUGUI modeText;
 
    public Canvas mainUi;
    public List<GameObject> selectedTargets = new List<GameObject>();
    public List<GameObject> selectedGoals = new List<GameObject>();
    public GameObject selectedTractor;

    public Sprite targetSprite;
    public Sprite tractorSprite;
    
    public GameObject pauseIcon;
    public GameObject escToStartText;

    //DEBUG
    // public GameObject[] turrets;

    private int curTurret= 0;

    public enum SelectionMode{
        TARGET,
        TRACTOR,
        // GOAL
    }
    private string[] modeStrings = {"TRGT", "TRCTR","GL"};
    public SelectionMode curMode = SelectionMode.TARGET;
    
    public enum PauseState{
        NONE,
        PLAYER_PAUSED,
        GAME_PAUSED
    }
    public PauseState curPauseState = PauseState.NONE;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        selectedTargets.RemoveAll(target => target == null);
        selectedGoals.RemoveAll(goal => goal == null);
        if (Keyboard.current.enterKey.wasPressedThisFrame){
           if (curPauseState == PauseState.GAME_PAUSED)
           {
                curPauseState = PauseState.NONE;
                Time.timeScale = 1f;
           }
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (curPauseState == PauseState.NONE)
            {
                curPauseState = PauseState.PLAYER_PAUSED;
            }
            else if (curPauseState == PauseState.PLAYER_PAUSED)
            {
                curPauseState = PauseState.NONE;
            }
        }
        
        if (curPauseState == PauseState.NONE)
        {
            pauseIcon.SetActive(false);
            escToStartText.SetActive(false);
        }
        else if (curPauseState == PauseState.PLAYER_PAUSED)
        {
            pauseIcon.SetActive(true);
            escToStartText.SetActive(true);

        }
        else
        {
            pauseIcon.SetActive(true);
            escToStartText.SetActive(false);
        }


        if (curPauseState == PauseState.NONE)
            Time.timeScale = 1f;
        else
            Time.timeScale = 0f;

        if (controlledObject != null && curPauseState == PauseState.NONE)
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

                if (Keyboard.current.qKey.isPressed)
                    movement += new Vector3(0f,0f,1f); 
                if (Keyboard.current.eKey.isPressed)
                    movement += new Vector3(0f,0f,-1f); 
               
                
                ship.HandleControlInput(movement,rotation, Keyboard.current.shiftKey.isPressed, Keyboard.current.cKey.isPressed);
                
                //DEBUG 
                // if (Keyboard.current.fKey.wasPressedThisFrame)
                // {
                //     curTurret = (curTurret + 1) % turrets.Length;
                //     GameObject newTurret = Instantiate(turrets[curTurret]);
                //     Turret t = newTurret.GetComponent<Turret>();
                //     t.hasGimbal = true;
                //     t.gameLogic = gameLogic;
                //     ship.ReplaceHardpoint(newTurret,0);
                //
                // }
                if (Keyboard.current.gKey.wasPressedThisFrame || Keyboard.current.gKey.wasReleasedThisFrame)
                {
                    if (ship != null)
                    {
                        bool keyDown = Keyboard.current.gKey.isPressed;
                        ship.Blink(keyDown);
                    }
                }
                if (Keyboard.current.spaceKey.isPressed)
                {
                    if (ship != null)
                    {
                        ship.Fire(selectedTargets.ToArray()); 
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
                                // Remove from selectedTractor if it's the same object
                                if (selectedTractor == hitObject)
                                {
                                    selectedTractor = null;
                                }
                                
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
                                // Remove from selectedTargets if it's the same object
                                selectedTargets.Remove(hitObject);
                                
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
        
            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                int modeCount = System.Enum.GetValues(typeof(SelectionMode)).Length;
                curMode = (SelectionMode)(((int)curMode + 1) % modeCount);
            }
            modeText.text = "SEL MODE: " + modeStrings[(int)curMode]; 
}
        
        if (selectedTractor != null) 
        {
            DrawGameObject(tractorSprite, selectedTractor, "TRCTR TRGT");
        }
        // Draw target UI for selected targets
        DrawSelectedTargets();

    }
    public void SetGamePause(bool isPaused)
    {
        if (isPaused && curPauseState == PauseState.NONE)
        {
            curPauseState = PauseState.GAME_PAUSED;
            Time.timeScale = 0f;
        }
        else if (!isPaused && curPauseState == PauseState.GAME_PAUSED)
        {
            curPauseState = PauseState.NONE;
            Time.timeScale = 1f;
        }
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
