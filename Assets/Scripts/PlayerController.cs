using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    //Variables
    public Transform playerCam, character, camCenter, desiredCam, gunRotator, camCenterMin;
    public Rigidbody rb;
    public Collider c;
    public LayerMask defaultLM, playerLM;
    public float mouseSensitivity, maxSpeed, acceleration, defaultCamAngle;
    public float camCenterAdjustment, camCenterDetection, camCenterX, camCenterXMin, camCenterXMax, camCenterAvoidance;
    public float defaultZoom, camZoomHitModifier, smoothTime;
    public int mouseInverted, mouseMin, mouseMax;

    private Vector3 movementV3 = new Vector3();
    private Vector2 movementV2 = new Vector2();
    private float zoom = 0, zoomVelocity = 0;
    private float mouseX, mouseY;
    private float moveFB, moveLR;
    private float speed = 0, dir = 0;

	// Use this for initialization
	void Start () {
        mouseY = defaultCamAngle;
        camCenterMin.localPosition = new Vector3(camCenterXMin, camCenter.localPosition.y, camCenter.localPosition.z);
    }
	
	// Update is called once per frame
	void Update () {
        //Player input
        moveFB = Input.GetAxis("Vertical");
        moveLR = Input.GetAxis("Horizontal");
        mouseX += Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY += Input.GetAxis("Mouse Y") * mouseSensitivity * mouseInverted;
    }

    private void FixedUpdate() {
        //variables
        RaycastHit hit;
        float targetZoom;

        Move();
        //set mouse look
        mouseY = Mathf.Clamp(mouseY, mouseMin, mouseMax);
        //rotate character and cam based on mouse look
        character.rotation = Quaternion.Euler(0, mouseX, 0);
        camCenter.localRotation = Quaternion.Euler(mouseY, camCenter.localEulerAngles.y, camCenter.localEulerAngles.z);
        //raycast for cam obstructions
        if (Physics.Raycast(camCenter.position, (desiredCam.position - camCenter.position), out hit,
            Vector3.Distance(camCenter.position, desiredCam.position), defaultLM, QueryTriggerInteraction.Ignore))
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
        gunRotator.localRotation = camCenter.localRotation;
    }

    private void Move()
    {
        float movementMag;

        //get input
        movementV2.x = moveLR;
        movementV2.y = moveFB;
        //normalize vector for input across all platforms
        movementMag = movementV2.magnitude;
        if (movementV2.sqrMagnitude > 1f)
            movementV2.Normalize();
        //Update speed
        if (movementMag > speed / maxSpeed)
            speed += acceleration;
        else if (movementMag < speed / maxSpeed)
            speed -= acceleration;
        if (speed < 0)
            speed = 0;
        if (speed > maxSpeed)
            speed = maxSpeed;
        //get vector direction
        if (moveLR == 0)
        {
            if (moveFB == 0)
            { }
            else if (moveFB < 0)
                dir = -Mathf.PI / 2;
            else
                dir = Mathf.PI / 2;
        }
        else if (moveFB == 0)
        {
            if (moveLR == 0)
            { }
            else if (moveLR < 0)
                dir = Mathf.PI;
            else
                dir = 0;
        }
        else
        {
            dir = Mathf.Atan2(moveFB, moveLR);
        }
        //Update vector3 with speed and direction
        movementV3.x = Mathf.Cos(dir) * speed;
        movementV3.z = Mathf.Sin(dir) * speed;
        movementV3 = RotateVector(movementV3, character.rotation);
        //move character
        character.position += movementV3;
    }

    private Vector3 RotateVector(Vector3 v, Quaternion q)
    {
        //rotate given vector by given quaternion
        Vector3 u = new Vector3();
        float s = q.w;
        u.x = q.x; u.y = q.y; u.z = q.z;
        v = 2f * Vector3.Dot(u, v) * u + (s * s - Vector3.Dot(u, u)) * v + 2f * s * Vector3.Cross(u, v);
        return v;
    }

    private void OnTriggerEnter(Collider other)
    {
        Collider oC = other.GetComponent<Collider>();
        Vector3 oV = new Vector3();
        if (oC != null)
        {
            oV = oC.ClosestPoint(c.bounds.center);
            character.position += c.ClosestPoint(oV) - oV;
            Debug.Log("triggered: " + (c.ClosestPoint(oV) - oV).ToString());
        }
    }
}
 