using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace FS_CombatSystem
{
    public class StaminaManager : MonoBehaviour
    {
        [Header("Stamina Settings")]
        public float maxStamina = 100f;
        public float currentStamina;
        public float staminaRegenRate = 5f;
        public float staminaRegenOutOfCombatRate = 10f;
        public float staminaRegenCooldown = 5f;
        public float outOfCombatTimeToRefill = 10f;
        public float zeroStaminaSlowdown = 0.5f;

        [Header("Stamina Costs")]
        public float lightAttackCost = 10f;
        public float heavyAttackCost = 20f;
        public float finisherCost = 40f;
        public float dodgeCost = 10f;
        public float rollCost = 15f;
        public float blockCost = 5f;

        [Header("UI Settings")]
        public Image staminaBar;
        public float pulseDuration = 1f;
        public GameObject refillGlowEffect;

        [Header("Sound Effects")]
        public AudioSource audioSource;
        public AudioClip staminaFullSound;
        public AudioClip lowStaminaSound;
        public AudioClip exhaustionSound;

        private MeleeFighter meleeFighter;
        private CombatController combatController;
        private bool isOutOfCombat = false;
        private bool isTakingDamage = false;
        private bool isExhausted = false;
        private float lastCombatTime = 0f;
        private float lastDamageTime = 0f;
        private float defaultMoveSpeed;

        private void Start()
        {
            meleeFighter = GetComponent<MeleeFighter>();
            combatController = GetComponent<CombatController>();

            if (meleeFighter == null)
            {
                Debug.LogError("[StaminaManager] MeleeFighter script missing!");
                return;
            }

            if (combatController == null)
            {
                Debug.LogError("[StaminaManager] CombatController script missing!");
                return;
            }

            defaultMoveSpeed = combatController.MoveSpeed; // Store original speed
            currentStamina = maxStamina;
            UpdateStaminaBar();

            // Subscribe to attack events
            if (meleeFighter.OnAttackEvent != null)
                meleeFighter.OnAttackEvent.AddListener(OnAttackPerformed);
        }

        private void Update()
        {
            if (meleeFighter == null) return;

            HandleStaminaRegeneration();
            CheckOutOfCombatRefill();

            if (meleeFighter.State == FighterState.Dodging)
                HandleDodge();

            if (meleeFighter.CanRoll && meleeFighter.State == FighterState.Rolling)
                HandleRoll();

            if (meleeFighter.IsBlocking)
                HandleBlock();
        }

        private void OnAttackPerformed(GameObject attackObject)
        {
            if (attackObject == null) return;

            // Get AttackData component (if it exists)
            AttackData attackData = attackObject.GetComponent<AttackData>();

            if (attackData == null)
            {
                Debug.LogWarning("[StaminaManager] AttackData missing on attack object!");
                return;
            }

            // Retrieve reaction tag
            string reactionTag = attackData.reactionTag.ToLower();

            switch (reactionTag)
            {
                case "light":
                    UseStamina(lightAttackCost);
                    break;
                case "heavy":
                    if (currentStamina >= heavyAttackCost)
                        UseStamina(heavyAttackCost);
                    else
                        Debug.Log("[StaminaManager] Not enough stamina for Heavy Attack!");
                    break;
                case "finisher":
                    if (currentStamina >= finisherCost)
                        UseStamina(finisherCost);
                    else
                        Debug.Log("[StaminaManager] Not enough stamina for Finisher!");
                    break;
                default:
                    Debug.Log("[StaminaManager] Unknown reaction tag: " + reactionTag);
                    break;
            }

            lastCombatTime = Time.time;
            isOutOfCombat = false;
        }

        private void HandleDodge()
        {
            if (currentStamina >= dodgeCost)
                UseStamina(dodgeCost);
            else
                Debug.Log("[StaminaManager] Not enough stamina to dodge!");
        }

        private void HandleRoll()
        {
            if (currentStamina >= rollCost)
                UseStamina(rollCost);
            else
                Debug.Log("[StaminaManager] Not enough stamina to roll!");
        }

        private void HandleBlock()
        {
            if (currentStamina >= blockCost)
                UseStamina(blockCost);
            else
                Debug.Log("[StaminaManager] Not enough stamina to block!");
        }

        private void UseStamina(float amount)
        {
            currentStamina = Mathf.Max(currentStamina - amount, 0);
            UpdateStaminaBar();
            PlaySound(lowStaminaSound);

            if (currentStamina == 0)
            {
                isExhausted = true;
                ApplyExhaustionEffect();
            }
        }

        private void ApplyExhaustionEffect()
        {
            if (combatController == null) return;

            combatController.MoveSpeed = defaultMoveSpeed * zeroStaminaSlowdown;
            Debug.Log("[StaminaManager] Movement slowed due to exhaustion!");
            PlaySound(exhaustionSound);

            StartCoroutine(RestoreMovementSpeed());
        }

        private IEnumerator RestoreMovementSpeed()
        {
            yield return new WaitForSeconds(3f);
            combatController.MoveSpeed = defaultMoveSpeed;
            Debug.Log("[StaminaManager] Movement speed restored.");
        }

        private void HandleStaminaRegeneration()
        {
            if (isTakingDamage) return;

            float regenRate = isOutOfCombat ? staminaRegenOutOfCombatRate : staminaRegenRate;

            if (currentStamina < maxStamina)
            {
                currentStamina += regenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
                UpdateStaminaBar();

                if (currentStamina == maxStamina)
                {
                    PlaySound(staminaFullSound);
                    StartRefillGlowEffect();
                }
            }
        }

        private void CheckOutOfCombatRefill()
        {
            if (Time.time - lastCombatTime >= outOfCombatTimeToRefill && isOutOfCombat && currentStamina < maxStamina)
            {
                currentStamina = maxStamina;
                UpdateStaminaBar();
                PlaySound(staminaFullSound);
                StartRefillGlowEffect();
            }
        }

        private void UpdateStaminaBar()
        {
            if (staminaBar != null)
            {
                staminaBar.fillAmount = currentStamina / maxStamina;
                staminaBar.color = currentStamina < 10f ? Color.red : Color.white;
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }

        private void StartRefillGlowEffect()
        {
            if (refillGlowEffect != null)
            {
                refillGlowEffect.SetActive(true);
                Invoke(nameof(HideGlowEffect), pulseDuration);
            }
        }

        private void HideGlowEffect()
        {
            if (refillGlowEffect != null)
                refillGlowEffect.SetActive(false);
        }

        private void OnDestroy()
        {
            if (meleeFighter != null && meleeFighter.OnAttackEvent != null)
                meleeFighter.OnAttackEvent.RemoveListener(OnAttackPerformed);
        }
    }
}
