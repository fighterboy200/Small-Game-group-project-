using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RotateGun : MonoBehaviour
{
    public GrapplingGun grappling; // reference to GrapplingGun script

    private Quaternion desiredRotation; // the rotation we want to smoothly rotate towards
    private float rotationSpeed = 5f; // how fast the object rotates 

    void Update()
    {
        // if the player is grappling match the parent object's rotation
        if (grappling.IsGrappling())
        {
            desiredRotation = transform.parent.rotation;
        }
        // rotate to face the grapple point
        else
        {
            desiredRotation = Quaternion.LookRotation(grappling.GetGrapplePoint() - transform.position);
        }

        // smoothly rotate towards the desired rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);
    }
}
