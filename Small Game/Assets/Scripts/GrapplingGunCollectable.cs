using UnityEngine;

public class GrapplingGunCollectable : MonoBehaviour
{
    public GrapplingGun target;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private int scoreValue = 1;

    private void OnTriggerEnter(Collider other)
    {
        

        // Check if the object that entered the trigger is the player
        if (other.CompareTag(playerTag))
        {
            
            Collect();
    
        }
    }

    private void Collect()
    {
        // 1. Add logic for score, inventory, or sound effects here
        Debug.Log("Grappling Gun Unlocked!");


        // Change the value permanently in the other script
        target.GraplinggunUnlocked = true;

        // 2. Destroy the object
        Destroy(gameObject);
 
        
    }
}

