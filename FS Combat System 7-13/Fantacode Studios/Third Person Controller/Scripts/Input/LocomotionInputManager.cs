using FS_Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS_ThirdPerson
{
    public class LocomotionInputManager : MonoBehaviour
    {
        [Header("Keys")]
        [SerializeField] KeyCode jumpKey = KeyCode.Space;
        [SerializeField] KeyCode dropKey = KeyCode.E;
        [SerializeField] KeyCode moveType = KeyCode.Tab;
        [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] KeyCode interactionKey = KeyCode.E;
        [SerializeField] KeyCode quickSwitchItemKey = KeyCode.CapsLock;
        [SerializeField] KeyCode selectKey = KeyCode.E;
        [SerializeField] KeyCode backKey = KeyCode.Escape;
        [SerializeField] KeyCode crouchKey = KeyCode.C;


        [Header("Buttons")]
        [SerializeField] string jumpButton;
        [SerializeField] string dropButton;
        [SerializeField] string moveTypeButton;
        [SerializeField] string sprintButton;
        [SerializeField] string interactionButton;
        [SerializeField] string quickSwitchItemButton;
        [SerializeField] string selectButton;
        [SerializeField] string backButton;
        [SerializeField] string crouchButton;

        public bool JumpKeyDown { get; set; }
        public bool Drop { get; set; }
        public Vector2 DirectionInput { get; set; }
        public Vector2 CameraInput { get; set; }
        public bool ToggleRun { get; set; }
        public bool SprintKey { get; set; }
        public bool Interaction { get; set; }
        public bool QuickSwitchItemDown { get; set; }
        public bool QuickSwitchItemUp { get; set; }
        public bool Crouch { get; set; }

        // Navigation
        public Vector2 NavigationInput { get; set; }
        public bool Select { get; set; }
        public bool Back { get; set; }

        public event Action OnInteractionPressed;
        public event Action<float> OnInteractionReleased;

        public float InteractionButtonHoldTime { get; set; } = 0f;
        bool interactionButtonDown;

        public string InteractionKeyName { get; private set; }

#if inputsystem
        FSSystemsInputAction input;
        private void OnEnable()
        {
            input = new FSSystemsInputAction();
            input.Enable();

#if inputsystem
            InteractionKeyName = input.Locomotion.Interaction.ToString();
#else
            InteractionKeyName = interactionKey.ToString();
#endif
        }
        private void OnDisable()
        {
            input.Disable();
        }
#endif


        private void Update()
        {
            HandleNavigationInput();
            HandleSelect();
            HandleBack();

            //Quick Switch
            HandleQuickSwitchUIInput();

            // Other inputs should not worked when paused
            if (Time.deltaTime == 0) return;

            //Horizontal and Vertical Movement
            HandleDirectionalInput();

            //Camera Movement
            HandlecameraInput();

            //JumpKeyDown
            HandleJumpKeyDown();

            //Drop
            HandleDrop();

            //Walk or Run 
            HandleToggleRun();

            //Sprint
            HandleSprint();

            //Interaction
            HandleInteraction();

            HandleCrouch();
        }

        void HandleDirectionalInput()
        {
#if inputsystem
            DirectionInput = input.Locomotion.MoveInput.ReadValue<Vector2>();
#else
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            DirectionInput = new Vector2(h, v);
#endif
        }

        void HandlecameraInput()
        {
#if inputsystem
            CameraInput = input.Locomotion.CameraInput.ReadValue<Vector2>();
#else
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");
            CameraInput = new Vector2(x, y);
#endif
        }

        void HandleJumpKeyDown()
        {
#if inputsystem
            JumpKeyDown = input.Locomotion.Jump.WasPressedThisFrame();
#else
            JumpKeyDown = Input.GetKeyDown(jumpKey) || (String.IsNullOrEmpty(jumpButton) ? false : Input.GetButtonDown(jumpButton));
#endif
        }

        void HandleDrop()
        {
#if inputsystem
            Drop = input.Locomotion.Drop.inProgress;
#else
            Drop = Input.GetKey(dropKey) || (String.IsNullOrEmpty(dropButton) ? false : Input.GetButton(dropButton));
#endif
        }

        void HandleToggleRun()
        {
#if inputsystem
            ToggleRun = input.Locomotion.MoveType.WasPressedThisFrame();
#else
            ToggleRun = Input.GetKeyDown(moveType) || IsButtonDown(moveTypeButton);
#endif
        }

        void HandleSprint()
        {
#if inputsystem
            SprintKey = input.Locomotion.SprintKey.inProgress;
#else
            SprintKey = Input.GetKey(sprintKey) || (String.IsNullOrEmpty(sprintButton) ? false : Input.GetButton(sprintButton));
#endif
        }

        void HandleInteraction()
        {
#if inputsystem
            if (input.Locomotion.Interaction.WasPressedThisFrame())
            {
                interactionButtonDown = true;
                Interaction = true;
            }
            else
            {
                Interaction = false;
            }

            if (interactionButtonDown)
            {
                if (input.Locomotion.Interaction.WasReleasedThisFrame())
                {
                    interactionButtonDown = false;
                    InteractionButtonHoldTime = 0f;
                }

                InteractionButtonHoldTime += Time.deltaTime;
            }

#else

            if (Input.GetKeyDown(interactionKey) || IsButtonDown(interactionButton))
            {
                interactionButtonDown = true;
                Interaction = true;
            }
            else
            {
                Interaction = false;
            }

            if (interactionButtonDown)
            {
                if (Input.GetKeyUp(interactionKey) || IsButtonUp(interactionButton))
                {
                    interactionButtonDown = false;
                    InteractionButtonHoldTime = 0f;
                }

                InteractionButtonHoldTime += Time.deltaTime;
            }
#endif
        }

        void HandleQuickSwitchUIInput()
        {
#if inputsystem
            QuickSwitchItemDown = input.Locomotion.QuickSwitchItem.WasPressedThisFrame();
            QuickSwitchItemUp = input.Locomotion.QuickSwitchItem.WasReleasedThisFrame();
#else
            QuickSwitchItemDown = Input.GetKeyDown(quickSwitchItemKey) || (String.IsNullOrEmpty(quickSwitchItemButton) ? false : Input.GetButtonDown(quickSwitchItemButton));
            QuickSwitchItemUp = Input.GetKeyUp(quickSwitchItemKey) || (String.IsNullOrEmpty(quickSwitchItemButton) ? false : Input.GetButtonUp(quickSwitchItemButton));
#endif
        }

        void HandleNavigationInput()
        {
#if inputsystem
            NavigationInput = input.Selection.Navigation.ReadValue<Vector2>();
#else
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            NavigationInput = new Vector2(h, v);
#endif
        }

        private void HandleSelect()
        {
#if inputsystem
            Select = input.Selection.Select.WasPressedThisFrame();
#else
            Select = Input.GetKeyDown(selectKey) || (!string.IsNullOrEmpty(selectButton) && Input.GetButtonDown(selectButton));
#endif
        }

        private void HandleBack()
        {
#if inputsystem
            Back = input.Selection.Back.WasPressedThisFrame();
#else
            Back = Input.GetKeyDown(backKey) || (!string.IsNullOrEmpty(backButton) && Input.GetButtonDown(backButton));
#endif
        }

        private void HandleCrouch()
        {
#if inputsystem
            Crouch = input.Locomotion.Crouch.WasPressedThisFrame();
#else
            Crouch = Input.GetKeyDown(crouchKey) || (!string.IsNullOrEmpty(crouchButton) && Input.GetButtonDown(crouchButton));
#endif
        }

        public bool IsButtonDown(string buttonName)
        {
            if (!String.IsNullOrEmpty(buttonName))
                return Input.GetButtonDown(buttonName);
            else
                return false;
        }

        public bool IsButtonUp(string buttonName)
        {
            if (!String.IsNullOrEmpty(buttonName))
                return Input.GetButtonUp(buttonName);
            else
                return false;
        }
    }
}