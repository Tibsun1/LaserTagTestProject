using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    //Variables
    public Transform playerCam, character, camCenter, desiredCam, gunRotator, camCenterMin;
    public Rigidbody rb;
    public CapsuleCollider c;
    public LayerMask defaultLM, playerCollLM;
    public float mouseSensitivity, maxSpeed, acceleration, defaultCamAngle, defaultLookDirection;
    public float camCenterAdjustment, camCenterDetection, camCenterX, camCenterXMin, camCenterXMax, camCenterAvoidance;
    public float defaultZoom, camZoomHitModifier, smoothTime;
    public int mouseInverted, mouseMin, mouseMax;

    private Vector3 movementV3 = new Vector3();
    private Vector2 movementV2 = new Vector2();
    private List<Collider> collLst;
    private float zoom = 0, zoomVelocity = 0;
    private float mouseX, mouseY;
    private float moveFB, moveLR;
    private float speed = 0, dir = 0;

	// Use this for initialization
	void Start () {
        mouseY = defaultCamAngle;
        mouseX = character.rotation.y;
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
        if (movementV2.magnitude > 0)
            dir = Vector2Direction(movementV2);
        //Update vector3 with speed and direction
        movementV3.x = Mathf.Cos(dir) * speed;
        movementV3.z = Mathf.Sin(dir) * speed;
        movementV3 = character.rotation * movementV3;
        //Add collisions
        if (collLst.Count > 0 && movementV3.magnitude > 0)
        {
            movementV3 = AddCollisions(movementV3);
        }
        //move character
        character.position += movementV3;
    }

    private void OnTriggerEnter(Collider other)
    {
        collLst.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        collLst.Remove(other);
    }

    private float Vector2Direction(Vector2 v)
    {
        //Convert vector2 to angle in radians
        float d, x, y;
        x = v.x;
        y = v.y;
        if (x == 0)
        {
            if (y == 0)
            {
                d = 0;
                Debug.Log("Pass 0 vector2 to direction");
            }
            else if (y < 0)
                d = -Mathf.PI / 2;
            else
                d = Mathf.PI / 2;
            return d;
        }
        else if (y == 0)
        {
            if (x == 0)
            {
                d = 0;
                Debug.Log("Pass 0 vector2 to direction");
            }
            else if (x < 0)
                d = Mathf.PI;
            else
                d = 0;
            return d;
        }
        else
        {
            d = Mathf.Atan2(y, x);
            return d;
        }
    }
    private Vector3 AddCollisions(Vector3 v)
    {
        //Add collisions on 2d plane
        foreach (Collider oC in collLst)
        {
            Vector3 oV = new Vector3();
            Vector3 cV2 = new Vector3();
            Vector2 cV = new Vector2();
            Vector2 mV = new Vector2();
            float dircV, magfV, dirV, dir, dot;
            //Get vector to collision point
            oV = oC.ClosestPoint(c.bounds.center);
            cV.x = (oV - c.bounds.center).x;
            cV.y = (oV - c.bounds.center).z;
            //Get movement vector
            mV.x = v.x;
            mV.y = v.z;
            //Get collision direction
            dircV = Vector2Direction(cV);
            dot = Vector2.Dot(mV, cV);
            //adjust movement direction if it is going towards collision
            if (dot > 0)
            {
                dir = Mathf.Acos(dot / (mV.magnitude * cV.magnitude));
                magfV = Mathf.Cos(dir) * v.magnitude;
                cV2.x = Mathf.Cos(dircV) * magfV;
                cV2.z= Mathf.Sin(dircV) * magfV;
                v -= cV2;
            }
        }
        return v;
    }
}
 