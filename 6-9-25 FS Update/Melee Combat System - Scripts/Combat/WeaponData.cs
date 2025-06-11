using FS_Core;
using FS_ThirdPerson;
using System.Collections.Generic;
using UnityEngine;
namespace FS_CombatSystem
{
    [CustomIcon(FolderPath.CombatIcons + "Weapon Icon.png")]
    [CreateAssetMenu(menuName = "Combat System/Create Weapon")]
    public class WeaponData : EquippableItem
    {
        [Tooltip("List of attack datas, each containing details about an attack and its conditions.")]
        [SerializeField] List<AttackContainer> attacks;

        [Tooltip("List of attack data that is considered a heavy attack, each containing details about an attack and its conditions.")]
        [SerializeField] List<AttackContainer> heavyAttacks;

        [Tooltip("List of attack data that is considered a special attacks, each containing details about an attack and its conditions.")]
        [SerializeField] List<AttackContainer> specialAttacks;

        [Tooltip("Data for reactions associated with the weapon.")]
        [SerializeField] ReactionsData reactionData;

        [Tooltip("Indicates if the weapon can block attacks.")]
        [SerializeField] bool canBlock;

        [Tooltip("The animation clip used for blocking with the attacks.")]
        [SerializeField] AnimationClip blocking;

        [Tooltip("The percentage of damage taken when getting hit while blocking.")]
        [Range(0, 100)]
        [SerializeField] float blockedDamage = 25f;

        [Tooltip("Data for reactions when blocking with the weapon.")]
        [SerializeField] ReactionsData blockReactionData;

        [Tooltip("Indicates if the weapon can perform counterattacks.")]
        [SerializeField] bool canCounter = true;

        [Tooltip("If true, the fighter will play a taunt action if the counter input is pressed while the enemy is not attacking. This can be used to prevent the misuse of the counter input.")]
        [SerializeField] bool playActionIfCounterMisused = false;
        [Tooltip("Animation clip of the action to play if the counter is pressed while the enemy is not attacking.")]
        [SerializeField] AnimationClip counterMisusedAction;


        [Tooltip("Indicates if the locomotion movement speed should be overridden.")]
        public bool overrideMoveSpeed;

        [Tooltip("Movement speed while the character is in combat mode(Locked into a target).")]
        public float combatMoveSpeed = 2f;
        [Tooltip("Indicates if root motion should be used for movement")]
        [SerializeField] bool useRootmotion;

        [Tooltip("Indicates if the fighter can dodge.")]
        [SerializeField] bool overrideDodge;
        [SerializeField] DodgeData dodgeData;

        [Tooltip("Indicates if the fighter can roll.")]
        [SerializeField] bool overrideRoll;
        [SerializeField] DodgeData rollData;

        [Tooltip("The minimum distance required for the weapon to have an effective attack.")]
        [SerializeField] float minAttackDistance = 0;

        public void InIt()
        {
            foreach (var attack in attacks)
                attack.AttackSlots.ForEach(a => a.Container = attack);

            foreach (var attack in heavyAttacks)
                attack.AttackSlots.ForEach(a => a.Container = attack);

            foreach (var attack in specialAttacks)
                attack.AttackSlots.ForEach(a => a.Container = attack);
        }

        public List<AttackContainer> Attacks => attacks;
        public List<AttackContainer> HeavyAttacks => heavyAttacks;
        public List<AttackContainer> SpecialAttacks => specialAttacks;
        public ReactionsData ReactionData => reactionData;
        public bool CanBlock => canBlock;
        public AnimationClip Blocking => blocking;
        public float BlockedDamage => blockedDamage;
        public ReactionsData BlockReactionData => blockReactionData;
        public bool CanCounter => canCounter;
        public bool PlayActionIfCounterMisused => playActionIfCounterMisused;
        public AnimationClip CounterMisusedAction => counterMisusedAction;
        public bool UseRootmotion => useRootmotion;
        public bool OverrideMoveSpeed => overrideMoveSpeed;
        public float CombatModeSpeed => combatMoveSpeed;
        public bool OverrideDodge => overrideDodge;
        public DodgeData DodgeData => dodgeData;
        public bool OverrideRoll => overrideRoll;
        public DodgeData RollData => rollData;
        public float MinAttackDistance => minAttackDistance;

        public void SetCategory()
        {
            category = Resources.Load<ItemCategory>("Category/Melee Weapon");
        }
    }
}