using UnityEngine;
using UnityEngine.Events;

namespace FS_CombatSystem
{
    public class HealthManager : MonoBehaviour
    {
        public MeleeFighter meleeFighter;
        public UnityEvent<float, float> OnHealthChanged; // currentHealth, maxHealth
        public UnityEvent<float> OnTakeDamage; // damageAmount
        public bool isKnockedDown = false; // Tracks if the player is knocked down

        private float lastHealth;

        private void Start()
        {
            if (meleeFighter == null)
            {
                meleeFighter = GetComponent<MeleeFighter>();
                if (meleeFighter == null)
                {
                    Debug.LogError("HealthManager requires a MeleeFighter reference!");
                    return;
                }
            }

            lastHealth = meleeFighter.CurrentHealth;
            OnHealthChanged?.Invoke(lastHealth, meleeFighter.MaxHealth);
        }

        private void Update()
        {
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
            meleeFighter.ModifyHealth(meleeFighter.CurrentHealth + amount);
        }

        public void ResetKnockdownState()
        {
            isKnockedDown = false; // Reset knockdown status after recovery
        }
    }
}
