using UnityEngine;

namespace FS_CombatSystem
{
    public class RagdollHandler : MonoBehaviour
    {
        private MeleeFighter fighter;

        private void Awake()
        {
            fighter = GetComponent<MeleeFighter>();
            if (fighter != null)
            {
                fighter.OnDeath += () =>
                {
                    Debug.Log("[RagdollHandler] OnDeath triggered");
                    fighter.SetRagdollState(true);
                };
            }
        }

    }
}
