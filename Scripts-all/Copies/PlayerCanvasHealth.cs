using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace FS_CombatSystem
{
    public class PlayerCanvasHealth : MonoBehaviour
    {
        public MeleeFighter meleeFighter;
        public HealthManager healthManager;

        [SerializeField] private UnityEngine.UI.Image healthBarImg;
        public UnityEngine.UI.Image HealthBarImg => healthBarImg;

        private float health;
        private bool initialized = false;

        private void Start()
        {
            StartCoroutine(Initialize());
        }

        private IEnumerator Initialize()
        {
            // Keep searching until we find a player with the required components
            while (!initialized)
            {
                GameObject[] allRoots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var root in allRoots)
                {
                    MeleeFighter foundFighter = root.GetComponentInChildren<MeleeFighter>();
                    HealthManager foundHealth = root.GetComponentInChildren<HealthManager>();

                    if (foundFighter != null && foundHealth != null)
                    {
                        meleeFighter = foundFighter;
                        healthManager = foundHealth;
                        initialized = true;
                        break;
                    }
                }

                if (!initialized)
                    yield return null;
            }

            Debug.Log("[PlayerCanvasHealth] Found MeleeFighter and HealthManager.");

            // Apply starting health
            health = meleeFighter.CurrentHealth / meleeFighter.MaxHealth;
            healthBarImg.fillAmount = health;

            // Subscribe to damage and healing
            meleeFighter.OnGotHit += ControlHealthBar;
            healthManager.OnHealthChanged.AddListener(UpdateHealthBar);
        }

        private void OnDisable()
        {
            if (meleeFighter != null)
                meleeFighter.OnGotHit -= ControlHealthBar;

            if (healthManager != null)
                healthManager.OnHealthChanged.RemoveListener(UpdateHealthBar);
        }

        public void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            health = currentHealth / maxHealth;
            healthBarImg.fillAmount = health;
        }

        private void ControlHealthBar(MeleeFighter fighter, Vector3 hitPoint, float hittingTime, bool isBlockedHit)
        {
            StartCoroutine(LerpHealth());
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
