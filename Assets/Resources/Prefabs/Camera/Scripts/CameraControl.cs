using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    // The target object to follow and rotate around
    public Transform target;

    //Camera Inputs
        float horizontalInput = 0; //default input to be smoothly lerped
        float verticalInput = 0; //default input to be smoothly lerped
        float rotSmooth = 10f;

    //Colliding with wall?
        public bool colliding = false;

    // The distance to keep from the target
        const float dist1 = 5f;
        const float dist2 = 10f;
        float curZoom = dist1;
        public float distance = dist1; 
        public float zoomSpd = 10f;

    // The smoothness of the camera's movement and rotation
    public float smoothness = 5f;

    // The height of the camera above the target
        public float height = 2f; //height above target

    // The rotation speed of the camera
    public float rotationSpeed = 10f;

    // The initial offset from the target
    Vector3 offset;

    void Start()
    {
        // Calculate the initial offset from the target
        offset = new Vector3(0, height, -distance);
    }

    void LateUpdate()
    {

        #region Change Distance

            if (colliding)
            {
                float targDist = Vector3.Distance(target.position, transform.position);
                if (Physics.Raycast(target.position, (transform.position-target.position).normalized, out RaycastHit hit, targDist + 0.5f, ~1<<6))
                {

                        Debug.Log("Hitting wall!");
                    if (targDist>hit.distance)
                    {
                        distance-=zoomSpd*Time.deltaTime;
                        if (distance<1f) distance = 1f;
                    }
                }
            }
            else
            {
                if (distance<curZoom)
                {
                    distance += zoomSpd*Time.deltaTime;
                    if (distance>curZoom) distance = curZoom;
                }

                if (distance>curZoom)
                {
                    distance -= zoomSpd*Time.deltaTime;
                    if (distance<curZoom) distance = curZoom;
                }
            }

            //offset += transform.position distance*transform.forward;

        #endregion
        

        #region Vertical
        
            verticalInput = Mathf.Lerp(verticalInput, Input.GetAxis("RightStickY"), rotSmooth * Time.deltaTime);
                verticalInput = Mathf.Clamp(verticalInput, -1f,1f);
                float eulerX = transform.rotation.eulerAngles.x;
                    if (eulerX>180f) eulerX = 0f;

                //Stop from rotating too far
                    float smoothStop = 10f;
                    if (eulerX==Mathf.Clamp(eulerX, 45f-smoothStop,90f)) verticalInput = Mathf.Clamp(verticalInput, -1f,(45f-eulerX)/smoothStop);
                    if (eulerX<=smoothStop) verticalInput = Mathf.Clamp(verticalInput, ((-eulerX)/smoothStop),1f);

            Quaternion vRot = Quaternion.AngleAxis(verticalInput * rotationSpeed * Time.deltaTime, transform.right);
            offset =  vRot* offset;
            transform.RotateAround(target.position, transform.right, verticalInput * rotationSpeed * Time.deltaTime);

            #region Clamp Rot
                    
                    //

            #endregion

        #endregion

        #region Horizontal

            horizontalInput = Mathf.Lerp(horizontalInput, (Input.GetAxis("LeftTrigger") - Input.GetAxis("RightTrigger")) + Input.GetAxis("RightStickX"), rotSmooth * Time.deltaTime);
                horizontalInput = Mathf.Clamp(horizontalInput, -1f,1f);
            offset = Quaternion.AngleAxis(horizontalInput * rotationSpeed * Time.deltaTime, Vector3.up) * offset;
            transform.RotateAround(target.position, Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);

        #endregion

        // Smoothly move the camera towards the target position
        transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.deltaTime * smoothness);

        // Look at the target
        transform.LookAt(target);
    }

    #region Methods
        
        void OnTriggerStay(Collider col) 
        {
            colliding = true;
        }

        void OnTriggerExit(Collider col) 
        {
            colliding = false;
        }
        
    #endregion
}
