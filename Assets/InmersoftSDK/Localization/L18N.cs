using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InmersoftSDK.Kernel;
using InmersoftSDK.Kernel.IO;
using InmersoftSDK.Singleton;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InmersoftSDK.Localization
{
    public class L18N : Singleton<L18N>
    {
        private const string LocaleFolderName = "L18N";
        private readonly string StandardLocaleId = "ES";
        private Locale _appLocale;

        private void Start()
        {
            LoadLocale(StandardLocaleId);
        }

        /// <summary>
        ///     Devuleve el ID del Locale actual
        ///     Ejepmlo ES, EN, FR
        /// </summary>
        /// <returns></returns>
        public string CurrentLocaleId()
        {
            if (_appLocale == null) LoadLocale(StandardLocaleId);
            return _appLocale.id;
        }

        private IEnumerator ProcessTranslation(Locale currentLocale)
        {
            var allAppText = Resources.FindObjectsOfTypeAll<Text>();
            var allAppTextMeshPro = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

            foreach (var textComp in allAppText)
            {
                var str = textComp.text;
                var index = GetIndexByText(str, currentLocale);
                if (index >= 0) textComp.text = _appLocale.dataLocale[index];

                yield return null;
            }

            foreach (var textMeshComp in allAppTextMeshPro)
            {
                var str = textMeshComp.text;
                var index = GetIndexByText(str, currentLocale);
                if (index >= 0) textMeshComp.text = _appLocale.dataLocale[index];

                yield return null;
            }
        }

        private int GetIndexByText(string strText, Locale currentLocale)
        {
            var index = -1;
            for (var i = 0; i < currentLocale.dataLocale.Count; i++)
            {
                if (!currentLocale.dataLocale[i].Equals(strText)) continue;
                index = i;
                break;
            }

            return index;
        }


        /// <summary>
        ///     Carga un locale desde el Resource
        ///     El ID del locale correspondiente Ej: ES, EN, FR
        /// </summary>
        /// <param name="id"></param>
        private void LoadLocale(string id)
        {
            _appLocale =
                JsonFile.LoadJsonAssetFile<Locale>(Path.Combine(LocaleFolderName, id));
        }


        /// <summary>
        ///     Este metodo se llama desde los scripts
        ///     Traduce un texto determinado por su correspondiente en el destinationLocale
        /// </summary>
        /// <param name="text2Translate"></param>
        /// <param name="destinyLocale"></param>
        /// <returns></returns>
        public string MakeTranslationByScriptCall(string text2Translate, string destinyLocale)
        {
            if (_appLocale == null) LoadLocale(StandardLocaleId);
            var currentLocale = _appLocale;
            LoadLocale(destinyLocale);
            if (_appLocale == null) return null;
            var index = -1;
            for (var i = 0; i < currentLocale.dataLocale.Count; i++)
            {
                if (!currentLocale.dataLocale[i].Equals(text2Translate)) continue;
                index = i;
                break;
            }

            return index >= 0 ? _appLocale.dataLocale[index] : null;
        }

        /// <summary>
        ///     ID corresponde a ES EN FR ...
        ///     Ejecuta una Traduccion general en tiempo de ejecucion
        /// </summary>
        /// <param name="id"></param>
        public void MakeTranslation(string id)
        {
            if (_appLocale == null) LoadLocale(StandardLocaleId);
            if (id.Equals(_appLocale.id)) return;
            var currentLocale = _appLocale;
            LoadLocale(id);
            if (_appLocale != null) StartCoroutine(ProcessTranslation(currentLocale));
        }

        public List<string> AllLocalesId()
        {
            var localesId = Resources.LoadAll<TextAsset>("L18N");
            return localesId.Select(localeAsset => localeAsset.name).ToList();
        }
    }
}