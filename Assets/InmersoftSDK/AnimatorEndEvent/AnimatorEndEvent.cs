using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace InmersoftSDK.AnimatorEndEvent
{
    [RequireComponent(typeof(Animator))]
    public class AnimationEndEvent : MonoBehaviour
    {
        private Animator animC;
        public UnityEvent OnAnimationEnd;
        public bool startAnimationOnAwake = true;

        private void Awake()
        {
            animC = GetComponent<Animator>();
            if (startAnimationOnAwake)
            {
                animC.Play(0);
                StartCoroutine(WaitAnimEnd());
            }
        }

        private void Start()
        {
            StartCoroutine(WaitAnimEnd());
        }

        /// <summary>
        /// Start first index animation and the coroutine to wait the end of animation
        /// </summary>
        public void StartAnimation()
        {
            animC.Play(0);
            StartCoroutine(WaitAnimEnd());
        }

        /// <summary>
        /// Start the animation with index equal to indexAnimation and start the coroutine to wait end of the animation
        /// </summary>
        /// <param name="indexAnim"></param>
        public void StartAnimation(int indexAnim)
        {
            animC.Play(indexAnim);
            StartCoroutine(WaitAnimEnd());
        }

        /// <summary>
        /// Start the animation named nameAnim and start the coroutine to wait end of the animation
        /// </summary>
        /// <param name="nameAnim"></param>
        public void StartAnimation(string nameAnim)
        {
            animC.Play(nameAnim);
            StartCoroutine(WaitAnimEnd());
        }

        /// <summary>
        /// This coroutine wait for the end of animation and then call the event OnAnimationEnd
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitAnimEnd()
        {
            while (animC.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f) yield return null;

            OnAnimationEnd.Invoke();
        }
    }
}