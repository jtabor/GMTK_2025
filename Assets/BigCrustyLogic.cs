using UnityEngine;

public class BigCrustyLogic : MonoBehaviour
{
    
    public GameObject gameLogicHelper;
    
    private GameLogicHelper helper;
    
    int displayed = 0;
    private string levelStartString = @"Escort the gem filled asteroid Big Crusty (the asteroid in front of you) to the harvesting zone without it breaking!";
    private string tractorExplain = @"Use the tractor beam to help you.  Select an asteroid, turn it on with [T] and get close to the 'roid!";
    public GameObject[] boundaryTriggerObjects;
    TriggerHelperGroup[] boundaryTriggers;

    public GameObject passTriggerObject;
    TriggerHelperGroup passTrigger;
    
    bool done = false; 

    public GameObject ship;
    public GameObject startObject;
    public GameObject endObject;
    public GameObject goalAsteroidObj;
    private Asteroid goalAsteroid;

    float firstTime = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        helper = gameLogicHelper.GetComponent<GameLogicHelper>();
        goalAsteroid = goalAsteroidObj.GetComponent<Asteroid>();
        
        passTrigger = passTriggerObject.GetComponent<TriggerHelperGroup>();

        boundaryTriggers = new TriggerHelperGroup[boundaryTriggerObjects.Length];
        for (int i = 0; i < boundaryTriggers.Length; i++)
        {
            boundaryTriggers[i] = boundaryTriggerObjects[i].GetComponent<TriggerHelperGroup>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
        float progress = goalAsteroid.curHealth/goalAsteroid.maxHealth;
        progress = Mathf.Clamp01(progress);
        if (goalAsteroid.curHealth <= 0)
        {
            helper.ShowFail("GAME OVER!", "Big Crusty was distroyed!");
            done = true;
        }
        helper.UpdateProgress(progress, "Big Crusty Health");

        if (displayed == 0)
        {
            helper.DisplayPauseMessage("LEVEL2: BIG CRUSTY", levelStartString);
            displayed++;
            firstTime = Time.time + 0.2f;
        }
        else if (displayed == 1 && Time.time > firstTime)
        {
            helper.DisplayPauseMessage("Tractor Beam!", tractorExplain);
            displayed++;
            firstTime = Time.time + 0.2f;
        }

        for (int i = 0; i < boundaryTriggers.Length; i++)
        {
            if (boundaryTriggers[i].isTriggered && !done)
            {
               helper.ShowFail("GAME OVER!", "You or big crusty left the play area.  Stay out of the red boxes.");
               done = true;
            }
        }
        if (passTrigger.isTriggered && !done)
        {
            helper.ShowPass("CONGRATS!", "Nice Job!  You led big crusty to slaughter");
            done = true;
        }

    }
}
