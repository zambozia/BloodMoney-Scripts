using UnityEngine;
using UnityEngine.UI;

public class AutoGridNavigation : MonoBehaviour
{
    [Header("Grid Dimensions")]
    public int columns = 1; // number of buttons per row
    public Button[] buttons;

    void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            Navigation nav = buttons[i].navigation;
            nav.mode = Navigation.Mode.Explicit;

            int row = i / columns;
            int col = i % columns;

            // Up
            int upIndex = i - columns;
            if (upIndex >= 0)
                nav.selectOnUp = buttons[upIndex];

            // Down
            int downIndex = i + columns;
            if (downIndex < buttons.Length)
                nav.selectOnDown = buttons[downIndex];

            // Left
            if (col > 0)
                nav.selectOnLeft = buttons[i - 1];

            // Right
            if (col < columns - 1 && i + 1 < buttons.Length)
                nav.selectOnRight = buttons[i + 1];

            buttons[i].navigation = nav;
        }
    }
}
