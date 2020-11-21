using UnityEngine;

namespace InmersoftSDK
{
    public static class InmersoftSdkVersion
    {
        private const string Version = "3.0.1";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ShowInmersoftSdkVersion()
        {
            Debug.Log("InmersoftSDK Version " + Version);
        }
    }
}