using UnityEngine;
using Unity.Cinemachine;


public class PlayerShip : MonoBehaviour
{
    public Vector3 maxForce = new Vector3(0f,0f);
    public float rotationalVelMax = 1.0f;
    private Vector3 currentForce = new Vector3(0f,0f);
    private Vector3 currentVelocity = new Vector3(0f,0f);
    private float currentRotVel = 0f;

    public const float ZOOM_SPEED = 7.0f;
    public const float ZOOM_MIN = 20.0f;
    public const float ZOOM_MAX = 90.0f;
    private float currentZoom = 60; 

    CinemachineCamera defaultCamera; 

    CinemachineInputAxisController camInputController;
    void Awake()
    {
        defaultCamera = FindFirstObjectByType<CinemachineCamera>();
 
        // defaultCamera = GetComponent<CinemachineCamera>();
        camInputController = defaultCamera.GetComponent<CinemachineInputAxisController>();

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
 
    }

    // Update is called once per frame
    void Update()
    {
        //TODO - probably want to make this a rigidbody instead.
        currentVelocity = currentVelocity + transform.TransformDirection(currentForce*Time.deltaTime); 
        transform.position = transform.position + currentVelocity*Time.deltaTime;
        transform.Rotate(0, currentRotVel * Time.deltaTime, 0, Space.Self);
    }

    public void HandleControlInput(Vector3 control_input, float rotation)
    {
        currentForce = Vector3.Scale(maxForce,control_input);
        currentRotVel = rotation*rotationalVelMax; 
    }
    public void HandleCameraInput(bool slaveToMouse, float scrollDelta)
    {
        // camInputController = defaultCamera.GetComponent<CinemachineInputAxisController>();
        camInputController.enabled = slaveToMouse;
        if (slaveToMouse){
            currentZoom += scrollDelta*ZOOM_SPEED;
            currentZoom = Mathf.Clamp(currentZoom,ZOOM_MIN,ZOOM_MAX);
            LensSettings currentLens = this.defaultCamera.Lens;
            currentLens.FieldOfView = currentZoom;
            this.defaultCamera.Lens = currentLens;
        }
    }


}
