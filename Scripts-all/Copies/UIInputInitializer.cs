using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputInitializer : MonoBehaviour
{
    public InputActionAsset actions;

    void OnEnable()
    {
        if (actions == null) return;

        actions.Enable();

        // Set the UI Action Map as the active one
        InputActionMap uiMap = actions.FindActionMap("UI", true);
        if (uiMap != null)
            uiMap.Enable();
    }

    void OnDisable()
    {
        actions.Disable();
    }
}
