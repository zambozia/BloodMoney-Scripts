using FS_Core;
using FS_ThirdPerson;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
namespace FS_ThirdPerson
{
    public static partial class AnimatorParameters
    {
        public static int combatMode = Animator.StringToHash("combatMode");
        public static int strafeSpeed = Animator.StringToHash("strafeSpeed");
        public static int isBlocking = Animator.StringToHash("IsBlocking");
    }
}
namespace FS_CombatSystem
{

    public enum TargetSelectionCriteria { DirectionAndDistance, Direction, Distance }

    public class CombatController : EquippableSystemBase
    {
        [SerializeField] float moveSpeed = 2f;
        [SerializeField] float rotationSpeed = 500f;

        [ShowEquipmentDropdown]
        [SerializeField] int meleeWeaponSlot;

        [Tooltip("Criteria used for selecting the target enemy the player should attack")]
        [SerializeField] TargetSelectionCriteria targetSelectionCriteria;

        [Tooltip("Increase this value if direction should be given more weight than distance for selecting the target. If distance should be given more weight, then decrease it.")]
        [HideInInspector] public float directionScaleFactor = 0.1f;

        public MeleeFighter meleeFighter { get; private set; }
        public Vector3 InputDir { get; private set; }

        Animator animator;
        AnimGraph animGraph;
        CharacterController characterController;
        CombatInputManager inputManager;
        LocomotionICharacter locomotionICharacter;
        Damagable damagable;
        PlayerController playerController;
        EnemyController targetEnemy;
        ItemEquipper equippableItemController;
        ItemAttacher itemAttacher;

        List<Collider> colliders = new List<Collider>();
        //bool isGrounded;
        float ySpeed;
        Quaternion targetRotation;
        bool combatMode;
        bool prevCombatMode;
        float _moveSpeed;

        public override List<Type> EquippableItems => new List<Type>() { typeof(WeaponData) };
        public override SystemState State => SystemState.Combat;

        public override void OnResetFighter()
        {
            locomotionICharacter.PreventAllSystems = false;
            characterController.enabled = true;
            colliders.ForEach(c => c.enabled = true);

            meleeFighter.ResetFighter();
        }

        public EnemyController TargetEnemy {
            get => targetEnemy;
            set {
                if (targetEnemy != value)
                {
                    targetEnemy?.OnRemovedAsTarget?.Invoke();
                    targetEnemy = value;
                    meleeFighter.Target = targetEnemy?.Fighter;
                    targetEnemy?.OnSelectedAsTarget?.Invoke();
                }

                if (targetEnemy == null)
                {
                    CombatMode = false;
                }
            }
        }

        public bool CombatMode {
            get => combatMode;
            set {
                combatMode = value;
                if (TargetEnemy == null)
                    combatMode = false;
                if (prevCombatMode != combatMode)
                {
                    if (combatMode)
                    {
                        locomotionICharacter.OnStartSystem(this);
                    }
                    else if (!meleeFighter.IsBusy)
                    {
                        locomotionICharacter.OnEndSystem(this);
                    }
                    prevCombatMode = combatMode;
                }


                animator?.SetBool(AnimatorParameters.combatMode, combatMode);
            }
        }

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            locomotionICharacter = GetComponent<LocomotionICharacter>();
            damagable = GetComponent<Damagable>();
            characterController = GetComponent<CharacterController>();
            meleeFighter = GetComponent<MeleeFighter>();
            inputManager = GetComponent<CombatInputManager>();
            locomotionICharacter = GetComponent<LocomotionController>();
            animGraph = GetComponent<AnimGraph>();
            equippableItemController = GetComponent<ItemEquipper>();
            itemAttacher = GetComponent<ItemAttacher>();

            _moveSpeed = moveSpeed;
        }

        private void Start()
        {
            animator = locomotionICharacter.Animator;

            inputManager.OnAttackPressed += OnAttackPressed;

            meleeFighter.IsPlayerForDebug = true;

            meleeFighter.OnGotHit += (MeleeFighter attacker, Vector3 hitPoint, float hittingTime, bool isBlockedHit) =>
            {
                if (equippableItemController.EquippedItem is WeaponData)
                    CombatMode = true;
                if (attacker != TargetEnemy.Fighter)
                    TargetEnemy = attacker.GetComponent<EnemyController>();
            };

            meleeFighter.OnAttack += (target) => { if (target != null) CombatMode = true; };

            meleeFighter.OnDeath += Death;
            meleeFighter.OnWeaponEquipAction += (WeaponData weaponData) =>
            {
                if (weaponData.OverrideMoveSpeed)
                {
                    moveSpeed = weaponData.CombatModeSpeed;
                }
            };
            meleeFighter.OnWeaponUnEquipAction += (WeaponData weaponData) =>
            {
                locomotionICharacter.ResetMoveSpeed();
                moveSpeed = _moveSpeed;
                //CombatMode = false;
            };

            meleeFighter.OnStartAction += StartSystem;
            meleeFighter.OnEndAction += EndSystem;

            itemAttacher.DefaultItem = meleeFighter.deafultWeapon;
        }

        void StartSystem()
        {
            if (meleeFighter.State != FighterState.SwitchingWeapon)
            {
                locomotionICharacter.UseRootMotion = true;
                locomotionICharacter.OnStartSystem(this);
                equippableItemController.PreventItemSwitching = true;
            }
        }

        void EndSystem()
        {
            if (!combatMode)
                locomotionICharacter.OnEndSystem(this);
            locomotionICharacter.UseRootMotion = false;
            equippableItemController.PreventItemSwitching = false;
        }

        void OnAttackPressed(float holdTime, bool isHeavyAttack, bool isCounter, bool isCharged, bool isSpecialAttack)
        {
            if (locomotionICharacter.PreventAllSystems || (playerController.CurrentEquippedSystem != null && playerController.CurrentEquippedSystem != this)
                || (playerController.CurrentSystemState != SystemState.Locomotion && playerController.CurrentSystemState != SystemState.Combat && playerController.CurrentSystemState != SystemState.Cover))
                return;
            if (TargetEnemy == null && isCounter && !CombatSettings.i.SameInputForAttackAndCounter) return;

            if (locomotionICharacter.IsGrounded && !meleeFighter.IsDead)
            {
                var dirToAttack = locomotionICharacter.MoveDir == Vector3.zero ? transform.forward : locomotionICharacter.MoveDir;

                var enemyToAttack = EnemyManager.i?.GetEnemyToTarget(dirToAttack);
                if (enemyToAttack != null)
                    TargetEnemy = enemyToAttack;

                if (CombatSettings.i.SameInputForAttackAndCounter)
                {
                    if (meleeFighter.IsBeingAttacked && meleeFighter.CurrAttacker.AttackState == AttackStates.Windup)
                        isCounter = true;
                    else
                        isCounter = false;
                }

                meleeFighter.TryToAttack(enemyToAttack?.Fighter, isHeavyAttack: isHeavyAttack, isCounter: isCounter, isCharged: isCharged, isSpecialAttack: isSpecialAttack);
            }
        }

        void Death()
        {
            characterController.enabled = false;
            colliders = GetComponentsInChildren<Collider>().ToList().Where(c => c.enabled).ToList();
            foreach (var collider in colliders)
                collider.enabled = false;

            locomotionICharacter.UseRootMotion = false;
            locomotionICharacter.PreventAllSystems = true;
            locomotionICharacter.OnEndSystem(this);
        }

        public Vector3 GetTargetingDir()
        {
            if (!CombatMode)
            {
                var vecFromCam = transform.position - Camera.main.transform.position;
                vecFromCam.y = 0f;
                return vecFromCam.normalized;
            }
            else
            {
                return transform.forward;
            }
        }

        private void Update()
        {
            meleeFighter.CanTakeHit = damagable.CanTakeHit;

            // Equip
            if (inputManager.Equip)
            {
                var attachedWeapon = itemAttacher.GetAttachedItem(meleeWeaponSlot) as WeaponData;
                if (attachedWeapon != null)
                    equippableItemController.EquipItem(attachedWeapon, true, onItemEnabled: () => itemAttacher.EquipItem(attachedWeapon));
            }
        }

        public override void HandleUpdate()
        {
            if (meleeFighter.CurrentWeaponData == null || meleeFighter.damagable.CurrentHealth <= 0)
            {
                //CombatMode = false;
                return;
            }

            if (inputManager.CombatMode && !meleeFighter.IsBusy)
                CombatMode = !combatMode;

            meleeFighter.IsBlocking = inputManager.Block && (!meleeFighter.IsBusy || meleeFighter.State == FighterState.TakingBlockedHit) && meleeFighter.CurrentWeaponData.CanBlock;


            if (meleeFighter.CanDodge && inputManager.Dodge && !meleeFighter.IsBusy)
            {
                if (!IsInFocus) return;
                if (!meleeFighter.OnlyDodgeInCombatMode || CombatMode)
                    StartCoroutine(meleeFighter.Dodge(locomotionICharacter.MoveDir));
                return;
            }

            if (meleeFighter.CanRoll && inputManager.Roll && !meleeFighter.IsBusy)
            {
                if (!IsInFocus) return;

                if (!meleeFighter.OnlyRollInCombatMode || CombatMode)
                    StartCoroutine(meleeFighter.Roll(locomotionICharacter.MoveDir));
                return;
            }

            // UnEquip
            if (inputManager.UnEquip)
            {
                var equippedItem = equippableItemController.EquippedItem as WeaponData;
                if (equippedItem != null)
                {
                    equippableItemController.EquipItem(meleeFighter.deafultWeapon, onPrevItemDisabled: () => itemAttacher.UnEquipItem(equippedItem));
                }
            }

            if (!CombatMode)
            {
                ApplyAnimationGravity();
                return;
            }

            if (meleeFighter != null && (meleeFighter.InAction || meleeFighter.damagable.CurrentHealth <= 0))
            {
                targetRotation = transform.rotation;
                animator.SetFloat(AnimatorParameters.moveAmount, 0f);
                ApplyAnimationGravity();
                return;
            }

            float h = locomotionICharacter.MoveDir.x;
            float v = locomotionICharacter.MoveDir.z;

            float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

            var moveDir = locomotionICharacter.MoveDir;
            InputDir = locomotionICharacter.MoveDir;

            var velocity = moveDir * moveSpeed;

            // Rotate and face the target enemy
            Vector3 targetVec = transform.forward;
            if (TargetEnemy != null)
            {
                targetVec = TargetEnemy.transform.position - transform.position;
                targetVec.y = 0;
            }

            if (moveAmount > 0)
            {
                targetRotation = Quaternion.LookRotation(targetVec);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                    rotationSpeed * Time.deltaTime);
            }


            // Split the velocity into it's forward and sideward component and set it into the forwardSpeed and strafeSpeed
            float forwardSpeed = Vector3.Dot(velocity, transform.forward);
            animator.SetFloat(AnimatorParameters.moveAmount, forwardSpeed / moveSpeed, 0.2f, Time.deltaTime);

            float angle = Vector3.SignedAngle(transform.forward, velocity, Vector3.up);
            float strafeSpeed = Mathf.Sin(angle * Mathf.Deg2Rad);
            animator.SetFloat(AnimatorParameters.strafeSpeed, strafeSpeed, 0.2f, Time.deltaTime);


            if (meleeFighter.CurrentWeaponData.UseRootmotion)
            {
                velocity = animator.deltaPosition;
                transform.rotation *= animator.deltaRotation;
            }
            else
                velocity = velocity * Time.deltaTime;

            velocity.y = ySpeed * Time.deltaTime;

            (moveDir, velocity) = locomotionICharacter.LedgeMovement(moveDir, velocity);
            if (!meleeFighter.StopMovement)
                characterController.Move(velocity);
        }

        public void ApplyAnimationGravity()
        {
            if (animGraph.currentClipStateInfo.isPlayingAnimation && animGraph.currentClipStateInfo.currentClipInfo.useGravity && !meleeFighter.IsMatchingTarget && IsInFocus)
            {
                ySpeed += Physics.gravity.y * Time.deltaTime;
                characterController.Move(ySpeed * Vector3.up * animGraph.currentClipInfo.gravityModifier.GetValue(animGraph.currentClipStateInfo.normalizedTime) * Time.deltaTime);
            }
            animator.SetBool(AnimatorParameters.IsGrounded, locomotionICharacter.CheckIsGrounded());

        }

        public override void HandleOnAnimatorMove(Animator animator)
        {
            if (locomotionICharacter.UseRootMotion && !meleeFighter.StopMovement)
            {
                //if (meleeFighter.IsDead)
                //    Debug.Log("Using root motion for death - Matching target - " + meleeFighter.IsMatchingTarget);
                transform.rotation *= animator.deltaRotation;

                var deltaPos = animator.deltaPosition;

                if (meleeFighter.IsMatchingTarget)
                    deltaPos = meleeFighter.MatchingTargetDeltaPos;
                var (newDeltaDir, newDelta) = locomotionICharacter.LedgeMovement(deltaPos.normalized, deltaPos);
                if (locomotionICharacter.IsOnLedge) return;

                if (meleeFighter.IgnoreCollisions)
                    transform.position += newDelta;
                else
                    characterController.Move(newDelta);
            }
        }

        public TargetSelectionCriteria TargetSelectionCriteria => targetSelectionCriteria;

        public override void ExitSystem()
        {
            if (meleeFighter.IsBlocking)
                meleeFighter.IsBlocking = false;
        }
    }
}