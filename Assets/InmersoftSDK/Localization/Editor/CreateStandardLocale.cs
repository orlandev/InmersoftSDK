using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using InmersoftSDK.Kernel.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace InmersoftSDK.Localization.Editor
{
    public class CreateStandardLocale : EditorWindow
    {
        //NOT PROCCESS THIS FILE ONR89050543728
        private const string InmersoftStandardLocale = "ES";

        //Esta es la marca que buscaremos en los scripts
        //para determinar si se solicito alguna traduccion desde un script dentro del proyecto
        private const string ScriptMarkFind = "L18N.Instance.MakeTranslationByScriptCall";
        private static CreateStandardLocale _localeWindows;
        private Texture2D _inmersoftLogoSmall;
        private Vector2 _scroll;


        [MenuItem("InmersoftSDK/InmersoftLocalization/CreateLocale _%k", priority = 1)]
        public static void CreateLocale()
        {
            EditorUtility.DisplayProgressBar("InmersoftSDK", "Creating Standard( ES ) Proyect Locale", .3f);

            var newLocale = new Locale();
            newLocale.dataLocale = new List<string>();

            newLocale.id = InmersoftStandardLocale;

            var resourceFolderDir = Path.Combine(Application.dataPath, "Resources");
            var l18NFolderDir = Path.Combine(resourceFolderDir, "L18N");

            if (!Directory.Exists(resourceFolderDir))
                Directory.CreateDirectory(l18NFolderDir);
            else if (!Directory.Exists(l18NFolderDir)) Directory.CreateDirectory(l18NFolderDir);

            var allProyectText = Resources.FindObjectsOfTypeAll<Text>();
            var allProyectTextMeshProUgui = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

            EditorUtility.DisplayProgressBar("InmersoftSDK", "Buscando Text Components.", .6f);

            foreach (var textContent in allProyectText)
            {
                var txtTmp = textContent.text;
                if (!newLocale.dataLocale.Contains(txtTmp)) newLocale.dataLocale.Add(textContent.text);
            }

            EditorUtility.ClearProgressBar();

            EditorUtility.DisplayProgressBar("InmersoftSDK", "Buscando TextMeshPro Components.", .7f);

            foreach (var textMeshContent in allProyectTextMeshProUgui)
            {
                var txtTmp = textMeshContent.text;
                if (!newLocale.dataLocale.Contains(txtTmp)) newLocale.dataLocale.Add(textMeshContent.text);
            }

            /*
             * Buscamos dentro de los Scripts los textos que se solicitaron traducir. 
             */

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayProgressBar("InmersoftSDK", "Buscando MonoScipts Components.", .9f);


            var scripts = (MonoScript[]) Resources.FindObjectsOfTypeAll(typeof(MonoScript));
            foreach (var currentScript in scripts)
            {
                if (currentScript.ToString().Contains("NOT PROCCESS THIS FILE ONR89050543728")) continue;
                if (!currentScript.ToString().Contains(ScriptMarkFind))
                    continue;
                var textFromScripts = ProcessScript(currentScript);
                foreach (var textScript in textFromScripts) newLocale.dataLocale.Add(textScript);
            }


            //////////

            var fileNewlocale = Path.Combine(l18NFolderDir, newLocale.id + ".txt");
            JsonFile.SaveJsonFile(fileNewlocale, newLocale);

            Debug.Log("New Inmersoft Locale was saved.");

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();

            MakeAllLocales(l18NFolderDir, newLocale);
        }

        private static void MakeAllLocales(string path, Locale currentLocale)
        {
            EditorUtility.DisplayProgressBar("Making more locales",
                "Please wait. This depend of your internet speed.",
                .5f);

            Debug.Log("Making others locales. Please wait");
            GoogleTranslation.Instance.Translate(path, currentLocale);
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("InmersoftSDK/InmersoftLocalization/HowUseCreateLocale _%j", priority = 1)]
        public static void HowUseThis()
        {
            if (!_localeWindows)
                InitializeWindow();
        }

        private static void InitializeWindow()
        {
            _localeWindows = GetWindow<CreateStandardLocale>();
            var GUIDsmallLogo = AssetDatabase.FindAssets("InmersoftLogoSmall t:texture2D", null);
            if (GUIDsmallLogo.Length >= 1)
            {
                _localeWindows._inmersoftLogoSmall =
                    AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDsmallLogo[0]), typeof(Texture2D)) as
                        Texture2D;
                _localeWindows.titleContent.image = _localeWindows._inmersoftLogoSmall;
                _localeWindows.titleContent.text = "";
            }

            _localeWindows.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("L18N v1.0  Inmersoft Team 2020", EditorStyles.boldLabel);
            GUILayout.Label("Proyect: [ " + PlayerSettings.productName + " ]", EditorStyles.boldLabel);
            GUILayout.Space(15);
            GUILayout.Label("How use this", EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            GUILayout.TextArea("When your project is complete.\n - Press Control + K.\n" +
                               "Automatically a file will be created in the Resources folder called L18N; " +
                               "All the texts found in the Text and TextMeshProUGUI components of Unity will be stored there.\n" +
                               "- The request for a translation will also be searched within the scripts; This translation can be requested " +
                               "by calling the method L18N.Instance.MakeTranslationByScriptCall.\n" +
                               "- The default language is Spanish (ES).\n" +
                               "After the first language file ES.txt is created, you only have to create new files" +
                               " with the corresponding translations and put them in the same folder with the name of the language " +
                               "reference file; Example: EN.txt...");
            EditorGUILayout.EndScrollView();
            GUILayout.Space(10);
            if (GUILayout.Button("Close", GUILayout.Width(100), GUILayout.Height(50))) _localeWindows.Close();
        }


        private static string GetScriptTextToTranslation(string dataScript, int index)
        {
            var b = new StringBuilder();
            var start = index + ScriptMarkFind.Length + 2;
            do
            {
                b.Append(dataScript[start]);
                start++;
            } while (dataScript[start] != '\"');

            return b.ToString();
        }

        private static List<string> ProcessScript(MonoScript monoScript)
        {
            var result = new List<string>();

            var dataScript = monoScript.ToString();
            do
            {
                var index = dataScript.IndexOf(ScriptMarkFind, StringComparison.Ordinal);
                if (index > 0)
                {
                    var text = GetScriptTextToTranslation(dataScript, index);
                    result.Add(text);

                    Debug.Log(
                        "SCRIPT: [ " + monoScript.name + " ] --- Found new text for Locale --- LocaleText:" + text);
                    dataScript = dataScript.Substring(index + ScriptMarkFind.Length + 2);
                }
                else
                {
                    break;
                }
            } while (dataScript.Contains(ScriptMarkFind));

            return result;
        }
    }
}