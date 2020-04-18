using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float rotationX;

    public float movementSpeed = 2f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    public float slopeForceRayLength;
    public float slopeForce; 
    
    Vector3 moveDirection = Vector3.zero;

    private CharacterController charController;
    private Camera cam;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        charController = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
    }

    private bool OnSlope()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, slopeForceRayLength))
        {
            if (hit.normal != Vector3.up)
                return true;
        }

        return false;
    }
    
    // Update is called once per frame
    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        
        float curSpeedX = movementSpeed * Input.GetAxis("Vertical");
        float curSpeedY = movementSpeed * Input.GetAxis("Horizontal");
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (moveDirection != Vector3.zero && OnSlope())
        {
            charController.Move((Vector3.down) * slopeForce * Time.deltaTime);
        }
        
        charController.SimpleMove(moveDirection);
        
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        cam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }
}
