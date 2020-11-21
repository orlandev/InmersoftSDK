using InmersoftSDK.Kernel;
using InmersoftSDK.Singleton;
using UnityEngine;

namespace InmersoftSDK.ScreenWakeLock
{
    public class ScreenWakeLock : Singleton<ScreenWakeLock>
    {
        /// <summary>
        /// Set the sleepTimeout to NeverSleep, the screen never turn off.
        /// </summary>
        public void AlwaysScreenOn()
        {
            if (!Application.isMobilePlatform) return;
            if (!UnityEngine.XR.XRSettings.enabled) Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        /// <summary>
        /// Set the custom sleepTimeout, the screen may be turn off.
        /// </summary>
        public void CustomScreen()
        {
            if (!Application.isMobilePlatform) return;
            if (!UnityEngine.XR.XRSettings.enabled) Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
    }
}