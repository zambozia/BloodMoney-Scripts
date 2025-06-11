using FS_Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FS_Util {

    public enum MessageBoxType { Negative, Positive }

    [FoldoutGroup("Sound Effects", false, "negativeMessageSound", "positiveMessageSound")]
    public class MessageUI : MonoBehaviour
    {
        [SerializeField] TMP_Text messageTxt;

        [SerializeField] AudioClip negativeMessageSound;
        [SerializeField] AudioClip positiveMessageSound;

        bool isShowing;

        private void Start()
        {
            
        }

        
        public void Show(string message, MessageBoxType type = MessageBoxType.Negative)
        {
            if (isShowing) return;

            isShowing = true;

            gameObject.SetActive(true);
            messageTxt.text = message;

            StartCoroutine(TweenPos());

            if (type == MessageBoxType.Negative)
                FSAudioUtil.PlaySfx(negativeMessageSound);
            else if (type == MessageBoxType.Positive)
                FSAudioUtil.PlaySfx(positiveMessageSound);
        }

        IEnumerator TweenPos()
        {
            var originalPos = transform.position;
            var showPos = originalPos + new Vector3(0, 200f);

            yield return TweenUtil.TweenPositionAsync(gameObject, showPos, 0.2f);
            yield return new WaitForSecondsRealtime(1f);
            yield return TweenUtil.TweenPositionAsync(gameObject, originalPos, 0.2f);

            gameObject.SetActive(false);
            isShowing = false;
        }
    }

}
