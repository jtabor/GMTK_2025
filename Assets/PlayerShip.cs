using UnityEngine;
using Unity.Cinemachine;


public class PlayerShip : MonoBehaviour
{
    public Vector3 maxForce = new Vector3(0f,0f,0f);
    public float rotationalVelMax = 1.0f;
    public float mass = 1000f;
    public float maxTorque = 100f;
    
    public GameObject[] hardpoints;
    public GameObject[] weapons;

    private Vector3 currentForce = new Vector3(0f,0f,0f);
    private Vector3 currentVelocity = new Vector3(0f,0f,0f);
    private float currentRotVel = 0f;

    public const float ZOOM_SPEED = 7.0f;
    public const float ZOOM_MIN = 20.0f;
    public const float ZOOM_MAX = 90.0f;
    private float currentZoom = 60; 
    
    private Rigidbody rb;

    CinemachineCamera defaultCamera; 

    CinemachineInputAxisController camInputController;
    void Awake()
    {
        defaultCamera = FindFirstObjectByType<CinemachineCamera>();
 
        // defaultCamera = GetComponent<CinemachineCamera>();
        camInputController = defaultCamera.GetComponent<CinemachineInputAxisController>();
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.maxAngularVelocity = rotationalVelMax;
        //Maybe set the CG here?  You can do it manually
        //
        weapons = new GameObject[hardpoints.Length];

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
 
    }

    // Update is called once per frame
    void Update()
    {
        //TODO - probably want to make this a rigidbody instead.
        // currentVelocity = currentVelocity + transform.TransformDirection(currentForce*Time.deltaTime); 
        // transform.position = transform.position + currentVelocity*Time.deltaTime;
        // transform.Rotate(0, currentRotVel * Time.deltaTime, 0, Space.Self);
        rb.AddForce(currentForce);
        rb.AddTorque(new Vector3(0,currentRotVel,0));
    }
    public void ReplaceHardpoint(GameObject weapon, int index)
    {
        
        Destroy(weapons[index]);
        weapons[index] = weapon;
        weapon.transform.SetParent(hardpoints[index].transform, false);

    }
    public void HandleControlInput(Vector3 control_input, float rotation)
    {
        currentForce = transform.TransformDirection(Vector3.Scale(maxForce,control_input));
        currentRotVel = rotation*maxTorque; 
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
