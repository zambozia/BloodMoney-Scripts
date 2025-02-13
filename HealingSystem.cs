using UnityEngine;
using System.Collections;

namespace FS_CombatSystem
{
    public class HealingSystem : MonoBehaviour
    {
        [Header("References")]
        public HealthManager healthManager; // Reference to the HealthManager

        [Header("Healing Settings")]
        public float healingAmount = 10f; // Amount of health to heal per tick
        public float healingInterval = 5f; // Time between healing ticks
        public float damageCooldown = 3f; // Time before healing resumes after taking damage
        private bool isHealing = false;
        private bool isTakingDamage = false; // Flag for damage status

        private int currentRegenLevel = 0; // 1 = Level 1, 2 = Level 2, 3 = Level 3

        private void Start()
        {
            if (healthManager == null)
            {
                healthManager = Object.FindAnyObjectByType<HealthManager>();
                Debug.Assert(healthManager != null, "HealingSystem requires a HealthManager reference!");
                if (healthManager == null) return;
            }

            // Subscribe to health changes and damage events
            healthManager.OnHealthChanged.AddListener(OnHealthUpdated);
            healthManager.OnTakeDamage.AddListener(OnDamageTaken);

            // Set initial regen level
            InitializeRegenLevel();
        }

        private void OnDestroy()
        {
            if (healthManager != null)
            {
                healthManager.OnHealthChanged.RemoveListener(OnHealthUpdated);
                healthManager.OnTakeDamage.RemoveListener(OnDamageTaken);
            }
        }

        private void OnHealthUpdated(float currentHealth, float maxHealth)
        {
            if (isTakingDamage) return; // Don't heal while taking damage

            // Prevent healing above 75% health
            if (currentHealth >= maxHealth * 0.75f)
            {
                Debug.Log("[HealingSystem] No regen needed. Current health is above 75%");
                StopHealing();
                return;
            }

            // Start healing only if health is below the cap for the current level
            if (!isHealing && currentHealth < GetTargetHealthCap())
            {
                StartCoroutine(HealOverTime());
            }
        }

        private IEnumerator HealOverTime()
        {
            isHealing = true;

            while (isHealing)
            {
                // If the player takes damage, stop healing
                if (isTakingDamage)
                {
                    Debug.Log("[HealingSystem] Healing stopped due to damage taken.");
                    StopHealing();
                    yield break;
                }

                // Update the regen level based on current health
                UpdateRegenLevel();

                // Determine the target cap based on the current locked regen level
                float targetHealthCap = GetTargetHealthCap();

                // Stop healing if health is at or above the target cap
                if (healthManager.meleeFighter.CurrentHealth >= targetHealthCap)
                {
                    Debug.Log($"[HealingSystem] Stopping healing. Current: {healthManager.meleeFighter.CurrentHealth}, Target Cap: {targetHealthCap}, Level: {currentRegenLevel}");
                    StopHealing();
                    yield break;
                }

                // Heal up to the target cap
                float healthToAdd = Mathf.Min(healingAmount, targetHealthCap - healthManager.meleeFighter.CurrentHealth);
                healthManager.Heal(healthToAdd);

                Debug.Log($"[HealingSystem] Healing... Current: {healthManager.meleeFighter.CurrentHealth}, Target Cap: {targetHealthCap}, Level: {currentRegenLevel}");

                yield return new WaitForSeconds(healingInterval);
            }
        }

        public void OnDamageTaken(float damageAmount)
        {
            Debug.Log($"[HealingSystem] Damage taken: {damageAmount}. Pausing regen.");
            isTakingDamage = true;

            // Stop healing immediately
            StopHealing();

            // Restart cooldown before allowing healing again
            StopAllCoroutines(); // Ensures we don't stack multiple coroutines
            StartCoroutine(DamageCooldownTimer());
        }


        private IEnumerator DamageCooldownTimer()
        {
            yield return new WaitForSeconds(damageCooldown);

            isTakingDamage = false;
            Debug.Log("[HealingSystem] Healing can resume after cooldown.");

            // Check if the player is still below their regen cap and restart healing if needed
            float currentHealth = healthManager.meleeFighter.CurrentHealth;
            float targetCap = GetTargetHealthCap();

            if (currentHealth < targetCap && !isHealing)
            {
                Debug.Log("[HealingSystem] Resuming healing after cooldown.");
                StartCoroutine(HealOverTime());
            }
        }


        private void UpdateRegenLevel()
        {
            float maxHealth = healthManager.meleeFighter.MaxHealth;
            float currentHealth = healthManager.meleeFighter.CurrentHealth;

            // Only allow progression to a lower cap level (higher severity)
            if (currentRegenLevel == 0 || currentRegenLevel > 3)
            {
                currentRegenLevel = 1; // If somehow reset, default to Level 1
            }

            if (currentRegenLevel == 1 && currentHealth < maxHealth * 0.50f)
            {
                currentRegenLevel = 2; // Move to Level 2 (lower cap)
                Debug.Log("[HealingSystem] Locking to Level 2. Cap: " + maxHealth * 0.50f);
            }

            if (currentRegenLevel == 2 && currentHealth < maxHealth * 0.25f)
            {
                currentRegenLevel = 3; // Move to Level 3 (lowest cap)
                Debug.Log("[HealingSystem] Locking to Level 3. Cap: " + maxHealth * 0.25f);
            }
        }


        private float GetTargetHealthCap()
        {
            float maxHealth = healthManager.meleeFighter.MaxHealth;

            // Return the cap for the current regen level
            switch (currentRegenLevel)
            {
                case 3:
                    return maxHealth * 0.25f; // Level 3 cap
                case 2:
                    return maxHealth * 0.50f; // Level 2 cap
                case 1:
                    return maxHealth * 0.75f; // Level 1 cap
                default:
                    return maxHealth; // Default to max health (this should not occur)
            }
        }

        private void InitializeRegenLevel()
        {
            float maxHealth = healthManager.meleeFighter.MaxHealth;
            float currentHealth = healthManager.meleeFighter.CurrentHealth;

            if (currentHealth < maxHealth * 0.25f)
            {
                currentRegenLevel = 3;
                Debug.Log("[HealingSystem] Initializing to Level 3. Cap: " + maxHealth * 0.25f);
            }
            else if (currentHealth < maxHealth * 0.50f)
            {
                currentRegenLevel = 2;
                Debug.Log("[HealingSystem] Initializing to Level 2. Cap: " + maxHealth * 0.50f);
            }
            else if (currentHealth < maxHealth * 0.75f)
            {
                currentRegenLevel = 1;
                Debug.Log("[HealingSystem] Initializing to Level 1. Cap: " + maxHealth * 0.75f);
            }
            else
            {
                currentRegenLevel = 0;
                Debug.Log("[HealingSystem] No regen needed on start. Current health is above 75%");
            }
        }

        private void StopHealing()
        {
            isHealing = false;
            Debug.Log("[HealingSystem] Healing stopped.");
        }

        public void ResetRegenLevel()
        {
            currentRegenLevel = 0;
            Debug.Log("[HealingSystem] Regen level reset.");
        }
    }
}
