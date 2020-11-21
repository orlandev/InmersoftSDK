using System.IO;
using InmersoftSDK.Kernel.IO;
using UnityEditor;
using UnityEngine;
using File = UnityEngine.Windows.File;

namespace InmersoftSDK.Kernel.Editor.ChangeLog
{
    /**
 *  This script belongs to Inmersoft Corporation
 * All Right Reserverd
 *
 * Inmersoft Programmers Team 2020
 * *
 */
    public class ChangeLog : EditorWindow
    {
        private static ChangeLog _iWindows;
        private readonly string fileName = "ChangeLog.json";
        private string _changeLoggerStr = "";

        private Texture2D InmersoftLogoSmall;
        private Vector2 scroll;

        //Cntrol+L abre la ventana
        [MenuItem("InmersoftSDK/ChangeLog _%l", priority = 1)]
        public static void ShowWindows()
        {
            if (!_iWindows)
                InitializeWindow();
        }

        private static ChangeLog InitializeWindow()
        {
            _iWindows = GetWindow<ChangeLog>();
            var guiDsmallLogo = AssetDatabase.FindAssets("InmersoftLogoSmall t:texture2D", null);
            if (guiDsmallLogo.Length >= 1)
            {
                _iWindows.InmersoftLogoSmall =
                    AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guiDsmallLogo[0]), typeof(Texture2D)) as
                        Texture2D;
                _iWindows.titleContent.image = _iWindows.InmersoftLogoSmall;
                _iWindows.titleContent.text = "Inmersoft Chnange Log";
            }

            _iWindows.LoadFileChangeLog();
            _iWindows.Show();
            return _iWindows;
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("Created by Inmersoft Programmers Team 2020");
            GUILayout.Label("This tool is Property of Inmersoft");
            GUILayout.Space(10);
            GUILayout.Label("Proyect: [ " + PlayerSettings.productName + " ]", EditorStyles.boldLabel);
            GUILayout.Space(10);
            GUILayout.Label("Changes and working to do.", EditorStyles.boldLabel);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            _changeLoggerStr = EditorGUILayout.TextArea(_changeLoggerStr, GUILayout.Height(position.height - 30));
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Save", GUILayout.Width(100), GUILayout.Height(50))) SaveChange();
        }

        private void SaveChange()
        {
            JsonFile.SaveJsonFile(Path.Combine(Application.dataPath, fileName), _changeLoggerStr);
            AssetDatabase.Refresh();
            Debug.Log("ChangeLog Saved.");
        }

        private void LoadFileChangeLog()
        {
            var filePath = Path.Combine(Application.dataPath, fileName);
            if (File.Exists(filePath)) _changeLoggerStr = JsonFile.LoadJsonFile<string>(filePath);
        }


        private void OnInspectorUpdate()
        {
            if (!_iWindows)
                InitializeWindow();
        }
    }
}