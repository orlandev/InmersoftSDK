using System.IO;
using UnityEngine;

namespace InmersoftSDK.Kernel.IO
{
    public static class JsonFile
    {
        /// <summary>
        ///     Carga un TextAsset dentro de una clase ya serializada
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T LoadJsonAssetFile<T>(string path) where T : class
        {
            var textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null) return null;
            var data = JsonUtility.FromJson<T>(textAsset.text);
            return data;
        }

        /// <summary>
        ///     /Carga un fichero de texto que se encuentre fuera del proyecto
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T LoadJsonFile<T>(string path) where T : class
        {
            if (!File.Exists(path)) return null;
            var file = new StreamReader(path);
            var fileContents = file.ReadToEnd();
            var data = JsonUtility.FromJson<T>(fileContents);
            file.Close();
            return data;
        }

        /// <summary>
        ///     Guarda datos en un fichero determinado
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        public static void SaveJsonFile<T>(string path, T data) where T : class
        {
            var file = new StreamWriter(path);
            var json = JsonUtility.ToJson(data);
            file.WriteLine(json);
            file.Close();
        }

        public static void SaveStringFile(string path, string data)
        {
            var file = new StreamWriter(path);
            file.WriteLine(data);
            file.Close();
        }
    }
}