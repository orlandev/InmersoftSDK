using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace InmersoftSDK.Kernel.Editor.PreBuildSettings
{
    /// <summary>
    ///     Programed by Orlando Novas Rodriguez
    ///     Inmersoft Programmer 2020
    ///     Inmersoft Team
    ///     Inmersoft Copyright 2020
    ///     Use only in Proyects property of Inmersoft
    /// </summary>
    public class PreBuildSettings : EditorWindow
    {
        static PreBuildSettings()
        {
            PlayerSettings.companyName = "Inmersoft";
            SetProjectKeystore();
            var inmersoft = new InmersoftPreBuild();
            
        }

        /// <summary>
        ///     Cuando se compile el proyecto se ejecutaran los cambios correspondientes
        ///     0- Version
        ///     1- BundleVersionCode con la que se va a subir a google play
        ///     2- Fecha de Compilacion
        ///     Ejemplo:
        ///     1.3.140520   Version 1, version del budleVersionCode 3, La fecha 14 de mayo del 2020
        /// </summary>
        private static void ProcessInmersoftSettings()
        {
            var date = DateTime.Now.ToString("dd/MM/yy").Replace("/", "");
            //Si es un proyecto nuevo le agregamos la nueva version bundle
            if (PlayerSettings.bundleVersion.Equals("0.1"))
            {
                PlayerSettings.applicationIdentifier =
                    "com." + PlayerSettings.companyName.ToLower() + "." + PlayerSettings.productName;
                PlayerSettings.bundleVersion = "1.1." + date;
                Debug.Log("INMERSOFT: New Project [ " + PlayerSettings.productName + " ] Good Locky.");
                Debug.Log("INMERSOFT: New Project ID: " + PlayerSettings.applicationIdentifier);
                Debug.Log("INMERSOFT: New App Version [ " + PlayerSettings.bundleVersion + " ]");
            }

            if (EditorUserBuildSettings.buildAppBundle)
            {
                PlayerSettings.Android.bundleVersionCode += 1;
                Debug.Log("INMERSOFT: Build App Bundle( Google Play) is active.");
                Debug.Log("INMERSOFT: Bundle Version Code is upgrade.");
            }

            //Aumentamos en 1 el Bundle Code Version


            var bundleVersion = PlayerSettings.bundleVersion.Split('.');

            if (date.Equals(bundleVersion[2])) return;
            var bundleVersionCode = PlayerSettings.Android.bundleVersionCode;
            var newAppVersion = bundleVersion[0] + "." + bundleVersionCode + "." + date;
            PlayerSettings.bundleVersion = newAppVersion;
            Debug.Log("INMERSOFT: Bundle Version Code [ " + PlayerSettings.Android.bundleVersionCode + " ]");
            Debug.Log("INMERSOFT: New App Version [ " + PlayerSettings.bundleVersion + " ]");
        }

        //   [MenuItem("InmersoftPlayerSettings/SetProjectKeystore _%&m", priority = 1)]
        private static void SetProjectKeystore()
        {
            var newFolderName = "Editor";
            var fileKeyName = "InmersoftKeyStore.json";
            var newDir = Path.Combine(Application.dataPath, newFolderName);
            if (!Directory.Exists(newDir)) Directory.CreateDirectory(newDir);

            if (!PlayerSettings.Android.useCustomKeystore)
            {
                Debug.Log("INMERSOFT: PROJECT KEYSTORE NOT FOUND.");
            }
            else if (!File.Exists(Path.Combine(newDir, fileKeyName)))
            {
                var iKey = new InmersoftKey();
                iKey.keyAliasName = PlayerSettings.Android.keyaliasName;
                iKey.keyAliasPass = PlayerSettings.Android.keyaliasPass;
                iKey.keyStoreName = PlayerSettings.Android.keystoreName;
                iKey.keyStorePass = PlayerSettings.Android.keystorePass;

                var json = JsonUtility.ToJson(iKey);
                var file = new StreamWriter(Path.Combine(newDir, fileKeyName));
                file.WriteLine(json);
                file.Close();
                AssetDatabase.Refresh();
                Debug.Log("INMERSOFT: KEY FILE SAVED. You don't need tape any more for this project.");
            }
            else
            {
                var file = new StreamReader(Path.Combine(newDir, fileKeyName));
                var jsonContent = file.ReadToEnd();
                var iKey = JsonUtility.FromJson<InmersoftKey>(jsonContent);
                PlayerSettings.Android.useCustomKeystore = true; //Activamos el suo de una Llave por defecto
                PlayerSettings.Android.keyaliasName = iKey.keyAliasName;
                PlayerSettings.Android.keyaliasPass = iKey.keyAliasPass;
                PlayerSettings.Android.keystoreName = iKey.keyStoreName;
                PlayerSettings.Android.keystorePass = iKey.keyStorePass;
                Debug.Log("INMERSOFT: APP KEY SETTED.");
            }
        }


        public class InmersoftPreBuild : IPreprocessBuildWithReport
        {
            public int callbackOrder { get; }

            public void OnPreprocessBuild(BuildReport report)
            {
                ProcessInmersoftSettings();
            }
        }

        [Serializable]
        private class InmersoftKey
        {
            public string keyAliasName;
            public string keyAliasPass;
            public string keyStoreName;
            public string keyStorePass;
        }
    }
}