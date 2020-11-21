using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace InmersoftSDK.SceneTransition
{
    /// <summary>
    ///     This class is responsible for managing the transitions between scenes that are performed in the game
    ///     via a classic fade.
    /// </summary>
    public class Transition : MonoBehaviour
    {
        private static GameObject canvasObject;

        private GameObject m_overlay;

        /// <summary>
        ///     Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            canvasObject = new GameObject("TransitionCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            DontDestroyOnLoad(canvasObject);
        }

        /// <summary>
        ///     Loads the specified level.
        /// </summary>
        /// <param name="level">The name of the level to load.</param>
        /// <param name="duration">The duration of the fade.</param>
        /// <param name="fadeColor">The color of the fade.</param>
        public static void LoadLevel(string level, float duration, Color fadeColor)
        {
            var fade = new GameObject("Transition");
            fade.AddComponent<Transition>();
            fade.GetComponent<Transition>().StartFade(level, duration, fadeColor);
            fade.transform.SetParent(canvasObject.transform, false);
            fade.transform.SetAsLastSibling();
        }

        /// <summary>
        /// Starts the fade to the new level.
        /// </summary>
        /// <param name="level">The name of the level to load.</param>
        /// <param name="duration">The duration of the fade.</param>
        /// <param name="fadeColor">The color of the fade.</param>
        /// <param name="fadeSprite">The sprite to use like a background</param>
        public static void LoadLevel(string level, float duration, Color fadeColor, Sprite fadeSprite)
        {
            var fade = new GameObject("Transition");
            fade.AddComponent<Transition>();
            fade.GetComponent<Transition>().StartFade(level, duration, fadeColor, fadeSprite);
            fade.transform.SetParent(canvasObject.transform, false);
            fade.transform.SetAsLastSibling();
        }

        /// <summary>
        ///     Starts the fade to the new level.
        /// </summary>
        /// <param name="level">The name of the level to load.</param>
        /// <param name="duration">The duration of the fade.</param>
        /// <param name="fadeColor">The color of the fade.</param>
        private void StartFade(string level, float duration, Color fadeColor)
        {
            StartCoroutine(RunFade(level, duration, fadeColor));
        }

        private void StartFade(string level, float duration, Color fadeColor, Sprite fadeSprite)
        {
            StartCoroutine(RunFade(level, duration, fadeColor, fadeSprite));
        }

        /// <summary>
        ///     This coroutine performs the actual work of fading out of the current scene and into the new scene.
        /// </summary>
        /// <param name="level">The name of the level to load.</param>
        /// <param name="duration">The duration of the fade.</param>
        /// <param name="fadeColor">The color of the fade.</param>
        /// <returns></returns>
        private IEnumerator RunFade(string level, float duration, Color fadeColor)
        {
            var bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, fadeColor);
            bgTex.Apply();

            m_overlay = new GameObject();
            var image = m_overlay.AddComponent<Image>();
            var rect = new Rect(0, 0, bgTex.width, bgTex.height);
            var sprite = Sprite.Create(bgTex, rect, new Vector2(0.5f, 0.5f), 1);
            image.material.mainTexture = bgTex;
            image.sprite = sprite;
            var newColor = image.color;
            image.color = newColor;
            image.canvasRenderer.SetAlpha(0.0f);

            m_overlay.transform.localScale = new Vector3(1, 1, 1);
            m_overlay.GetComponent<RectTransform>().sizeDelta = canvasObject.GetComponent<RectTransform>().sizeDelta;
            m_overlay.transform.SetParent(canvasObject.transform, false);
            m_overlay.transform.SetAsFirstSibling();

            var time = 0.0f;
            var halfDuration = duration / 2.0f;
            while (time < halfDuration)
            {
                time += Time.deltaTime;
                image.canvasRenderer.SetAlpha(Mathf.InverseLerp(0, 1, time / halfDuration));
                yield return new WaitForEndOfFrame();
            }

            image.canvasRenderer.SetAlpha(1.0f);
            yield return new WaitForEndOfFrame();

            SceneManager.LoadScene(level);

            time = 0.0f;
            while (time < halfDuration)
            {
                time += Time.deltaTime;
                image.canvasRenderer.SetAlpha(Mathf.InverseLerp(1, 0, time / halfDuration));
                yield return new WaitForEndOfFrame();
            }

            image.canvasRenderer.SetAlpha(0.0f);
            yield return new WaitForEndOfFrame();

            Destroy(canvasObject);
        }

        /// <summary>
        ///     This coroutine performs the actual work of fading out of the current scene and into the new scene.
        /// </summary>
        /// <param name="level">The name of the level to load.</param>
        /// <param name="duration">The duration of the fade.</param>
        /// <param name="fadeColor">The color of the fade.</param>
        /// <param name="fadeSprite">The sprite used like a background</param>
        /// <returns></returns>
        private IEnumerator RunFade(string level, float duration, Color fadeColor, Sprite fadeSprite)
        {
            var bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, fadeColor);
            bgTex.Apply();

            m_overlay = new GameObject();
            var image = m_overlay.AddComponent<Image>();
            var rect = new Rect(0, 0, bgTex.width, bgTex.height);
            var sprite = Sprite.Create(bgTex, rect, new Vector2(0.5f, 0.5f), 1);
            image.material.mainTexture = bgTex;
            image.sprite = fadeSprite;
            var newColor = image.color;
            image.color = newColor;
            image.canvasRenderer.SetAlpha(0.0f);

            m_overlay.transform.localScale = new Vector3(1, 1, 1);
            m_overlay.GetComponent<RectTransform>().sizeDelta = canvasObject.GetComponent<RectTransform>().sizeDelta;
            m_overlay.transform.SetParent(canvasObject.transform, false);
            m_overlay.transform.SetAsFirstSibling();

            var time = 0.0f;
            var halfDuration = duration / 2.0f;
            while (time < halfDuration)
            {
                time += Time.deltaTime;
                image.canvasRenderer.SetAlpha(Mathf.InverseLerp(0, 1, time / halfDuration));
                yield return new WaitForEndOfFrame();
            }

            image.canvasRenderer.SetAlpha(1.0f);
            yield return new WaitForEndOfFrame();

            SceneManager.LoadScene(level);

            time = 0.0f;
            while (time < halfDuration)
            {
                time += Time.deltaTime;
                image.canvasRenderer.SetAlpha(Mathf.InverseLerp(1, 0, time / halfDuration));
                yield return new WaitForEndOfFrame();
            }

            image.canvasRenderer.SetAlpha(0.0f);
            yield return new WaitForEndOfFrame();

            Destroy(canvasObject);
        }
    }
}