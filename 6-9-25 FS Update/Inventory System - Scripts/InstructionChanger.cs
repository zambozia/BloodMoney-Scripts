using FS_Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS_InventorySystem
{
    public class InstructionChanger : MonoBehaviour
    {
        [SerializeField] UISwitcher uiSwitcher;
        [SerializeField] List<GameObject> instructions = new List<GameObject>();

        private void Awake()
        {
            uiSwitcher.OnUIChanged += (int activeIndex, int overallIndex) =>
            {
                UpdateInstructions(overallIndex);
            };
        }

        private void Start()
        {
            
        }

        void UpdateInstructions(int overallIndex)
        {
            for (int i = 0; i < instructions.Count; i++)
                instructions[i].SetActive(i == overallIndex);
        }
    }
}
