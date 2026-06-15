using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrapplingGun : MonoBehaviour
{
    private LineRenderer lr; // a line renderer used to visually draw the rope
    private Vector3 grapplePoint; // world space point where the grapple attaches
    public LayerMask whatIsGrappleable; // defines which layers can be grappled 
    public Transform gunTip, camera, player; 
    private float maxDistance = 100f; // max distance the grapple can reach
    private SpringJoint joint; // spring joint used to simulate rope physics 
    public bool GraplinggunUnlocked = false;

    // makes sure grapple is stopped when game starts
    void Start()
    {
        StopGrapple();
    }

    // gets a line render component 
    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    // gets mouse inputs and assigns functions to them
    void Update()
    {
        if (Input.GetMouseButtonDown(0)&& GraplinggunUnlocked == true)
        {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }
    }

   // calls draw rope function after all void Update() functions are called
    void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple()
    {

        // shoots a raycast foward from the player camera up to a certain distance set in maxDistance variable and checks if it hits something
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance))
        {
            grapplePoint = hit.point; // saves hit position as grapplePoint
            joint = player.gameObject.AddComponent<SpringJoint>(); // adds a SpringJoint to the player 
            joint.autoConfigureConnectedAnchor = false; // disables unity's auto anchor config 
            joint.connectedAnchor = grapplePoint; // attaches the joint to the grapple point in the world space 

            // calculates the distance from the player to the grapple point
            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            // sets how long the rope can stretch and compress
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            // configures swing physics
            joint.spring = 4.5f; // pull strength 
            joint.damper = 7f; // reduces oscillation
            joint.massScale = 4.5f; // makes player feel heavier or lighter

            // set up line renderer for rope drawing
            lr.positionCount = 2;
        }

    }

    // draws the visual rope
    void DrawRope()
    {
        if (!joint) return; // if there is no joint then we are not grappling 

        // sets start and end of the line
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, grapplePoint);
    }

    void StopGrapple()
    {
        lr.positionCount = 0; // removes rope from screen
        Destroy(joint); // destroys the joint 
    }

    // checks if player is grappling
    public bool IsGrappling()
    {
        return joint == null;
    }

    // returns the current grapple point in world space 
    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
}
