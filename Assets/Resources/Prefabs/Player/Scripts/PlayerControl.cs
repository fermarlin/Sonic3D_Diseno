using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    #region Variables
        const float bounceSpeed = 20f; // Velocidad del rebote al tocar un enemigo

        // The camera to use for movement
        public Camera mainCamera;

        #region Components

            public Animator anim;
                string animStateLast = "";

            AudioSource aud;

            Transform mesh; // The mesh of the player

        #endregion

        #region States

            public int state = stAir;
                const int stGrounded = 0; // State when the character is on the ground
                const int stAir = 1; // State when the character is in the air

                //Totally not sus new code

        #endregion

        #region Ground Movement

            float acc = groundAcc;
            float dec = groundDec;
            float fric = groundFric;
            const float groundAcc = 16f;
            const float groundDec = 24f;
            const float groundFric = 3f;
            const float revFric = 6f; //reverse friction (how fast your legs recover)
            float drift; // The amount of speed to reduce when turning a corner drift = currentSpd; currentSpd -= (drift*(angle/180f))

        #endregion

        #region Air (Jumping, Falling, Homing Attack)

            const float airAcc = 14f;
            const float airDec = 24f;
            const float airFric = 3f;

            //Homing Attack
                const float homingSpd = 5f;

        #endregion

        // The maximum speed of the player
            const float maxSpeed = 25f;
            float maxMomentum = 15f;
            float minMomentum = 0f;

        // The smoothness of the player's rotation
            const float rotationSmoothness = 15f; // The speed to rotate toward the intended movement direction
            const float slopeRotSmooth = 120f; // The speed to rotate toward the current slope angle
            float rotTurningAdjust = 12f; // the amount to lower rot Smoothness (slow down rotation/turning angle) with speed
            const float airRotSmooth = 20f;

        // The gravity force of the player
            public float gravityForce = 25f;
            const float gravStart = -5f;
            public float ySpd = gravStart;
            Vector3 measuredVelocity; //the velocity that has been measured each step
                float yVel; //the Y velocity that has been measured each step
            float termVel = -18f;
            float groundStickForce = -10f;

        //The stick correction (slow down if not holding the stick down all the way)
            float stickCorrection = 0f;
            float scSmooth = 40f; //the stick correction smoothing speed

        //Animation Speeds //the speeds needed for animation
            public const float walkSpd = maxSpeed*0.4f; //the top walking speed before transitioning to jogging
            public const float jogSpd = maxSpeed*0.8f; //the top Jogging speed speed before transitioning to running
            public const float runSpd = maxSpeed*1.2f; //the top run speed before going into sprint (circle feet)

        //Grounded
            public bool isGrounded = false;
            float groundCheckLength = 0.5f;
            float groundChecking = 0f;
            const float jumpTime = 0.1f;
            const float fallTime = 0.1f;
            const float hitGroundTime = 0.1f;

        //Jumping
            const float jumpSpd = 16f; //initial jump spd
            const float jumpAdd = 12f; //amount of jump to add each step if the button is held
            public bool jumping = false;

        // The character controller component
        CharacterController controller;

        // The movement direction of the player
        Vector3 movementDirection;

        // The movement acceleration of the player
        Vector3 movement;
        Vector3 momentum;

        // The current speed of the player
        public float currentSpeed;
        public float momentumSpd;

        // Rotation
            Quaternion targetRotation = Quaternion.identity;
            Quaternion rotationToGround = Quaternion.identity;
            Quaternion rotationLast = Quaternion.identity;
            float turnAngle = 0f; // the angle that you turn the character each step
            const float turningSlowdownGround = 22f; // the amount of slowdown when turning
            const float turningSlowdownAir = 10f; // the amount of slowdown when turning in air

        [Header("Sounds")]

        //Sounds
            public AudioClip aJump; //Jump Sound
            public AudioClip aHoming; //Homing Sound

        #region Inputs

            bool inputSpindashPress() { return Input.GetButtonDown("Button 1") || Input.GetButtonDown("Button 2"); }

        #endregion

        #region Slope

            Vector3 slopeNormal = Vector3.up; // the normal of the slope you're currently on
            Vector3 slopeChange = Vector3.up; // the normal of the slope change measurement

        #endregion
    
    #endregion
    

    void Start()
    {
        // Get components
            controller = GetComponent<CharacterController>();
            aud = GetComponent<AudioSource>();
            mesh = transform.Find("Mesh");

        #region Setup

            #region Set "Change/Last" Measurement variables

                rotationLast = transform.rotation;

            #endregion

        #endregion
    }

    void Update()
    {

        #region Get Input

            //Get initial position
                Vector3 initPos = transform.position;

            // Get the movement input
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");
                Vector3 stickInput = new Vector3(horizontalInput, 0, verticalInput);
                    float stickDZ = 0.25f;
                    float stickInputClamp = Mathf.Clamp(stickInput.magnitude, stickDZ, 1f);

            // Calculate the movement direction based on the camera's forward and right vectors
                movementDirection = mainCamera.transform.forward * verticalInput + mainCamera.transform.right * horizontalInput;

            // Normalize the movement direction
                movementDirection.Normalize();
                movementDirection.y = 0;

            #region Stick Correction (Slow down if stick isn't held down all the way)

                if (stickCorrection<stickInputClamp)
                {
                    stickCorrection = stickInputClamp;
                    if (stickCorrection<stickDZ) stickCorrection = stickDZ;
                    if (stickCorrection > stickInputClamp) stickCorrection = stickInputClamp;
                }
                if (stickCorrection>stickInputClamp && stickInput.magnitude>0)
                {
                    stickCorrection -= (dec/scSmooth) * Time.deltaTime; 
                    if (stickCorrection < stickInputClamp) stickCorrection = stickInputClamp;
                }
                if (stickInput.magnitude<stickDZ) 
                {
                    stickCorrection -= (dec/scSmooth) * Time.deltaTime; 
                    if (stickCorrection < stickInputClamp) stickCorrection = stickInputClamp;
                }

            #endregion

        #endregion

        #region States

            switch(state)
            {

                #region Grounded State

                    case stGrounded:

                        #region Ground Checking Timer

                            if (groundChecking>0)
                            {
                                groundChecking -= Time.deltaTime;
                                if (groundChecking<0) groundChecking = 0;
                            }

                        #endregion

                        #region Raycast Variables

                            float sphereCastYOffset = controller.height/2f - controller.radius;
                            Vector3 castOrigin = transform.position - (slopeNormal*sphereCastYOffset);
                            Vector3 groundOrigin = transform.position - (slopeNormal*(controller.height/2f));

                        #endregion
                    
                        #region Fall

                            if (groundChecking<=0 && !Physics.Raycast(castOrigin, -slopeNormal, out RaycastHit hit, groundCheckLength + controller.skinWidth + (controller.radius-0.01f)))
                            {
                                Fall();
                                break;
                            }

                        #endregion

                        #region Grounded Movement

                            if (movementDirection != Vector3.zero && stickInput.magnitude>stickDZ)
                            {
                                // Calculate the acceleration based on the movement direction
                                    movement = transform.forward;
                                    
                                // Calculate the current speed based on the acceleration
                                    currentSpeed += acc*stickCorrection*Time.deltaTime;

                                // If the current speed is above the maximum speed
                                    if (currentSpeed > (maxSpeed*stickCorrection)+momentumSpd)
                                    {
                                        // Set the current speed to the maximum speed
                                            currentSpeed = (maxSpeed*stickCorrection)+momentumSpd;
                                    }

                                //Set movement
                                    movement = movement.normalized * currentSpeed;

                            }
                            else // If Movement Direction is 0
                            {
                                // Calculate the current speed based on the deceleration
                                    currentSpeed -= dec*Time.deltaTime;
                                    currentSpeed += momentumSpd*Time.deltaTime;

                                    movement = movement.normalized * currentSpeed;

                                // If the current speed is below the minimum speed
                                if (currentSpeed < 0.01f)
                                {
                                    // Set the current speed to zero
                                    currentSpeed = 0;

                                    // Set the acceleration to zero
                                    movement = Vector3.zero;
                                }
                            }

                        #endregion

                        #region Set Current Slope

                            if (Physics.SphereCast(castOrigin, controller.radius-0.01f, -slopeNormal, out hit, groundCheckLength + controller.skinWidth))
                            {

                                slopeNormal = hit.normal;

                            }

                        #endregion

                        #region Ground Behavior

                            Collider col = hit.collider;
                            float angle = Vector3.Angle(Vector3.up, hit.normal);
                            
                            if (yVel!=0)
                            {
                                if (yVel<0f && momentumSpd<maxMomentum) 
                                {
                                    momentumSpd -= yVel;
                                    if (momentumSpd>maxMomentum) momentumSpd = maxMomentum;
                                }
                                if (yVel>0 && momentumSpd>minMomentum) 
                                {
                                    momentumSpd -= yVel;
                                    if (momentumSpd<minMomentum) momentumSpd = minMomentum;
                                }
                                
                            }
                            else
                            {
                                momentum.x = 0;
                                momentum.y = 0;
                                if (momentumSpd>0f) 
                                {
                                    momentumSpd -= fric*Time.deltaTime;
                                    if (momentumSpd<0f) momentumSpd = 0f;
                                }
                                if (momentumSpd<0f) 
                                {
                                    momentumSpd += revFric*Time.deltaTime;
                                    if (momentumSpd>0f) momentumSpd = 0f;
                                }
                            }

                            #region Ground Rotation / Ground Stick

                                
                                #region Rotate the transform towards the movement direction smoothly

                                    //Get the turn smoothing speed based on the character's movement speed
                                        float rotSmooth = rotationSmoothness - ((currentSpeed/(maxSpeed+maxMomentum))*rotTurningAdjust);

                                    //Set target Rotation
                                        if (stickCorrection>0f && movementDirection.magnitude>0) targetRotation = Quaternion.LookRotation(movementDirection);

                                    //Rotate toward the target rotation smoothly
                                        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotSmooth * Time.deltaTime);

                                #endregion


                                #region Get the Ground Plane & set movement to be based on the ground plane

                                    // Calculate the forward direction projected on the ground plane
                                        Vector3 forwardProjection = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;

                                    // Calculate the rotation to match the ground normal
                                        Quaternion groundRotation = Quaternion.LookRotation(forwardProjection, hit.normal);

                                    // Apply the rotation to the player model
                                        transform.rotation = Quaternion.Lerp(transform.rotation, groundRotation, slopeRotSmooth * Time.deltaTime);

                                    // Adjust the movement direction to align with the slope's orientation
                                        movement = Vector3.ProjectOnPlane(movement, slopeNormal);

                                #endregion
                                
                                #region Reduce speed based on current turning angle

                                    //Set the current turning angle based on the last rotation compared to this rotation
                                        turnAngle = Mathf.Abs(Quaternion.Angle(transform.rotation, rotationLast));
                                        rotationLast = transform.rotation;

                                    drift = currentSpeed*(turnAngle/180f); 
                                    currentSpeed -= drift*turningSlowdownGround*Time.deltaTime;

                                #endregion

                                #region Ground Stick

                                    float slopeAngle = Vector3.Angle(Vector3.up, slopeNormal);

                                    // Apply Slope based gravity (not normal -y gravity)
                                        Vector3 movementForce = (slopeNormal * groundStickForce) * Time.deltaTime;
                                        if (currentSpeed>0f) movement += movementForce;

                                #endregion

                            #endregion

                            #region Wall Hit
                                
                                Vector3 headOrigin = transform.position + Vector3.up*((controller.height/2f) - controller.radius);
                                if (Physics.SphereCast(castOrigin, controller.radius-0.01f, transform.rotation*(Vector3.forward), out hit, controller.skinWidth+0.01f))
                                {
                                    float wallAngle = Vector3.Angle(Vector3.up, hit.normal);
                                    
                                    if (wallAngle==Mathf.Clamp(wallAngle, controller.slopeLimit+10f,100f))
                                    {
                                        //Wall Hit
                                        
                                        currentSpeed = 0f;
                                        momentumSpd = 0;
                                    }
                                    
                                }

                            #endregion

                            #region Jump

                                if (Input.GetButtonDown("Button 0"))
                                {
                                    Jump();
                                    break;
                                }

                            #endregion

                        #endregion

                    break;
                
                #endregion

                #region Air State

                    case stAir:

                        #region Air Movement

                            if (movementDirection != Vector3.zero && stickInput.magnitude>stickDZ)
                            {
                                // Calculate the acceleration based on the movement direction
                                    movement = transform.forward;

                                // Calculate the current speed based on the acceleration
                                    currentSpeed += acc*stickCorrection*Time.deltaTime;

                                // If the current speed is above the maximum speed
                                    if (currentSpeed > (maxSpeed*stickCorrection)+momentumSpd)
                                    {
                                        // Set the current speed to the maximum speed
                                            currentSpeed = (maxSpeed*stickCorrection)+momentumSpd;
                                    }

                                //Set movement
                                    movement = movement.normalized * currentSpeed;
                            }
                            else // If Movement Direction is 0
                            {
                                // Calculate the current speed based on the deceleration
                                    currentSpeed -= dec*Time.deltaTime;
                                    currentSpeed += momentumSpd*Time.deltaTime;

                                    movement = movement.normalized * currentSpeed;

                                // If the current speed is below the minimum speed
                                if (currentSpeed < 0.01f)
                                {
                                    // Set the current speed to zero
                                    currentSpeed = 0;

                                    // Set the acceleration to zero
                                    movement = Vector3.zero;
                                }
                            }

                        #endregion

                        #region Flip Player (Back to Vector3.Up)

                            // Reset Slope Normal & flip player
                            slopeNormal = Vector3.up; 

                            // Calculate the rotation to match the ground normal
                            Quaternion targRot = Quaternion.LookRotation(transform.forward, Vector3.up);

                            transform.rotation = Quaternion.Slerp(transform.rotation, targRot, airRotSmooth);

                            

                        #endregion

                        #region Jump Add

                            //Add Jump Spd if button is held down
                            if (jumping && Input.GetButton("Button 0"))
                            {
                                ySpd += jumpAdd*Time.deltaTime;
                            }

                        #endregion

                        #region Cancel Momentum & Fall

                            if (inputSpindashPress())
                            {
                                movement = Vector3.zero;
                                momentum = Vector3.zero;
                                jumping = false; // Turn off jump & fall
                            }

                        #endregion

                        #region Terminal Velocity Check

                            if (ySpd>termVel) 
                            {
                                ySpd -= gravityForce * Time.deltaTime;
                                if (ySpd<=termVel) ySpd = termVel;
                            }
                            movement.y = ySpd;

                        #endregion

                        #region Ground Checking Timer

                            if (groundChecking>0)
                            {
                                groundChecking -= Time.deltaTime;
                                if (groundChecking<0) groundChecking = 0;
                            }

                        #endregion

                        #region Air Rotation (Y Rotation)

                                #region Rotate the transform towards the movement direction smoothly

                                    //Get the turn smoothing speed based on the character's movement speed
                                        rotSmooth = rotationSmoothness - ((currentSpeed/(maxSpeed+maxMomentum))*rotTurningAdjust);

                                    //Set target Rotation
                                        if (stickCorrection>0f && movementDirection.magnitude>0) targetRotation = Quaternion.LookRotation(movementDirection);

                                    //Set the current turning angle
                                        turnAngle = Quaternion.Angle(transform.rotation, targetRotation);

                                    //Rotate toward the target rotation smoothly
                                        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotSmooth * Time.deltaTime);

                                #endregion

                                #region Reduce speed based on current turning angle

                                    drift = currentSpeed*(turnAngle/180f); 
                                    currentSpeed -= drift*turningSlowdownAir*Time.deltaTime;

                                #endregion

                        #endregion

                        #region Hit Ground

                            sphereCastYOffset = controller.height/2f - controller.radius;
                            castOrigin = transform.position - (slopeNormal*sphereCastYOffset);
                            groundOrigin = transform.position - (slopeNormal*(controller.height/2f));
                            if (groundChecking <= 0 && Physics.SphereCast(castOrigin, controller.radius - 0.01f, -slopeNormal, out hit, groundCheckLength + controller.skinWidth))
                            {
                                if (!hit.collider.CompareTag("Enemy")) // Asegúrate de que el objeto no sea un "Enemy"
                                {
                                    HitGround();
                                    break;
                                }
                            }


                        #endregion

                    break;

                #endregion
            
            }

        #endregion

        #region Perform Actual Movement
        
            // Move the player with the character controller
                controller.Move((movement) * Time.deltaTime);

            //Velocity
                measuredVelocity = transform.position-initPos;
                yVel = (measuredVelocity.y/Time.deltaTime);
                if (yVel==Mathf.Clamp(yVel, -0.01f, 0.01f)) yVel = 0f;

        #endregion

        #region Animations

            Animations(); //Control animations

        #endregion
    }

    #region Methods
        
        #region Movement

            #region Ground

                void SetGroundAccel()
                {
                    acc = groundAcc;
                    dec = groundDec;
                    fric = groundFric;
                }

                public void HitGround()
                {
                    state = stGrounded;
                    groundChecking = hitGroundTime;

                    jumping = false;
                    SetGroundAccel();
                }

            #endregion

            #region Air

                void SetAirAccel()
                {
                    acc = airAcc;
                    dec = airDec;
                    fric = airFric;
                }

                public void Jump()
                {
                    ySpd = jumpSpd;
                    float InclineJump = 0.5f; //the amount to multiply the measured velocity by the incline velocity
                    if (yVel>0f)
                    {
                        ySpd += (yVel)*InclineJump;
                        currentSpeed /= 4f;
                        movement = movement.normalized * currentSpeed;
                    } 
                    else
                    {
                        if (yVel<0f)
                        {
                            currentSpeed *= 1.25f;
                            movement = movement.normalized * currentSpeed;
                        }
                        else
                        {
                            currentSpeed /= 1.5f;
                            movement = movement.normalized * currentSpeed;
                        }
                    }

                    SetAirAccel();
                    
                    movement.y += ySpd; // Set Jump Speed in Movement
                    state = stAir;
                    jumping = true;

                    //Play Sound
                        PlaySound(aJump);

                    groundChecking = jumpTime;
                }

                public void Fall() // Make the Player Fall
                {
                    groundChecking = fallTime;

                    state = stAir;
                    ySpd = yVel;

                    SetAirAccel();
                }

            #endregion

        #endregion
        
    #endregion

    #region Animations

        public void Animations() //Control the animator here
        {
            string animState = "idle";
            bool transitionAnim = false;

            switch(state)
            {
                #region Grounded

                    case stGrounded:

                        if (currentSpeed>controller.minMoveDistance) 
                        {

                            //Run is default max speed animation
                                animState = "run";
                                anim.speed = currentSpeed / runSpd;
                            

                            if (currentSpeed<=jogSpd)
                            {
                                animState = "jog";
                                anim.speed = currentSpeed / jogSpd;
                            }

                            if (currentSpeed<=walkSpd)
                            {
                                animState = "walk";
                                anim.speed = currentSpeed / walkSpd;
                            }
                            
                        }
                        else
                        {
                            anim.speed = 1f;
                        }

                        if (animStateLast!="" && animStateLast!="jump" && animStateLast!="fall") transitionAnim = true;

                    break;

                #endregion

                #region Air

                    case stAir:

                        if (jumping)
                        {
                            animState = "jump";
                            anim.speed = 0.3f + (currentSpeed/200f);
                        }
                        else
                        {
                            animState = "fall";
                            anim.speed = 1f;
                            transitionAnim = true;
                        }

                    break;

                #endregion

            }

            if (animState!=animStateLast) 
            {
                if (transitionAnim && animStateLast!="") anim.SetTrigger(animStateLast + " to " + animState);
                else anim.Play(animState);

                switch(animState) 
                {
                    case "idle":

                        //if (animStateLast=="") anim.Play(animState);
                        //if (animStateLast=="walk") anim.SetTrigger("walk to idle");

                    break;

                    case "walk":

                        //if (animStateLast=="idle") anim.SetTrigger("idle to walk");
                        //if (animStateLast=="jog") anim.SetTrigger("idle to walk");

                    break;

                }
            }

            animStateLast = animState;
        }

    #endregion

    #region Sounds

        public void PlaySound(AudioClip clip)
        {
            aud.clip = clip;
            aud.Play();
        }

    #endregion

    #region Collision
            void BounceOffEnemy()
        {
            ySpd = bounceSpeed; // Aplica la velocidad de rebote
            SetAirAccel(); // Configura el estado para el aire
            jumping = true; // Asegúrate de que el estado sea de salto

            PlaySound(aJump); // Opcional: reproduce el sonido del salto
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider.CompareTag("Enemy")) // Asegúrate de que los enemigos tengan el tag "Enemy"
            {
                BounceOffEnemy();
            }
        }

    #endregion
}
