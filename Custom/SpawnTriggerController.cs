using UnityEngine;

namespace FS_CombatSystem
{
    public class SpawnTriggerController : MonoBehaviour
    {
        public SpawnSystemController spawnSystem; // Reference to SpawnSystemController
        public bool isPlayerInTrigger = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (!isPlayerInTrigger)
                {
                    isPlayerInTrigger = true;
                    spawnSystem?.StartSpawning(); // Start spawning when the player enters the trigger
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                isPlayerInTrigger = false;
                spawnSystem?.StopSpawning(); // Stop spawning when the player exits
            }
        }
    }
}
