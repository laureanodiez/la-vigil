    using UnityEngine;

    public class Teleporter : MonoBehaviour
    {
        public Transform teleportDestination; // Assign your TeleportDestination GameObject here in the Inspector

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if the colliding object is the player (by tag or other identification)
            if (other.CompareTag("Player")) 
            {
                // Teleport the player to the destination
                other.transform.position = teleportDestination.position;
            }
        }
    }