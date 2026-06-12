using UnityEngine;

public class collectable : MonoBehaviour
{
    
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private int scoreValue = 1;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Entered");

        // Check if the object that entered the trigger is the player
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Collect script triggered");
            Collect();
    
        }
    }

    private void Collect()
    {
        // 1. Add logic for score, inventory, or sound effects here
        Debug.Log("Item Collected! Adding " + "to Inventory " );

        // 2. Destroy the object
        Destroy(gameObject);
    }

}
