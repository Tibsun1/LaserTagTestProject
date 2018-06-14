using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    //Variables
    public Transform playerCam, character, camCenter, desiredCam;
    public Rigidbody rb;
    public LayerMask lm;
    public float mouseSensitivity, moveSpeed;
    public int mouseInverted, mouseMin, mouseMax;
    public float defaultZoom, camZoomHitModifier, smoothTime;

    private RaycastHit hit;
    private Vector3 movement = new Vector3();
    private float zoom, targetZoom, zoomVelocity;
    private float mouseX, mouseY;
    private float moveFB, moveLR;

	// Use this for initialization
	void Start () {
        		
	}
	
	// Update is called once per frame
	void Update () {
        //Player input
        moveFB = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        moveLR = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        mouseX += Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY += Input.GetAxis("Mouse Y") * mouseSensitivity * mouseInverted;
    }

    private void FixedUpdate() {
        //character movement
        movement.x = moveLR;
        movement.z = moveFB;
        character.Translate(movement);
        //set mouse look
        mouseY = Mathf.Clamp(mouseY, mouseMin, mouseMax);
        //rotate character and cam based on mouse look
        character.rotation = Quaternion.Euler(0, mouseX, 0);
        camCenter.localRotation = Quaternion.Euler(mouseY, camCenter.localEulerAngles.y, camCenter.localEulerAngles.z);
        //raycast for cam obstructions
        if (Physics.Raycast(camCenter.position, (desiredCam.position - camCenter.position), out hit,
            Vector3.Distance(camCenter.position, desiredCam.position), lm, QueryTriggerInteraction.Ignore))
        {
            targetZoom = hit.distance * camZoomHitModifier;
        }
        else
        {
            targetZoom = defaultZoom;
        }
        //smoothdamp to target zoom
        zoom = Mathf.SmoothDamp(zoom, targetZoom, ref zoomVelocity, smoothTime);
        playerCam.localPosition = new Vector3(playerCam.localPosition.x, playerCam.localPosition.y, zoom);
        playerCam.LookAt(camCenter);
    }
}
 