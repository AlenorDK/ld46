using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float rotationX;

    public float movementSpeed = 2f;
    public float crouchingSpeed = 1f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    public float slopeForceRayLength;
    public float slopeForce;
    public float checkingDistance = 5f;

    public float transitSpeed = 5f;

    public Transform walkingCameraPos;
    public Transform crouchingCameraPos;
    public Vector3 currentCameraPos;
    
    public GameObject walkingCollider;
    public GameObject crouchingCollider;
    
    public PlayerState currentState = PlayerState.Walking;
    
    Vector3 moveDirection = Vector3.zero;

    private CharacterController charController;
    private Camera cam;
    
    public LayerMask maskWithoutPlayer;
    public LayerMask maskWithoutBox;
    
    public GameObject heldObject;
    public GameObject boxPlacement;
    public Transform handheldObjectTransform;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        charController = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();

        currentCameraPos = cam.transform.position;
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
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject != null)
                TryPlace();
            else
                Interact();
        }
        
        if (Input.GetKey(KeyCode.LeftControl))
        {
            currentState = PlayerState.Crouching;
            currentCameraPos = crouchingCameraPos.position;
            walkingCollider.SetActive(false);
            crouchingCollider.SetActive(true);
        }
        else
        {
            currentState = PlayerState.Walking;
            currentCameraPos = walkingCameraPos.position;
            walkingCollider.SetActive(true);
            crouchingCollider.SetActive(false);
        }
        
        cam.transform.position =
            Vector3.Lerp(cam.transform.position, currentCameraPos, transitSpeed * Time.deltaTime);

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        
        float curSpeedX = (currentState == PlayerState.Walking ? movementSpeed : crouchingSpeed) * Input.GetAxis("Vertical");
        float curSpeedY = (currentState == PlayerState.Walking ? movementSpeed : crouchingSpeed) * Input.GetAxis("Horizontal");
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
        
        Debug.DrawRay(cam.transform.position, cam.transform.forward * checkingDistance, Color.red);
    }

    void TryPlace()
    {
        Collider[] hitColliders = Physics.OverlapBox(boxPlacement.transform.position,
            boxPlacement.transform.localScale / 2, Quaternion.identity, maskWithoutBox);
        
        if (hitColliders.Length == 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(boxPlacement.transform.position, Vector3.down, out hit, checkingDistance))
            {
                var distanceToGround = hit.distance;
                heldObject.transform.position = new Vector3(boxPlacement.transform.position.x, 
                    boxPlacement.transform.position.y - hit.distance + 0.5f, 
                    boxPlacement.transform.position.z);
                heldObject.transform.parent = null;
                heldObject.transform.rotation =
                    transform.rotation * heldObject.GetComponent<ContainerController>().origRotation;
                heldObject.GetComponent<ContainerController>().Place();
                heldObject = null;
            }
        }
    }

    void Interact()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, checkingDistance,
            maskWithoutPlayer))
        {
            
            if (hit.collider.GetComponentInParent<ContainerController>())
            {
                ContainerController controller = hit.collider.GetComponentInParent<ContainerController>();
                controller.PickUp();
                controller.transform.parent = handheldObjectTransform;
                controller.transform.localPosition = Vector3.zero;
                controller.transform.localRotation = Quaternion.Euler(0, 0, 0);
                heldObject = controller.gameObject;
            }
            else 
                Debug.Log(hit.collider.name);
        }
    }
}

public enum PlayerState
{
    Walking    = 0,
    Crouching  = 1
}
