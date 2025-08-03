using UnityEngine;
using Unity.Cinemachine;
using Unity.Mathematics;


public class PlayerShip : MonoBehaviour
{
    public RenderTexture minimapTexture;
    public Camera minimapCamera;

    public GameObject gameLogic;
    private AudioManager audioManager;   
    
    //CLAUDE - Make a feature to display the shield and health.  The health has 4 icons, and the shield has 2
    // Display them in order they're in inside the arrays.
    // For Shields don't display any shields if curShields <= 0, 1 if it's between 0 and 50%, and two if it's above 50%
    // Do the same for the health, but split it in 4 instead.
    
    public GameObject[] blinkIcons;
    public GameObject[] healthIcons;
    public GameObject[] shieldIcons;

    public AudioClip shieldBreakClip;
    public AudioClip shieldHitClip;
    public AudioClip blinkClip;

    public Vector3 maxForce = new Vector3(100f,0f,100f);
    public float boostForce = 200f;
    public float rotationalVelMax = 1.0f;
    public float mass = 1000f;
    public float maxTorque = 100f;
    public float brakeDamping = 0.1f;
    bool isBraking = false;

    public float maxHealth = 100f;
    public float maxSheilds = 100f;
    public float curHealth = 100f;
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
    private ParticleSystem.EmissionModule tractorIndicatorEmitter;
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

    private float invulnerableTimestamp = -99f;
    private ParticleSystem engineParticleSystem;
    public float engineEmmisionRateOverTime = 50f;
    public float engineEmmisionRateOverTimeBoost = 100f;
    public Color engineNormalColor;
    public Color engineBoostColor;

    private AudioSource audioSource; 

    CinemachineInputAxisController camInputController;
    Collider shipCollider;
    void Awake()
    {
        defaultCamera = FindFirstObjectByType<CinemachineCamera>();
        minimapCamera.targetTexture = minimapTexture;
        
        shipCollider = gameObject.GetComponent<Collider>();

        // defaultCamera = GetComponent<CinemachineCamera>();
        camInputController = defaultCamera.GetComponent<CinemachineInputAxisController>();
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.maxAngularVelocity = rotationalVelMax;
        //Maybe set the CG here?  You can do it manually
        //
        // weapons = new GameObject[hardpoints.Length];
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i] = Instantiate(weapons[i],hardpoints[i].transform);
            Turret t = weapons[i].GetComponent<Turret>();
            if (t != null)
            {
                t.gameLogic = gameLogic;
            }
        }
        tractorIndicatorRenderer = tractorIndicator.GetComponent<Renderer>();
        tractorIndicatorEmitter = tractorIndicator.GetComponent<ParticleSystem>().emission;
        currentBlinkCharges = maxBlinkCharges;
        UpdateBlinkChargeDisplay();
        
        blinkFocus = Instantiate(blinkFocus, transform.position, transform.rotation);

        GameObject blinkIndicator = blinkFocus.transform.Find("BlinkIndicator").gameObject;
        blinkIndicatorRenderer = blinkIndicator.GetComponent<Renderer>();
        blinkIndicatorRenderer.enabled = false;
        
        // Initialize shield system
        curHealth = maxHealth;
        curShields = maxSheilds;
        UpdateHealthDisplay();
        UpdateShieldDisplay();
        if (shieldObject != null)
        {
            Renderer shieldRenderer = shieldObject.GetComponent<Renderer>();
            if (shieldRenderer != null)
            {
                shieldMaterial = shieldRenderer.material;
                originalShieldColor = shieldMaterial.GetColor("_shieldColor");
            }
        }


    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioManager = gameLogic.GetComponent<AudioManager>();
        engineParticleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (tractorIndicatorRenderer.enabled)
        {
            Material m = tractorIndicatorRenderer.material;
            m.SetVector("_center", tractorIndicator.transform.position);
        }
    }

    void FixedUpdate()
    {
        prevShields = curShields;

        if (isBraking)
        {
            Vector3 brakeForce = - rb.linearVelocity * brakeDamping;
            rb.AddForce(brakeForce);
        }
        
        if (!shipCollider.enabled && Time.time > invulnerableTimestamp)
        {
            shipCollider.enabled = true;
        }

        rb.AddForce(currentForce);
        rb.AddTorque(new Vector3(0,currentRotVel,0));
        if (tractorTarget == null)
        {
            isTractorConnected = false;
        }
        {
            tractorIndicatorRenderer.enabled = (isTractorConnected || isTractorEnabled);
            tractorIndicatorEmitter.enabled = (isTractorConnected || isTractorEnabled);
        }
        if (!isTractorEnabled)
        {
            if (isTractorConnected)
            {
                isTractorConnected = false;
                //TODO: Disconnect Effect 
            }
        }
        if (!isTractorConnected)
        {
            audioManager.StopTractorNoise();
        }
        if (isTractorEnabled)
        {
            if (isTractorConnected)
            {
                audioManager.StartTractorNoise();                

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
            UpdateBlinkChargeDisplay();
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
            audioManager.StartBlinkNoise();
        }
        else
        {
            audioManager.StopBlinkNoise();
        }
        
        // Handle shield recharging
        if (curShields < maxSheilds && Time.time >= lastShieldDamageTime + shieldRechargeDelay)
        {
            float previousShields = curShields;
            curShields += shieldRechargeRate * Time.deltaTime;
            curShields = Mathf.Min(curShields, maxSheilds);
            
            if (previousShields != curShields)
            {
                UpdateShieldDisplay();
            }
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
    
    private void UpdateBlinkChargeDisplay()
    {
        if (blinkIcons == null) return;
        
        for (int i = 0; i < blinkIcons.Length; i++)
        {
            if (blinkIcons[i] != null)
            {
                blinkIcons[i].SetActive(i < currentBlinkCharges);
            }
        }
    }
    
    private void UpdateShieldDisplay()
    {
        if (shieldIcons == null) return;
        
        int shieldsToShow = 0;
        if (curShields > 0)
        {
            float shieldPercentage = curShields / maxSheilds;
            if (shieldPercentage > 0.5f)
            {
                shieldsToShow = 2;
            }
            else
            {
                shieldsToShow = 1;
            }
        }
        
        for (int i = 0; i < shieldIcons.Length; i++)
        {
            if (shieldIcons[i] != null)
            {
                shieldIcons[i].SetActive(i < shieldsToShow);
            }
        }
    }
    
    private void UpdateHealthDisplay()
    {
        if (healthIcons == null) return;
        
        int healthIconsToShow = 0;
        if (curHealth > 0)
        {
            float healthPercentage = curHealth / maxHealth;
            if (healthPercentage > 0.75f)
            {
                healthIconsToShow = 4;
            }
            else if (healthPercentage > 0.5f)
            {
                healthIconsToShow = 3;
            }
            else if (healthPercentage > 0.25f)
            {
                healthIconsToShow = 2;
            }
            else
            {
                healthIconsToShow = 1;
            }
        }
        
        for (int i = 0; i < healthIcons.Length; i++)
        {
            if (healthIcons[i] != null)
            {
                healthIcons[i].SetActive(i < healthIconsToShow);
            }
        }
    }
    
    public void ReplaceHardpoint(GameObject weapon, int index)
    {
        
        Destroy(weapons[index]);
        weapons[index] = weapon;
        weapon.transform.SetParent(hardpoints[index].transform, false);

    }
    public void HandleControlInput(Vector3 control_input, float rotation, bool boostActive, bool brakeActive)
    {
        currentForce = transform.TransformDirection(Vector3.Scale(maxForce,control_input));
        ParticleSystem.MainModule mainModule = engineParticleSystem.main;
        ParticleSystem.EmissionModule emissionModule = engineParticleSystem.emission;
        bool playEngineSound = false;
        if (currentForce.magnitude > 0)
        {
            mainModule.startColor = engineNormalColor;
            emissionModule.rateOverTime = engineEmmisionRateOverTime;
            playEngineSound = true;
        }

        if (boostActive)
        {
            currentForce = currentForce + transform.TransformDirection(new Vector3(boostForce,0f,0f));
            mainModule.startColor = engineBoostColor;
            emissionModule.rateOverTime = engineEmmisionRateOverTimeBoost;
            playEngineSound = true;
        }
        if (playEngineSound)
        {
            audioManager.StartEngineNoise();
        }
        else
        {
            audioManager.StopEngineNoise();
        }
        if (Vector3.Dot(currentForce, transform.localToWorldMatrix * (new Vector3(1, 0, 0))) > 0)
        {
            if (!emissionModule.enabled)
            {
                emissionModule.enabled = true;
            }
        }
        else
        {
            if (emissionModule.enabled)
            {
                emissionModule.enabled = false;
            }
        }
        currentRotVel = rotation*maxTorque;
        
        isBraking = brakeActive;
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
            else
            {
                blinkIndicatorRenderer.enabled = false;
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
                if (audioManager != null && shieldBreakClip != null)
                {
                    audioManager.PlayEffectClip(blinkClip, AudioManager.AudioSourceType.EFFECT, transform.position, -1f);
                }
                // Consume a charge and set cooldown
                currentBlinkCharges--;
                lastBlinkTime = Time.time;
                UpdateBlinkChargeDisplay();
                
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
                    invulnerableTimestamp = Time.time + 0.5f;
                    // shipCollider.enabled = false;
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
            DamageNumbers otherDamageNumbers = otherObject.GetComponent<DamageNumbers>();
            damage = rb.mass* relVel.magnitude;
            if (otherDamageNumbers)
            {
                damage = Mathf.Min(otherDamageNumbers.maxDamageDone, damage);
            }
            damage = Mathf.Min(GetComponent<DamageNumbers>().maxDamageTaken, damage);
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

            if (curShields <= 0)
            {
                if (audioManager != null && shieldBreakClip != null)
                {
                    audioManager.PlayEffectClip(shieldBreakClip, AudioManager.AudioSourceType.EFFECT, transform.position, -1f);
                }
            }
            else
            {
                if (audioManager != null && shieldHitClip != null)
                {
                    audioManager.PlayEffectClip(shieldHitClip, AudioManager.AudioSourceType.EFFECT, transform.position, -1f);
                }
            }

            // Record shield damage time for recharge delay
            lastShieldDamageTime = Time.time;
            
            // Start shield visual effect
            if (shieldMaterial != null && shieldDamage > 0)
            {
                Color flashColor = originalShieldColor;
                flashColor.a = 50f / 255f;
                shieldMaterial.SetColor("_shieldColor", flashColor);
                shieldMaterial.SetVector("_hitDir", transform.worldToLocalMatrix.MultiplyVector(-hitDir.normalized));
                shieldMaterial.SetFloat("_hitStrength", 2);
                isShieldFading = true;
                shieldFadeStartTime = Time.time;
            }
            
            Debug.Log("Shield damage: " + shieldDamage + " - Shields remaining: " + curShields);
            UpdateShieldDisplay();
        }
        
        // Any remaining damage goes to health
        if (damage > 0)
        {
            curHealth -= damage;
            Debug.Log("Health damage: " + damage + " - Health remaining: " + curHealth);
            UpdateHealthDisplay();
        }
        
        if (curHealth <= 0) {
            Debug.Log("PLAYER SHIP DESTROYED!");
            //TODO Add Losing.
        }
    }


}
