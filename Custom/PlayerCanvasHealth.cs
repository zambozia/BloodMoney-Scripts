using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

namespace FS_CombatSystem
{
    public class PlayerCanvasHealth : MonoBehaviour
    {
        public MeleeFighter meleeFighter;
        public HealthManager healthManager; // Reference to HealthManager for healing updates

        [SerializeField] private Image healthBarImg;
        private float health;

        private void Awake()
        {
            if (meleeFighter == null)
            {
                meleeFighter = GetComponentInParent<MeleeFighter>();
                Debug.Assert(meleeFighter != null, "MeleeFighter not found in parent! Please assign it manually.");
            }

            if (healthManager == null)
            {
                healthManager = GetComponentInParent<HealthManager>();
                Debug.Assert(healthManager != null, "HealthManager not found in parent! Please assign it manually.");
            }
        }


        private void OnEnable()
        {
            // Subscribe to both damage and healing events
            meleeFighter.OnGotHit += ControlHealthBar;

            if (healthManager != null)
                healthManager.OnHealthChanged.AddListener(UpdateHealthBar);
        }

        public void OnDisable()
        {
            meleeFighter.OnGotHit -= ControlHealthBar;

            if (healthManager != null)
                healthManager.OnHealthChanged.RemoveListener(UpdateHealthBar);
        }

        // Update health bar when taking damage
        private void ControlHealthBar(MeleeFighter fighter, Vector3 hitPoint, float hittingTime, bool isBlockedHit)
        {
            StartCoroutine(LerpHealth());
        }

        // Update health bar when healing
        public void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            health = currentHealth / maxHealth;
            healthBarImg.fillAmount = health;
        }

        private IEnumerator LerpHealth()
        {
            float targetHealth = meleeFighter.CurrentHealth / meleeFighter.MaxHealth;
            while (health > targetHealth)
            {
                health = Mathf.MoveTowards(health, targetHealth, Time.deltaTime);
                healthBarImg.fillAmount = health;
                yield return null;
            }

            health = targetHealth;
            healthBarImg.fillAmount = health;
        }
    }
}
