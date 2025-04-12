using UnityEngine;
using UnityEngine.EventSystems;

public class NavigationDebug : MonoBehaviour
{
    private void Update()
    {
        if (EventSystem.current != null)
        {
            Debug.Log("Current Selected: " + EventSystem.current.currentSelectedGameObject);
        }
    }
}
