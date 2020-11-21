using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TimeKeeper.Editor.Kernel
{
    [InitializeOnLoad]
    public class TimeKeeper : EditorWindow
    {
        private const int WindowWidth = 512;
        private const int WindowHeight = 500;
        private const int ImageWidthX = 2;
        private const int ImageHeightY = 215;
        private const int ImageWidth = 509;
        private const int ImageHeight = 270;

        private const int MinModuleBarHeight = 90;
        private static TimeKeeper _timeKeepWindows;
        private static readonly string windowsName = TimeKeeperConstants.ProductName;

        private static bool _autoTimeKeeper = true;


        private static Texture2D _toolFacebookIcon;

        private static Texture2D _toolGooglePLayIcon;
        private Texture2D _keepSaveLogo;
        private Texture2D _mUiSplash;
        private Texture2D _toogle;
        private Texture2D _toogleOff;
        private Texture2D _toogleOn;

        static TimeKeeper()
        {
            EditorApplication.playModeStateChanged += AutoSaveOnRun;
        }

        private GUIStyle ModulePanelStyle => TimeKeeperGuiSkin.GetCustomStyle("Module Box");

        private GUIStyle ModuleTitleStyle => TimeKeeperGuiSkin.GetCustomStyle("Module Selection Title");

        private static GUIStyle ModuleDescriptionStyle =>
            TimeKeeperGuiSkin.GetCustomStyle("Module Selection Description");

        private GUIStyle ModuleIconStyle => TimeKeeperGuiSkin.GetCustomStyle("Module Selection Icon");

        private GUIStyle ModuleHeaderIconStyle => TimeKeeperGuiSkin.GetCustomStyle("Module Header Icon");

        public static Texture2D ToolFacebookIcon
        {
            get
            {
                if (_toolFacebookIcon != null) return _toolFacebookIcon;
                var iconName = "facebook.png";
                _toolFacebookIcon =
                    AssetDatabase.LoadAssetAtPath<Texture2D>(
                        TimeKeeperConstants.SkinTextureFolder + "/" + iconName);

                return _toolFacebookIcon;
            }
        }

        public static Texture2D ToolGooglePLayIcon
        {
            get
            {
                if (_toolGooglePLayIcon != null) return _toolGooglePLayIcon;
                var iconName = "googleplay.psd";
                _toolGooglePLayIcon =
                    AssetDatabase.LoadAssetAtPath<Texture2D>(
                        TimeKeeperConstants.SkinTextureFolder + "/" + iconName);

                return _toolGooglePLayIcon;
            }
        }

        private void OnEnable()
        {
#if UNITY_PRE_5_1
            title = windowsName;
#else
            titleContent = new GUIContent(windowsName);
#endif


            var fixedSizes = new Vector2(WindowWidth, WindowHeight);
            maxSize = fixedSizes;
            minSize = fixedSizes;
        }

        [MenuItem("TimeKeeper/Settings _%&k", priority = 1)]
        public static void ShowWindows()
        {
            if (!_timeKeepWindows)
                InitializeWindow();
        }

        //   [MenuItem("TimeKeeper/About _%&o", priority = 1)]
        //    public static void AboutInmersoft()
        //   {
        //      EditorWindow.GetWindow<TK_About>(true);
        //   }

        [MenuItem("TimeKeeper/Make a Backup _%&l", priority = 1)]
        public static void MakeBackup()
        {
            Debug.Log("Making a backup");
            SaveAllProjectInUnityPackage();
        }

        private static TimeKeeper InitializeWindow()
        {
            _autoTimeKeeper = FileIO.LoadSettingsBoolValue(TimeKeeperConstants.EditorSettings);
            _timeKeepWindows = GetWindow<TimeKeeper>();
            LoadEditorResources();
            _timeKeepWindows.Show();
            return _timeKeepWindows;
        }

        private static void LoadEditorResources()
        {
            var guiDsmallLogo = AssetDatabase.FindAssets("KeepSaveLogo t:texture2D", null);
            if (guiDsmallLogo.Length >= 1)
            {
                _timeKeepWindows._keepSaveLogo =
                    AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guiDsmallLogo[0]), typeof(Texture2D)) as
                        Texture2D;
                _timeKeepWindows.titleContent.image = _timeKeepWindows._keepSaveLogo;
                _timeKeepWindows.titleContent.text = windowsName;
            }

            var guiDachievement = AssetDatabase.FindAssets("TimeKeeperSplash t:texture2D", null);
            if (guiDachievement.Length >= 1)
                _timeKeepWindows._mUiSplash =
                    AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guiDachievement[0]), typeof(Texture2D))
                        as Texture2D;
        }

        private void OnInspectorUpdate()
        {
            if (!_timeKeepWindows)
                InitializeWindow();
        }

        private void OnGUI()
        {
            if (!_timeKeepWindows) return;
            if (_timeKeepWindows._mUiSplash)
                GUI.DrawTexture(new Rect(ImageWidthX, ImageHeightY, ImageWidth, ImageHeight), _mUiSplash,
                    ScaleMode.ScaleAndCrop);
            //  GUI.DrawTexture(new Rect(115f, 260f, ImageWidth, ImageHeight), _mUiSplash, ScaleMode.ScaleAndCrop);
            DrawModuleSelectBar();
        }

        private static bool ToggleController(bool toggle)
        {
            var result = EditorGUILayout.Toggle(
                toggle,
                TimeKeeperGuiSkin.GetCustomStyle("Module Toggle"),
                GUILayout.Width(44));

            return result;
        }


        private static void AutoSaveOnRun(PlayModeStateChange state)
        {
            if (!_autoTimeKeeper) return;
            if (!EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying) return;
            Debug.Log("TimeKeeper: AutoSaving...");
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            EditorApplication.Beep();
        }


        public void DrawModuleSelectBar()
        {
            var productName = TimeKeeperConstants.ProductName;
            var description = TimeKeeperConstants.TimeKeeperDescription;
            var icon =
                AssetDatabase.LoadAssetAtPath<Texture2D>(TimeKeeperConstants.SkinTextureFolder + "/KeepSaveLogo");

            //  Action disableModule = GetDisableModuleAction(module);
            //  Action enableModule = GetEnableModuleAction(module);

            EditorGUILayout.BeginVertical(ModulePanelStyle, GUILayout.MinHeight(MinModuleBarHeight));
            EditorGUILayout.BeginHorizontal();


            GUILayout.Label(icon, ModuleHeaderIconStyle);


            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(productName, ModuleTitleStyle);
            EditorGUILayout.EndVertical();


            var toggleRect = Rect.zero;
            EditorGUI.BeginChangeCheck();
            _autoTimeKeeper = ToggleController(_autoTimeKeeper);

            if (EditorGUI.EndChangeCheck())
            {
                FileIO.SaveSettingsBoolValue(TimeKeeperConstants.EditorSettings, _autoTimeKeeper);

                EditorUtility.DisplayDialog(TimeKeeperConstants.ProductName,
                    _autoTimeKeeper
                        ? TimeKeeperConstants.ProductName +
                          " is active. Your project will be save any time when UnityEditor changes to PlayMode. If you want to make a backup, press Ctrl+Alt+L"
                        : TimeKeeperConstants.ProductName + " it's turned off. Your project will not be save.",
                    "OK");

                Debug.Log(_autoTimeKeeper
                    ? TimeKeeperConstants.ProductName + " is ON"
                    : TimeKeeperConstants.ProductName + " is OFF");
            }

            toggleRect = GUILayoutUtility.GetLastRect();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(description, ModuleDescriptionStyle);

            EditorGUILayout.EndVertical();


            DrawButton(OpenInmersoftFacebookPage, "Facebook Inmersoft", ToolFacebookIcon);
            DrawButton(OpenInmersoftGooglePlay, "Google Play Inmersoft", ToolGooglePLayIcon);

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField(TimeKeeperConstants.Copyright, ModuleDescriptionStyle);
        }

        public static void SaveAllProjectInUnityPackage()
        {
            var mainDirectoryName = Application.productName;
            var packageName = mainDirectoryName + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") +
                              ".unitypackage";
            var newFolderName = "TimeKeeperBackup";
            var newDir = Application.dataPath + Path.DirectorySeparatorChar + ".." +
                         Path.DirectorySeparatorChar + newFolderName;
            Directory.CreateDirectory(newDir);
            //    string exportDest = EditorUtility.OpenFolderPanel("Select Export Destination", newDir, "");
            AssetDatabase.ExportPackage(TimeKeeperConstants.AssetPath, newDir + "/" + packageName,
                ExportPackageOptions.Default | ExportPackageOptions.Interactive | ExportPackageOptions.Recurse);
        }


        public static void OpenInmersoftFacebookPage()
        {
            Application.OpenURL(TimeKeeperConstants.InmersoftFacebookUrl);
        }

        public static void OpenInmersoftGooglePlay()
        {
            Application.OpenURL(TimeKeeperConstants.InmersoftGoogleUrl);
        }


        private void DrawButton(Action toolAction, string buttonLabel, Texture2D icon)
        {
            // int itemCount = 1;
            // float buttonWidth = position.width / itemCount - (30f / itemCount);
            var buttonWidth = position.width - 2;
            float buttonHeight = 50;

            var style = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(2, 0, 0, 0),
                padding = new RectOffset(6, 6, 6, 6),
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                fixedWidth = buttonWidth,
                fixedHeight = buttonHeight
            };

            if (GUILayout.Button(new GUIContent(buttonLabel, icon), style))
                toolAction();
        }
    }
}