using UnityEngine;

public class WrongWayGameLogic : MonoBehaviour
{
    
    public GameObject gameLogicHelper;
    
    private GameLogicHelper helper;
    
    bool displayed = false;
    private string levelStartString = @"Make it to the end of the asteroid field!
Movement:\t\t\t
W/S forward/back, A/D rotate L/R, Q/E strafe L/R, C Inertial Brake
Camera
Scroll to zoom
RMB Free look";
    
    bool displayedHardRock = false;
    public GameObject hardRockTriggerBlock;
    TriggerHelper hardRockTrigger;

    public GameObject[] boundaryTriggerObjects;
    TriggerHelper[] boundaryTriggers;

    public GameObject passTriggerObject;
    TriggerHelper passTrigger;
    
    public GameObject blinkTriggerBlock;
    TriggerHelper blinkTrigger;
    bool displayedBlink = false; 
    bool done = false; 

    public GameObject ship;
    public GameObject startObject;
    public GameObject endObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        hardRockTrigger = hardRockTriggerBlock.GetComponent<TriggerHelper>(); 
        helper = gameLogicHelper.GetComponent<GameLogicHelper>();
        
        passTrigger = passTriggerObject.GetComponent<TriggerHelper>();
        blinkTrigger = blinkTriggerBlock.GetComponent<TriggerHelper>();

        boundaryTriggers = new TriggerHelper[boundaryTriggerObjects.Length];
        for (int i = 0; i < boundaryTriggers.Length; i++)
        {
            boundaryTriggers[i] = boundaryTriggerObjects[i].GetComponent<TriggerHelper>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 startPos = startObject.transform.position;
        Vector3 endPos = endObject.transform.position;
        Vector3 shipPos = ship.transform.position;
        
        Vector3 startToEnd = endPos - startPos;
        Vector3 startToShip = shipPos - startPos;
        
        float progress = Vector3.Dot(startToShip, startToEnd) / startToEnd.sqrMagnitude;
        progress = Mathf.Clamp01(progress);
        
        helper.UpdateProgress(progress, "Distance Left");

        if (!displayed)
        {
            Debug.Log("SENT STARTUP MESSAGE");
            helper.DisplayPauseMessage("LEVEL1: WRONG WAY", levelStartString);
            displayed = true;    
        }

        if (hardRockTrigger.isTriggered && !displayedHardRock)
        {
            helper.DisplayGameMessage("WARNING", "Metallic asteroids can't be damaged from laser fire.  Avoid them",10f);
            displayedHardRock = true;
        }
        
        if (blinkTrigger.isTriggered && !displayedBlink)
        {
            helper.DisplayGameMessage("Blink and miss!", "Sometimes things are impassible.  You can use blink [G] to get through.  Press [G] to preview and blink on release!",10f);
            displayedBlink = true;
        }
        for (int i = 0; i < boundaryTriggers.Length; i++)
        {
            if (boundaryTriggers[i].isTriggered && !done)
            {
               helper.ShowFail("GAME OVER!", "You left the play area.  Stay out of the red boxes.");
               done = true;
            }
        }
        if (passTrigger.isTriggered && !done)
        {
            helper.ShowPass("CONGRATS!", "Nice Job!  You successfully navigated an asteroid field!");
            done = true;
        }

    }
}
