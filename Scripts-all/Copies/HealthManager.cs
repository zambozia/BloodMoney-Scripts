using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Diagnostics;

namespace FS_CombatSystem
{
    public class HealthManager : MonoBehaviour
    {
        public MeleeFighter meleeFighter;
        public UnityEvent<float, float> OnHealthChanged; // currentHealth, maxHealth
        public UnityEvent<float> OnTakeDamage; // damageAmount
        public bool isKnockedDown = false; // Tracks if the player is knocked down

        private float lastHealth;
        private bool hasSpawned = false;

        private IEnumerator Start()
        {
            if (meleeFighter == null)
            {
                meleeFighter = GetComponent<MeleeFighter>();
                if (meleeFighter == null)
                {
                    UnityEngine.Debug.LogError("HealthManager requires a MeleeFighter reference!");
                    yield break;
                }
            }

            // Delay to allow setup of the character
            yield return new WaitForSeconds(0.5f);

            lastHealth = meleeFighter.CurrentHealth;
            hasSpawned = true;

            OnHealthChanged?.Invoke(lastHealth, meleeFighter.MaxHealth);
        }

        private void Update()
        {
            if (!hasSpawned) return;

            if (meleeFighter.CurrentHealth != lastHealth)
            {
                float damageTaken = lastHealth - meleeFighter.CurrentHealth;
                lastHealth = meleeFighter.CurrentHealth;

                OnHealthChanged?.Invoke(lastHealth, meleeFighter.MaxHealth);

                if (damageTaken > 0)
                {
                    OnTakeDamage?.Invoke(damageTaken); // Trigger damage event
                }

                if (lastHealth <= 0)
                {
                    isKnockedDown = true; // Mark player as knocked down
                }
            }
        }

        public void Heal(float amount)
        {
            float newHealth = Mathf.Min(meleeFighter.CurrentHealth + amount, meleeFighter.MaxHealth);
            meleeFighter.ModifyHealth(newHealth); // Ensure it does NOT exceed max health
        }

        public void ResetKnockdownState()
        {
            isKnockedDown = false; // Reset knockdown status after recovery
        }
    }
}
