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
   
    public float tractorRange = 10f;
    public float tractorMaxForce = 1000f;
    public float tractorBreakDistance = 15f;
    public float tractorSpringConstant = 500f;
    public float tractorDamping = 50f;
    public GameObject tractorIndicator;
    private Renderer tractorIndicatorRenderer;
    public GameObject tractorTarget;
    public GameObject tractorFocus;
    private bool isTractorEnabled = false;
    private bool isTractorConnected = false;
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
        tractorIndicatorRenderer = tractorIndicator.GetComponent<Renderer>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         
    }

    // Update is called once per frame
    void Update()
    {
        rb.AddForce(currentForce);
        rb.AddTorque(new Vector3(0,currentRotVel,0));
        if (tractorTarget == null)
        {
            isTractorConnected = false;
        }
        tractorIndicatorRenderer.enabled = isTractorEnabled;

        if (isTractorEnabled)
        {
            if (isTractorConnected)
            {
                Vector3 tractorDirection = tractorTarget.transform.position - tractorFocus.transform.position;
                float distance = tractorDirection.magnitude;

                if (distance > tractorBreakDistance)
                {
                    isTractorConnected = false;
                }
                else
                {
                    Rigidbody targetRb = tractorTarget.GetComponent<Rigidbody>();
                    Vector3 tractorForce = tractorDirection.normalized * tractorSpringConstant * distance*distance;
                    Vector3 relativeVelocity = rb.linearVelocity - targetRb.linearVelocity;
                    Vector3 dampingForce = -relativeVelocity * tractorDamping;
                    Vector3 totalTractorForce = Vector3.ClampMagnitude(tractorForce + dampingForce, tractorMaxForce);

                    // rb.AddForce(totalTractorForce);

                    if (targetRb != null)
                    {
                        targetRb.AddForce(-totalTractorForce);
                    }
                }
            }
            else 
            {
                float distance = Vector3.Distance(tractorFocus.transform.position, tractorTarget.transform.position);
                if (distance <= tractorRange)
                {
                    isTractorConnected = true;
                }
            }
        }

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

    public void Fire(GameObject[] targets)
    {
        foreach (GameObject weapon in weapons)
        {
            Turret script = weapon.GetComponent<Turret>();
            script.Fire(targets);
        }
    }
    public void ToggleTractorState()
    {
        isTractorEnabled = !isTractorEnabled;
    }
}
