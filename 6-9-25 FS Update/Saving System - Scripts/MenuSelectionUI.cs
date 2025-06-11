using FS_Core;
using FS_Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FS_ThirdPerson 
{

    public class MenuSelectionUI : SelectionUI<ImageSlot>
    {
        [SerializeField] List<ImageSlot> slots;
        [SerializeField] Image faderImage;

        public event Action OnClose;

        private void Start()
        {
            Init();
            SetItems(slots);

            OnSelected += MenuSelected;
        }

        private void MenuSelected(int selection)
        {
            if (selection == 0)
            {
                StartCoroutine(Save());
            }
            else if (selection == 1)
            {
                StartCoroutine(Load());
            }
        }

        IEnumerator Save()
        {
            yield return TweenUtil.TweenAlphaAsync(faderImage, 1, 0.5f);
            SavingSystem.i.Save("slot1");
            Time.timeScale = 1;
            yield return TweenUtil.TweenAlphaAsync(faderImage, 0, 0.5f);

            OnClose?.Invoke();
        }

        IEnumerator Load()
        {
            yield return TweenUtil.TweenAlphaAsync(faderImage, 1, 0.5f);
            SavingSystem.i.Load("slot1");
            Time.timeScale = 1;
            yield return TweenUtil.TweenAlphaAsync(faderImage, 0, 0.5f);

            OnClose?.Invoke();
        }

        private void Update()
        {
            base.HandleUpdate();
        }
    }

}
