
using UnityEngine;
using System.Collections;
// using System.Diagnostics;

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
                healthManager = FindObjectOfType<HealthManager>();

                if (healthManager == null)
                {
                    UnityEngine.Debug.LogError("[HealingSystem] ERROR: Missing HealthManager reference in the scene!");
                    return;
                }
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

        public void OnHealthUpdated(float currentHealth, float maxHealth)
        {
            if (isTakingDamage) return;

            float targetCap = GetTargetHealthCap();

            if (currentHealth >= targetCap)
            {
                UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] No regen needed. Health at threshold ({targetCap}).");
                StopHealing();
                return;
            }

            if (!isHealing)
            {
                UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] Starting healing coroutine. Current: {currentHealth}, Target Cap: {targetCap}");
                StartCoroutine(HealOverTime());
            }
        }

        private IEnumerator HealOverTime()
        {
            isHealing = true;

            while (isHealing)
            {
                if (isTakingDamage)
                {
                    UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] Healing stopped due to damage taken.");
                    StopHealing();
                    yield break;
                }

                UpdateRegenLevel();
                float targetCap = GetTargetHealthCap();
                float currentHealth = healthManager.meleeFighter.CurrentHealth;

                if (currentHealth >= targetCap)
                {
                    UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] Stopping healing. Current: {currentHealth}, Target Cap: {targetCap}, Level: {currentRegenLevel}");
                    StopHealing();
                    yield break;
                }

                float healthToAdd = Mathf.Min(healingAmount, targetCap - currentHealth);

                if (healthToAdd > 0)
                {
                    UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] Healing {healthToAdd} HP...");
                    healthManager.Heal(healthToAdd);
                    UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] Current: {healthManager.meleeFighter.CurrentHealth}, Cap: {targetCap}, Level: {currentRegenLevel}");
                }
                else
                {
                    UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] Already at cap, stopping healing.");
                    StopHealing();
                    yield break;
                }

                yield return new WaitForSeconds(healingInterval);
            }
        }


        public void OnDamageTaken(float damageAmount)
        {
            if (isTakingDamage) return;

            UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] Damage taken: {damageAmount}. Pausing regen.");
            isTakingDamage = true;
            StopHealing();
            StartCoroutine(DamageCooldownTimer());
        }

        private IEnumerator DamageCooldownTimer()
        {
            yield return new WaitForSeconds(damageCooldown);
            isTakingDamage = false;

            UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] Healing can resume after cooldown.");

            float currentHealth = healthManager.meleeFighter.CurrentHealth;
            float targetCap = GetTargetHealthCap();

            if (currentHealth < targetCap && !isHealing)
            {
                UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] Resuming healing after cooldown.");
                StartCoroutine(HealOverTime());
            }
        }

        private void UpdateRegenLevel()
        {
            float maxHealth = healthManager.meleeFighter.MaxHealth;
            float currentHealth = healthManager.meleeFighter.CurrentHealth;

            if (currentRegenLevel == 0 || currentRegenLevel > 3)
            {
                currentRegenLevel = 1;
            }

            if (currentRegenLevel == 1 && currentHealth < maxHealth * 0.50f)
            {
                currentRegenLevel = 2;
                UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] Locking to Level 2. Cap: {maxHealth * 0.50f}");
            }

            if (currentRegenLevel == 2 && currentHealth < maxHealth * 0.25f)
            {
                currentRegenLevel = 3;
                UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] Locking to Level 3. Cap: {maxHealth * 0.25f}");
            }
        }

        private float GetTargetHealthCap()
        {
            float maxHealth = healthManager.meleeFighter.MaxHealth;

            // Ensure currentRegenLevel is valid
            if (currentRegenLevel < 1 || currentRegenLevel > 3)
            {
                UnityEngine.Debug.Log("[HealingSystem] Invalid regen level, defaulting to Level 1");
                currentRegenLevel = 1;
            }

            if (healthManager == null || healthManager.meleeFighter == null)
            {
                Debug.LogError("[HealingSystem] ERROR: healthManager or meleeFighter is null!");
                return 0f;
            }

            // Set targetHealth based on the current regen level
            float targetHealth = maxHealth * (currentRegenLevel == 3 ? 0.25f : currentRegenLevel == 2 ? 0.50f : 0.75f);

            // Debugging line to verify calculations
            UnityEngine.Debug.Log($"[HealingSystem] Target Health Cap: {targetHealth} at Regen Level: {currentRegenLevel}");

            return targetHealth;
        }


        private void StopHealing()
        {
            if (isHealing)
            {
                isHealing = false;
                StopAllCoroutines();
                UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] Healing stopped.");
            }
        }

        private void InitializeRegenLevel()
        {
            float currentHealth = healthManager.meleeFighter.CurrentHealth;
            float maxHealth = healthManager.meleeFighter.MaxHealth;

            if (currentHealth < maxHealth * 0.25f)
            {
                currentRegenLevel = 3;
                UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] Initializing to Level 3. Cap: {maxHealth * 0.25f}");
            }
            else if (currentHealth < maxHealth * 0.50f)
            {
                currentRegenLevel = 2;
                UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] Initializing to Level 2. Cap: {maxHealth * 0.50f}");
            }
            else if (currentHealth < maxHealth * 0.75f)
            {
                currentRegenLevel = 1;
                UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] Initializing to Level 1. Cap: {maxHealth * 0.75f}");
            }
            else
            {
                UnityEngine.Debug.Log($"[{Time.time}][HealingSystem] No regen needed on start. Current health above 75%");
                currentRegenLevel = 0;
            }
        }
    }
}
