//
//
// A simple First-person perspective player controller. 
//
//


using UnityEngine;
using System.Collections;
using System;

struct ActivatePlayer
{
}
struct DeactivatePlayer
{
}

struct FollowCommand
{
    public Vector3 point;
    public Transform trans;
}
struct StayCommand
{
    public Vector3 point;
}

public class PlayerController : FFComponent
{

    FFAction.ActionSequence transformationSeq;

    public bool active = false;
    
    public float m_movementSpeed = 3.2f;
    public float m_jumpSpeed = 3.5f;
    public float m_sprintMultiplier = 0.5f;

    public float m_maxMoveSpeed = 5.0f;
    public float m_movementDecay = 0.25f; // how quickly we reach our speed limits

    public float m_mouseSensitivity = 0.4f;


    public Color pigColor = Color.red;
    public Color GhostColor = Color.cyan;
    public float TransformationTime = 1.25f;
    public float verticalSteerOffset = 1.0f;


    public float PigSpeed = 1.5f;
    public float PigRotSpeed = 5.5f;
    public float GhostSpeed = 3.25f;
    public float GhostRotSpeed = 10.5f;


    public AudioClip TurnToGhostAudio;
    public AudioClip TurnToPigAudio;

    public enum State
    {
        None,
        Ghost,
        Pig,
    }

    public Camera playerCamera;
    public State state = State.None;

    //private Rigidbody rigid; 

    // Use this for initialization
    void Start ()
    {
        limitPlayClipSeq = action.Sequence();
        UpdatePlayClipLimitTime(); // start updating this

        transformationSeq = action.Sequence();

        //rigid = GetComponent<Rigidbody>();
        UpdatePlayerControllerData();
        
        FFMessage<ActivatePlayer>.Connect(OnActivatePlayer);
        FFMessage<DeactivatePlayer>.Connect(OnDeactivatePlayer);
        FFMessageBoard<FallingIntoPit>.Connect(OnFallingIntoPit, gameObject);
        FFMessageBoard<OnSolidGround>.Connect(OnOnSolidGround, gameObject);

        {// Setup colors for materials
            var pigMaterial = FFResource.Load_Material("Pig");
            var ghostMaterial = FFResource.Load_Material("Ghost");
            pigMaterial.color = pigColor;
            ghostMaterial.color = GhostColor;
        }
        

        // Start as a ghost
        ChangeState(State.Ghost);
	}
    

    void OnDestroy()
    {
        FFMessage<ActivatePlayer>.Disconnect(OnActivatePlayer);
        FFMessage<DeactivatePlayer>.Disconnect(OnDeactivatePlayer);
        FFMessageBoard<FallingIntoPit>.Disconnect(OnFallingIntoPit, gameObject);
        FFMessageBoard<OnSolidGround>.Disconnect(OnOnSolidGround, gameObject);
    }


    float FallTimmer = 0;
    float FallTimeTillReset = 1.5f;
    private void OnOnSolidGround(OnSolidGround e)
    {
        FallTimmer = 0.0f;
    }

    private void OnFallingIntoPit(FallingIntoPit e)
    {
        //Debug.Log("FAlling into pit!");
        FallTimmer += Time.fixedDeltaTime;

        if (FallTimmer > FallTimeTillReset)
        {
            FallTimmer = 0.0f;
            ResetPlayerToLastCheckpoint rptlc;
            FFMessage<ResetPlayerToLastCheckpoint>.SendToLocal(rptlc);
        }
    }

    private void OnActivatePlayer(ActivatePlayer e)
    {
        active = true;
        UpdatePlayerControllerData();
    }

    private void OnDeactivatePlayer(DeactivatePlayer e)
    {
        UpdatePlayerControllerData();
        active = false;
    }

    #region helpers

    void DisbleGO(object go)
    {
        ((GameObject)go).SetActive(false);
    }
    void EnableGo(object go)
    {
        ((GameObject)go).SetActive(true);
    }
    public Vector3 GetPlayerPosition()
    {
        return transform.position;
    }

    #endregion


    float transformationCooldownTimer = 0.0f;
    public float TransformationCooldownTime = 1.6f;

    // Update is called once per frame
    void Update ()
    {
        if(MenuController.GetState() != MenuState.PlayGame) // only take input if we are in the play game state
        {
            return;
        }

        {// Player Input
            var steering = GetComponent<Steering>();
            var mousePos = Input.mousePosition;
            var mousePress = Input.GetMouseButtonDown(0);
            Ray ray = playerCamera.ScreenPointToRay(mousePos);
            RaycastHit rayHit;
            string[] moveLayers = { "Default", "CameraFloor" };
            int raycastMask = LayerMask.GetMask(moveLayers); // default, TODO Add floor
            var raycastHitSomething = Physics.Raycast(ray, out rayHit, 100.0f, raycastMask);

            // Try and limit vertical movement when ghost so that we don't fly away!
            if(state == State.Ghost)
            {
                var rigid = GetComponent<Rigidbody>();
                var newYVelocity = Mathf.Lerp(rigid.velocity.y, 0.0f, 0.2f);

                rigid.velocity = new Vector3(
                    rigid.velocity.x,
                    newYVelocity,
                    rigid.velocity.z);

                // if we have a floor below, gradtually move to it. Don't if its a pit!
                {
                    Ray downRay = new Ray(transform.position, Vector3.down);
                    RaycastHit downRayHit;
                    string[] glideDownLayers = { "Default" };
                    if (Physics.Raycast(downRay, out downRayHit, 100.0f, LayerMask.GetMask(glideDownLayers)))
                    {
                        if(downRayHit.transform.tag != "Pit")
                        {
                            // height of player is 2.0 * 0.35...
                            // So we want to have thepoint of the ray be offset by 0.5 * height
                            var vecToHit = (downRayHit.point + (Vector3.up * (0.5f * 2.0f * 0.35f))) - transform.position;

                            //Debug.Log(vecToHit);
                            if(vecToHit.magnitude > 0.1f) // to high/low
                            {
                                rigid.AddForce(10.0f * vecToHit * Time.fixedDeltaTime, ForceMode.VelocityChange);
                            }
                        }
                    }
                }
            }
            

            // Interact/Move (Mouse)
            if (mousePress && raycastHitSomething)
            {
                // randomly choose between 1-3 for the pig noise to use
                if(state == State.Pig)
                {
                    int random1_3 = UnityEngine.Random.Range(1, 3);
                    PlayClipLimited(FFResource.Load_AudioClip("SFX/PigNoise/PigMove" + random1_3));
                }
                
                var targetPoint = rayHit.point + new Vector3(0, verticalSteerOffset, 0.0f); // offset 0.5 up
                steering.SetupTarget(rayHit.transform, targetPoint);
            }

            // Toggle Manifestation (Space bar)
            transformationCooldownTimer += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if(transformationCooldownTimer > TransformationCooldownTime)
                {
                    transformationCooldownTimer = 0.0f;

                    if (state == State.Pig)
                    {
                        ChangeState(State.Ghost);
                    }
                    else if (state == State.Ghost)
                    {
                        ChangeState(State.Pig);
                    }

                    UpdateSteeringValues();
                }
            }

            // Commands
            {
                // Stay
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // randomly choose between 1-3 for the pig noise to use
                    if (state == State.Pig)
                    {
                        PlayClipLimited(FFResource.Load_AudioClip("SFX/PigNoise/StayCommand"));
                    }

                    StayCommand sc;
                    sc.point = GetPlayerPosition();
                    FFMessage<StayCommand>.SendToLocal(sc);
                }

                // Follow
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    // randomly choose between 1-3 for the pig noise to use
                    if (state == State.Pig)
                    {
                        PlayClipLimited(FFResource.Load_AudioClip("SFX/PigNoise/FollowCommand"));
                    }

                    FollowCommand fc;
                    fc.trans = transform;
                    fc.point = GetPlayerPosition();
                    FFMessage<FollowCommand>.SendToLocal(fc);
                }
            }

            // WASD movement
            { // WASD movement
                Vector3 movementVector = Vector3.zero;

                if(Input.GetKey(KeyCode.W)) // Go Up
                {
                    movementVector += new Vector3(-1,0,0);
                }

                if (Input.GetKey(KeyCode.S)) // Go Down
                {
                    movementVector += new Vector3(1, 0, 0);
                }

                if (Input.GetKey(KeyCode.A)) // Go Left
                {
                    movementVector += new Vector3(0, 0, -1);
                }

                if (Input.GetKey(KeyCode.D)) // Go Right
                {
                    movementVector += new Vector3(0, 0, 1);
                }

                // Any movement?
                if(movementVector != Vector3.zero)
                {
                    var normMovementVec = Vector3.Normalize(movementVector);
                    var rayLocation = transform.position + (normMovementVec * (steering.targetRadius * 3.0f));
                    
                    RaycastHit movementHit;
                    Ray movementRay = new Ray(rayLocation + (Vector3.up * 50.0f), Vector3.down);

                    var rayHitStuff = Physics.Raycast(movementRay, out movementHit, 60.0f, raycastMask);

                    if(raycastHitSomething)
                    {
                        steering.SetupTarget(movementHit.transform, movementHit.point);
                    }
                    else
                    {
                        steering.SetupTarget(null, rayLocation);
                    }

                    ArrowMovementVector = movementVector;
                }
                else if(ArrowMovementVector != Vector3.zero) // used arrow key to move last update
                {
                    
                    var rayLocation = transform.position;

                    RaycastHit movementHit;
                    Ray movementRay = new Ray(rayLocation + (Vector3.up * 50.0f), Vector3.down);

                    var rayHitStuff = Physics.Raycast(movementRay, out movementHit, 60.0f, raycastMask);

                    if (raycastHitSomething)
                    {
                        steering.SetupTarget(movementHit.transform, movementHit.point);
                    }
                    else
                    {
                        steering.SetupTarget(null, rayLocation);
                    }

                    ArrowMovementVector = Vector3.zero;
                }


            }

        } // END Player Input


	}
    Vector3 ArrowMovementVector = Vector3.zero;
    

    float limitPlayClipTime = 1.7f;
    bool limitPlayClip = false;
    FFAction.ActionSequence limitPlayClipSeq;
    void UpdatePlayClipLimitTime()
    {
        limitPlayClip = false;
        limitPlayClipSeq.Delay(limitPlayClipTime);
        limitPlayClipSeq.Sync();
        limitPlayClipSeq.Call(UpdatePlayClipLimitTime);
    }
    void PlayClipLimited(AudioClip clip)
    {
        if (limitPlayClip)
            return;

        limitPlayClip = true;
        var audioSrc = GetComponent<AudioSource>();
        audioSrc.PlayOneShot(clip);
    }

    void PlayClip(AudioClip clip)
    {
        var audioSrc = GetComponent<AudioSource>();
        audioSrc.PlayOneShot(clip);
    }

    private void ChangeState(State newState)
    {
        if (newState == state)
            return;
        

        // Get model roots
        GameObject pigObj = transform.Find("PigModel").gameObject;
        GameObject ghostObj = transform.Find("GhostModel").gameObject;

        // reset sequence
        transformationSeq.ClearSequence();

        //  get change model material
        var pigMaterial = FFResource.Load_Material("Pig");
        var ghostMaterial = FFResource.Load_Material("Ghost");
        var pigColorRef = new FFRef<Color>(() => pigMaterial.color, (v) => { pigMaterial.color = v; });
        var ghostColorRef = new FFRef<Color>(() => ghostMaterial.color, (v) => { ghostMaterial.color = v; });

        if (newState == State.Ghost) // turned into a ghost
        {
            // enable ghost model
            ghostObj.SetActive(true);

            // disable gravity
            var rigid = GetComponent<Rigidbody>();
            rigid.useGravity = false;

            // Can move through walls
            //string ghostMask = "Ghost";
            //gameObject.layer = LayerMask.NameToLayer(ghostMask);
            PlayClip(TurnToGhostAudio);
            ActivateChildParticles(transform.Find("TurnToGhostParticles"));

            // Color property for each model mesh to change its color.
            transformationSeq.Property(pigColorRef, pigColor.MakeClear(), FFEase.E_SmoothStartEnd, TransformationTime);
            transformationSeq.Property(ghostColorRef, GhostColor, FFEase.E_SmoothStartEnd, TransformationTime);
            transformationSeq.Sync();
            transformationSeq.Call(DisbleGO, pigObj);
        }
        else if(newState == State.Pig) // turned into a pig
        {
            // enable pig model
            pigObj.SetActive(true);
            
            // enable gravity
            var rigid = GetComponent<Rigidbody>();
            rigid.useGravity = true;


            PlayClip(TurnToPigAudio);
            ActivateChildParticles(transform.Find("TurnToPigParticles"));

            // Cannot move thorugh walls
            //string physicalMask = "Physical";
            //gameObject.layer = LayerMask.NameToLayer(physicalMask);

            // Color property for each model mesh to change its color.
            transformationSeq.Property(pigColorRef, pigColor, FFEase.E_SmoothStartEnd, TransformationTime);
            transformationSeq.Property(ghostColorRef, GhostColor.MakeClear(), FFEase.E_SmoothStartEnd, TransformationTime);
            transformationSeq.Sync();
            transformationSeq.Call(DisbleGO, ghostObj);
        }

        state = newState;

        UpdateSteeringValues();
    }

    private void ActivateChildParticles(Transform transform)
    {
        foreach(Transform child in transform)
        {
            var particleSystem = child.GetComponent<ParticleSystem>();

            if(particleSystem != null)
            {
                particleSystem.Play();
            }
        }
    }

    private void UpdateSteeringValues()
    {
        var steering = GetComponent<Steering>();
        if (state == State.Ghost)
        {
            steering.maxSpeed = GhostSpeed;
            steering.rotationSpeed = GhostRotSpeed;
        }
        else if (state == State.Pig)
        {
            steering.maxSpeed = PigSpeed;
            steering.rotationSpeed = PigRotSpeed;
        }
    }

    private void UpdateUI()
    {
        // Update UI related stuff
        if(Input.GetKey(KeyCode.Escape))
        {
            // @TODO Add event to setup the menu
        }
    }

    //Vector2 m_MouseMovement; // movement of the mouse between frames
    private void UpdatePlayerControllerData()
    {
        // calculate movement movement and update Mouse position
        {
            //m_MouseMovement = new Vector2(
            //    Input.GetAxis("Mouse X"),
            //    Input.GetAxis("Mouse Y"));
        }
    }
}
