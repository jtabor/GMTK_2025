using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    public GameObject controlledObject;
    private float zoomDegrees = 90;
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
                    rotation += 1f; 
                if (Keyboard.current.dKey.isPressed)
                    rotation += -1f;

                ship.HandleControlInput(movement,rotation);

                // Handle QE turret rotation
                float turretRotation = 0f;
                if (Keyboard.current.qKey.isPressed)
                    turretRotation = -1f;
                if (Keyboard.current.eKey.isPressed)
                    turretRotation = 1f;
                // if (Keyboard.current.spaceKey.isPressed)
                // {
                //     ship.shoot();
                // }
                // ship.HandleTurretRotation(turretRotation);
                Vector2 scrollDelta = Mouse.current.scroll.ReadValue();
                float scrollY = scrollDelta.y;
                bool rtMouse = Mouse.current.rightButton.isPressed;
                ship.HandleCameraInput(rtMouse,scrollY);

            }
        }
        
    }



}
