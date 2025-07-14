using FS_ThirdPerson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FS_Core
{
    public class QuickSwitchHandler : MonoBehaviour
    {
        [Header("Quick Switch Settings")]
        [SerializeField] private bool enableQuickSwitchItemUI = true;
        [SerializeField] private int maxItemsToAddQuickSwitchWheel = 8;
        [SerializeField] private Transform wheelParent;
        [SerializeField] private GameObject segmentPrefab;
        [SerializeField] private Sprite hoverSprite;
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private List<EquippableItem> quickSwitchItems = new();

        private const float MIN_JOYSTICK_THRESHOLD = 0.1f;
        private const float POPUP_SCALE = 1.2f;
        private const float ANIMATION_DURATION = 0.3f;
        private const float WHEEL_PADDING = 40f;
        private const float BUTTON_SCALE = 0.5f;

        private Vector3 originalScale;
        private Coroutine activeCoroutine;
        private Dictionary<Image, EquippableItem> quickSwitchUIElements = new();
        private Dictionary<Image, Button> quickSwitchUIButtons = new();
        private Image currentHoveringButton;

        private bool uiShowing;
        private CursorState prevCursorState;

        private LocomotionInputManager inputManager;
        private ItemEquipper equipHandler;
        private LocomotionICharacter player;

        private struct CursorState
        {
            public bool visible;
            public CursorLockMode lockMode;
            public float timeScale;
        }

        private void Awake()
        {
            InitializeComponents();
            InitializeWheel();

            equipHandler.OnEquip += (item) => {
                if (!quickSwitchItems.Contains(item))
                    AddQuickSwitchItem(item);
                SetButtonHoverState(null); 
            };
            equipHandler.OnUnEquip += () => { SetButtonHoverState(null, true); };

            equipHandler.OnItemBecameUnused += RemoveQuickSwitchItem;
        }

        private void InitializeComponents()
        {
            inputManager = GetComponent<LocomotionInputManager>();
            equipHandler = GetComponent<ItemEquipper>();
            player = GetComponent<LocomotionICharacter>();
        }

        private void InitializeWheel()
        {
            if (quickSwitchItems.Count > 0 && wheelParent != null)
            {
                originalScale = wheelParent.localScale;
                wheelParent.localScale = Vector3.zero;
                wheelParent.gameObject.SetActive(false);
                GenerateWeaponWheel();
            }
        }

        private void Update()
        {
            if (!enableQuickSwitchItemUI) return;

            if (inputManager.QuickSwitchItemDown)
            {
                PopupUI();
            }

            if (uiShowing)
            {
                HandleUIInteraction();
            }
        }

        private void HandleUIInteraction()
        {
            HighlightButtonWithJoystick(inputManager.NavigationInput);

            if (ShouldHideUI())
            {
                HideAndEquipItem();
            }
        }

        private bool ShouldHideUI()
        {
            Vector3 mousePos = Input.mousePosition;
            return inputManager.QuickSwitchItemUp ||
                   mousePos.x < 0 || mousePos.y < 0 ||
                   mousePos.x > Screen.width || mousePos.y > Screen.height;
        }

        private void HideAndEquipItem()
        {
            HideUI();

            if (currentHoveringButton != null &&
                quickSwitchUIElements.TryGetValue(currentHoveringButton, out var item))
            {
                equipHandler.EquipItem(item);
            }
        }

        private void GenerateWeaponWheel()
        {
            CleanupExistingElements();

            int weaponCount = Mathf.Min(maxItemsToAddQuickSwitchWheel, quickSwitchItems.Count);
            float angleStep = 360f / weaponCount;

            for (int i = 0; i < weaponCount; i++)
            {
                CreateWheelSegment(quickSwitchItems[i], angleStep, i);
            }

            ArrangeUIInCircle();
        }

        private void CleanupExistingElements()
        {
            foreach (var image in quickSwitchUIElements.Keys.ToList())
            {
                if (image != null) Destroy(image.gameObject);
            }
            foreach (var button in quickSwitchUIButtons.Values.ToList())
            {
                if (button != null) Destroy(button.gameObject);
            }
            quickSwitchUIButtons.Clear();
            quickSwitchUIElements.Clear();
        }

        private void CreateWheelSegment(EquippableItem item, float angleStep, int index)
        {
            var segment = Instantiate(segmentPrefab, wheelParent);
            var image = segment.GetComponent<Image>();

            SetupSegmentImage(image, angleStep);
            var rt = segment.GetComponent<RectTransform>();
            rt.localRotation = Quaternion.Euler(0, 0, -angleStep * index);

            quickSwitchUIElements[image] = item;
        }

        private void SetupSegmentImage(Image image, float angleStep)
        {
            image.sprite = hoverSprite;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillAmount = 1f / (360f / angleStep);
            image.fillOrigin = (int)Image.Origin360.Top;
            image.fillClockwise = true;
        }

        private void PopupUI()
        {
            if (wheelParent == null || quickSwitchItems.Count == 0) return;

            SaveCursorState();
            SetUIState(true);
            AnimateWheel(true);
        }

        private void HideUI()
        {
            if (wheelParent == null || quickSwitchItems.Count == 0) return;

            RestoreCursorState();
            SetUIState(false);
            AnimateWheel(false);
        }

        private void SaveCursorState()
        {
            prevCursorState = new CursorState
            {
                visible = Cursor.visible,
                lockMode = Cursor.lockState,
                timeScale = Time.timeScale == 0 ? 1 : Time.timeScale
            };
        }

        private void RestoreCursorState()
        {
            Time.timeScale = prevCursorState.timeScale;
            Cursor.visible = prevCursorState.visible;
            Cursor.lockState = prevCursorState.lockMode;
        }

        private void SetUIState(bool show)
        {
            player.PreventAllSystems = show;
            if (show)
            {
                Time.timeScale = 0f;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            uiShowing = show;
        }

        private void AnimateWheel(bool show)
        {
            if (activeCoroutine != null) StopCoroutine(activeCoroutine);

            wheelParent.gameObject.SetActive(true);
            Vector3 targetScale = show ? originalScale * POPUP_SCALE : Vector3.zero;
            Action onComplete = show ? null : () => wheelParent.gameObject.SetActive(false);

            activeCoroutine = StartCoroutine(ScaleUI(wheelParent.localScale, targetScale, onComplete));
        }

        private IEnumerator ScaleUI(Vector3 startScale, Vector3 endScale, Action onComplete = null)
        {
            float elapsed = 0f;
            while (elapsed < ANIMATION_DURATION)
            {
                wheelParent.localScale = Vector3.Lerp(startScale, endScale, elapsed / ANIMATION_DURATION);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            wheelParent.localScale = endScale;
            onComplete?.Invoke();
        }


        private void HighlightButtonWithJoystick(Vector2 joystickInput)
        {
            if (quickSwitchUIElements == null ||
                quickSwitchUIElements.Count == 0 ||
                joystickInput.sqrMagnitude < MIN_JOYSTICK_THRESHOLD)
            {
                if(currentHoveringButton != null)
                    SetButtonHoverState(null);
                return;
            }

            float inputAngle = (Mathf.Atan2(joystickInput.y, joystickInput.x) * Mathf.Rad2Deg + 360f) % 360f;
            float angleStep = 360f / quickSwitchUIElements.Count;

            Image closestImage = FindClosestSegment(inputAngle, angleStep);
            SetButtonHoverState(closestImage);
        }

        private Image FindClosestSegment(float inputAngle, float angleStep)
        {
            Image closestImage = null;
            float smallestAngleDiff = angleStep / 2f;

            foreach (var pair in quickSwitchUIElements)
            {
                float segmentAngle = (pair.Key.rectTransform.localEulerAngles.z + 360f) % 360f;
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(segmentAngle, inputAngle));

                if (angleDiff < smallestAngleDiff)
                {
                    smallestAngleDiff = angleDiff;
                    closestImage = pair.Key;
                }
            }

            return closestImage;
        }

        private void SetButtonHoverState(Image hoveredButtonImage, bool reset = false)
        {
            Image equippedItemImage = GetEquippedItemImage();

            foreach (var pair in quickSwitchUIElements)
            {
                var image = pair.Key;

                if (!reset)
                {
                    bool isEquipped = image == equippedItemImage;
                    bool isHovered = image == hoveredButtonImage;

                    image.sprite = isEquipped ? selectedSprite :
                                 isHovered ? hoverSprite : null;
                    image.enabled = isEquipped || isHovered;

                    currentHoveringButton = hoveredButtonImage;
                }
                else
                {
                    image.sprite =  null;
                    image.enabled = false;
                    currentHoveringButton = null;
                }
            }
        }

        public Image GetEquippedItemImage()
        {
            var currentItem = equipHandler.EquippedItem;
            if (currentItem == null) return null;

            foreach (var kvp in quickSwitchUIElements)
            {
                if (kvp.Value == currentItem) return kvp.Key;
            }
            return null;
        }

        private void ArrangeUIInCircle()
        {
            if (quickSwitchUIElements == null || quickSwitchUIElements.Count == 0)
            {
                Debug.LogWarning("Invalid parameters for UI alignment.");
                return;
            }

            float radius = CalculateWheelRadius();
            ArrangeButtons(radius);
        }

        private float CalculateWheelRadius()
        {
            return ((wheelParent.GetComponent<RectTransform>().rect.width - WHEEL_PADDING) *
                    originalScale.x) / 2f;
        }

        private void ArrangeButtons(float radius)
        {
            int count = quickSwitchUIElements.Count;
            float angleStep = 360f / count;
            var uiElements = quickSwitchUIElements.Keys.ToList();

            for (int i = 0; i < count; i++)
            {
                var element = uiElements[i];
                ArrangeButton(element, i, angleStep, radius);
            }
        }

        private void ArrangeButton(Image element, int index, float angleStep, float radius)
        {
            float startAngle = (-angleStep * index) + 90;
            float midAngle = startAngle - (angleStep / 2f);

            var button = element.GetComponentInChildren<Button>();
            var buttonRect = button.GetComponent<RectTransform>();
            var buttonImage = button.GetComponent<Image>();

            SetupButtonTransform(buttonRect, midAngle, radius);
            SetupButtonVisuals(buttonRect, buttonImage, midAngle);
            SetupButtonBehavior(button, index);

            quickSwitchUIButtons.Add(element, button);
        }

        private void SetupButtonTransform(RectTransform buttonRect, float midAngle, float radius)
        {
            buttonRect.transform.SetParent(wheelParent);
            buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);

            float radianAngle = midAngle * Mathf.Deg2Rad;
            buttonRect.anchoredPosition = new Vector2(
                Mathf.Cos(radianAngle) * radius,
                Mathf.Sin(radianAngle) * radius
            );
        }

        private void SetupButtonVisuals(RectTransform buttonRect, Image buttonImage, float midAngle)
        {
            buttonRect.localRotation = Quaternion.Euler(0, 0, midAngle + 90);
            if (buttonImage != null)
            {
                buttonImage.transform.localRotation = Quaternion.identity;
                buttonImage.rectTransform.localScale = Vector3.one;
            }
            buttonRect.localScale = Vector3.one * BUTTON_SCALE;
        }

        private void SetupButtonBehavior(Button button, int index)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                equipHandler.EquipItem(quickSwitchItems[index]);
                HideUI();
            });

            button.GetComponent<Image>().sprite = quickSwitchItems[index].Icon;
        }


        public void AddQuickSwitchItem(EquippableItem newItem)
        {
            if (wheelParent == null || newItem == null ||
                quickSwitchItems.Contains(newItem) ||
                quickSwitchItems.Count >= maxItemsToAddQuickSwitchWheel)
                return;

            quickSwitchItems.Add(newItem);
            GenerateWeaponWheel();
        }

        public void RemoveQuickSwitchItem(EquippableItem itemToRemove)
        {
            if (wheelParent == null || itemToRemove == null ||
                !quickSwitchItems.Contains(itemToRemove))
                return;

            quickSwitchItems.Remove(itemToRemove);
            GenerateWeaponWheel();
        }
    }
}
