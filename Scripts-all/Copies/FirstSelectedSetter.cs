using UnityEngine;
using UnityEngine.EventSystems;

public class FirstSelectedSetter : MonoBehaviour
{
    public GameObject firstButton;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(firstButton);
    }
}
