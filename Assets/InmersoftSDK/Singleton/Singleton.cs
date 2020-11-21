using UnityEngine;

namespace InmersoftSDK.Singleton
{
    public abstract class Singleton<T> : MonoBehaviour where T : Component
    {
        /***
         * This Sigleton was creted by Inmersoft Team 2019
         * Programmed by Orlando Novas Rodriguez
         * Property of Inmersoft
         */

        #region Fields

        /// <summary>
        ///     The instance.
        /// </summary>
        private static T _instance;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindObjectOfType<T>();
                if (_instance != null) return _instance;
                var obj = new GameObject();
                obj.name = typeof(T).Name;
                _instance = obj.AddComponent<T>();
                return _instance;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Use this for initialization.
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion
    }
}