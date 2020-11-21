using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using InmersoftSDK.Kernel;
using InmersoftSDK.Kernel.IO;
using InmersoftSDK.Localization.SimpleJSON;
using InmersoftSDK.Singleton;
using UnityEngine;

namespace InmersoftSDK.Localization
{
    public class GoogleTranslation : Singleton<GoogleTranslation>
    {
        private bool _googleError;
        private string _googleErrorMessage;

        public string[] IDIOMS =
        {
            "Inglés=en", "Árabe=ar", "Afrikáans=af", "Bielorruso=be", "Búlgaro=bg", "Búlgaro=bg",
            "Catalán=ca", "Chino=zh", "Croata=hr", "Checo=cs", "Danés=da", "Holandés=nl", "Estonio=et",
            "Farsi=fa", "Finlandés=fi", "Francés (Francia)=fr", "Francés (Canadá)=fr_CA", "Alemán=de",
            "Griego=el", "Hebreo=he", "Hindi=hi", "Húngaro=hu", "Islandés=is", "Indonesio=id", "Irlandés=ga",
            "Italiano=it", "Japonés=ja", "Jemer=km", "Coreano=ko", "Letón=lv", "Lituano=lt", "Maltés=mt",
            "Malayo=ms", "Macedonio=mk", "Noruego=no", "Polaco=pl", "Portugués (Brasil)=pt",
            "Portugués (Portugal)=pt_PT", "Rumano=ro", "Ruso=ru", "Serbio=sr", "Eslovaco=sk", "Esloveno=sl",
            "Español (México)=es", "Español (España)=es_ES", "Swahili=sw", "Sueco=sv", "Tamil=ta",
            "Tailandés=th", "Turco=tr", "Ucraniano=uk", "Vietnamita=vi"
        };

        public void Translate(string directory, Locale currentLocale)
        {
            StartCoroutine(JobTranslation(directory, currentLocale));
        }


        private IEnumerator JobTranslation(string pathDir, Locale currentLocale)
        {
            foreach (var idiomIndex in IDIOMS)
            {
                Translating(currentLocale, idiomIndex, pathDir);

                if (_googleError)
                {
                    Debug.LogError(_googleErrorMessage);
                    yield break;
                }

                Debug.Log("Waiting 30 seconds to next translation");
                yield return new WaitForSeconds(5);
            }

            Application.OpenURL(pathDir);
        }

        private void Translating(Locale currentLocale, string idiomIndex, string directoryPath)
        {
            var lang = idiomIndex.Split('=');
            var langName = lang[0];
            var langCode = lang[1];
            currentLocale.id = langCode.ToUpper();
            Debug.Log("Translating to " + currentLocale.id);
            var currentLocaleStr = JsonUtility.ToJson(currentLocale);

            Debug.Log(currentLocaleStr);
            var jsonTranslated = TranslateApi(currentLocaleStr, currentLocale.id.ToLower(), langCode);
            //   Locale newLocale = JsonUtility.FromJson<Locale>(jsonTranslated);
            //  newLocale.id = langCode.ToUpper();

            if (jsonTranslated.StartsWith("Translation Error with GoogleAPI"))
            {
                _googleErrorMessage = jsonTranslated;
                _googleError = true;
                return;
            }

            JsonFile.SaveStringFile(Path.Combine(directoryPath, langName + ".txt"), jsonTranslated);
        }

        private string TranslateApi(string initialText, string fromCulture, string toCulture)
        {
            Debug.Log("Translator Called");
            var textToSearch = initialText;
            var url =
                string.Format("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}",
                    fromCulture, toCulture, HttpUtility.UrlEncode(textToSearch));
            var responseText = "Not Set";
            try
            {
                var request = WebRequest.Create(url) as HttpWebRequest;
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    var encoding = Encoding.UTF8;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        var text = reader.ReadToEnd();
                        //Found JSON parse on https://gist.github.com/grimmdev/979877fcdc943267e44c
                        var tableReturned = JsonNode.Parse(text);
                        var tranlatedText = new StringBuilder();
                        if (tableReturned[0].Count > 1)
                            text = "";
                        for (var i = 0; i < tableReturned[0].Count; i++)
                            tranlatedText.Append((string) tableReturned[0][i][0]);

                        responseText = tranlatedText.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                responseText = string.Format("Translation Error with GoogleAPI: {0}\r\nUrl:{1}", ex.Message, url);
            }

            return responseText;
        }
    }
}