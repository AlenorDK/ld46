using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
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
    public float shotDistance = 25f;

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

    private bool needsToInteract = false;
    private bool needsToPlace = false;
    private bool needsToShoot = false;

    private bool isJumping;

    public AnimationCurve jumpFallOff;
    public float jumpMultiplier;

    public bool canShoot = true;
    public float shotDelay = 0.5f;

    public GameObject hands;

    public int bulletDamage = 1;
    
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
        hands.SetActive(heldObject == null);

        if (Input.GetMouseButtonDown(0) && canShoot && heldObject == null)
            needsToShoot = true;
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject != null)
                needsToPlace = true;
            else
                needsToInteract = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            isJumping = true;
            StartCoroutine(JumpEvent());
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

    private IEnumerator JumpEvent()
    {
        charController.slopeLimit = 90f;
        float timeInAir = 0.0f;

        do
        {
            float jumpForce = jumpFallOff.Evaluate(timeInAir);
            charController.Move(Vector3.up * jumpForce * jumpMultiplier * Time.deltaTime);
            timeInAir += Time.deltaTime;
            yield return null;
        } while (!charController.isGrounded && charController.collisionFlags != CollisionFlags.Above);

        isJumping = false;
        charController.slopeLimit = 60f;
    }
    
    
    void TryPlace()
    {
        RaycastHit hit0;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit0, checkingDistance,
            maskWithoutPlayer))
        {
            if (hit0.collider.GetComponentInParent<ContainerLoaderController>())
            {
                hit0.collider.GetComponentInParent<ContainerLoaderController>().Place(heldObject);
                heldObject.GetComponent<ContainerController>().PlaceInCharger();
                heldObject = null;
                return;
            }

            if (hit0.collider.GetComponentInParent<PressablePlate>())
            {
                hit0.collider.GetComponentInParent<PressablePlate>().Place(heldObject);
                heldObject.GetComponent<ContainerController>().Place();
                heldObject = null;
                return;
            }
        }

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

    private void FixedUpdate()
    {
        if (needsToInteract)
        {
            Interact();
            needsToInteract = false;
        }

        if (needsToPlace)
        {
            TryPlace();
            needsToPlace = false;
        }

        if (needsToShoot)
        {
            StartCoroutine(Shoot());
            needsToShoot = false;
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
            else if (hit.collider.GetComponentInParent<InteractableObject>())
            {
                if (hit.collider.GetComponentInParent<PressablePlate>() && hit.collider.GetComponentInParent<PressablePlate>().hasContainer)
                {
                    ContainerController controller = hit.collider.GetComponentInChildren<ContainerController>();
                    hit.collider.GetComponentInParent<InteractableObject>().Activate(false);
                    controller.transform.parent = handheldObjectTransform;
                    controller.transform.localPosition = Vector3.zero;
                    controller.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    heldObject = controller.gameObject;
                    return;
                }

                hit.collider.GetComponentInParent<InteractableObject>().Activate(false);
            }
        }
    }

    IEnumerator Shoot()
    {
        canShoot = false;
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, shotDistance,
            maskWithoutPlayer))
        {
            if (hit.collider.GetComponentInParent<Enemy>())
                hit.collider.GetComponentInParent<Enemy>().Damage(bulletDamage);
                
        }
        yield return new WaitForSeconds(shotDelay);
        canShoot = true;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PlatformCollider" && other.GetComponentInParent<PressablePlate>())
        {
            other.GetComponentInParent<PressablePlate>().isActivated = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "PlatformCollider" && other.GetComponentInParent<PressablePlate>())
        {
            other.GetComponentInParent<PressablePlate>().isActivated = false;
        }
    }
}

public enum PlayerState
{
    Walking    = 0,
    Crouching  = 1
}
