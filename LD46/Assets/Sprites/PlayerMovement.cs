﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

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
    public float verticalPlacement;
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

    public int health = 3;

    public bool isAlive = true;

    public Animator anim;

    public float lerpingSpeed = 5f;
    public Image fade;
    public GameObject[] GameOverTexts;

    public TextMeshProUGUI subs;

    public AudioSource src;
    public AudioClip[] shots, hits, deaths;

    public GameObject box;

    public GameObject HealthBar;

    public Sprite fullHeart, nullHeart;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        charController = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();

        currentCameraPos = cam.transform.position;
        
        box = GameObject.FindGameObjectWithTag("Box");
    }

    private bool OnSlope()
    {
        if (isJumping)
            return false;
        
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, charController.height / 2 * slopeForceRayLength))
        {
            if (hit.normal != Vector3.up)
                return true;
        }

        return false;
    }
    
    void Update()
    {
        UpdateHealthBar();
        
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

        if (box != null && box.GetComponent<ContainerController>().Energy <= 0 && isAlive)
        {
            src.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            src.volume = 0.3f;
            src.PlayOneShot(deaths[UnityEngine.Random.Range(0, shots.Length)]);
            StartCoroutine(Die());
        }
        
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            isJumping = true;
            StartCoroutine(JumpEvent());
        }
        
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

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward * 5f, out hit, maskWithoutPlayer))
        {
            if (hit.collider.GetComponent<Subtitles>())
                subs.text = hit.collider.GetComponent<Subtitles>().text;
            else
            {
                subs.text = "";
            }
        }
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
                        boxPlacement.transform.position.y - hit.distance + verticalPlacement,
                        boxPlacement.transform.position.z);
                    heldObject.transform.parent = null;
                    heldObject.transform.rotation =
                        transform.rotation * heldObject.GetComponent<ContainerController>().origRotation;
                    heldObject.GetComponent<ContainerController>().Place();
                    heldObject = null;
                }
            }
    }

    public void UpdateHealthBar()
    {
        for (int i = 0; i < 3; i++)
        {
            HealthBar.transform.GetChild(i).GetComponent<Image>().sprite = health > i ? fullHeart : nullHeart;
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
        anim.SetTrigger("Shoot");
        canShoot = false;
        RaycastHit hit;
        src.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        src.volume = 0.3f;
        src.PlayOneShot(shots[UnityEngine.Random.Range(0, shots.Length)]);
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, shotDistance,
            maskWithoutPlayer))
        {
            if (hit.collider.GetComponentInParent<Enemy>())
                hit.collider.GetComponentInParent<Enemy>().Damage(bulletDamage);
                
        }
        yield return new WaitForSeconds(shotDelay);
        canShoot = true;
    }

    public void Damage(int incomingDamage)
    {
        if (health > 0)
            health -= incomingDamage;

        if (health >= 0)
        {
            src.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            src.volume = 0.3f;
            src.PlayOneShot(hits[UnityEngine.Random.Range(0, shots.Length)]);
        }

        if (health <= 0 && isAlive)
        {
            StartCoroutine(Die());
        }
    }

    public IEnumerator Die()
    {
        HealthBar.transform.GetChild(0).GetComponent<Image>().sprite = nullHeart;
        src.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        src.volume = 0.3f;
        src.PlayOneShot(deaths[UnityEngine.Random.Range(0, shots.Length)]);
        isAlive = false;
        movementSpeed = 0f;
        lookSpeed = 0f;
        canShoot = false;
        float deathTime = 0f;
        while (deathTime < 2f)
        {
            deathTime += Time.deltaTime;
            yield return null;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.Euler(new Vector3(transform.rotation.x, transform.rotation.y, -90f)),
                lerpingSpeed * Time.deltaTime);
        }

        StartCoroutine(StartFade());
    }

    IEnumerator StartFade()
    {
        float fadeTime = 0f;
        TextMeshProUGUI randomText = GameOverTexts[UnityEngine.Random.Range(0, GameOverTexts.Length)].GetComponent<TextMeshProUGUI>();
        while (fadeTime < 3f)
        {
            fadeTime += Time.deltaTime;
            yield return null;
            
            randomText.color = new Color(randomText.color.r, randomText.color.g, randomText.color.b,
                Mathf.Lerp(randomText.color.a, 1, lerpingSpeed * Time.deltaTime));
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b,
                Mathf.Lerp(fade.color.a, 1, lerpingSpeed * Time.deltaTime));
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PlatformCollider" && other.GetComponentInParent<PressablePlate>())
        {
            other.GetComponentInParent<PressablePlate>().isActivated = true;
        }

        if (other.gameObject.tag == "MovingPlatform")
        {
            transform.parent = other.gameObject.transform.parent;
        }

        if (other.gameObject.tag == "NextLevel")
        {
            SceneManager.LoadScene("Level_2_Ocean");
        }
        
        if (other.gameObject.tag == "FinishGame")
        {
            StartCoroutine(Finish());
        }
    }

    IEnumerator Finish()
    {
        float fadeTime = 0f;
        TextMeshProUGUI randomText = GameOverTexts[UnityEngine.Random.Range(0, GameOverTexts.Length)].GetComponent<TextMeshProUGUI>();
        randomText.text =
            "When we leave to underworld,\nWhen the Sun will shine on us no more,\nWhen we will feel no wind upon our skin, \nWhen the Great Water is no more,\nWhen the Seed of Humanity will be no more\nWhen the Seed of Life will be no more, \nAnd when the Celestials arrive, \nThey will be no more";
        while (fadeTime < 3f)
        {
            fadeTime += Time.deltaTime;
            yield return null;
            
            randomText.color = new Color(randomText.color.r, randomText.color.g, randomText.color.b,
                Mathf.Lerp(randomText.color.a, 1, lerpingSpeed * Time.deltaTime));
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b,
                Mathf.Lerp(fade.color.a, 1, lerpingSpeed * Time.deltaTime));
        }

        yield return new WaitForSeconds(10f);
        Application.Quit();
        Debug.Log("Game over");
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "PlatformCollider" && other.GetComponentInParent<PressablePlate>())
        {
            other.GetComponentInParent<PressablePlate>().isActivated = false;
        }
        
        if (other.gameObject.tag == "MovingPlatform")
        {
            transform.parent = null;
        }
    }
}

public enum PlayerState
{
    Walking    = 0,
    Crouching  = 1
}
