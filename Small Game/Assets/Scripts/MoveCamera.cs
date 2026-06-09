using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveCamera : MonoBehaviour
{
    [Tooltip("Drag the Transform that represents the camera's target position (e.g., Player Head)")]
    public Transform cameraPosition;

    private void Update()
    {
        transform.position = cameraPosition.position;
        transform.rotation = cameraPosition.rotation;
    }  
    
}
