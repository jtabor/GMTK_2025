using UnityEngine;
using Unity.Cinemachine;


public class PlayerShip : MonoBehaviour
{
    public Vector3 maxForce = new Vector3(0f,0f,0f);
    public float boostForce = 200f;
    public float rotationalVelMax = 1.0f;
    public float mass = 1000f;
    public float maxTorque = 100f;
   
    public float maxHealth = 100f;
    public float maxSheilds = 100f;
    private float curHealth = 100f;
    private float curShields = 100f;

    public float shieldRechargeRate = 10f;
    public float shieldRechargeDelay = 3f;
    public float shieldFadeOutTime = 2f;
    public GameObject shieldObject;

    //To prevent race conditions on collisions
    private float prevShields = 0f;
    private float lastShieldDamageTime = -999f;
    private Material shieldMaterial;
    private Color originalShieldColor;
    private bool isShieldFading = false;
    private float shieldFadeStartTime;

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
    
    public float blinkDistance = 5;
    public int maxBlinkCharges = 3;
    public float blinkRechargeTime = 2f;
    public float blinkCooldownTime = 0.5f;
    
    private int currentBlinkCharges;
    private float lastBlinkTime = -999f;
    private float lastRechargeTime = 0f;
    private bool isBlinkPreviewing = false;
    private bool isBlinkKeyHeld = false;
    public GameObject blinkFocus;
    private Renderer blinkIndicatorRenderer;
    CinemachineCamera defaultCamera; 

    private Vector3 hitPos;
    private Vector3 hitDir;


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
        currentBlinkCharges = maxBlinkCharges;
        
        blinkFocus = Instantiate(blinkFocus, transform.position, transform.rotation);

        GameObject blinkIndicator = blinkFocus.transform.Find("BlinkIndicator").gameObject;
        blinkIndicatorRenderer = blinkIndicator.GetComponent<Renderer>();
        blinkIndicatorRenderer.enabled = false;
        
        // Initialize shield system
        curHealth = maxHealth;
        curShields = maxSheilds;
        if (shieldObject != null)
        {
            Renderer shieldRenderer = shieldObject.GetComponent<Renderer>();
            if (shieldRenderer != null)
            {
                Debug.Log("DEBUG GOT SHIELD RENDER");
                shieldMaterial = shieldRenderer.material;
                originalShieldColor = shieldMaterial.GetColor("_shieldColor");
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         
    }

    // Update is called once per frame
    void Update()
    {
        prevShields = curShields;
        rb.AddForce(currentForce);
        rb.AddTorque(new Vector3(0,currentRotVel,0));
        if (tractorTarget == null)
        {
            isTractorConnected = false;
        }
        tractorIndicatorRenderer.enabled = (isTractorConnected || isTractorEnabled);
        
        if (!isTractorEnabled)
        {
            if (isTractorConnected)
            {
                isTractorConnected = false;
                //TODO: Disconnect Effect 
            }
        }

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

        // Handle blink charge recharging
        if (currentBlinkCharges < maxBlinkCharges && Time.time >= lastRechargeTime + blinkRechargeTime)
        {
            currentBlinkCharges++;
            lastRechargeTime = Time.time;
        }
        
        // Check if blink key is held but we couldn't preview due to no charges/cooldown
        if (isBlinkKeyHeld && !isBlinkPreviewing)
        {
            // Check if blink is now available
            if (currentBlinkCharges > 0 && Time.time >= lastBlinkTime + blinkCooldownTime)
            {
                // Start previewing now that blink is available
                isBlinkPreviewing = true;
                blinkIndicatorRenderer.enabled = true;
                UpdateBlinkPreview();
            }
        }
        
        // Update blink preview position continuously while previewing
        if (isBlinkPreviewing)
        {
            UpdateBlinkPreview();
        }
        
        // Handle shield recharging
        if (curShields < maxSheilds && Time.time >= lastShieldDamageTime + shieldRechargeDelay)
        {
            curShields += shieldRechargeRate * Time.deltaTime;
            curShields = Mathf.Min(curShields, maxSheilds);
        }
        
        // Handle shield visual effects
        if (isShieldFading && shieldMaterial != null)
        {
            float fadeProgress = (Time.time - shieldFadeStartTime) / shieldFadeOutTime;
            if (fadeProgress >= 1f)
            {
                // Fade complete
                isShieldFading = false;
                Color finalColor = originalShieldColor;
                finalColor.a = originalShieldColor.a;
                shieldMaterial.SetColor("_shieldColor", finalColor);
            }
            else
            {
                // Fade from 50/255 alpha back to original alpha
                float startAlpha = 50f / 255f;
                float targetAlpha = originalShieldColor.a;
                float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, fadeProgress);
                
                Color currentColor = originalShieldColor;
                currentColor.a = currentAlpha;
                shieldMaterial.SetColor("_shieldColor", currentColor);
            }
        }
    }
    
    private void UpdateBlinkPreview()
    {
        Vector3 targetPosition;
        
        if (isTractorConnected && tractorTarget != null)
        {
            // Calculate tractor blink position
            Vector3 shipToTarget = tractorTarget.transform.position - rb.transform.position;
            float distanceToTarget = shipToTarget.magnitude;
            Vector3 directionToTarget = shipToTarget.normalized;
            targetPosition = rb.transform.position + directionToTarget * (distanceToTarget * 2f);
        }
        else
        {
            // Normal blink position
            Vector3 blinkOffset = rb.transform.TransformDirection(Vector3.right * blinkDistance);
            targetPosition = rb.transform.position + blinkOffset;
        }
        
        blinkFocus.transform.position = targetPosition;
    }
    
    public void ReplaceHardpoint(GameObject weapon, int index)
    {
        
        Destroy(weapons[index]);
        weapons[index] = weapon;
        weapon.transform.SetParent(hardpoints[index].transform, false);

    }
    public void HandleControlInput(Vector3 control_input, float rotation, bool boostActive)
    {
        currentForce = transform.TransformDirection(Vector3.Scale(maxForce,control_input));
        if (boostActive)
        {
            currentForce = currentForce + transform.TransformDirection(new Vector3(boostForce,0f,0f));
        }
        currentRotVel = rotation*maxTorque; 
    }

    public void HandleCameraInput(bool slaveToMouse, float scrollDelta)
    {
        // camInputController = defaultCamera.GetComponent<CinemachineInputAxisController>();
        camInputController.enabled = slaveToMouse;
        currentZoom += scrollDelta*ZOOM_SPEED;
        currentZoom = Mathf.Clamp(currentZoom,ZOOM_MIN,ZOOM_MAX);
        LensSettings currentLens = this.defaultCamera.Lens;
        currentLens.FieldOfView = currentZoom;
        this.defaultCamera.Lens = currentLens;
    }

    public void Blink(bool keyDown)
    {
        isBlinkKeyHeld = keyDown;
        
        if (keyDown)
        {
            // Check if blink is available (has charges and not on cooldown)
            if (currentBlinkCharges > 0 && Time.time >= lastBlinkTime + blinkCooldownTime)
            {
                // Start preview: enable indicator and start continuous updating
                isBlinkPreviewing = true;
                blinkIndicatorRenderer.enabled = true;
                UpdateBlinkPreview();
            }
            // If not available, isBlinkKeyHeld will be true and Update() will handle it when charges are available
        }
        else
        {
            // Key released: perform the actual blink if we were previewing
            if (isBlinkPreviewing)
            {
                isBlinkPreviewing = false;
                blinkIndicatorRenderer.enabled = false;
                
                // Consume a charge and set cooldown
                currentBlinkCharges--;
                lastBlinkTime = Time.time;
                
                //TODO Effects
                if (isTractorConnected && tractorTarget != null)
                {
                    // Calculate direction from ship to tractor target
                    Vector3 shipToTarget = tractorTarget.transform.position - rb.transform.position;
                    float distanceToTarget = shipToTarget.magnitude;
                    Vector3 directionToTarget = shipToTarget.normalized;
                    
                    // Blink twice the distance in the direction of the target
                    Vector3 newPosition = rb.transform.position + directionToTarget * (distanceToTarget * 2f);
                    rb.transform.position = newPosition;
                    
                    // Mirror the y rotation along the line perpendicular to the ship-tractorTarget line
                    Vector3 perpendicular = Vector3.Cross(directionToTarget, Vector3.up);
                    if (perpendicular.magnitude > 0.001f) // Avoid division by zero
                    {
                        perpendicular = perpendicular.normalized;
                        Vector3 currentForward = rb.transform.forward;
                        Vector3 mirroredForward = Vector3.Reflect(currentForward, perpendicular);
                        rb.transform.rotation = Quaternion.LookRotation(mirroredForward, Vector3.up);
                    }
                }
                else
                {
                    // Normal blink: move blinkDistance along the x-axis
                    Vector3 blinkOffset = rb.transform.TransformDirection(Vector3.right * blinkDistance);
                    rb.transform.position += blinkOffset;
                }
            }
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
   
    //Check if there are sheilds left for applying damage to other stuff
    public bool HasShields()
    {   
        return prevShields > 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject otherObject = collision.gameObject;
        // Laser laser = otherObject.GetComponent<Laser>();
        Rigidbody rb = otherObject.GetComponent<Rigidbody>();
        // PlayerShip ship = otherObject.GetComponent<PlayerShip>();
        // Ship only has collision damage for now
        float damage = 0;
        
        if (rb != null && otherObject.tag != "Player")
        {
            Vector3 relVel = collision.relativeVelocity;
            damage = rb.mass * relVel.magnitude; 
            hitPos = collision.contacts[0].point;
            hitDir = relVel.normalized;
            DoDamage(damage, hitDir);
        }
        
    }
    private void DoDamage(float damage, Vector3 hitDir)
    {
        if (curShields > 0)
        {
            // Shield takes damage first
            float shieldDamage = Mathf.Min(damage, curShields);
            curShields -= shieldDamage;
            damage -= shieldDamage;
            
            // Record shield damage time for recharge delay
            lastShieldDamageTime = Time.time;
            
            // Start shield visual effect
            if (shieldMaterial != null && shieldDamage > 0)
            {
                Color flashColor = originalShieldColor;
                flashColor.a = 50f / 255f;
                Debug.Log("hitDir: " + hitDir);
                shieldMaterial.SetColor("_shieldColor", flashColor);
                shieldMaterial.SetVector("_hitDir", -hitDir.normalized);
                shieldMaterial.SetFloat("_hitStrength", 2);
                isShieldFading = true;
                shieldFadeStartTime = Time.time;
            }
            
            Debug.Log("Shield damage: " + shieldDamage + " - Shields remaining: " + curShields);
        }
        
        // Any remaining damage goes to health
        if (damage > 0)
        {
            curHealth -= damage;
            Debug.Log("Health damage: " + damage + " - Health remaining: " + curHealth);
        }
        
        if (curHealth <= 0) {
            Debug.Log("PLAYER SHIP DESTROYED!");
            //TODO Add Losing.
        }
    }


}
